/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaLuaStatesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaLuaStatesService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.Connected += DebugServiceConnected;
            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_luaStatesEditor.TreeListViewAdapter.ItemChecked += TreeListViewAdapterItemChecked;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            CreateLuaStatesCollection();
        }

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaStateBegin:
                    m_luaStatesCollection.ValidationBeginning();
                    break;

                case Scmp.LuaTypeCodes.LuaStateAdd:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaStateAdd>();

                    var domNode = new DomNode(SledLuaSchema.SledLuaStateType.Type);

                    var luaState = domNode.As<SledLuaStateType>();
                    luaState.Address = scmp.Address;
                    luaState.Name = scmp.Name;
                    luaState.Checked = scmp.Debugging == 1;

                    m_luaStatesCollection.LuaStates.Add(luaState);
                }
                break;

                case Scmp.LuaTypeCodes.LuaStateRemove:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaStateRemove>();

                    var sentinel =
                        m_luaStatesCollection.LuaStates.FirstOrDefault(
                            luaState => string.Compare(luaState.Address, scmp.Address, StringComparison.Ordinal) == 0);

                    if (sentinel != null)
                        m_luaStatesCollection.LuaStates.Remove(sentinel);
                }
                break;

                case Scmp.LuaTypeCodes.LuaStateEnd:
                    m_luaStatesCollection.ValidationEnding();
                    break;
            }
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            DestroyLuaStatesCollection();
        }

        #endregion

        #region Member Methods

        private void CreateLuaStatesCollection()
        {
            var root =
                new DomNode(
                    SledLuaSchema.SledLuaStateListType.Type,
                    SledLuaSchema.SledLuaStatesListRootElement);

            m_luaStatesCollection = root.As<SledLuaStateListType>();

            m_luaStatesCollection.Name =
                string.Format(
                    "{0}{1}{2}",
                    m_projectService.ProjectName,
                    Resources.Resource.Space,
                    Resources.Resource.LuaStateList);

            m_luaStatesEditor.View = m_luaStatesCollection;
        }

        private void DestroyLuaStatesCollection()
        {
            m_luaStatesEditor.View = null;

            if (m_luaStatesCollection != null)
                m_luaStatesCollection.LuaStates.Clear();

            m_luaStatesCollection = null;
        }

        private void TreeListViewAdapterItemChecked(object sender, TreeListViewAdapter.NodeAdapterEventArgs e)
        {
            var luaState = e.Node.As<SledLuaStateType>();
            if (luaState == null)
                return;

            var oldCheckedValue = luaState.Checked;

            // Maintain state
            luaState.Checked = e.Node.CheckState == CheckState.Checked;

            // Bail if value hasn't changed
            if (luaState.Checked == oldCheckedValue)
                return;

            m_debugService.SendScmp(
                new Scmp.LuaStateToggle(
                    m_luaLanguagePlugin.LanguageId,
                    luaState.Address));
        }

        #endregion
        
        private SledLuaStateListType m_luaStatesCollection;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaStatesEditor m_luaStatesEditor;

        [Import]
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledDebugService m_debugService;

#pragma warning restore 649

        #region Private Classes

        [Export(typeof(SledLuaStatesEditor))]
        [Export(typeof(IContextMenuCommandProvider))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaStatesEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            public SledLuaStatesEditor()
                : base(
                    Localization.SledLuaLuaStates,
                    null,
                    SledLuaStateListType.TheColumnNames,
                    TreeListView.Style.CheckedList,
                    StandardControlGroup.Right)
            {
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledLuaStateType>();
                if (clicked == null)
                    yield break;

                yield return StandardCommand.EditCopy;
            }

            #endregion

            #region SledTreeListViewEditor Overrides

            protected override string GetCopyText()
            {
                if ((TreeListViewAdapter == null) ||
                    (!TreeListViewAdapter.Selection.Any()))
                    return string.Empty;

                const string tab = "\t";
                const string indicator = "x";
                const string space = " ";

                var sb = new StringBuilder();
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledLuaStateType>();

                foreach (var item in items)
                {
                    sb.Append(item.Checked ? indicator : space);
                    sb.Append(tab);
                    sb.Append(item.Address);
                    sb.Append(tab);
                    sb.Append(item.Name);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion
        }

        #endregion
    }
}
