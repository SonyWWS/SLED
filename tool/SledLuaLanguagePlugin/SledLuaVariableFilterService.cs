/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
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
    [Export(typeof(ISledLuaVariableFilterService))]
    [Export(typeof(SledLuaVariableFilterService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaVariableFilterService : IInitializable, ISledLuaVariableFilterService, ICommandClient
    {
        [ImportingConstructor]
        public SledLuaVariableFilterService(MainForm mainForm, ICommandService commandService)
        {
            m_mainForm = mainForm;

            // Global variable filter command
            commandService.RegisterCommand(
                Command.GlobalVariableFilter,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaFilterGlobalVars,
                Localization.SledLuaFilterGlobalVarsComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Local variable filter command
            commandService.RegisterCommand(
                Command.LocalVariableFilter,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaFilterLocalVars,
                Localization.SledLuaFilterLocalVarsComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Upvalue variable filter command
            commandService.RegisterCommand(
                Command.UpvalueVariableFilter,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaFilterUpvalueVars,
                Localization.SledLuaFilterUpvalueVarsComments,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Environment table variable filter command
            commandService.RegisterCommand(
                Command.EnvironmentVariableFilter,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaFilterEnvVarVars,
                Localization.SledLuaFilterEnvVarVarsComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_luaLanguagePlugin =
                SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.Connected += DebugServiceConnected;
            m_debugService.DataReady += DebugServiceDataReady;

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closing += ProjectServiceClosing;
        }

        #endregion

        #region Commands, Menu, CommandGroup

        enum Command
        {
            GlobalVariableFilter,
            LocalVariableFilter,
            UpvalueVariableFilter,
            EnvironmentVariableFilter,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command) commandTag)
                {
                    case Command.GlobalVariableFilter:
                    case Command.LocalVariableFilter:
                    case Command.UpvalueVariableFilter:
                    case Command.EnvironmentVariableFilter:
                        bEnabled = true;
                        break;
                }
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.GlobalVariableFilter:
                {
                    var form =
                        new SledLuaVariableFilterForm
                            {
                                LocalFilterTypes = m_globalVariableFilterState.LocalFilterTypes,
                                LocalFilterNames = m_globalVariableFilterState.LocalFilterNames,
                                TargetFilterTypes = m_globalVariableFilterState.NetFilterTypes,
                                TargetFilterNames = m_globalVariableFilterState.NetFilterNames,
                                Text = Localization.SledLuaVariableFilterGlobalTitle
                            };
                    form.ShowDialog(m_mainForm);
                    if (form.LocalFilterStateChanged)
                        RefilterVars(SledLuaSchema.SledLuaVarGlobalType.Type);
                    if (form.TargetFilterStateChanged)
                        SendNetVarFilterState(true);
                    if (form.LocalFilterStateChanged || form.TargetFilterStateChanged)
                        SaveVarFilters(null);
                }
                break;

                case Command.LocalVariableFilter:
                {
                    var form =
                        new SledLuaVariableFilterForm
                            {
                                LocalFilterTypes = m_localVariableFilterState.LocalFilterTypes,
                                LocalFilterNames = m_localVariableFilterState.LocalFilterNames,
                                TargetFilterTypes = m_localVariableFilterState.NetFilterTypes,
                                TargetFilterNames = m_localVariableFilterState.NetFilterNames,
                                Text = Localization.SledLuaVariableFilterLocalTitle
                            };
                    form.ShowDialog(m_mainForm);
                    if (form.LocalFilterStateChanged)
                        RefilterVars(SledLuaSchema.SledLuaVarLocalType.Type);
                    if (form.TargetFilterStateChanged)
                        SendNetVarFilterState(true);
                    if (form.LocalFilterStateChanged || form.TargetFilterStateChanged)
                        SaveVarFilters(null);
                }
                break;

                case Command.UpvalueVariableFilter:
                {
                    var form =
                        new SledLuaVariableFilterForm
                            {
                                LocalFilterTypes = m_upvalueVariableFilterState.LocalFilterTypes,
                                LocalFilterNames = m_upvalueVariableFilterState.LocalFilterNames,
                                TargetFilterTypes = m_upvalueVariableFilterState.NetFilterTypes,
                                TargetFilterNames = m_upvalueVariableFilterState.NetFilterNames,
                                Text = Localization.SledLuaVariableFilterUpvalueTitle
                            };
                    form.ShowDialog(m_mainForm);
                    if (form.LocalFilterStateChanged)
                        RefilterVars(SledLuaSchema.SledLuaVarUpvalueType.Type);
                    if (form.TargetFilterStateChanged)
                        SendNetVarFilterState(true);
                    if (form.LocalFilterStateChanged || form.TargetFilterStateChanged)
                        SaveVarFilters(null);
                }
                break;

                case Command.EnvironmentVariableFilter:
                {
                    var form =
                        new SledLuaVariableFilterForm
                            {
                                LocalFilterTypes = m_envVarVariableFilterState.LocalFilterTypes,
                                LocalFilterNames = m_envVarVariableFilterState.LocalFilterNames,
                                TargetFilterTypes = m_envVarVariableFilterState.NetFilterTypes,
                                TargetFilterNames = m_envVarVariableFilterState.NetFilterNames,
                                Text = Localization.SledLuaVariableFilterEnvVarTitle
                            };
                    form.ShowDialog(m_mainForm);
                    if (form.LocalFilterStateChanged)
                        RefilterVars(SledLuaSchema.SledLuaVarEnvType.Type);
                    if (form.TargetFilterStateChanged)
                        SendNetVarFilterState(true);
                    if (form.LocalFilterStateChanged || form.TargetFilterStateChanged)
                        SaveVarFilters(null);
                }
                break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledLuaVariableFilterService Interface

        /// <summary>
        /// Returns true if a variable is [locally] filtered by name or by LUA_T&lt;type&gt;
        /// </summary>
        /// <param name="luaVar">Variable to check</param>
        /// <returns>True if variable is [locally] filtered by name or by LUA_T&lt;type&gt; and false if not</returns>
        public bool IsVariableFiltered(ISledLuaVarBaseType luaVar)
        {
            if (luaVar == null)
                return false;

            SledLuaVariableFilterState filterState = null;
            var nodeType = luaVar.DomNode.Type;

            if (nodeType == SledLuaSchema.SledLuaVarGlobalType.Type)
                filterState = m_globalVariableFilterState;
            else if (nodeType == SledLuaSchema.SledLuaVarLocalType.Type)
                filterState = m_localVariableFilterState;
            else if (nodeType == SledLuaSchema.SledLuaVarUpvalueType.Type)
                filterState = m_upvalueVariableFilterState;
            else if (nodeType == SledLuaSchema.SledLuaVarEnvType.Type)
                filterState = m_envVarVariableFilterState;

            if (filterState == null)
                return false;

            // Check if type is filtered
            if (filterState.LocalFilterTypes[SledLuaUtil.LuaTypeStringToInt(luaVar.What)])
                return true;

            // Check if name is filtered
            if (VarNameFiltered(luaVar.Name, filterState.LocalFilterNames))
                return true;

            return false;
        }

        /// <summary>
        /// Event fired when variable filtering needs to be done for a specific set of variables (globals, locals, etc.)
        /// </summary>
        public event EventHandler<FilteringEventArgs> Filtering;

        /// <summary>
        /// Event fired when variable filtering has finished for a specific set of variables (globals, locals, etc.)
        /// </summary>
        public event EventHandler<FilteredEventArgs> Filtered;

        #endregion

        #region ISledDebugService Events

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            SendNetVarFilterState();
        }

        void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaLimits:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaLimits>();

                    m_iMaxVarFilters = scmp.MaxVarFilters;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(
                            "[%s0] Maximum variable filters " + 
                            "set to %s1 on the target.",
                            m_luaLanguagePlugin.LanguageName, m_iMaxVarFilters));
                }
                break;
            }
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_project = e.Project;
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_project = e.Project;

            LoadSavedVarFilters();
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_project = null;
            m_iMaxVarFilters = -1;
        }

        #endregion

        #region Member Methods

        private void RefilterVars(DomNodeType nodeType)
        {
            var fea = new FilteringEventArgs(nodeType);

            // Fire filtering event
            Filtering.Raise(this, fea);

            // Do any filtering
            foreach (var luaVar in fea.LuaVarsToFilter)
                luaVar.Visible = !IsVariableFiltered(luaVar);

            // Fire filtered event
            Filtered.Raise(this, new FilteredEventArgs(nodeType));
        }

        private void SendNetVarFilterState()
        {
            SendNetVarFilterState(false);
        }

        private void SendNetVarFilterState(bool bForce)
        {
            if (!m_debugService.IsConnected)
                return;

            // If no variable filters are active and !bForce then return
            if (!m_globalVariableFilterState.FiltersActive &&
                !m_localVariableFilterState.FiltersActive &&
                !m_upvalueVariableFilterState.FiltersActive &&
                !m_envVarVariableFilterState.FiltersActive &&
                !bForce)
                return;

            SledOutDevice.OutLine(SledMessageType.Info, Localization.SledLuaSendNetVarFilterState);

            //// Send message to clear filter types/name list
            //SledTargetConnection.SendString("{filterc:c}");

            // Send all lua type filters
            SendNetVarFilterTypesInternal(m_globalVariableFilterState.NetFilterTypes, 'g');
            SendNetVarFilterTypesInternal(m_localVariableFilterState.NetFilterTypes, 'l');
            SendNetVarFilterTypesInternal(m_upvalueVariableFilterState.NetFilterTypes, 'u');
            SendNetVarFilterTypesInternal(m_envVarVariableFilterState.NetFilterTypes, 'e');

            // Send all name filters
            SendNetVarFilterNamesInternal(m_globalVariableFilterState.NetFilterNames, 'g');
            SendNetVarFilterNamesInternal(m_localVariableFilterState.NetFilterNames, 'l');
            SendNetVarFilterNamesInternal(m_upvalueVariableFilterState.NetFilterNames, 'u');
            SendNetVarFilterNamesInternal(m_envVarVariableFilterState.NetFilterNames, 'e');
        }

        private void SendNetVarFilterTypesInternal(bool[] bFilterTypes, char chWhat)
        {
            m_debugService.SendScmp(new Scmp.LuaVarFilterStateTypeBegin(m_luaLanguagePlugin.LanguageId, chWhat));
            m_debugService.SendScmp(new Scmp.LuaVarFilterStateType(m_luaLanguagePlugin.LanguageId, chWhat, bFilterTypes));
            m_debugService.SendScmp(new Scmp.LuaVarFilterStateTypeEnd(m_luaLanguagePlugin.LanguageId, chWhat));
        }

        private void SendNetVarFilterNamesInternal(IEnumerable<string> lstNames, char chWhat)
        {
            m_debugService.SendScmp(new Scmp.LuaVarFilterStateNameBegin(m_luaLanguagePlugin.LanguageId, chWhat));

            foreach (var filter in lstNames)
                m_debugService.SendScmp(new Scmp.LuaVarFilterStateName(m_luaLanguagePlugin.LanguageId, chWhat, filter));

            m_debugService.SendScmp(new Scmp.LuaVarFilterStateNameEnd(m_luaLanguagePlugin.LanguageId, chWhat));
        }

        private bool VarNameFiltered(string name, ICollection<string> lstFilteredNames)
        {
            if (lstFilteredNames.Count <= 0)
                return false;

            foreach (var filterName in lstFilteredNames)
            {
                // All occurrences of asterisks
                var asterisks = SledUtil.IndicesOf(filterName, '*');

                if (asterisks.Length > 0)
                {
                    // 1 or more asterisks so pattern check

                    // Is first character of filter pattern an asterisk?
                    var bFirst = (asterisks[0] == 0);

                    // Is last character of filter pattern an asterisk?
                    var bLast = (asterisks[asterisks.Length - 1] == (filterName.Length - 1));

                    // Patterns combined are longer than name so name can't contain patterns
                    if ((filterName.Length - asterisks.Length) > name.Length)
                        continue;

                    // Check for just asterisk
                    if (bFirst && bLast && (filterName.Length == 1))
                        return true;

                    // Array of patterns
                    var patterns = filterName.Split(m_chPatternSep, StringSplitOptions.RemoveEmptyEntries);
                    if (patterns.Length <= 0)
                        continue;

                    var iPos = -1;
                    var bFailed = false;

                    // Go through checking if all patterns exist in name
                    for (var i = 0; (i < patterns.Length) && !bFailed; i++)
                    {
                        iPos = name.IndexOf(patterns[i], iPos + 1, StringComparison.Ordinal);

                        // Pattern was not found
                        if (iPos == -1)
                        {
                            bFailed = true;
                            continue;
                        }

                        // On first iteration check bFirst condition
                        if ((i == 0) && !bFirst && (iPos != 0))
                        {
                            bFailed = true;
                            continue;
                        }

                        // On last iteration check bLast condition
                        if ((i == (patterns.Length - 1)) && !bLast && (iPos != (name.Length - patterns[patterns.Length - 1].Length)))
                        {
                            bFailed = true;
                        }
                    }

                    if (bFailed)
                        continue;
                    
                    return true;
                }
                
                // No asterisks so simple name check
                if (name == filterName)
                    return true;
            }

            return false;
        }

        private static SledLuaVarFiltersType FindLuaVarFiltersElement(SledProjectFilesType project)
        {
            var filtersElement =
                project.UserSettings.FirstOrDefault(
                    setting => setting.Is<SledLuaVarFiltersType>());

            return
                filtersElement == null
                    ? null
                    : filtersElement.As<SledLuaVarFiltersType>();
        }

        private void LoadSavedVarFilters()
        {
            var varFilters =
                FindLuaVarFiltersElement(m_project);

            // Create variable filters
            if (varFilters == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFiltersType.Type);

                varFilters = domNode.As<SledLuaVarFiltersType>();
                m_project.UserSettings.Add(varFilters);
                SaveVarFilters(varFilters);
            }
            else
            {
                varFilters.Load(
                    m_globalVariableFilterState,
                    m_localVariableFilterState,
                    m_upvalueVariableFilterState,
                    m_envVarVariableFilterState);
            }
        }

        private void SaveVarFilters(SledLuaVarFiltersType varFilters)
        {
            if (varFilters == null)
            {
                varFilters = FindLuaVarFiltersElement(m_project);
                if (varFilters == null)
                    return;
            }

            varFilters.Setup(
                m_globalVariableFilterState,
                m_localVariableFilterState,
                m_upvalueVariableFilterState,
                m_envVarVariableFilterState);

            m_projectService.SaveSettings();
        }

        #endregion

        private SledProjectFilesType m_project;
        private ISledDebugService m_debugService;
        private ISledProjectService m_projectService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

        private int m_iMaxVarFilters = -1;

        private readonly MainForm m_mainForm;

        private readonly char[] m_chPatternSep = { '*' };

        private readonly SledLuaVariableFilterState m_globalVariableFilterState = 
            new SledLuaVariableFilterState();
        private readonly SledLuaVariableFilterState m_localVariableFilterState = 
            new SledLuaVariableFilterState();
        private readonly SledLuaVariableFilterState m_upvalueVariableFilterState = 
            new SledLuaVariableFilterState();
        private readonly SledLuaVariableFilterState m_envVarVariableFilterState = 
            new SledLuaVariableFilterState();

        #region Variable Filtering EventArgs

        public class FilteringEventArgs : EventArgs
        {
            public FilteringEventArgs(DomNodeType nodeType)
            {
                m_nodeType = nodeType;
                m_luaVarsToFilter = new List<ISledLuaVarBaseType>();
            }

            /// <summary>
            /// DomNodeType of variables that are getting filtered
            /// </summary>
            public DomNodeType NodeType
            {
                get { return m_nodeType; }
            }

            /// <summary>
            /// List of variables that need to be filtered.
            /// Clients should fill this list.
            /// </summary>
            public List<ISledLuaVarBaseType> LuaVarsToFilter
            {
                get { return m_luaVarsToFilter; }
            }

            private readonly DomNodeType m_nodeType;
            private readonly List<ISledLuaVarBaseType> m_luaVarsToFilter;
        }

        public class FilteredEventArgs : EventArgs
        {
            public FilteredEventArgs(DomNodeType nodeType)
            {
                m_nodeType = nodeType;
            }

            /// <summary>
            /// DomNodeType of variables that are getting filtered
            /// </summary>
            public DomNodeType NodeType
            {
                get { return m_nodeType; }
            }

            private readonly DomNodeType m_nodeType;
        }

        #endregion
    }

    interface ISledLuaVariableFilterService
    {
        /// <summary>
        /// Returns true if a variable is [locally] filtered by name or by LUA_T&lt;type&gt;
        /// </summary>
        /// <param name="luaVar">Variable to check</param>
        /// <returns>True if variable is [locally] filtered by name or by LUA_T&lt;type&gt; and false if not</returns>
        bool IsVariableFiltered(ISledLuaVarBaseType luaVar);

        /// <summary>
        /// Event fired when variable filtering needs to be done for a specific set of variables (globals, locals, etc.)
        /// </summary>
        event EventHandler<SledLuaVariableFilterService.FilteringEventArgs> Filtering;

        /// <summary>
        /// Event fired when variable filtering has finished for a specific set of variables (globals, locals, etc.)
        /// </summary>
        event EventHandler<SledLuaVariableFilterService.FilteredEventArgs> Filtered;
    }
}
