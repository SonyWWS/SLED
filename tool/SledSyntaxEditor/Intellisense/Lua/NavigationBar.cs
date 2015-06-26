/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;

using Sce.Atf.Applications;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Provides a drop-down list of function declarations for the given file.
    /// </summary>
    internal sealed class NavigationBar : ComboBox
    {
        public NavigationBar()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            Sorted = true;

            m_broker = LuaIntellisenseBroker.Get();

            SkinService.ApplyActiveSkin(this);
            SkinService.SkinChangedOrApplied += SkinServiceSkinChangedOrApplied;
        }

        public ActiproSoftware.SyntaxEditor.SyntaxEditor Editor
        {
            get { return m_editor; }
            set { SetEditor(value); value.ParentChanged += EditorParentChanged; }
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            base.OnSelectionChangeCommitted(e);

            if ((SelectedItem == null) ||
                (m_editor == null) ||
                (m_editor.SelectedView == null))
                return;

            var item = SelectedItem as Item;
            if (item == null)
                return;

            m_editor.SelectedView.Selection.StartOffset = item.Node.StartOffset;
            m_editor.Focus();
        }

        private void EditorParentChanged(object sender, EventArgs e)
        {
            if (m_editor == null)
                return;

            if (m_editor.Parent == null)
                SkinService.SkinChangedOrApplied -= SkinServiceSkinChangedOrApplied;
        }

        private void DocumentChanged(object sender, EventArgs e)
        {
            SetDocument(Editor.Document);
        }

        private void SelectionChanged(object sender, SelectionEventArgs e)
        {
            UpdateSelectedItem();
        }

        private void SemanticParseDataChanged(object sender, EventArgs e)
        {
            m_broker.TaskQueue.AddTask(DoUpdateList, TaskQueue.Thread.UI);
        }

        private void SkinServiceSkinChangedOrApplied(object sender, EventArgs e)
        {
            if (IsDisposed)
                return;

            SkinService.ApplyActiveSkin(this);
        }

        private void SetEditor(ActiproSoftware.SyntaxEditor.SyntaxEditor editor)
        {
            if (null != m_editor)
            {
                SetDocument(null);
                m_editor.DocumentChanged -= DocumentChanged;
                m_editor.SelectionChanged -= SelectionChanged;
            }

            m_editor = editor;

            if (m_editor != null)
            {
                m_editor.DocumentChanged += DocumentChanged;
                m_editor.SelectionChanged += SelectionChanged;
                SetDocument(editor.Document);
            }
        }

        private void SetDocument(Document document)
        {
            if (m_document != null)
                m_document.SemanticParseDataChanged -= SemanticParseDataChanged;

            m_document = document;

            if (m_document != null)
            {
                m_document.SemanticParseDataChanged += SemanticParseDataChanged;
                m_broker.TaskQueue.AddTask(DoUpdateList, TaskQueue.Thread.Worker);
            }
        }

        private void UpdateSelectedItem()
        {
            if ((m_editor == null) ||
                (m_editor.SelectedView == null))
                return;

            int offset = m_editor.SelectedView.Selection.StartOffset;

            Item best = null;
            const int bestOffset = 0;

            foreach (Item item in Items)
            {
                if (item.Node.Contains(offset))
                {
                    best = item.Node.StartOffset > bestOffset ? item : best;
                }
            }

            if (best != null)
            {
                SelectedItem = best;
            }
        }

        private void DoUpdateList()
        {
            if (m_document == null)
                return;

            var cu = m_document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return;

            Item[] items = BuildList(null, cu);
            m_broker.TaskQueue.AddTask(() => { Items.Clear(); Items.AddRange(items); }, TaskQueue.Thread.UI);
            UpdateSelectedItem();
        }

        private Item[] BuildList(string path, LuatAstNodeBase node)
        {
            var items = new List<Item>();

            var assignment = node as AssignmentStatement;
            if (assignment != null)
            {
                int count = Math.Min(assignment.Variables.Count, assignment.Values.Count);
                for (int index = 0; index < count; ++index)
                {
                    string name = assignment.Variables[index].DisplayText;
                    var expr = assignment.Values[index] as Expression;
                    string subpath = (null != path) ? (path + "\u2219" + name) : name; // \u2219 is a bullet

                    if (expr == null)
                        continue;

                    foreach (LuatValue value in expr.ResolvedValues.Values)
                    {
                        var func = value as LuatFunction;
                        var funcType = value.Type as LuatTypeFunction;
                        if ((func != null) && (funcType != null))
                        {
                            string desc = subpath + funcType.Arguments.ToArray().ToCommaSeperatedList().Parenthesize();
                            items.Add(new Item(desc, expr));
                            break;
                        }
                    }

                    foreach (LuatAstNodeBase child in expr.ChildNodes)
                    {
                        items.AddRange(BuildList(subpath, child));
                    }
                }
            }
            else
            {
                foreach (LuatAstNodeBase child in node.ChildNodes)
                {
                    items.AddRange(BuildList(path, child));
                }
            }

            return items.ToArray();
        }

        #region Private Classes

        private class Item
        {
            public Item(string text, LuatAstNodeBase node)
            {
                Text = text;
                Node = node;
            }

            public override string ToString()
            {
                return Text;
            }
            
            public LuatAstNodeBase Node { get; private set; }
            
            private string Text { get; set; }
        }

        #endregion

        private Document m_document;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor m_editor;

        private readonly LuaIntellisenseBroker m_broker;
    }
}