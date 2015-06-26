/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(SledLuaEnvironmentVariableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaEnvironmentVariableService : SledLuaBaseVariableService<SledLuaVarEnvListType, SledLuaVarEnvType>
    {
        #region SledLuaBaseVariableService Overrides

        protected override void Initialize()
        {
            m_luaCallStackService = SledServiceInstance.Get<ISledLuaCallStackService>();
            m_luaCallStackService.Clearing += LuaCallStackServiceClearing;
            m_luaCallStackService.LevelAdding += LuaCallStackServiceLevelAdding;
            m_luaCallStackService.StackLevelChanging += LuaCallStackServiceStackLevelChanging;
            m_luaCallStackService.StackLevelChanged += LuaCallStackServiceStackLevelChanged;
        }

        protected override string ShortName
        {
            get { return Localization.SledLuaEnvironmentTitleShort; }
        }

        protected override string PopupPrefix
        {
            get { return "[E]"; }
        }

        protected override SledLuaTreeListViewEditor Editor
        {
            get { return m_editor; }
        }

        protected override DomNodeType NodeType
        {
            get { return SledLuaSchema.SledLuaVarEnvType.Type; }
        }

        protected override void OnDebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                //case Scmp.LuaTypeCodes.LuaVarEnvVarBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarEnvVar:
                {
                    if (!LuaWatchedVariableService.ReceivingWatchedVariables)
                    {
                        var envVar = LuaVarScmpService.GetScmpBlobAsLuaEnvironmentVar();
                        RemoteTargetCallStackEnvVar(envVar);
                    }
                }
                break;

                //case Scmp.LuaTypeCodes.LuaVarEnvVarEnd:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarEnvVarLookupBegin:
                    OnDebugServiceLookupBegin();
                    break;

                case Scmp.LuaTypeCodes.LuaVarEnvVarLookupEnd:
                    OnDebugServiceLookupEnd();
                    break;
            }
        }

        protected override void OnLuaVariableFilterServiceFiltered(object sender, SledLuaVariableFilterService.FilteredEventArgs e)
        {
            if (e.NodeType != NodeType)
                return;

            if (m_luaCallStackService.CurrentStackLevel >= Collection.Count)
                return;

            m_editor.View = Collection[m_luaCallStackService.CurrentStackLevel];
        }

        #endregion

        #region ISledLuaCallStackService Events

        private void LuaCallStackServiceClearing(object sender, EventArgs e)
        {
            m_editor.View = null;

            if (m_luaCallStackService.CurrentStackLevel != 0)
            {
                if (Collection.Count > 0)
                    Collection[0].ResetExpandedStates();
            }

            foreach (var collection in Collection)
                collection.Variables.Clear();
        }

        private void LuaCallStackServiceLevelAdding(object sender, SledLuaCallStackServiceEventArgs e)
        {
            if ((e.NewLevel + 1) > Collection.Count)
            {
                var root =
                    new DomNode(
                        SledLuaSchema.SledLuaVarEnvListType.Type,
                        SledLuaSchema.SledLuaVarEnvListRootElement);

                var envVars = root.As<SledLuaVarEnvListType>();
                envVars.Name =
                    string.Format(
                        "{0}{1}{2}{3}",
                        ProjectService.ProjectName,
                        Resource.Space,
                        Resource.LuaEnvironmentVariables,
                        e.NewLevel);

                envVars.DomNode.AttributeChanged += DomNodeAttributeChanged;
                Collection.Add(envVars);
            }

            if ((e.NewLevel == 0) && (Collection.Count > 0))
                m_editor.View = Collection[0];
        }

        private void LuaCallStackServiceStackLevelChanging(object sender, SledLuaCallStackServiceEventArgs e)
        {
            foreach (var collection in Collection)
                collection.ValidationBeginning();

            m_editor.View = null;
            Collection[e.OldLevel].SaveExpandedStates();

            m_editor.View = Collection[e.NewLevel];
        }

        private void LuaCallStackServiceStackLevelChanged(object sender, SledLuaCallStackServiceEventArgs e)
        {
            foreach (var collection in Collection)
                collection.ValidationEnded();
        }

        #endregion

        #region Member Methods

        private void RemoteTargetCallStackEnvVar(SledLuaVarEnvType variable)
        {
            var iCount = Collection.Count;
            if (iCount <= variable.Level)
                return;
            
            Collection[variable.Level].ValidationBeginning();

            // Add any locations
            LuaVarLocationService.AddLocations(variable);

            // Do any filtering
            variable.Visible = !LuaVariableFilterService.IsVariableFiltered(variable);

            // Figure out where to insert
            if (!LookingUp)
            {
                Collection[variable.Level].Variables.Add(variable);
            }
            else
            {
                if (ListInsert.Count > 0)
                    ListInsert[0].EnvVars.Add(variable);
                else
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "[SledLuaEnvironmentVariableService] " +
                        "Failed to add environment variable: {0}",
                        variable.Name);
                }
            }
        }

        #endregion

        private ISledLuaCallStackService m_luaCallStackService;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaEnvVarsEditor m_editor;

#pragma warning restore 649

        #region Private Classes

        [Export(typeof(SledLuaEnvVarsEditor))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaEnvVarsEditor : SledLuaTreeListViewEditor
        {
            [ImportingConstructor]
            public SledLuaEnvVarsEditor()
                : base(
                    Localization.SledLuaEnvironmentTitle,
                    null,
                    SledLuaVarBaseType.ColumnNames,
                    StandardControlGroup.Right)
            {
            }
        }

        #endregion
    }
}