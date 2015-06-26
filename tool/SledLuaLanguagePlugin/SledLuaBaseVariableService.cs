/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(ISledDocumentPlugin))]
    [InheritedExport(typeof(ISledLuaVariableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    abstract class SledLuaBaseVariableService<TList, TType> : IInitializable, ISledDocumentPlugin, ISledLuaVariableService
        where TList : SledLuaVarBaseListType<TType>
        where TType : class, ISledLuaVarBaseType
    {
        #region IInitializable Interface
        
        void IInitializable.Initialize()
        {
            ProjectService.Closing += ProjectServiceClosing;

            DebugService.Connected += DebugServiceConnected;
            DebugService.DataReady += DebugServiceDataReady;
            DebugService.UpdateBegin += DebugServiceUpdateBegin;
            DebugService.UpdateSync += DebugServiceUpdateSync;
            DebugService.UpdateEnd += DebugServiceUpdateEnd;
            DebugService.Disconnected += DebugServiceDisconnected;

            LuaVariableFilterService.Filtering += LuaVariableFilterServiceFiltering;
            LuaVariableFilterService.Filtered += LuaVariableFilterServiceFiltered;

            Initialize();

            Editor.TreeListViewAdapter.ItemLazyLoad += TreeListViewAdapterItemLazyLoad;

            // Adjust to shortened name if this language plugin is the only one loaded
            if (LanguagePluginService.Count == 1)
                Editor.Name = ShortName;
        }

        #endregion

        #region ISledDocumentPlugin Interface

        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            return OnGetPopupCommandTags(args);
        }

        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return OnGetMouseHoverOverTokenValues(args);
        }

        #endregion

        #region ISledLuaVariableService Interface

        public bool TryGetVariable(string name, out ISledLuaVarBaseType luaVar)
        {
            return OnTryGetVariable(name, out luaVar);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            OnProjectServiceClosing(sender, e);
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            OnDebugServiceConnected(sender, e);
        }

        private void DebugServiceUpdateBegin(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            OnDebugServiceUpdateBegin(sender, e);
        }

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            OnDebugServiceDataReady(sender, e);
        }

        private void DebugServiceUpdateSync(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            OnDebugServiceUpdateSync(sender, e);
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            OnDebugServiceUpdateEnd(sender, e);
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            OnDebugServiceDisconnected(sender, e);
        }

        #endregion

        #region ISledLuaVariableFilterService Events

        private void LuaVariableFilterServiceFiltering(object sender, SledLuaVariableFilterService.FilteringEventArgs e)
        {
            OnLuaVariableFilterServiceFiltering(sender, e);
        }

        private void LuaVariableFilterServiceFiltered(object sender, SledLuaVariableFilterService.FilteredEventArgs e)
        {
            OnLuaVariableFilterServiceFiltered(sender, e);
        }

        #endregion

        #region Member Methods

        protected abstract void Initialize();

        protected abstract string ShortName { get; }

        protected abstract string PopupPrefix { get; }

        protected abstract SledLuaTreeListViewEditor Editor { get; }

        protected abstract DomNodeType NodeType { get; }

        protected virtual IList<object> OnGetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            return null;
        }

        protected virtual IList<string> OnGetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            if (!DebugService.IsConnected)
                return null;

            var szFullToken = SledLuaUtil.GetFullHoverOvenToken(args);
            if (string.IsNullOrEmpty(szFullToken))
                return null;

            var variables =
                (from list in Collection
                let luaVar = SledDomUtil.FindFirstInWhere(list.DomNode, (TType variable) => string.Compare(variable.Name, szFullToken, StringComparison.Ordinal) == 0)
                where luaVar != null
                select luaVar).ToList();

            return
                !variables.Any()
                    ? null
                    : variables.Select(variable => string.Format("{0}: {1}", PopupPrefix, SledUtil.MakeXmlSafe(variable.Value))).ToList();
        }

        protected virtual bool OnTryGetVariable(string name, out ISledLuaVarBaseType luaVar)
        {
            luaVar = null;

            return Collection.Count > 0 && Collection[0].TryGetVariable(name, out luaVar);
        }

        protected virtual void OnProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            DestroyCollection();
        }

        protected virtual void OnDebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
        }

        protected virtual void OnDebugServiceUpdateBegin(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            if (Collection.Count > 0)
                Collection[0].ValidationBeginning();

            // Save while items still in collection & on GUI
            if (Collection.Count > 0)
                Collection[0].SaveExpandedStates();

            Editor.SaveState();

            // Clear GUI
            Editor.View = null;

            // Clear out all items
            for (var i = 0; i < Collection.Count; i++)
            {
                Collection[i].Variables.Clear();

                if (i > 0)
                    Collection[i].ResetExpandedStates();
            }

            if (Collection.Count > 0)
                Editor.View = Collection[0];

            // Reset
            ListInsert.Clear();
            ListNameInsert.Clear();
            ListNameInsertDict.Clear();
        }

        protected virtual void OnDebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
        }

        protected virtual void OnDebugServiceUpdateSync(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            if (Collection.Count <= 0)
                return;

            // See if any variables need to be looked up
            foreach (var state in Collection[0].ExpandedStates)
            {
                foreach (var node in state.GetFlattenedHierarchy())
                {
                    if (node.LookUp == null)
                        continue;

                    // Already looking up this item
                    if (ListNameInsert.Contains(node.Variable.Name))
                        continue;

                    // Keep track of items getting looked up
                    ListNameInsert.Add(node.Variable.Name);
                    ListNameInsertDict.Add(node.Variable.Name, new Pair<string, IList<SledLuaVarNameTypePairType>>(node.Variable.DisplayName, node.Variable.TargetHierarchy));

                    //SledOutDevice.OutLine(SledMessageType.Error, "[Variable Lookup] {0}", node.LookUp);

                    DebugService.SendScmp(new Scmp.LuaVarLookUp(LuaLanguagePlugin.LanguageId, node.LookUp));
                }
            }
        }

        protected virtual void OnDebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            foreach (var collection in Collection)
                collection.ValidationEnded();

            Editor.RestoreState();
        }

        protected virtual void OnDebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            // Clear GUI
            Editor.View = null;

            CleanupCollection();

            LookingUp = false;
            ListInsert.Clear();
            ListNameInsert.Clear();
            ListNameInsertDict.Clear();
        }

        protected virtual void OnDebugServiceLookupBegin()
        {
            LookingUp = true;

            // If not during an update it's a
            // manual lookup and we are good to go
            if (!DebugService.IsUpdateInProgress)
                return;

            // Uh-oh
            if (ListNameInsert.Count <= 0)
                return;

            // Find where to place the incoming variable
            try
            {
                var pieces = new List<string>();
                {
                    Pair<string, IList<SledLuaVarNameTypePairType>> temp;
                    if (!ListNameInsertDict.TryGetValue(ListNameInsert[0], out temp))
                        throw new InvalidOperationException("Unknown variable");

                    pieces.AddRange(temp.Second.Select(kv => kv.Name)); // to navigate to the right table
                    pieces.Add(temp.First); // actual variable name
                }

                TType insert = null;

                // Try and find where the variable should be inserted
                for (var i = 0; i < pieces.Count; i++)
                {
                    IList<TType> lstVariables;

                    if (i == 0)
                        lstVariables = Collection[0].Variables;
                    else
                    {
                        if (insert == null)
                            throw new InvalidOperationException("Variable not found");

                        lstVariables =
                            new List<TType>(
                                insert.Variables.Select(v => v.As<TType>()));
                    }

                    var iPos = -1;

                    for (var j = 0; j < lstVariables.Count; j++)
                    {
                        if (string.Compare(lstVariables[j].DisplayName, pieces[i], StringComparison.Ordinal) != 0)
                            continue;

                        iPos = j;
                        break;
                    }

                    if (iPos == -1)
                        throw new InvalidOperationException("Variable not found");

                    insert = lstVariables[iPos];
                }

                if (insert == null)
                    throw new InvalidOperationException("Variable not found");

                // Finally found where to place lookups
                ListInsert.Add(insert);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(SledMessageType.Error, "{0}: Exception in OnDebugServiceLookupBegin: {1}", this, ex.Message);
            }
            finally
            {
                // Remove the item we just used
                if (ListNameInsert.Count > 0)
                {
                    ListNameInsertDict.Remove(ListNameInsert[0]);
                    ListNameInsert.RemoveAt(0);
                }
            }
        }

        protected virtual void OnDebugServiceLookupEnd()
        {
            if (ListInsert.Count > 0)
                ListInsert.RemoveAt(0);

            LookingUp = false;

            foreach (var collection in Collection)
                collection.ValidationEnded();
        }

        protected virtual void OnLuaVariableFilterServiceFiltering(object sender, SledLuaVariableFilterService.FilteringEventArgs e)
        {
            if (e.NodeType != NodeType)
                return;

            if (Collection.Count <= 0)
                return;

            var objects = new List<TType>();
            foreach (var list in Collection)
                SledDomUtil.GatherAllAs(list.DomNode, objects);

            foreach (var luaVar in objects)
                e.LuaVarsToFilter.Add(luaVar);

            Editor.View = null;
        }

        protected virtual void OnLuaVariableFilterServiceFiltered(object sender, SledLuaVariableFilterService.FilteredEventArgs e)
        {
            if (e.NodeType != NodeType)
                return;

            if (Collection.Count <= 0)
                return;

            Editor.View = Collection[0];
        }

        protected virtual void TreeListViewAdapterItemLazyLoad(object sender, TreeListViewAdapter.ItemLazyLoadEventArgs e)
        {
            if (!DebugService.IsConnected || DebugService.IsDebugging)
                return;

            // Get object as a Lua variable
            var luaVar = e.Item.As<TType>();
            if (luaVar == null)
                return;

            // Item already being looked up
            if (ListInsert.Contains(luaVar))
                return;

            if (luaVar.What == SledLuaVarBaseType.InvalidVarWhat)
                return;

            // Get lookup string
            var lookUp = SledLuaVarLookUpType.FromLuaVar(luaVar, SledLuaVarLookUpContextType.Normal);

            // Convert string to SCMP message
            var scmp = new Scmp.LuaVarLookUp(LuaLanguagePlugin.LanguageId, lookUp);

            //SledOutDevice.OutLine(SledMessageType.Error, "[Variable Lookup] {0}", lookUp.ToString());

            luaVar.Expanded = true;

            // Keep track of item so that children can be added properly
            ListInsert.Add(luaVar);

            // Send lookup message
            DebugService.SendScmp(scmp);
        }

        protected virtual void CleanupCollection()
        {
            foreach (var collection in Collection)
            {
                collection.DomNode.AttributeChanged -= DomNodeAttributeChanged;
                collection.Variables.Clear();
                collection.ResetExpandedStates();
            }

            Collection.Clear();
        }

        protected virtual void DestroyCollection()
        {
            Editor.View = null;
            CleanupCollection();
        }

        protected virtual void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (UndoingAttribute)
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
                    DebugService.IsConnected &&
                    !DebugService.IsDebugging;

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

                // Cheat and use this to get all the key value pairs.
                // This takes into account watched variables as well!
                var lookUp = SledLuaVarLookUpType.FromLuaVar(luaVar, SledLuaVarLookUpContextType.Normal);
                if (lookUp == null)
                    return;

                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledLuaModifyVarSent);

                // Send message off
                DebugService.SendScmp(new Scmp.LuaVarUpdate(LuaLanguagePlugin.LanguageId, lookUp, szNewValue, (int)luaVar.LuaType));

                bUndo = false;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(
                    SledMessageType.Error,
                    "{0}: Exception in DomNodeAttributeChanged: {1}",
                    this, ex.Message);

                bUndo = true;
            }
            finally
            {
                if (bUndo)
                    UndoAttributeChange(e);
            }
        }
        
        protected virtual void UndoAttributeChange(AttributeEventArgs e)
        {
            try
            {
                UndoingAttribute = true;
                e.DomNode.SetAttribute(e.AttributeInfo, e.OldValue);
            }
            finally
            {
                UndoingAttribute = false;
            }
        }

        #endregion

#pragma warning disable 649 // Field is never assigned

        [Import]
        protected ISledProjectService ProjectService;

        [Import]
        protected ISledDebugService DebugService;

        [Import]
        protected ISledLanguagePluginService LanguagePluginService;

        [Import]
        protected SledLuaLanguagePlugin LuaLanguagePlugin;

        [Import]
        protected ISledLuaVarScmpService LuaVarScmpService;

        [Import]
        protected ISledLuaVariableLocationService LuaVarLocationService;

        [Import]
        protected ISledLuaWatchedVariableService LuaWatchedVariableService;

        [Import]
        protected ISledLuaVariableFilterService LuaVariableFilterService;

#pragma warning restore 649

        protected bool LookingUp;
        protected bool UndoingAttribute;

        protected readonly List<TList> Collection =
            new List<TList>();

        protected readonly List<string> ListNameInsert =
            new List<string>();

        protected readonly Dictionary<string, Pair<string, IList<SledLuaVarNameTypePairType>>> ListNameInsertDict =
            new Dictionary<string, Pair<string, IList<SledLuaVarNameTypePairType>>>();

        protected readonly List<TType> ListInsert =
            new List<TType>();
    }
}