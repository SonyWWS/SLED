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

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    abstract class SledLuaTreeListViewEditor : SledTreeListViewEditor
    {
        protected SledLuaTreeListViewEditor(
            string name,
            string image,
            string[] columns,
            StandardControlGroup controlGroup)
            : base(name, image, columns, TreeListView.Style.TreeList, controlGroup)
        {
            TreeListView.AllowDrop = true;
            TreeListView.NodeSorter = new LuaVarSorter(TreeListView);
            TreeListView.Renderer = new LuaVarRenderer(TreeListView);
            TreeListView.Columns.ElementAt(ValueColumnIndexValue).AllowPropertyEdit = true;

            // TODO:
            //AllowDrag = true;
            AllowDebugFreeze = true;

            TreeListView.NodeExpandedChanged += TreeListViewNodeExpandedChanged;
            TreeListViewAdapter.CanItemPropertyChange += TreeListViewAdapterCanItemPropertyChange;
            TreeListViewAdapter.ItemPropertyChanged += TreeListViewAdapterItemPropertyChanged;
            MouseDoubleClick += SledLuaTreeListViewEditorMouseDoubleClick;
        }

        public new ITreeListView View
        {
            get { return base.View; }

            set
            {
                if (m_observableContext != null)
                    m_observableContext.ItemInserted -= ObservableContextItemInserted;

                base.View = value;

                m_observableContext = value.As<IObservableContext>();
                if (m_observableContext != null)
                    m_observableContext.ItemInserted += ObservableContextItemInserted;
            }
        }

        public void SaveState()
        {
            SaveTopItem();
            SaveSelection();
            
            TreeListView.BeginUpdate();
        }

        public void RestoreState()
        {
            TreeListView.EndUpdate();

            RestoreTopItem();
            RestoreSelection();
        }

        #region SledTreeListViewEditor Overrides

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
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #region Member Methods

        private void TreeListViewNodeExpandedChanged(object sender, TreeListView.NodeEventArgs e)
        {
            if (base.View == null)
                return;

            var variable = e.Node.As<ISledLuaVarBaseType>();
            if (variable == null)
                return;

            variable.Expanded = e.Node.Expanded;
        }

        private void TreeListViewAdapterCanItemPropertyChange(object sender, TreeListViewAdapter.CanItemPropertyChangeEventArgs e)
        {
            e.CanChange = false;

            var luaVar = e.Item.As<ISledLuaVarBaseType>();
            if (luaVar == null)
                return;

            e.CanChange =
                DebugService.IsConnected &&
                !DebugService.IsDebugging &&
                (e.PropertyIndex == ValuePropertyIndexValue) &&
                SledLuaUtil.IsEditableLuaType(luaVar);
        }

        private void TreeListViewAdapterItemPropertyChanged(object sender, TreeListViewAdapter.ItemPropertyChangedEventArgs e)
        {
            e.CancelChange = true;

            var luaVar = e.Item.As<ISledLuaVarBaseType>();
            if (luaVar == null)
                return;

            if (e.Value == null)
                return;

            var text = e.Value.ToString();
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                luaVar.Value = text;
                e.CancelChange = false;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[{0} Variable GUI] Exception " +
                    "setting value \"{1}\": {2}",
                    Name, text, ex.Message);
            }
        }

        private void SledLuaTreeListViewEditorMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var editor = sender.As<SledLuaTreeListViewEditor>();
            if (editor == null)
                return;

            var luaVar = editor.LastHit.As<ISledLuaVarBaseType>();
            if (luaVar == null)
                return;

            // if it's a cell that can't be edited then go to to the variable

            var iColumn = editor.GetItemColumnIndexAt(e.Location);
            var canModifyVariable =
                DebugService.IsConnected &&
                !DebugService.IsDebugging &&
                (iColumn == ValueColumnIndexValue) &&
                SledLuaUtil.IsEditableLuaType(luaVar);

            if (!canModifyVariable)
                GotoService.GotoVariable(luaVar);
        }

        private void ObservableContextItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            CheckIfTopItem(e.Item);
            CheckIfSelection(e.Item);
        }

        private void SaveTopItem()
        {
            m_topItem = null;
            m_topItemHash = 0;

            var item = TreeListViewAdapter.TopItem.As<ISledLuaVarBaseType>();
            if (item == null)
                return;

            m_topItemHash = item.UniqueNameMd5Hash;
        }

        private void RestoreTopItem()
        {
            if (m_topItem == null)
                return;

            try
            {
                TreeListViewAdapter.TopItem = m_topItem;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception restoring TopItem " + 
                    "on TreeListView \"{0}\": {1}",
                    TreeListView.Name, ex.Message);
            }
            finally
            {
                m_topItem = null;
                m_topItemHash = 0;
            }
        }

        private void CheckIfTopItem(object item)
        {
            if (item == null)
                return;

            var variable = item.As<ISledLuaVarBaseType>();
            if (variable == null)
                return;

            if (m_topItemHash == variable.UniqueNameMd5Hash)
                m_topItem = item;
        }

        private void SaveSelection()
        {
            m_lstSelection.Clear();
            m_lstSelectionHashes.Clear();

            foreach (var item in TreeListViewAdapter.Selection)
            {
                var variable = item.As<ISledLuaVarBaseType>();
                if (variable == null)
                    continue;

                m_lstSelectionHashes.Add(variable.UniqueNameMd5Hash);
            }
        }

        private void RestoreSelection()
        {
            if (m_lstSelection.Count <= 0)
                return;

            Selection = m_lstSelection;

            m_lstSelection.Clear();
            m_lstSelectionHashes.Clear();
        }

        private void CheckIfSelection(object item)
        {
            if (m_lstSelectionHashes.Count <= 0)
                return;

            if (item == null)
                return;

            var variable = item.As<ISledLuaVarBaseType>();
            if (variable == null)
                return;

            if (!m_lstSelectionHashes.Contains(variable.UniqueNameMd5Hash))
                return;

            if (m_lstSelection.Contains(variable))
                return;

            m_lstSelection.Add(variable);
        }

        #endregion

