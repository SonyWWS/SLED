/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using ActiproSoftware.SyntaxEditor;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    internal sealed class LuaIntellisenseBroker : ILuaIntellisenseBroker
    {
        private LuaIntellisenseBroker()
        {
            TaskQueue = new TaskQueue();
            TaskQueue.Start();
            Application.ApplicationExit += (s, e) => TaskQueue.Stop();

            CustomLuaSyntaxLanguage = new LuatSyntaxLanguage(this);

            Database = new Database(TaskQueue, CustomLuaSyntaxLanguage, this);

            m_fauxControl = new FauxSyntaxEditorColorer(CustomLuaSyntaxLanguage);
            SkinService.SkinChangedOrApplied += SkinServiceSkinChangedOrApplied;
        }

        #region Implementation of ILuaIntellisenseBroker

        public void ProjectOpened(ILuaIntellisenseProject project)
        {
            m_project = project;

            foreach (var document in project.OpenDocuments)
                RegisterIntellipromptTipImageHandler(document);

            ProjectModified(m_project);
        }

        public void ProjectModified(ILuaIntellisenseProject project)
        {
            if (project != m_project)
                return;

            if (project == null)
            {
                Database.Clear();
                Database.LoadStandardLibrary();
            }
            else
            {
                Database.Rebuild(project);
            }

            TaskQueue.AddTask(ReparseOpenDocuments, TaskQueue.Thread.UI);
        }

        public void ProjectClosed()
        {
            m_project = null;
        }

        public void DocumentOpened(ILuaIntellisenseDocument document)
        {
            AddNavigationBar(document);
            RegisterIntellipromptTipImageHandler(document);
            ReparseDocuments(new[] { document });
        }

        public void DocumentClosed(ILuaIntellisenseDocument document)
        {
            UnregisterIntellipromptTipImageHandler(document);
            RemoveNavigationBar(document);
        }

        public LuaIntellisenseCustomScriptRegistrationHandler CustomScriptRegistrationHandler { get; set; }

        public ILuaintellisenseStatus Status
        {
            get { return Database.Status; }
        }

        #endregion

        #region Implementation of ILuaIntellisenseNavigator

        public LuaIntellisenseOpenAndSelectDelegate OpenAndSelectHandler { get; set; }

        public bool CanGotoPreviousPosition
        {
            get { return (m_currentPosition != null) && (m_currentPosition.Previous != null); }
        }

        public void GotoPreviousPosition()
        {
            if ((m_currentPosition != null) && (m_currentPosition.Previous != null))
            {
                m_currentPosition = m_currentPosition.Previous;
                m_currentPosition.Value();
                m_lastUserAction = UserAction.JumpedPosition;
            }
        }

        public bool CanGotoNextPosition
        {
            get { return (m_currentPosition != null) && (m_currentPosition.Next != null); }
        }

        public void GotoNextPosition()
        {
            if ((m_currentPosition != null) && (m_currentPosition.Next != null))
            {
                m_currentPosition = m_currentPosition.Next;
                m_currentPosition.Value();
                m_lastUserAction = UserAction.JumpedPosition;
            }
        }

        public bool CanGotoDefinition(ILuaIntellisenseDocument document)
        {
            return CanGoto(document);
        }

        public void GotoDefinition(ILuaIntellisenseDocument document)
        {
            if (!CanGoto(document))
                return;

            Func<Expression, List<LuatValue.IReference>> func =
                expression =>
                {
                    var definitions = new List<LuatValue.IReference>();
                    foreach (LuatValue value in expression.ResolvedValues.Values)
                    {
                        var variable = value as LuatVariable;
                        if (variable != null)
                            definitions.Merge(variable.Assignments.ToArray());
                    }

                    return definitions;
                };

            GotoDefinitions("Goto definition", document, func);
        }

        public bool CanGotoReference(ILuaIntellisenseDocument document)
        {
            return CanGoto(document);
        }

        public void GotoReference(ILuaIntellisenseDocument document)
        {
            if (!CanGoto(document))
                return;

            Func<Expression, List<LuatValue.IReference>> func =
                expression =>
                {
                    var definitions = new List<LuatValue.IReference>();
                    foreach (LuatValue value in expression.ResolvedValues.Values)
                    {
                        definitions.Merge(value.References.ToArray());
                    }

                    return definitions;
                };

            GotoDefinitions("Goto reference", document, func);
        }

        public bool CanRenameVariable(ILuaIntellisenseDocument document)
        {
            VariableExpression expression;
            List<LuatValue.IReference> definitions;
            return CanRenameVariable(document, out expression, out definitions);
        }

        public void RenameVariable(ILuaIntellisenseDocument document)
        {
            VariableExpression expression;
            List<LuatValue.IReference> definitions;
            if (!CanRenameVariable(document, out expression, out definitions))
                return;

            using (var dialog = new RenameVariableForm())
            {
                dialog.InputText = expression.DisplayText;

                var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
                if (dialog.ShowDialog(se) != DialogResult.OK)
                    return;

                if (CheckVariableNameInUse(expression, dialog.InputText))
                {
                    const string message = "A variable with this name already exists. Do you want to continue?";
                    const string caption = "Warning";

                    if (MessageBox.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
                        return;
                }

                try
                {
                    // save cursor and scroll pos to set it back to where it was after we are done replacing
                    //int caretPos = se.Caret.Offset;
                    //int scrollPos = se.SelectedView.FirstVisibleDisplayLineIndex;

                    var options =
                        new FindReplaceOptions
                            {
                                FindText = expression.DisplayText,
                                MatchCase = true,
                                MatchWholeWord = true,
                                ReplaceText = dialog.InputText,
                                SearchHiddenText = true,
                                SearchType = FindReplaceSearchType.Normal
                            };

                    se.Document.FindReplace.ReplaceAll(options);

                    //// remove duplicates (for instance, if scripts are referenced multiple times in a project)
                    //for (var i = 0; i < definitions.Count; ++i)
                    //{
                    //    var original = definitions[i];

                    //    var duplicates = new List<LuatValue.IReference>();
                    //    for (var j = i + 1; j < definitions.Count; ++j)
                    //    {
                    //        var potentialDuplicate = definitions[j];

                    //        if ((original.Line == potentialDuplicate.Line) &&
                    //            (original.TextRange.StartOffset == potentialDuplicate.TextRange.StartOffset) &&
                    //            (original.TextRange.EndOffset == potentialDuplicate.TextRange.EndOffset) &&
                    //            (string.Compare(original.Path, potentialDuplicate.Path, true) == 0))
                    //        {
                    //            duplicates.Add(potentialDuplicate);
                    //        }
                    //    }

                    //    foreach (LuatValue.IReference duplicate in duplicates)
                    //        definitions.Remove(duplicate);
                    //}

                    //// remove spans that are included in larger spans
                    //for (var i = 0; i < definitions.Count; ++i)
                    //{
                    //    var original = definitions[i];

                    //    var duplicates = new List<LuatValue.IReference>();
                    //    foreach (var potentialDuplicate in definitions)
                    //    {
                    //        if (ReferenceEquals(original, potentialDuplicate))
                    //            continue;

                    //        if (string.Compare(original.Path, potentialDuplicate.Path, true) != 0)
                    //            continue;

                    //        if ((potentialDuplicate.TextRange.StartOffset >= original.TextRange.StartOffset) &&
                    //            (potentialDuplicate.TextRange.EndOffset <= original.TextRange.EndOffset))
                    //        {
                    //            duplicates.Add(potentialDuplicate);
                    //        }
                    //    }

                    //    foreach (LuatValue.IReference duplicate in duplicates)
                    //        definitions.Remove(duplicate);
                    //}

                    //// sort the references so we replace the text bottom to top to avoid invalidating the
                    //// other references text range
                    //definitions.Sort(
                    //    (a, b) =>
                    //    {
                    //        TextRange aRange = ((SyntaxEditorTextRange)a.TextRange).ToTextRange();
                    //        TextRange bRange = ((SyntaxEditorTextRange)b.TextRange).ToTextRange();

                    //        return bRange.FirstOffset.CompareTo(aRange.FirstOffset);
                    //    });

                    //foreach (LuatValue.IReference reference in definitions)
                    //{
                    //    TextRange textRange = ((SyntaxEditorTextRange)reference.TextRange).ToTextRange();
                    //    se.Document.ReplaceText(DocumentModificationType.Replace, textRange, dialog.InputText);
                    //}

                    //// adjust the position of the cursor based on changes made to the document
                    //int diff = dialog.InputText.Length - expression.DisplayText.Length;
                    //caretPos += (diff * (definitions.Count - 1));

                    //se.Caret.Offset = caretPos;
                    //se.SelectedView.FirstVisibleDisplayLineIndex = scrollPos;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        public bool UseNavigationBar { get; set; }

        #endregion

        internal static LuaIntellisenseBroker Create()
        {
            return s_instance ?? (s_instance = new LuaIntellisenseBroker());
        }

        internal static LuaIntellisenseBroker Get()
        {
            return s_instance;
        }

        internal Database Database { get; private set; }

        internal TaskQueue TaskQueue { get; private set; }

        internal LuatSyntaxLanguage CustomLuaSyntaxLanguage { get; private set; }

        internal void OnUserAction(UserAction userAction, ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            if (OpenAndSelectHandler == null)
                return;

            if ((userAction != m_lastUserAction) ||
                (syntaxEditor != m_lastUserActionEditor))
            {
                m_lastUserAction = userAction;
                m_lastUserActionEditor = syntaxEditor;
                StoreCurrentPosition();
            }

            string path = syntaxEditor.Document.Filename;
            ISyntaxEditorTextRange position = new SyntaxEditorTextRange(syntaxEditor.SelectedView.Selection.TextRange);
            SetCurrentPosition(() => OpenAndSelectHandler(path, position));
        }

        #region Hidden/Private Classes and Wrappers

        internal enum UserAction
        {
            Unknown,
            JumpedPosition,
            Typed,
            MovedCaret
        }

        internal sealed class FauxSyntaxEditorColorer : Control
        {
            public FauxSyntaxEditorColorer(LuatSyntaxLanguage language)
            {
                m_language = language;
            }

            /// <summary>
            /// Sets the foreground color for a specified language-specific keyword in the text editing area</summary>
            public IEnumerable<TextHighlightStyle> TextHighlightStyles
            {
                get
                {
                    var styles = new List<TextHighlightStyle>();
                    for (int i = 0; i < m_language.HighlightingStyles.Count; i++)
                    {
                        var item = m_language.HighlightingStyles[i];
                        styles.Add(new TextHighlightStyle(item.Key, item.ForeColor, item.BackColor));
                    }
                    return styles;
                }

                set
                {
                    foreach (var item in value)
                    {
                        var style = m_language.HighlightingStyles[item.Key];
                        if (style != null)
                        {
                            style.ForeColor = item.ForeColor;
                            style.BackColor = item.BackColor;
                        }
                    }
                }
            }

            private readonly LuatSyntaxLanguage m_language;
        }

        #endregion

        private void ReparseDocuments(IEnumerable<ILuaIntellisenseDocument> documents)
        {
            if (documents == null)
                return;

            foreach (var document in documents)
            {
                if ((document == null) || (document.SyntaxEditorControl == null))
                    continue;

                // ensure things are always setup; these methods also prevents duplicate registration
                {
                    AddNavigationBar(document);
                    RegisterIntellipromptTipImageHandler(document);
                }

                var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
                if (se.Document == null)
                    continue;

                se.Document.Filename = document.Uri.LocalPath;
                se.Document.Language = CustomLuaSyntaxLanguage;
            }
        }

        private void ReparseOpenDocuments()
        {
            if (m_project == null)
                return;

            ReparseDocuments(m_project.OpenDocuments);
        }

        private void RegisterIntellipromptTipImageHandler(ILuaIntellisenseDocument document)
        {
            if (document == null)
                return;

            if (document.SyntaxEditorControl != null)
            {
                var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
                se.IntelliPromptTipImageRequested -= IntelliPromptTipImageRequested; // lame, but makes sure we don't register more than once
                se.IntelliPromptTipImageRequested += IntelliPromptTipImageRequested;
            }
        }

        private void UnregisterIntellipromptTipImageHandler(ILuaIntellisenseDocument document)
        {
            if (document == null)
                return;

            if (document.SyntaxEditorControl != null)
            {
                var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
                se.IntelliPromptTipImageRequested -= IntelliPromptTipImageRequested;
            }
        }

        private void IntelliPromptTipImageRequested(object sender, IntelliPromptTipImageRequestedEventArgs e)
        {
            try
            {
                var images = LuaIntellisenseIcons.GetImageList();

                var icontype = (LuaIntellisenseIconType)Enum.Parse(typeof(LuaIntellisenseIconType), e.Source, true);
                e.Image = images.Images[(int)icontype];
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void SkinServiceSkinChangedOrApplied(object sender, EventArgs e)
        {
            SkinService.ApplyActiveSkin(m_fauxControl);
        }

        private void StoreCurrentPosition()
        {
            if (m_currentPosition == null)
                return;

            // If we've moved back through the recorded positions and now
            // adding a new position, make sure all positions forward of the
            // m_currentLocation are removed from the list
            while (m_currentPosition.Next != null)
            {
                m_recordedLocations.Remove(m_currentPosition.Next);
            }

            // Make sure the list doesn't grow too large
            while (m_recordedLocations.Count > MaxRecordedLocations)
            {
                m_recordedLocations.RemoveFirst();
            }

            // Add the recorded position
            m_recordedLocations.AddLast(m_currentPosition.Value);
            m_currentPosition = m_recordedLocations.Last;
        }

        private void SetCurrentPosition(Action gotoPosition)
        {
            if (m_currentPosition == null)
            {
                m_recordedLocations.AddLast(gotoPosition);
                m_currentPosition = m_recordedLocations.Last;
            }
            else
            {
                m_currentPosition.Value = gotoPosition;
            }
        }

        private void AddNavigationBar(ILuaIntellisenseDocument document)
        {
            if (!UseNavigationBar)
                return;

            if (document == null)
                return;

            if (document.SyntaxEditorControl == null)
                return;

            var se = document.SyntaxEditorControl.As<ActiproSoftware.SyntaxEditor.SyntaxEditor>();
            if (se == null)
                return;

            var panel = se.Parent.As<Control>();
            if (panel == null)
                return;

            if (panel.Controls["NavBar"] != null)
                return;

            var navBar =
                new NavigationBar
                    {
                        Width = panel.Width,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        Tag = "NavBar",
                        Editor = se
                    };
            panel.Controls.Add(navBar);

            se.Top = navBar.Height;
            se.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            se.Dock = DockStyle.None;
            se.Width = panel.Width;
            se.Height = panel.Height - se.Top;
        }

        private void RemoveNavigationBar(ILuaIntellisenseDocument document)
        {
            if (document == null)
                return;

            if (document.SyntaxEditorControl == null)
                return;

            var se = document.SyntaxEditorControl.As<ActiproSoftware.SyntaxEditor.SyntaxEditor>();
            if (se == null)
                return;

            var panel = se.Parent.As<Control>();
            if (panel == null)
                return;

            var navBar = panel.Controls["NavBar"];
            if (navBar == null)
                return;

            panel.Controls.Remove(navBar);
        }

        private static bool CanGoto(ILuaIntellisenseDocument document)
        {
            return
                (document != null) &&
                (document.SyntaxEditorControl != null) &&
                (((ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl).SelectedView != null);
        }

        private static bool CanRenameVariable(ILuaIntellisenseDocument document, out VariableExpression expression, out List<LuatValue.IReference> definitions)
        {
            expression = null;
            definitions = null;

            if (!CanGoto(document))
                return false;

            if (document.SyntaxEditorControl == null)
                return false;

            var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
            if ((se.SelectedView == null) || (se.SelectedView.Selection == null))
                return false;

            expression = GetExpressionAt(se.Document, se.SelectedView.Selection.StartOffset) as VariableExpression;
            if (expression == null)
                return false;

            definitions = new List<LuatValue.IReference>();
            foreach (LuatValue value in expression.ResolvedValues.Values)
            {
                var variable = value.As<LuatVariable>();
                if (variable != null)
                    definitions.Merge(variable.References.ToArray());
            }

            return definitions.Count > 0;
        }

        private static Expression GetExpressionAt(Document document, int offset)
        {
            var cu = document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return null;

            var expression = cu.FindDescendant(typeof(Expression), offset) as Expression;
            return expression;
        }

        private static void GotoDefinitions(string title, ILuaIntellisenseDocument document, Func<Expression, List<LuatValue.IReference>> func)
        {
            if ((document == null) ||
                (document.SyntaxEditorControl == null) ||
                (func == null))
                return;

            var se = (ActiproSoftware.SyntaxEditor.SyntaxEditor)document.SyntaxEditorControl;
            if ((se.Document == null) ||
                (se.SelectedView == null))
                return;

            var expression = GetExpressionAt(se.Document, se.SelectedView.Selection.StartOffset);
            if (expression == null)
                return;

            var definitions = func(expression);
            if (definitions == null)
                return;

            if (definitions.Count == 0)
                return;

            if (definitions.Count == 1)
            {
                definitions[0].Goto();
                return;
            }

            using (var gfx = se.SelectedView.CreateGraphics())
            {
                var menu = new ContextMenuStrip { ImageList = LuaIntellisenseIcons.GetImageList() };
                SkinService.ApplyActiveSkin(menu);

                KeyValuePair<string, LuatValue.IReference[]>[] groups = definitions.GroupItems(a => a.Context, a => a);

                ToolStripLabel label;
                {
                    label = new ToolStripLabel(title) {Enabled = false};
                    menu.Items.Add(label);
                }

                int index = 0;

                foreach (KeyValuePair<string, LuatValue.IReference[]> group in groups)
                {
                    menu.Items.Add(new ToolStripSeparator());

                    if (groups.Length > 0)
                    {
                        label = new ToolStripLabel("Context: " + group.Key) { Enabled = false };
                        menu.Items.Add(label);
                    }

                    var items = new List<ToolStripItem>();
                    foreach (LuatValue.IReference definition in group.Value)
                    {
                        string text = GetReferenceText(definition, document.Project);
                        string file = System.IO.Path.GetFileName(definition.Path);
                        int line = definition.Line;

                        LuatValue.GotoReference gotoRef = definition.Goto;
                        var item =
                            new ToolStripMenuItem
                                {
                                    ImageIndex =
                                        (int)(definition.Value != null
                                                  ? definition.Value.Type.Icon
                                                  : LuaIntellisenseIconType.Unknown),
                                    Text = string.Format("&{0} {1}({2}): {3}", index++, file, line, text),
                                    Font = se.Font,
                                    Tag = line
                                };
                        item.Width = (int)gfx.MeasureString(item.Text, item.Font).Width;
                        item.Click += (s, e) => gotoRef();
                        items.Add(item);
                    }

                    menu.Items.AddRange(items.OrderBy(x => (int)x.Tag).ToArray());
                }

                var rect = se.SelectedView.GetCharacterBounds(se.SelectedView.Selection.StartOffset);
                menu.Show(se, rect.Location);
            }
        }

        private static string GetReferenceText(LuatValue.IReference reference, ILuaIntellisenseProject project)
        {
            string fileContents;
            {
                var uri = new Uri(reference.Path);
                var document = project[uri];
                var useOpenDocument =
                    (document != null) &&
                    (!string.IsNullOrEmpty(document.Contents));

                try
                {
                    fileContents =
                        useOpenDocument
                            ? document.Contents
                            : System.IO.File.ReadAllText(uri.LocalPath);
                }
                catch (Exception ex)
                {
                    fileContents = string.Empty;
                    ex.ToString();
                }

                fileContents = fileContents.Replace("\r\n", "\n");
            }

            if (string.IsNullOrEmpty(fileContents))
                return reference.DisplayText;

            int lineStart, lineEnd;

            for (lineStart = reference.TextRange.StartOffset; lineStart > 0; --lineStart)
            {
                if (fileContents[lineStart] == '\n')
                {
                    ++lineStart;
                    break;
                }
            }

            for (lineEnd = reference.TextRange.StartOffset; lineEnd < fileContents.Length; ++lineEnd)
            {
                if (fileContents[lineEnd] == '\n')
                {
                    break;
                }
            }

            return fileContents.Substring(lineStart, lineEnd - lineStart);
        }

        private static bool CheckVariableNameInUse(VariableExpression expression, string variableName)
        {
            if (expression == null)
                return false;

            var scripts = new List<LuatScript>();

            var bs = expression.FindAncestor<BlockStatement>();
            while (bs != null)
            {
                foreach (var i in bs.Locals.Entries)
                {
                    LuatTable table = i.Value;

                    if (!scripts.Contains(i.Key))
                        scripts.Add(i.Key);

                    foreach (var v in table.Children)
                    {
                        string name = v.Key;
                        if (name == variableName)
                            return true;
                    }
                }

                bs = bs.FindAncestor<BlockStatement>();
            }

            foreach (LuatScript script in scripts)
            {
                var visited = new HashSet<LuatValue>();
                IEnumerable<KeyValuePair<string, LuatValue>> children = script.Table.GetChildren(ref visited);

                foreach (KeyValuePair<string, LuatValue> child in children)
                {
                    if (child.Key == variableName)
                        return true;
                }
            }

            return false;
        }

        private ILuaIntellisenseProject m_project;
        private LinkedListNode<Action> m_currentPosition;
        private UserAction m_lastUserAction = UserAction.Unknown;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor m_lastUserActionEditor;

        private readonly FauxSyntaxEditorColorer m_fauxControl;

        private readonly LinkedList<Action> m_recordedLocations =
            new LinkedList<Action>();

        private static LuaIntellisenseBroker s_instance;

        private const int MaxRecordedLocations = 100;
    }
}