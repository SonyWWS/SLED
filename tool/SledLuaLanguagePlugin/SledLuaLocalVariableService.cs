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
    [Export(typeof(SledLuaLocalVariableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaLocalVariableService : SledLuaBaseVariableService<SledLuaVarLocalListType, SledLuaVarLocalType>
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
            get { return Localization.SledLuaLocalsTitleShort; }
        }

        protected override string PopupPrefix
        {
            get { return "[L]"; }
        }

        protected override SledLuaTreeListViewEditor Editor
        {
            get { return m_editor; }
        }

        protected override DomNodeType NodeType
        {
            get { return SledLuaSchema.SledLuaVarLocalType.Type; }
        }

        protected override void OnDebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                //case Scmp.LuaTypeCodes.LuaVarLocalBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarLocal:
                {
                    if (!LuaWatchedVariableService.ReceivingWatchedVariables)
                    {
                        var local = LuaVarScmpService.GetScmpBlobAsLuaLocalVar();
                        RemoteTargetCallStackLocal(local);
                    }
                }
                break;

                //case Scmp.LuaTypeCodes.LuaVarLocalEnd:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarLocalLookupBegin:
                    OnDebugServiceLookupBegin();
                    break;

                case Scmp.LuaTypeCodes.LuaVarLocalLookupEnd:
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
                        SledLuaSchema.SledLuaVarLocalListType.Type,
                        SledLuaSchema.SledLuaVarLocalsRootElement);

                var locals = root.As<SledLuaVarLocalListType>();
                locals.Name =
                    string.Format(
                        "{0}{1}{2}{3}",
                        ProjectService.ProjectName,
                        Resource.Space,
                        Resource.LuaLocals,
                        e.NewLevel);

                locals.DomNode.AttributeChanged += DomNodeAttributeChanged;
                Collection.Add(locals);
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

        private void RemoteTargetCallStackLocal(SledLuaVarLocalType variable)
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
                    ListInsert[0].Locals.Add(variable);
                else
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "[SledLuaLocalVariableService] " +
                        "Failed to add local: {0}",
                        variable.Name);
                }
            }
        }

        #endregion

        private ISledLuaCallStackService m_luaCallStackService;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaLocalsEditor m_editor;

#pragma warning restore 649

        #region Private Classes

        [Export(typeof(SledLuaLocalsEditor))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaLocalsEditor : SledLuaTreeListViewEditor
        {
            [ImportingConstructor]
            public SledLuaLocalsEditor()
                : base(
                    Localization.SledLuaLocalsTitle,
                    null,
                    SledLuaVarBaseType.ColumnNames,
                    StandardControlGroup.Right)
            {
            }
        }

        #endregion
    }
}