#pragma warning disable 649 // Field is never assigned

        [Import]
        protected ISledGotoService GotoService;

        [Import]
        protected ISledDebugService DebugService;

#pragma warning restore 649

        private object m_topItem;
        private Int64 m_topItemHash;

        private IObservableContext m_observableContext;

        private readonly List<object> m_lstSelection =
            new List<object>();

        private readonly List<Int64> m_lstSelectionHashes =
            new List<long>();

        private const int ValueColumnIndexValue = 2;
        private const int ValuePropertyIndexValue = 1;

        #region Private Classes

        private class LuaVarRenderer : TreeListView.NodeRenderer
        {
            public LuaVarRenderer(TreeListView owner)
                : base(owner)
            {
            }

            public override void DrawLabel(TreeListView.Node node, Graphics gfx, Rectangle bounds, int column)
            {
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

            private static string GetObjectString(object value)
            {
                var formattable = value as IFormattable;

                return
                    formattable != null
                        ? formattable.ToString(null, null)
                        : value.ToString();
            }
        }

        private class LuaVarSorter : IComparer<TreeListView.Node>
        {
            public LuaVarSorter(TreeListView owner)
            {
                Owner = owner;
            }

            public int Compare(TreeListView.Node x, TreeListView.Node y)
            {
                if ((x == null) && (y == null))
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                if (ReferenceEquals(x, y))
                    return 0;

                var luaVarX = x.Tag.As<ISledLuaVarBaseType>();
                var luaVarY = y.Tag.As<ISledLuaVarBaseType>();

                var result = 0;
                SortFunction[] sortFuncs;

                switch (Owner.SortColumn)
                {
                    default: sortFuncs = s_sortColumn0; break;
                    case 1: sortFuncs = s_sortColumn1; break;
                    case 2: sortFuncs = s_sortColumn2; break;
                    case 3: sortFuncs = s_sortColumn3; break;
                }

                for (var i = 0; i < sortFuncs.Length; i++)
                {
                    result = sortFuncs[i](luaVarX, luaVarY);
                    if (result != 0)
                        break;
                }

                if (Owner.SortOrder == SortOrder.Descending)
                    result *= -1;

                return result;
            }

            private TreeListView Owner { get; set; }

            #region Sort Functions

            private static int CompareNames(ISledLuaVarBaseType x, ISledLuaVarBaseType y)
            {
                //
                // When comparing root level watch items we don't
                // want to do the number check as two table items
                // at differing stack heights will compare as
                // equivalent (and perhaps other variable types
                // would, too).
                // For instance take the Lua table values "a.5.1"
                // and "a.5.1.1". Both of these items have the
                // same "CompareNames" result ("1" vs. "1"), the
                // same "CompareWhat" result ("LUA_TTABLE" vs.
                // "LUA_TTABLE") and the same "CompareValue" result
                // ("<table>" vs. "<table>") but they are clearly
                // not the same item! This makes the SortedList
                // used in the SledListTreeViewEditor class barf
                // when trying to set up the root node (it thinks
                // there are items with duplicate keys). So, lets
                // not do the number comparison on root level watch
                // list items!
                //

                var var1Root = x.DomNode.GetRoot();
                var var2Root = y.DomNode.GetRoot();

                var bWatchList =
                    ((var1Root.Type == SledLuaSchema.SledLuaVarWatchListType.Type) ||
                    (var2Root.Type == SledLuaSchema.SledLuaVarWatchListType.Type));

                var bWatchListRootItem =
                    bWatchList &&
                    (ReferenceEquals(x.DomNode.Parent, var1Root) ||
                    ReferenceEquals(y.DomNode.Parent, var2Root));

                double d1, d2;
                if (!bWatchListRootItem &&
                    double.TryParse(x.DisplayName, out d1) &&
                    double.TryParse(y.DisplayName, out d2))
                {
                    if (d1 == d2)
                        return 0;
                    if (d1 < d2)
                        return -1;
                    return 1;
                }

                return System.Globalization.SortKey.Compare(x.NameSortKey, y.NameSortKey);
            }

            private static int CompareWhat(ISledLuaVarBaseType x, ISledLuaVarBaseType y)
            {
                return System.Globalization.SortKey.Compare(x.WhatSortKey, y.WhatSortKey);
            }

            private static int CompareValue(ISledLuaVarBaseType x, ISledLuaVarBaseType y)
            {
                double d1, d2;
                if (double.TryParse(x.Value, out d1) &&
                    double.TryParse(y.Value, out d2))
                {
                    if (d1 == d2)
                        return 0;
                    if (d1 < d2)
                        return -1;
                    return 1;
                }

                return System.Globalization.SortKey.Compare(x.ValueSortKey, y.ValueSortKey);
            }

            private static int CompareDomNodeType(ISledLuaVarBaseType x, ISledLuaVarBaseType y)
            {
                var domNodeType1 = x.DomNode.Type;
                var domNodeType2 = y.DomNode.Type;

                if (domNodeType1 == domNodeType2)
                    return 0;

                var nodeType1 = domNodeType1.ToString();
                var nodeType2 = domNodeType2.ToString();

                return string.Compare(nodeType1, nodeType2, StringComparison.Ordinal);
            }

            private delegate int SortFunction(ISledLuaVarBaseType x, ISledLuaVarBaseType y);

            private static readonly SortFunction[] s_sortColumn0 =
            {
                CompareNames,
                CompareWhat,
                CompareValue,
                CompareDomNodeType
            };

            private static readonly SortFunction[] s_sortColumn1 =
            {
                CompareWhat,
                CompareNames,
                CompareValue,
                CompareDomNodeType
            };

            private static readonly SortFunction[] s_sortColumn2 =
            {
                CompareValue,
                CompareNames,
                CompareWhat,
                CompareDomNodeType
            };

            private static readonly SortFunction[] s_sortColumn3 =
            {
                CompareDomNodeType,
                CompareNames,
                CompareWhat,
                CompareValue
            };

            #endregion
        }

        #endregion
    }
}