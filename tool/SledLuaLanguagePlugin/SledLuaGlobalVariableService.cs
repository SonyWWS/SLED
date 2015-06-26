/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

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
    [Export(typeof(SledLuaGlobalVariableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaGlobalVariableService : SledLuaBaseVariableService<SledLuaVarGlobalListType, SledLuaVarGlobalType>
    {
        #region SledLuaBaseVariableService Overrides

        protected override void Initialize()
        {
        }

        protected override string ShortName
        {
            get { return Localization.SledLuaGlobalsTitleShort; }
        }

        protected override string PopupPrefix
        {
            get { return "[G]"; }
        }

        protected override SledLuaTreeListViewEditor Editor
        {
            get { return m_editor; }
        }

        protected override DomNodeType NodeType
        {
            get { return SledLuaSchema.SledLuaVarGlobalType.Type; }
        }

        protected override void OnDebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            CreateGlobalsCollection();
        }

        protected override void OnDebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                //case Scmp.LuaTypeCodes.LuaVarGlobalBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarGlobal:
                {
                    if (!LuaWatchedVariableService.ReceivingWatchedVariables)
                    {
                        var global = LuaVarScmpService.GetScmpBlobAsLuaGlobalVar();
                        RemoteTargetGlobalAdd(global);
                    }
                }
                break;

                //case Scmp.LuaTypeCodes.LuaVarGlobalEnd:
                //    break;

                case Scmp.LuaTypeCodes.LuaVarGlobalLookupBegin:
                    OnDebugServiceLookupBegin();
                    break;

                case Scmp.LuaTypeCodes.LuaVarGlobalLookupEnd:
                    OnDebugServiceLookupEnd();
                    break;
            }
        }

        #endregion

        #region Member Methods

        private void CreateGlobalsCollection()
        {
            if (Collection.Count <= 0)
            {
                var root =
                    new DomNode(
                        SledLuaSchema.SledLuaVarGlobalListType.Type,
                        SledLuaSchema.SledLuaVarGlobalsRootElement);

                var collection = root.As<SledLuaVarGlobalListType>();
                collection.Name =
                    string.Format(
                        "{0}{1}{2}",
                        ProjectService.ProjectName,
                        Resource.Space,
                        Resource.LuaGlobals);

                collection.DomNode.AttributeChanged += DomNodeAttributeChanged;
                Collection.Add(collection);
            }

            if (Collection.Count > 0)
                m_editor.View = Collection[0];
        }

        private void RemoteTargetGlobalAdd(SledLuaVarGlobalType variable)
        {
            if (Collection.Count > 0)
                Collection[0].ValidationBeginning();

            // Add any locations
            LuaVarLocationService.AddLocations(variable);

            // Do any filtering
            variable.Visible = !LuaVariableFilterService.IsVariableFiltered(variable);

            // Figure out where to insert
            if (!LookingUp)
            {
                Collection[0].Variables.Add(variable);
            }
            else
            {
                if (ListInsert.Count > 0)
                    ListInsert[0].Globals.Add(variable);
                else
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "[SledLuaGlobalVariableService] " +
                        "Failed to add variable: {0}",
                        variable.Name);
                }
            }
        }

        #endregion

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaGlobalsEditor m_editor;

#pragma warning restore 649

        #region Private Classes

        [Export(typeof(SledLuaGlobalsEditor))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaGlobalsEditor : SledLuaTreeListViewEditor
        {
            [ImportingConstructor]
            public SledLuaGlobalsEditor()
                : base(
                    Localization.SledLuaGlobalsTitle,
                    null,
                    SledLuaVarBaseType.ColumnNames,
                    StandardControlGroup.Right)
            {
            }
        }

        #endregion
    }
}
