/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaWatchedVariableService))]
    [Export(typeof(SledLuaWatchedVariableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaWatchedVariableService : IInitializable, ISledLuaWatchedVariableService
    {
        static SledLuaWatchedVariableService()
        {
            s_luaVarScopeTypes = (IEnumerable<SledLuaVarScopeType>)Enum.GetValues(typeof(SledLuaVarScopeType));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.UpdateBegin += DebugServiceUpdateBegin;
            m_debugService.UpdateSync += DebugServiceUpdateSync;
            m_debugService.UpdateEnd += DebugServiceUpdateEnd;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_projectService = SledServiceInstance.Get<ISledProjectService>();
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closing += ProjectServiceClosing;

            m_luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();
            m_luaVarScmpService = SledServiceInstance.Get<ISledLuaVarScmpService>();
            m_languagePluginService = SledServiceInstance.Get<ISledLanguagePluginService>();
            m_luaVarLocationService = SledServiceInstance.Get<ISledLuaVariableLocationService>();

            m_editor.TreeListViewAdapter.ItemLazyLoad += TreeListViewAdapterItemLazyLoad;
            m_editor.KeyUp += EditorKeyUp;

            // Adjust to shortened name if this language plugin is the only one loaded
            if (m_languagePluginService.Count == 1)
                m_editor.Name = Localization.SledLuaWatchListTitleShort;
        }

        #endregion

        #region ISledLuaWatchedVariableService

        public bool ReceivingWatchedVariables { get; private set; }

        public bool IsLuaVarWatched(ISledLuaVarBaseType luaVar)
        {
            foreach (var kv in m_dictProjWatchToLuaVar)
            {
                var projLuaVar = kv.Value.LuaVar;
                if (projLuaVar == null)
                    continue;

                if (m_comparer.Equals(projLuaVar, luaVar))
                    return true;
            }

            return false;
        }

        public void AddWatchedLuaVar(ISledLuaVarBaseType luaVar)
        {
            if ((m_project == null) ||
                (m_watchCollection == null) ||
                (m_editor == null))
                return;

            if (IsLuaVarWatched(luaVar))
                return;

            // Item is live and we just need to copy it to the watch list GUI
            var watch = SledLuaProjectFilesWatchType.CreateFromLuaVar(luaVar);

            // Add to the project watches list
            m_project.Watches.Add(watch);

            var clone = (ISledLuaVarBaseType)luaVar.Clone();
            var handle = new LuaVarHandle { LuaVar = clone };
            m_dictProjWatchToLuaVar.Add(watch, handle);

            // Add to GUI
            m_watchCollection.Add(clone);

            // Force save settings so the watch gets written to the .spf/.lpf
            m_projectService.SaveSettings(true);
        }

        public void RemoveWatchedLuaVar(ISledLuaVarBaseType luaVar)
        {
            if ((m_project == null) ||
                (m_watchCollection == null) ||
                (m_editor == null))
                return;

            if (!IsLuaVarWatched(luaVar))
                return;

            // Remove from GUI
            m_watchCollection.Remove(luaVar);

            var pair = m_dictProjWatchToLuaVar.First(kv => m_comparer.Equals(kv.Value.LuaVar, luaVar));
            m_dictProjWatchToLuaVar.Remove(pair.Key);
            m_project.Watches.Remove(pair.Key);

            // Save .spf/.lpf changes
            m_projectService.SaveSettings(true);
        }

        public bool ReceivingWatchedCustomVariables { get; private set; }

        public bool IsCustomWatchedVariable(ISledLuaVarBaseType luaVar)
        {
            return (m_watchCollection != null) && m_watchCollection.IsCustomWatchedVariable(luaVar);
        }

        public void AddCustomWatchedVariable(SledLuaWatchedCustomVariable variable)
        {
            // TODO:
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaWatchLookupBegin:
                    ReceivingWatchedVariables = true;
                    break;

                case Scmp.LuaTypeCodes.LuaWatchLookupEnd:
                {
                    ReceivingWatchedVariables = false;
                    ReceivingWatchedCustomVariables = ReceivingWatchedVariables;
                    RemoteTargetWatchFinished();
                }
                break;

                case Scmp.LuaTypeCodes.LuaWatchLookupClear:
                {
                    ReceivingWatchedVariables = true;
                    m_lastWatchRootNode = null;
                }
                break;

                case Scmp.LuaTypeCodes.LuaWatchLookupProjectBegin:
                {
                    m_recvProjWatchLuaVar = true;
                }
                break;

                case Scmp.LuaTypeCodes.LuaWatchLookupProjectEnd:
                {
                    m_recvProjWatchLuaVar = false;
                    if (m_lstProjectWatches.Count > 0)
                        m_lstProjectWatches.RemoveAt(0);
                }
                break;

                case Scmp.LuaTypeCodes.LuaWatchLookupCustomBegin:
                {
                    ReceivingWatchedCustomVariables = true;
                }
                break;

                case Scmp.LuaTypeCodes.LuaWatchLookupCustomEnd:
                {
                    ReceivingWatchedCustomVariables = false;
                }
                break;

                case Scmp.LuaTypeCodes.LuaVarGlobal:
                {
                    if (ReceivingWatchedVariables)
                    {
                        var global = m_luaVarScmpService.GetScmpBlobAsLuaGlobalVar();
                        if (global != null)
                            ProcessWatchedVar(global);
                    }
                }
                break;

                case Scmp.LuaTypeCodes.LuaVarLocal:
                {
                    if (ReceivingWatchedVariables)
                    {
                        var local = m_luaVarScmpService.GetScmpBlobAsLuaLocalVar();
                        if (local != null)
                            ProcessWatchedVar(local);
                    }
                }
                break;

                case Scmp.LuaTypeCodes.LuaVarUpvalue:
                {
                    if (ReceivingWatchedVariables)
                    {
                        var upvalue = m_luaVarScmpService.GetScmpBlobAsLuaUpvalueVar();
                        if (upvalue != null)
                            ProcessWatchedVar(upvalue);
                    }
                }
                break;

                case Scmp.LuaTypeCodes.LuaVarEnvVar:
                {
                    if (ReceivingWatchedVariables)
                    {
                        var envVar = m_luaVarScmpService.GetScmpBlobAsLuaEnvironmentVar();
                        if (envVar != null)
                            ProcessWatchedVar(envVar);
                    }
                }
                break;
            }
        }

        private void DebugServiceUpdateBegin(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            // Save while items still in collection
            m_watchCollection.SaveExpandedStates();

            // Save while items still on GUI
            m_editor.SaveState();

            // Clear GUI
            m_editor.View = null;

            // Clear all items
            m_watchCollection.Clear();
            m_editor.View = m_watchCollection;
        }

        private void DebugServiceUpdateSync(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            HandleDefaultWatchSync(e);
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            FinalizeWatchedLists();
            m_editor.RestoreState();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            // Clear GUI
            m_editor.View = null;

            // Clear all items
            m_watchCollection.Clear();
            m_editor.View = m_watchCollection;

            FinalizeWatchedLists();
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_project = e.Project;
            CreateWatchCollection();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_project = e.Project;
            CreateWatchCollection();
            LoadSavedWatches();
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            DestroyWatchCollection();

            m_dictProjWatchToLuaVar.Clear();
            m_dictProjWatchToCustomVar.Clear();
            m_lstProjectWatches.Clear();
            m_recvProjWatchLuaVar = false;

            ReceivingWatchedVariables = false;
            ReceivingWatchedCustomVariables = ReceivingWatchedVariables;
            m_lstQueuedLookUps.Clear();
            m_lastWatchRootNode = null;

            m_project = null;
        }

        #endregion

        private void CreateWatchCollection()
        {
            var root =
                new DomNode(
                    SledLuaSchema.SledLuaVarWatchListType.Type,
                    SledLuaSchema.SledLuaVarWatchListRootElement);

            m_watchCollection = root.As<SledLuaVarWatchListType>();
            m_watchCollection.Name =
                string.Format(
                    "{0}{1}{2}",
                    m_projectService.ProjectName,
                    Resources.Resource.Space,
                    Resources.Resource.LuaWatchList);

            m_watchCollection.DomNode.AttributeChanged += DomNodeAttributeChanged;

            m_editor.View = m_watchCollection;
        }

        private void DestroyWatchCollection()
        {
            // Clear GUI
            m_editor.View = null;

            if (m_watchCollection == null)
                return;

            m_watchCollection.DomNode.AttributeChanged -= DomNodeAttributeChanged;

            m_watchCollection.Clear();
            m_watchCollection = null;
        }

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (m_bUndoingAttribute)
                return;

            if ((e.AttributeInfo != SledLuaSchema.SledLuaVarGlobalType.valueAttribute) &&
                (e.AttributeInfo != SledLuaSchema.SledLuaVarLocalType.valueAttribute) &&
                (e.AttributeInfo != SledLuaSchema.SledLuaVarUpvalueType.valueAttribute) &&
                (e.AttributeInfo != SledLuaSchema.SledLuaVarEnvType.valueAttribute))
                return;

            var bUndo = true;

            try
            {
                var bCanModifyVariables =
                    m_debugService.IsConnected &&
                    !m_debugService.IsDebugging;

                if (!bCanModifyVariables)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        Localization.SledLuaModifyVarInvalidTime);

                    return;
                }

                var luaVar = e.DomNode.As<ISledLuaVarBaseType>();
                if (luaVar == null)
                    return;

                // Check if variable type is modifiable by SLED
                if (!SledLuaUtil.IsEditableLuaType(luaVar))
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        Localization.SledLuaModifyVarInvalidType);

                    return;
                }

                string szNewValue = null;

                // Check if what the user entered is valid for that particular Lua type
                switch (luaVar.LuaType)
                {
                    case LuaType.LUA_TNUMBER:
                    {
                        // Make sure no letters
                        double dNumber;
                        var szValue = e.NewValue.ToString();
                        if (!double.TryParse(szValue, out dNumber))
                        {
                            SledOutDevice.OutLine(
                                SledMessageType.Error,
                                SledUtil.TransSub(Localization.SledLuaModifyVarInvalidValue1, szValue));

                            return;
                        }

                        szNewValue = szValue;
                    }
                    break;

                    case LuaType.LUA_TSTRING:
                    {
                        szNewValue = e.NewValue.ToString();
                    }
                    break;

                    case LuaType.LUA_TBOOLEAN:
                    {
                        // Make sure true/false/1/0
                        var szValue = e.NewValue.ToString().ToLower();
                        if (string.Compare(szValue, Resources.Resource.True, StringComparison.OrdinalIgnoreCase) == 0)
                            szNewValue = Resources.Resource.One;
                        else if (string.Compare(szValue, Resources.Resource.False, StringComparison.OrdinalIgnoreCase) == 0)
                            szNewValue = Resources.Resource.Zero;
                        else
                        {
                            if ((string.Compare(szValue, Resources.Resource.One, StringComparison.Ordinal) != 0) &&
                                (string.Compare(szValue, Resources.Resource.Zero, StringComparison.Ordinal) != 0))
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    SledUtil.TransSub(Localization.SledLuaModifyVarInvalidValue2, e.NewValue));

                                return;
                            }

                            szNewValue = szValue;
                        }
                    }
                    break;
                }

                // Somehow the value the user wanted didn't make it through
                if (string.IsNullOrEmpty(szNewValue))
                    return;

                // Grab the correct context so that the runtime knows how to manage the Lua stack
                var context = SledLuaVarLookUpContextType.WatchProject;
                {
                    foreach (var kv in m_dictProjWatchToLuaVar)
                    {
                        var projLuaVar = kv.Value.LuaVar;
                        if (projLuaVar == null)
                            continue;

                        var luaVarRootParent = GetRootLevelParent(luaVar);
                        if (luaVarRootParent == null)
                            continue;

                        if (!m_comparer.Equals(projLuaVar, luaVarRootParent))
                            continue;

                        context = kv.Key.Context;
                        break;
                    }
                }
                
                var lookUp = SledLuaVarLookUpType.FromLuaVar(luaVar, context);
                if (lookUp == null)
                    return;

                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledLuaModifyVarSent);

                // Send message off
                m_debugService.SendScmp(new Scmp.LuaVarUpdate(m_luaLanguagePlugin.LanguageId, lookUp, szNewValue, (int)luaVar.LuaType));

                bUndo = false;
            }
            finally
            {
                if (bUndo)
                    UndoAttributeChange(e);
            }
        }

        private void UndoAttributeChange(AttributeEventArgs e)
        {
            try
            {
                m_bUndoingAttribute = true;
                e.DomNode.SetAttribute(e.AttributeInfo, e.OldValue);
            }
            finally
            {
                m_bUndoingAttribute = false;
            }
        }

        private void TreeListViewAdapterItemLazyLoad(object sender, TreeListViewAdapter.ItemLazyLoadEventArgs e)
        {
            if (!m_debugService.IsConnected || m_debugService.IsDebugging)
                return;

            // Get object as a Lua variable
            var luaVar = e.Item.As<SledLuaVarBaseType>();
            if (luaVar == null)
                return;

            if (luaVar.What == SledLuaVarBaseType.InvalidVarWhat)
                return;

            var context = SledLuaVarLookUpContextType.WatchProject;
            {
                foreach (var kv in m_dictProjWatchToLuaVar)
                {
                    var projLuaVar = kv.Value.LuaVar;
                    if (projLuaVar == null)
                        continue;

                    var luaVarRootParent = GetRootLevelParent(luaVar);
                    if (luaVarRootParent == null)
                        continue;

                    if (!m_comparer.Equals(projLuaVar, luaVarRootParent))
                        continue;

                    context = kv.Key.Context;
                    break;
                }
            }

            var lookUp = SledLuaVarLookUpType.FromLuaVar(luaVar, context);
            if (lookUp == null)
            {
                SledOutDevice.OutLine(SledMessageType.Error, "[Lua Watched Variable Service] Error generating lookUp for {0}!", luaVar.DisplayName);
                return;
            }

            // Convert string to SCMP message
            var scmp = new Scmp.LuaVarLookUp(m_luaLanguagePlugin.LanguageId, lookUp);

            // Add to the list of queued lookups
            m_lstQueuedLookUps.Add(luaVar);

            luaVar.Expanded = true;

            // Send message saying watched variable lookup is beginning
            m_debugService.SendScmp(new Scmp.LuaWatchLookupBegin(m_luaLanguagePlugin.LanguageId, lookUp.Scope));

            // Send lookup message
            m_debugService.SendScmp(scmp);

            // Send message saying watched variable lookup is beginning
            m_debugService.SendScmp(new Scmp.LuaWatchLookupEnd(m_luaLanguagePlugin.LanguageId, lookUp.Scope));
        }

        private void EditorKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            var editor = sender.As<SledLuaWatchesEditor>();
            if (editor == null)
                return;

            var luaVars = new List<ISledLuaVarBaseType>(editor.SelectionAs<ISledLuaVarBaseType>());

            foreach (var luaVar in luaVars)
            {
                var rootLevelLuaVar = SledLuaUtil.GetRootLevelVar(luaVar);
                if (!IsLuaVarWatched(rootLevelLuaVar))
                    continue;

                RemoveWatchedLuaVar(rootLevelLuaVar);
            }
        }

        private void ProcessWatchedVar(ISledLuaVarBaseType watchedLuaVar)
        {
            // Add any locations
            m_luaVarLocationService.AddLocations(watchedLuaVar);

            // Assume visible (not filtered)
            watchedLuaVar.Visible = true;

            if (ReceivingWatchedVariables && !m_debugService.IsUpdateInProgress)
            {
                // This is a variable whose ancestor is being watched and
                // we need to add it to an existing node on the watch list

                // We get this kind of situation when already stopped on a
                // breakpoint and the user clicks to expand a node in the
                // watch GUI
                if (m_lstQueuedLookUps.Count > 0)
                {
                    m_watchCollection.AddToItem(m_lstQueuedLookUps[0], watchedLuaVar, false);
                }
            }
            else if (ReceivingWatchedVariables && m_debugService.IsUpdateInProgress)
            {
                // This is a variable being watched and is either a root watch variable
                // (meaning the user selected "add to watch" on this specific variable)
                // or it is a child of the root watch and represents an expanded item
                // on the GUI. 

                // We get this kind of situation when a breakpoint is hit and watch
                // items are already expanded on the watch GUI and we need to restore
                // all those expanded states for the breakpoint just hit

                if (m_lastWatchRootNode != null)
                {
                    // Add as a child
                    m_watchCollection.AddToItem(m_lastWatchRootNode, watchedLuaVar, true);
                }
                else
                {
                    m_watchCollection.Add(watchedLuaVar);
                    m_lastWatchRootNode = watchedLuaVar;
                }

                // Associate the incoming variable with a project watch
                if (m_recvProjWatchLuaVar && (m_lstProjectWatches.Count > 0))
                {
                    LuaVarHandle handle;
                    if (m_dictProjWatchToLuaVar.TryGetValue(m_lstProjectWatches[0], out handle))
                    {
                        handle.LuaVar = watchedLuaVar;
                        CheckLuaVarAndProjectWatchScope(watchedLuaVar, m_lstProjectWatches[0]);
                    }
                }
            }
        }

        private void RemoteTargetWatchFinished()
        {
            if (m_lstQueuedLookUps.Count <= 0)
                return;

            m_lstQueuedLookUps.RemoveAt(0);
        }

        private void LoadSavedWatches()
        {
            if (m_project == null)
                return;

            // Keep a list of invalid watches
            var lstInvalidWatches = new List<SledProjectFilesWatchType>();

            // Add in watched variables
            foreach (var watch in m_project.Watches)
            {
                var luaWatch = watch.As<SledLuaProjectFilesWatchType>();
                if (luaWatch == null)
                    continue;

                // Can't do anything if we don't know how to look this variable up
                if (luaWatch.LookUp == null)
                {
                    lstInvalidWatches.Add(luaWatch);
                    continue;
                }

                try
                {
                    // Create a fake item to add to the GUI until the real item is received from the target
                    var fakeLuaVar = CreateFakeLuaVar(luaWatch);
                    if (fakeLuaVar == null)
                    {
                        // Remove later
                        lstInvalidWatches.Add(watch);
                        continue;
                    }

                    var handle = new LuaVarHandle { LuaVar = fakeLuaVar };
                    m_dictProjWatchToLuaVar.Add(luaWatch, handle);

                    // Add clone to watch list GUI
                    m_watchCollection.Add(fakeLuaVar);

                    // Project .spf/.lpf does not need saving
                }
                catch (Exception)
                {
                    // Lua watch is invalid... so remove it
                    lstInvalidWatches.Add(watch);
                }
            }

            if (lstInvalidWatches.Count <= 0)
                return;

            // Remove any invalid watches
            foreach (var invalidWatch in lstInvalidWatches)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledLuaWatchFormatIncorrect, invalidWatch.Name));

                m_project.Watches.Remove(invalidWatch);
            }

            // Save to .spf/.lpf since watches were removed            
            m_projectService.SaveSettings();
        }

        private void FinalizeWatchedLists()
        {
            // Create fake entries for items we didn't receive during the network update
            foreach (var kv in m_dictProjWatchToLuaVar)
            {
                if (kv.Value.LuaVar != null)
                    continue;

                var fakeLuaVar = CreateFakeLuaVar(kv.Key);
                kv.Value.LuaVar = fakeLuaVar;

                DoDebugMessage(SledMessageType.Warning, "[Lua Watched Variable Service] Creating fake: {0}", fakeLuaVar.Name);

                try
                {
                    if (kv.Key.Context == SledLuaVarLookUpContextType.WatchCustom)
                        ReceivingWatchedCustomVariables = true;

                    m_watchCollection.Add(fakeLuaVar);
                }
                finally
                {
                    ReceivingWatchedCustomVariables = false;
                }
            }
        }

        private void HandleDefaultWatchSync(SledDebugServiceBreakpointEventArgs args)
        {
            //
            // Build up a list of how watched variables should be requested.
            //
            // The order of this list is very important. Any items that are
            // expanded must be in the list right after their parent item or they
            // may be added to the GUI in a weird place and/or parented incorrectly.
            //

            m_lstProjectWatches.Clear();
            m_lastWatchRootNode = null;

            // grab all custom variables from providers
            var allCustomVariables = new List<SledLuaWatchedCustomVariable>();
            {
                var providers = SledServiceInstance.GetAll<ISledLuaWatchedVariableProvider>();
                foreach (var provider in providers)
                {
                    allCustomVariables.AddRange(provider.GetVariables(args));
                }
            }

            // drop any custom variables we're not supposed to continue watching
            {
                // determine which custom variables exist currently
                var currentCustomProjectWatchGuids = m_dictProjWatchToLuaVar
                    .Where(kv => kv.Key.Context == SledLuaVarLookUpContextType.WatchCustom)
                    .Select(kv => kv.Key.Guid);

                var currentCustomVarGuids = allCustomVariables.Select(v => v.Guid);

                var toRemove = currentCustomProjectWatchGuids.Except(currentCustomVarGuids).ToList();
                foreach (var remove in toRemove)
                {
                    var localRemove = remove;
                    var pair = m_dictProjWatchToLuaVar.First(kv => kv.Key.Guid == localRemove);
                    m_dictProjWatchToLuaVar.Remove(pair.Key);
                }
            }

            // merge project watches and custom variables together
            {
                KeyValuePair<SledLuaProjectFilesWatchType, LuaVarHandle> defaultValue =
                    default(KeyValuePair<SledLuaProjectFilesWatchType, LuaVarHandle>);

                foreach (var customVariable in allCustomVariables)
                {
                    var localCustomVariable = customVariable;
                    var alreadyExistsAsProjectWatch = m_dictProjWatchToLuaVar.FirstOrDefault(kv => kv.Key.Guid == localCustomVariable.Guid);

                    if (!alreadyExistsAsProjectWatch.Equals(defaultValue))
                        continue;

                    var tempList = CreateNamesAndTypesFrom(customVariable.NamesAndTypes);
                    var watch =
                        SledLuaProjectFilesWatchType.CreateFromCustom(
                            customVariable.Alias,
                            customVariable.Scope,
                            tempList,
                            customVariable.Guid);

                    DoDebugMessage(SledMessageType.Warning, "[Lua Watched Variable Service] Creating new project watch for {0}", customVariable.Alias);
                    m_dictProjWatchToLuaVar.Add(watch, new LuaVarHandle());
                }
            }

            // keep a mapping of project watches to custom variables
            {
                m_dictProjWatchToCustomVar.Clear();
                var currentCustomProjectWatches = m_dictProjWatchToLuaVar
                    .Where(kv => kv.Key.Context == SledLuaVarLookUpContextType.WatchCustom)
                    .Select(kv => kv.Key);

                foreach (var customProjectWatch in currentCustomProjectWatches)
                {
                    var localCustomProjectWatch = customProjectWatch;
                    m_dictProjWatchToCustomVar.Add(customProjectWatch, allCustomVariables.First(v => v.Guid == localCustomProjectWatch.Guid));
                }
            }

            var lstLookUps = new List<SledWatchNodeLookUpInfo>();

            foreach (var kv in m_dictProjWatchToLuaVar)
            {
                try
                {
                    var watchNodeLookUpInfo = new SledWatchNodeLookUpInfo(kv.Key.LookUp, kv.Key);
                    if (watchNodeLookUpInfo.LookUp == null)
                        continue;

                    lstLookUps.Add(watchNodeLookUpInfo);

                    if (kv.Value.LuaVar == null)
                        continue;

                    if (kv.Value.LuaVar.What == SledLuaVarBaseType.InvalidVarWhat)
                        continue;

                    var localSavedLuaVar = kv.Value.LuaVar;
                    var nodeState = m_watchCollection.ExpandedStates.FirstOrDefault(node => node.Variable == localSavedLuaVar);
                    if (nodeState != null)
                    {
                        lstLookUps.AddRange(GetExpandedNodesFromState(nodeState));
                    }
                }
                finally
                {
                    // reset so a fake item will get added if the real stuff doesn't come in from the runtime
                    kv.Value.LuaVar = null;
                }
            }

            // No watch lookups to do so skip the rest
            if (lstLookUps.Count <= 0)
                return;

            // Arrange things [correctly] in the order they're going to be sent
            foreach (var scopeType in s_luaVarScopeTypes)
            {
                var localScopeType = scopeType;
                foreach (var watchLookUp in lstLookUps.Where(l => l.LookUp.Scope == localScopeType))
                {
                    if (watchLookUp.Watch != null)
                        m_lstProjectWatches.Add(watchLookUp.Watch);
                }
            }

            // Loop through sending all of one-type-of-variable lookups then repeat. This
            // ordering is important for the runtime, so don't change it unless also
            // adjusting the runtime code:
            // - send globals on 1st pass, locals on 2nd, upvalues on 3rd, env vars on 4th
            foreach (var scopeType in s_luaVarScopeTypes)
            {
                // Send message saying what variable type is coming over
                m_debugService.SendScmp(new Scmp.LuaWatchLookupBegin(m_luaLanguagePlugin.LanguageId, scopeType));

                var localScopeType = scopeType;
                foreach (var watchLookUp in lstLookUps.Where(l => l.LookUp.Scope == localScopeType))
                {
                    var scmpLookUp = new Scmp.LuaVarLookUp(m_luaLanguagePlugin.LanguageId, watchLookUp.LookUp, watchLookUp.Root);
                    m_debugService.SendScmp(scmpLookUp);
                }

                // Send message saying what var type is done coming over
                m_debugService.SendScmp(new Scmp.LuaWatchLookupEnd(m_luaLanguagePlugin.LanguageId, scopeType));
            }
        }

        private SledLuaWatchedCustomVariable GetCustomVariableForLuaVar(ISledLuaVarBaseType luaVar)
        {
            if (luaVar == null)
                return null;

            var rootLevelParent = GetRootLevelParent(luaVar);
            if (rootLevelParent == null)
                return null;

            foreach (var kv in m_dictProjWatchToLuaVar)
            {
                if (kv.Value.LuaVar == null)
                    continue;

                if (!m_comparer.Equals(kv.Value.LuaVar, rootLevelParent))
                    continue;

                SledLuaWatchedCustomVariable customVar;
                m_dictProjWatchToCustomVar.TryGetValue(kv.Key, out customVar);
                return customVar;
            }

            return null;
        }

        private static IEnumerable<SledWatchNodeLookUpInfo> GetExpandedNodesFromState(SledLuaVarBaseListType<ISledLuaVarBaseType>.ExpandedState state)
        {
            return state.GetFlattenedHierarchy().Select(node => new SledWatchNodeLookUpInfo(node.LookUp, null));
        }

        private static ISledLuaVarBaseType CreateFakeLuaVar(SledLuaProjectFilesWatchType watch)
        {
            ISledLuaVarBaseType luaVar = null;
            {
                switch (watch.Scope)
                {
                    case SledLuaVarScopeType.Global:
                        luaVar = new DomNode(SledLuaSchema.SledLuaVarGlobalType.Type).As<SledLuaVarGlobalType>();
                        break;

                    case SledLuaVarScopeType.Local:
                        luaVar = new DomNode(SledLuaSchema.SledLuaVarLocalType.Type).As<SledLuaVarLocalType>();
                        break;

                    case SledLuaVarScopeType.Upvalue:
                        luaVar = new DomNode(SledLuaSchema.SledLuaVarUpvalueType.Type).As<SledLuaVarUpvalueType>();
                        break;

                    case SledLuaVarScopeType.Environment:
                        luaVar = new DomNode(SledLuaSchema.SledLuaVarEnvType.Type).As<SledLuaVarEnvType>();
                        break;
                }
            }

            if (luaVar == null)
                throw new NullReferenceException("luaVar is null");

            luaVar.DisplayName = watch.LookUp.NamesAndTypes[watch.LookUp.NamesAndTypes.Count - 1].Name;
            for (var i = 0; i < (watch.LookUp.NamesAndTypes.Count - 1); ++i)
            {
                // create new since it's a DomNode and simply adding it
                // to a new list will reparent it which breaks things!
                luaVar.TargetHierarchy.Add((SledLuaVarNameTypePairType)watch.LookUp.NamesAndTypes[i].Clone());
            }

            luaVar.Name = SledLuaVarBaseType.CreateFlattenedHierarchyName(luaVar.DisplayName, luaVar.TargetHierarchy, SledLuaVarScmpService.HierarchySeparatorString);
            luaVar.KeyType = watch.LookUp.NamesAndTypes[watch.LookUp.NamesAndTypes.Count - 1].NameType;
            luaVar.What = SledLuaVarBaseType.InvalidVarWhat;
            luaVar.Value = SledLuaVarBaseType.InvalidVarWhat;
            luaVar.Visible = true;

            return luaVar;
        }

        private static IList<SledLuaVarNameTypePairType> CreateNamesAndTypesFrom(IList<KeyValuePair<string, int>> namesAndTypes)
        {
            var list = new List<SledLuaVarNameTypePairType>();

            foreach (var kv in namesAndTypes)
            {
                list.Add(SledLuaVarNameTypePairType.Create(kv.Key, kv.Value));
            }

            return list;
        }

        private static ISledLuaVarBaseType GetRootLevelParent(ISledLuaVarBaseType luaVar)
        {
            var listNode = luaVar.DomNode.GetRoot();

            if (ReferenceEquals(luaVar.DomNode.Parent, listNode))
                return luaVar;

            // skip the list node
            var ancestry = luaVar.DomNode.Ancestry.TakeWhile(d => !ReferenceEquals(d, listNode));
            return ancestry.Last().As<ISledLuaVarBaseType>();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void DoDebugMessage(SledMessageType messageType, string format, params object[] args)
        {
            SledOutDevice.OutLine(messageType, format, args);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckLuaVarAndProjectWatchScope(ISledLuaVarBaseType luaVar, SledLuaProjectFilesWatchType watch)
        {
            if (luaVar.Scope != watch.Scope)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Warning,
                        "[Lua Watched Variable Service] Associating {0} Lua variable ({1}) with {2} watch ({3})!",
                        luaVar.Scope, luaVar.DisplayName, watch.Scope, watch.Name);
            }
        }

        #region Watch Private Classes

        private class SledWatchNodeLookUpInfo
        {
            public SledWatchNodeLookUpInfo(SledLuaVarLookUpType lookUp, SledLuaProjectFilesWatchType watch)
            {
                LookUp = lookUp;
                Watch = watch;

                Root = watch != null;
            }

            public SledLuaVarLookUpType LookUp { get; private set; }

            public SledLuaProjectFilesWatchType Watch { get; private set; }

            public bool Root { get; private set; }
        }

        private class LuaVarHandle
        {
            public ISledLuaVarBaseType LuaVar { get; set; }
        }

        private class ILuaVarBaseTypeEqualityComparer : IEqualityComparer<ISledLuaVarBaseType>
        {
            #region Implementation of IEqualityComparer<in SledLuaProjectFilesWatchType>

            public bool Equals(ISledLuaVarBaseType x, ISledLuaVarBaseType y)
            {
                if ((x == null) || (y == null))
                    return false;

                if (ReferenceEquals(x, y))
                    return true;

                if (x.Scope != y.Scope)
                    return false;

                if (x.KeyType != y.KeyType)
                    return false;

                if (string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal) != 0)
                    return false;

                if (x.TargetHierarchy.Count != y.TargetHierarchy.Count)
                    return false;

                for (var i = 0; i < x.TargetHierarchy.Count; ++i)
                {
                    if (x.TargetHierarchy[i].NameType != y.TargetHierarchy[i].NameType)
                        return false;

                    if (string.Compare(x.TargetHierarchy[i].Name, y.TargetHierarchy[i].Name, StringComparison.Ordinal) != 0)
                        return false;
                }

                return true;
            }

            public int GetHashCode(ISledLuaVarBaseType obj)
            {
                if (obj == null)
                    throw new NullReferenceException("obj");

                var hash = obj.Scope.GetHashCode();

                hash += (obj.KeyType.GetHashCode() + obj.DisplayName.GetHashCode());

                foreach (var kv in obj.TargetHierarchy)
                    hash += (kv.Name.GetHashCode() + kv.NameType.GetHashCode());

                return hash;
            }

            #endregion
        }

        [Export(typeof(SledLuaWatchesEditor))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaWatchesEditor : SledLuaTreeListViewEditor
        {
            [ImportingConstructor]
            public SledLuaWatchesEditor()
                : base(
                    Localization.SledLuaWatchListTitle,
                    null,
                    SledLuaVarBaseType.WatchColumnNames,
                    StandardControlGroup.Right)
            {
                TreeListView.Renderer = new WatchRenderer(TreeListView);
            }

            #region SledLuaTreeListViewEditor Overrides

            protected override string GetCopyText()
            {
                if ((TreeListViewAdapter == null) ||
                    (!TreeListViewAdapter.Selection.Any()))
                    return string.Empty;

                const string tab = "\t";

                var sb = new StringBuilder();
                var luaVars = TreeListViewAdapter.Selection.AsIEnumerable<ISledLuaVarBaseType>();

                foreach (var luaVar in luaVars)
                {
                    sb.Append(luaVar.Name);
                    sb.Append(tab);
                    sb.Append(SledLuaUtil.LuaTypeToString(luaVar.LuaType));
                    sb.Append(tab);
                    sb.Append(luaVar.Value);
                    sb.Append(tab);
                    sb.Append(SledLuaVarScopeTypeString.ToString(luaVar.Scope));
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion

            #region Private Classes

            private class WatchRenderer : TreeListView.NodeRenderer
            {
                public WatchRenderer(TreeListView owner)
                    : base(owner)
                {
                }

                public override void DrawBackground(TreeListView.Node node, Graphics gfx, Rectangle bounds)
                {
                    if (!ShouldCustomDraw(DrawPart.Background, node, gfx, bounds, -1))
                        base.DrawBackground(node, gfx, bounds);
                }

                public override void DrawLabel(TreeListView.Node node, Graphics gfx, Rectangle bounds, int column)
                {
                    if (ShouldCustomDraw(DrawPart.Label, node, gfx, bounds, column))
                        return;

                    var text =
                        column == 0
                            ? node.Label
                            : ((node.Properties != null) &&
                               (node.Properties.Length >= column))
                                ? GetObjectString(node.Properties[column - 1])
                                : null;

                    if (string.IsNullOrEmpty(text))
                        text = string.Empty;

                    var editable = false;
                    if (column == 2)
                    {
                        var luaVar = node.Tag.As<ISledLuaVarBaseType>();
                        if (luaVar != null)
                            editable = SledLuaUtil.IsEditableLuaType(luaVar);
                    }

                    var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

                    // Add ellipsis if needed
                    {
                        var textSize = TextRenderer.MeasureText(gfx, text, Owner.Control.Font);

                        if (textSize.Width > bounds.Width)
                            flags |= TextFormatFlags.EndEllipsis;
                    }

                    if (node.Selected && Owner.Control.Enabled)
                    {
                        using (var b = new SolidBrush(Owner.HighlightBackColor))
                            gfx.FillRectangle(b, bounds);
                    }

                    var textColor =
                        node.Selected
                            ? Owner.HighlightTextColor
                            : Owner.TextColor;

                    if (editable)
                        textColor = node.Selected
                            ? Owner.ModifiableHighlightTextColor
                            : Owner.ModifiableTextColor;

                    if (!Owner.Control.Enabled)
                        textColor = Owner.DisabledTextColor;

                    TextRenderer.DrawText(gfx, text, Owner.Control.Font, bounds, textColor, flags);
                }

                public override void DrawImage(TreeListView.Node node, Graphics gfx, Rectangle bounds)
                {
                    if (!ShouldCustomDraw(DrawPart.Image, node, gfx, bounds, -1))
                        base.DrawImage(node, gfx, bounds);
                }

                public override void DrawStateImage(TreeListView.Node node, Graphics gfx, Rectangle bounds)
                {
                    if (!ShouldCustomDraw(DrawPart.StateImage, node, gfx, bounds, -1))
                        base.DrawStateImage(node, gfx, bounds);
                }

                private bool ShouldCustomDraw(DrawPart part, TreeListView.Node node, Graphics gfx, Rectangle bounds, int column)
                {
                    var luaVar = node.Tag.As<ISledLuaVarBaseType>();
                    if (luaVar == null)
                        return false;

                    if (!WatchedVariableService.IsCustomWatchedVariable(luaVar))
                        return false;

                    var customVariable = WatchedVariableService.GetCustomVariableForLuaVar(luaVar);
                    if (customVariable == null)
                        return false;

                    var renderer = customVariable.Renderer;
                    if (renderer == null)
                        return false;

                    var args = new SledLuaWatchedCustomVariableRendererArgs(Owner, node, gfx, bounds, column, luaVar, customVariable);

                    switch (part)
                    {
                        case DrawPart.Background: renderer.DrawBackground(args); break;
                        case DrawPart.Label: renderer.DrawLabel(args); break;
                        case DrawPart.Image: renderer.DrawImage(args); break;
                        case DrawPart.StateImage: renderer.DrawStateImage(args); break;
                    }

                    return !args.DrawDefault;
                }

                private static string GetObjectString(object value)
                {
                    var formattable = value as IFormattable;

                    return
                        formattable != null
                            ? formattable.ToString(null, null)
                            : value.ToString();
                }

                private SledLuaWatchedVariableService WatchedVariableService
                {
                    get { return m_watchedVariableService ?? (m_watchedVariableService = SledServiceInstance.Get<SledLuaWatchedVariableService>()); }
                }

                private enum DrawPart
                {
                    Background,
                    Label,
                    Image,
                    StateImage,
                }

                private SledLuaWatchedVariableService m_watchedVariableService;
            }

            #endregion
        }

        #endregion

        private bool m_bUndoingAttribute;
        private bool m_recvProjWatchLuaVar;
        private SledProjectFilesType m_project;
        private ISledLuaVarBaseType m_lastWatchRootNode;
        private SledLuaVarWatchListType m_watchCollection;

        private ISledDebugService m_debugService;
        private ISledProjectService m_projectService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;
        private ISledLuaVarScmpService m_luaVarScmpService;
        private ISledLanguagePluginService m_languagePluginService;
        private ISledLuaVariableLocationService m_luaVarLocationService;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaWatchesEditor m_editor;

#pragma warning restore 649

        private readonly ILuaVarBaseTypeEqualityComparer m_comparer =
            new ILuaVarBaseTypeEqualityComparer();

        private readonly List<ISledLuaVarBaseType> m_lstQueuedLookUps =
            new List<ISledLuaVarBaseType>();

        private readonly List<SledLuaProjectFilesWatchType> m_lstProjectWatches =
            new List<SledLuaProjectFilesWatchType>();

        private readonly Dictionary<SledLuaProjectFilesWatchType, LuaVarHandle> m_dictProjWatchToLuaVar =
            new Dictionary<SledLuaProjectFilesWatchType, LuaVarHandle>();

        private readonly Dictionary<SledLuaProjectFilesWatchType, SledLuaWatchedCustomVariable> m_dictProjWatchToCustomVar =
            new Dictionary<SledLuaProjectFilesWatchType, SledLuaWatchedCustomVariable>();

        private static readonly IEnumerable<SledLuaVarScopeType> s_luaVarScopeTypes;
    }
}