/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser;
using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    // The LuatSyntaxLanguage to be used by the SyntaxEditor control
    class LuatSyntaxLanguage : MergableSyntaxLanguage
    {
        /// <summary>
        /// Initializes a new instance of the <c>LuatSyntaxLanguage</c> class.
        /// </summary>
        public LuatSyntaxLanguage(LuaIntellisenseBroker plugin)
            : base("Luat")
        {
            m_plugin = plugin;

            ExampleText = @"/*
                            function HelloWorld()
                              local a = 1
                              local b : FwVector4;
                            end";

            // Initialize highlighting styles (same as the LuaDefinition.xml values); helps SkinService
            HighlightingStyles.Add(new HighlightingStyle(ReservedWordStyleString, null, Color.Blue, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(FunctionStyleString, null, Color.Magenta, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(GlobalVariableStyleString, null, Color.Teal, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(OperatorStyleString, null, Color.Black, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(NumberStyleString, null, Color.Purple, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(StringDelimiterStyleString, null, Color.Maroon, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(StringDefaultStyleString, null, Color.Maroon, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(CommentDelimiterStyleString, null, Color.Green, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(CommentUrlStyleString, null, Color.Green, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(CommentTagStyleString, null, Color.Gray, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(CommentDefaultStyleString, null, Color.Green, Color.Empty));
            
            // Initialize custom highlighting styles
            HighlightingStyles.Add(new HighlightingStyle(WarningStyleString, null, Color.Green, Color.Empty));
            HighlightingStyles.Add(new HighlightingStyle(ReferenceStyleString, null, Color.Empty, Color.LightCyan));
            HighlightingStyles.Add(new HighlightingStyle(AssignmentStyleString, null, Color.Empty, Color.MistyRose));

            // Initialize lexical states
            LexicalStates.Add(new DefaultLexicalState(LuatLexicalStateId.Default, "DefaultState"));
            DefaultLexicalState = LexicalStates["DefaultState"];
            LexicalStates["DefaultState"].DefaultHighlightingStyle = HighlightingStyles["DefaultStyle"];
        }

        /// <summary>
        /// Resets the <see cref="AutomaticOutliningBehavior"/> property to its default value.
        /// </summary>
        public override void ResetAutomaticOutliningBehavior()
        {
            AutomaticOutliningBehavior = AutomaticOutliningBehavior.SemanticParseDataChange;
        }

        /// <summary>
        /// Indicates whether the <see cref="AutomaticOutliningBehavior"/> property should be persisted.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the property value has changed from its default; otherwise, <c>false</c>.
        /// </returns>
        public override bool ShouldSerializeAutomaticOutliningBehavior()
        {
            return (AutomaticOutliningBehavior != AutomaticOutliningBehavior.SemanticParseDataChange);
        }

        /// <summary>
        /// Creates an <see cref="IToken"/> that represents the end of a document.
        /// </summary>
        /// <param name="startOffset">The start offset of the <see cref="IToken"/>.</param>
        /// <param name="lexicalState">The <see cref="ILexicalState"/> that contains the token.</param>
        /// <returns>An <see cref="IToken"/> that represents the end of a document.</returns>
        public override IToken CreateDocumentEndToken(int startOffset, ILexicalState lexicalState)
        {
            return new LuatToken(startOffset, 0, LexicalParseFlags.None, null, new LexicalStateAndIDTokenLexicalParseData(lexicalState, LuatTokenId.DocumentEnd));
        }

        /// <summary>
        /// Creates an <see cref="IToken"/> that represents an invalid range of text.
        /// </summary>
        /// <param name="startOffset">The start offset of the <see cref="IToken"/>.</param>
        /// <param name="length">The length of the <see cref="IToken"/>.</param>
        /// <param name="lexicalState">The <see cref="ILexicalState"/> that contains the token.</param>
        /// <returns>An <see cref="IToken"/> that represents an invalid range of text.</returns>
        public override IToken CreateInvalidToken(int startOffset, int length, ILexicalState lexicalState)
        {
            return new LuatToken(startOffset, length, LexicalParseFlags.None, null, new LexicalStateAndIDTokenLexicalParseData(lexicalState, LuatTokenId.Invalid));
        }

        /// <summary>
        /// Creates an <see cref="IToken"/> that represents the range of text with the specified lexical parse data.
        /// </summary>
        /// <param name="startOffset">The start offset of the token.</param>
        /// <param name="length">The length of the token.</param>
        /// <param name="lexicalParseFlags">The <see cref="LexicalParseFlags"/> for the token.</param>
        /// <param name="parentToken">The <see cref="IToken"/> that starts the current state scope specified by the <see cref="ITokenLexicalParseData.LexicalState"/> property.</param>
        /// <param name="lexicalParseData">The <see cref="ITokenLexicalParseData"/> that contains lexical parse information about the token.</param>
        /// <returns></returns>
        public override IToken CreateToken(int startOffset, int length, LexicalParseFlags lexicalParseFlags, IToken parentToken, ITokenLexicalParseData lexicalParseData)
        {
            return new LuatToken(startOffset, length, lexicalParseFlags, parentToken, lexicalParseData);
        }

        /// <summary>
        /// Resets the <see cref="ActiproSoftware.SyntaxEditor.SyntaxLanguage.ErrorDisplayEnabled"/> property to its default value.
        /// </summary>
        public override void ResetErrorDisplayEnabled()
        {
            ErrorDisplayEnabled = true;
        }

        /// <summary>
        /// Indicates whether the <see cref="ActiproSoftware.SyntaxEditor.SyntaxLanguage.ErrorDisplayEnabled"/> property should be persisted.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the property value has changed from its default; otherwise, <c>false</c>.
        /// </returns>
        public override bool ShouldSerializeErrorDisplayEnabled()
        {
            return !ErrorDisplayEnabled;
        }

        /// <summary>
        /// Returns the <see cref="HighlightingStyle"/> for the specified <see cref="IToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="IToken"/> to examine.</param>
        /// <returns>The <see cref="HighlightingStyle"/> for the specified <see cref="IToken"/>.</returns>
        public override HighlightingStyle GetHighlightingStyle(IToken token)
        {
            switch (token.ID)
            {
                case LuatTokenId.SingleLineComment:
                case LuatTokenId.MultiLineComment:
                    return HighlightingStyles[CommentDefaultStyleString];

                case LuatTokenId.Local:
                case LuatTokenId.Function:
                    return HighlightingStyles[ReservedWordStyleString];

                case LuatTokenId.Number:
                    return HighlightingStyles[NumberStyleString];

                case LuatTokenId.String:
                    return HighlightingStyles[StringDefaultStyleString];

                default:
                    if ((token.ID > LuatTokenId.KeywordStart) && (token.ID < LuatTokenId.KeywordEnd))
                        return HighlightingStyles[ReservedWordStyleString];

                return token.LexicalState.DefaultHighlightingStyle;
            }
        }

        /// <summary>
        /// Gets the token string representation for the specified token ID.
        /// </summary>
        /// <param name="tokenId">The ID of the token to examine.</param>
        /// <returns>The token string representation for the specified token ID.</returns>
        public override string GetTokenString(int tokenId)
        {
            if ((tokenId > LuatTokenId.KeywordStart) && (tokenId < LuatTokenId.KeywordEnd))
                return LuatTokenId.GetTokenKey(tokenId).ToLower();

            switch (tokenId)
            {
                // General
                case LuatTokenId.DocumentEnd:
                    return "Document end";
                case LuatTokenId.Whitespace:
                    return "Whitespace";
                case LuatTokenId.LineTerminator:
                    return "Line terminator";
                case LuatTokenId.SingleLineComment:
                    return "Single-line comment";
                case LuatTokenId.MultiLineComment:
                    return "Multi-line comment";
                case LuatTokenId.Number:
                    return "Number";
                case LuatTokenId.Identifier:
                    return "Identifier";
                // Operators
                case LuatTokenId.OpenCurlyBrace:
                    return "{";
                case LuatTokenId.CloseCurlyBrace:
                    return "}";
                case LuatTokenId.OpenParenthesis:
                    return "(";
                case LuatTokenId.CloseParenthesis:
                    return ")";
                case LuatTokenId.OpenSquareBracket:
                    return "[";
                case LuatTokenId.CloseSquareBracket:
                    return "]";
                case LuatTokenId.Comma:
                    return ",";
                case LuatTokenId.SemiColon:
                    return ";";
                case LuatTokenId.Addition:
                    return "+";
                case LuatTokenId.Subtraction:
                    return "-";
                case LuatTokenId.Multiplication:
                    return "*";
                case LuatTokenId.Division:
                    return "/";
                case LuatTokenId.Assignment:
                    return "=";
                case LuatTokenId.Equality:
                    return "==";
                case LuatTokenId.Inequality:
                    return "!=";
            }

            return null;
        }

        /// <summary>
        /// Performs an auto-complete if the <see cref="SyntaxEditor"/> context with which the IntelliPrompt member list is initialized causes a single selection.
        /// Otherwise, displays a member list in the <see cref="SyntaxEditor"/>.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will display the IntelliPrompt member list.</param>
        /// <returns>
        /// <c>true</c> if an auto-complete occurred or if an IntelliPrompt member list is displayed; otherwise, <c>false</c>.
        /// </returns>
        public override bool IntelliPromptCompleteWord(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            // Triggered by Ctrl-Space
            return ShowIntelliPromptMemberList(syntaxEditor, true);
        }

        /// <summary>
        /// Gets whether IntelliPrompt member list features are supported by the language.
        /// </summary>
        /// <value>
        /// <c>true</c> if IntelliPrompt member list features are supported by the language; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// If this property is <c>true</c>, then the <see cref="IntelliPromptCompleteWord"/> and <see cref="ShowIntelliPromptMemberList"/> methods may be used.
        /// </remarks>
        public override bool IntelliPromptMemberListSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether IntelliPrompt parameter info features are supported by the language.
        /// </summary>
        /// <value>
        /// <c>true</c> if IntelliPrompt parameter info features are supported by the language; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// If this property is <c>true</c>, then the <see cref="ShowIntelliPromptParameterInfo"/> method may be used.
        /// </remarks>
        public override bool IntelliPromptParameterInfoSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether IntelliPrompt quick info features are supported by the language.
        /// </summary>
        /// <value>
        /// <c>true</c> if IntelliPrompt quick info features are supported by the language; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// If this property is <c>true</c>, then the <see cref="ShowIntelliPromptQuickInfo"/> method may be used.
        /// </remarks>
        public override bool IntelliPromptQuickInfoSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Resets the <see cref="SyntaxLanguage.LineCommentDelimiter"/> property to its default value.
        /// </summary>
        public override void ResetLineCommentDelimiter()
        {
            LineCommentDelimiter = "//";
        }

        /// <summary>
        /// Indicates whether the <see cref="SyntaxLanguage.LineCommentDelimiter"/> property should be persisted.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the property value has changed from its default; otherwise, <c>false</c>.
        /// </returns>
        public override bool ShouldSerializeLineCommentDelimiter()
        {
            return (LineCommentDelimiter != "//");
        }

        public void ProcessSyntaxWarnings(Document document)
        {
            // Remove any pending ProcessSyntaxWarnings tasks for this document
            object key = new KeyValuePair<Document, object>(document, m_processSyntaxWarningsKey);
            m_plugin.TaskQueue.RemoveTasks(key);

            // Add a new ProcessSyntaxWarnings tasks for this document
            m_plugin.TaskQueue.AddTask(() => DoProcessSyntaxWarnings(document), TaskQueue.Thread.UI, key);
        }

        /// <summary>
        /// Performs automatic outlining over the specified <see cref="TextRange"/> of the <see cref="Document"/>.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to examine.</param>
        /// <param name="parseTextRange">A <see cref="TextRange"/> indicating the offset range to parse.</param>
        /// <returns>A <see cref="TextRange"/> containing the offset range that was modified by outlining.</returns>
        public override TextRange PerformAutomaticOutlining(Document document, TextRange parseTextRange)
        {
            // If there is another pending semantic parser request (probably due to typing), assume that the existing outlining structure 
            //   in the document is more up-to-date and wait until the final request comes through before updating the outlining again
            if (!m_plugin.TaskQueue.HasTasks(document))
                return new CollapsibleNodeOutliningParser().UpdateOutlining(document, parseTextRange, document.SemanticParseData as CompilationUnit);
            
            return TextRange.Deleted;
        }

        /// <summary>
        /// Semantically parses the specified <see cref="TextRange"/> of the <see cref="Document"/>.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to examine.</param>
        /// <param name="parseTextRange">A <see cref="TextRange"/> indicating the offset range to parse.</param>
        /// <param name="flags">A <see cref="SemanticParseFlags"/> that indicates semantic parse flags.</param>
        public override void PerformSemanticParse(Document document, TextRange parseTextRange, SemanticParseFlags flags)
        {
            if (null == document.Filename)
            {
                throw new Exception("Document Filename must be set");
            }

            string source = document.GetCoreTextBuffer().ToString();
            string filename = System.IO.Path.GetFullPath(document.Filename); // Normalise the path;

            // Remove any pending SemanticParse tasks for this document
            object key = new KeyValuePair<Document, object>(document, m_performSemanticParseKey);
            m_plugin.TaskQueue.RemoveTasks(key);

            // Queue a SemanticParse task for this document
            m_plugin.TaskQueue.AddTask(() =>
            {
                var textBufferReader = new StringTextBufferReader(source, 0, 0);
                var cu = MergableLexicalParserManager.PerformSemanticParse(this, textBufferReader, filename) as CompilationUnit;
                if (null != cu)
                {
                    cu.SourceKey = filename;
                    cu.Source = source;
                    m_plugin.Database.ProcessSynchronous(cu);
                    document.SemanticParseData = cu;
                }
            }, TaskQueue.Thread.Worker, key);

        }

        /// <summary>
        /// Displays an IntelliPrompt member list in a <see cref="SyntaxEditor"/> based on the current context.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will display the IntelliPrompt member list.</param>
        /// <returns>
        /// <c>true</c> if an IntelliPrompt member list is displayed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Only call this method if the <see cref="IntelliPromptMemberListSupported"/> property is set to <c>true</c>.
        /// </remarks>
        public override bool ShowIntelliPromptMemberList(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            return ShowIntelliPromptMemberList(syntaxEditor, false);
        }

        /// <summary>
        /// Displays IntelliPrompt parameter info in a <see cref="SyntaxEditor"/> based on the current context.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will display the IntelliPrompt parameter info.</param>
        /// <returns>
        /// <c>true</c> if IntelliPrompt parameter info is displayed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Only call this method if the <see cref="IntelliPromptParameterInfoSupported"/> property is set to <c>true</c>.
        /// </remarks>
        public override bool ShowIntelliPromptParameterInfo(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            // Queue a task to display the parameter info prompt
            m_plugin.TaskQueue.AddTask(() => ShowIntelliPromptParameterInfoImmediate(syntaxEditor), TaskQueue.Thread.UI);

            return false;
        }

        /// <summary>
        /// Displays IntelliPrompt quick info in a <see cref="SyntaxEditor"/> based on the current context.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will display the IntelliPrompt quick info.</param>
        /// <returns>
        /// <c>true</c> if IntelliPrompt quick info is displayed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Only call this method if the <see cref="IntelliPromptQuickInfoSupported"/> property is set to <c>true</c>.
        /// </remarks>
        public override bool ShowIntelliPromptQuickInfo(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            int offset = syntaxEditor.Caret.Offset;

            // Get the info for the context at the caret
            string quickInfo = GetQuickInfo(syntaxEditor, ref offset);

            // No info was found... try the offset right before the caret
            if (offset > 0)
            {
                offset = syntaxEditor.Caret.Offset - 1;
                quickInfo = GetQuickInfo(syntaxEditor, ref offset);
            }

            // Show the quick info if there is any
            if (quickInfo != null)
            {
                syntaxEditor.IntelliPrompt.QuickInfo.Show(offset, quickInfo);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="IMergableLexicalParser"/> that can be used for lexical parsing of the language.
        /// </summary>
        /// <value>The <see cref="IMergableLexicalParser"/> that can be used for lexical parsing of the language.</value>
        protected override IMergableLexicalParser MergableLexicalParser
        {
            get { return m_lexicalParser; }
        }

        /// <summary>
        /// Semantically parses the text in the <see cref="MergableLexicalParserManager"/>.
        /// </summary>
        /// <param name="manager">The <see cref="MergableLexicalParserManager"/> that is managing the mergable language and the text to parse.</param>
        /// <returns>An object that contains the results of the semantic parsing operation.</returns>
        protected override object PerformSemanticParse(MergableLexicalParserManager manager)
        {
            var lexicalParser = new LuatRecursiveDescentLexicalParser(this, manager);
            lexicalParser.InitializeTokens();

            var semanticParser = new LuatSemanticParser(lexicalParser);
            semanticParser.Parse();

            return semanticParser.CompilationUnit;
        }

        /// <summary>
        /// Called when the compilation unit is assigned to the document
        /// </summary>
        /// <param name="document"></param>
        /// <param name="e"></param>
        protected override void OnDocumentSemanticParseDataChanged(Document document, EventArgs e)
        {
            ProcessSyntaxWarnings(document);

            base.OnDocumentSemanticParseDataChanged(document, e);
        }

        protected override void OnSyntaxEditorIntelliPromptParameterInfoParameterIndexChanged(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, EventArgs e)
        {
            IntelliPromptParameterInfo pi = syntaxEditor.IntelliPrompt.ParameterInfo;

            if (UpdateParameterInfoText(pi))
            {
                syntaxEditor.IntelliPrompt.ParameterInfo.MeasureAndResize(pi.Bounds.Location);
            }
        }

        protected override void OnSyntaxEditorViewMouseDown(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, EditorViewMouseEventArgs e)
        {
            m_plugin.OnUserAction(LuaIntellisenseBroker.UserAction.MovedCaret, syntaxEditor);

            base.OnSyntaxEditorViewMouseDown(syntaxEditor, e);
        }

        /// <summary>
        /// Occurs before a <see cref="SyntaxEditor.KeyTyped"/> event is raised 
        /// for a <see cref="SyntaxEditor"/> that has a <see cref="Document"/> using this language.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will raise the event.</param>
        /// <param name="e">An <c>KeyTypedEventArgs</c> that contains the event data.</param>
        protected override void OnSyntaxEditorKeyTyped(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, KeyTypedEventArgs e)
        {
            m_timeLastKey = DateTime.Now;

            if (e.KeyData == Keys.Up ||
                 e.KeyData == Keys.Down ||
                 e.KeyData == Keys.Left ||
                 e.KeyData == Keys.Right ||
                 e.KeyData == Keys.PageUp ||
                 e.KeyData == Keys.PageDown ||
                 e.KeyData == Keys.End ||
                 e.KeyData == Keys.Home)
            {
                m_plugin.OnUserAction(LuaIntellisenseBroker.UserAction.MovedCaret, syntaxEditor);
            }
            else if (e.KeyChar != 0 ||
                      e.KeyData == Keys.Delete ||
                      e.KeyData == Keys.Enter ||
                      e.KeyData == Keys.Back)
            {
                m_plugin.OnUserAction(LuaIntellisenseBroker.UserAction.Typed, syntaxEditor);
            }

            // Prevent overzealous auto completion
            if (0 != e.KeyChar && false == Char.IsLetterOrDigit(e.KeyChar))
            {
                syntaxEditor.IntelliPrompt.MemberList.Abort();
            }

            switch (e.KeyChar)
            {
                default:
                    if (Char.IsLetter(e.KeyChar))
                    {
                        ShowIntelliPromptMemberList(syntaxEditor);
                    }
                    break;
                case '.':
                case ':':
                    // Show the parameter info for code
                    ShowIntelliPromptMemberList(syntaxEditor);
                    break;
                case '(':
                    ShowIntelliPromptParameterInfo(syntaxEditor);
                    break;
                case ')':
                    ShowIntelliPromptParameterInfo(syntaxEditor);
                    break;
                case ',':
                    if ((!syntaxEditor.IntelliPrompt.ParameterInfo.Visible) && (syntaxEditor.SelectedView.GetCurrentToken().LexicalState == LexicalStates["DefaultState"]))
                    {
                        // Show the parameter info for the context level if parameter info is not already displayed
                        ShowIntelliPromptParameterInfo(syntaxEditor);
                    }
                    break;
            }
        }

        /// <summary>
        /// Occurs when the mouse is hovered over an <see cref="EditorView"/>.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will raise the event.</param>
        /// <param name="e">An <c>EditorViewMouseEventArgs</c> that contains the event data.</param>
        protected override void OnSyntaxEditorViewMouseHover(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, EditorViewMouseEventArgs e)
        {
            if ((e.HitTestResult.Token == null) || (e.ToolTipText != null))
                return;

            int offset = e.HitTestResult.Token.StartOffset;
            e.ToolTipText = GetQuickInfo(syntaxEditor, ref offset);
        }

        protected override void OnSyntaxEditorSelectionChanged(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, SelectionEventArgs e)
        {
            Document document = syntaxEditor.Document;

            SpanIndicatorLayer referenceLayer = document.SpanIndicatorLayers[ReferenceLayerId];
            if (referenceLayer == null)
            {
                referenceLayer = new SpanIndicatorLayer(ReferenceLayerId, ReferenceLayerPriority);
                document.SpanIndicatorLayers.Add(referenceLayer);
            }

            SpanIndicatorLayer assignmentLayer = document.SpanIndicatorLayers[AssignmentLayerId];
            if (assignmentLayer == null)
            {
                assignmentLayer = new SpanIndicatorLayer(AssignmentLayerId, AssignmentLayerPriority);
                document.SpanIndicatorLayers.Add(assignmentLayer);
            }

            m_plugin.TaskQueue.AddTask(() =>
            {
                referenceLayer.Clear();
                assignmentLayer.Clear();

                var cu = document.SemanticParseData as CompilationUnit;
                if (cu == null)
                    return;

                var expression = cu.FindNodeRecursive<Expression>(e.Selection.StartOffset);
                if (expression == null)
                    return;

                string path = System.IO.Path.GetFullPath(document.Filename);

                foreach (LuatValue value in expression.ResolvedValues.Values)
                {
                    var variable = value as LuatVariable;
                    if (null != variable)
                    {
                        foreach (LuatValue.IReference reference in value.References)
                        {
                            if (path == System.IO.Path.GetFullPath(reference.Path))
                            {
                                referenceLayer.Add(new HighlightingStyleSpanIndicator(null, ReferenceStyle), ((SyntaxEditorTextRange)reference.TextRange).ToTextRange(), false);
                            }
                        }
                        foreach (LuatValue.IReference assignment in variable.Assignments)
                        {
                            if (path == System.IO.Path.GetFullPath(assignment.Path))
                            {
                                assignmentLayer.Add(new HighlightingStyleSpanIndicator(null, AssignmentStyle), ((SyntaxEditorTextRange)assignment.TextRange).ToTextRange(), false);
                            }
                        }
                    }
                }
            }, TaskQueue.Thread.Worker);

            base.OnSyntaxEditorSelectionChanged(syntaxEditor, e);
        }

        /// <summary>
        /// Processes the syntax warnings for the document
        /// </summary>
        /// <param name="document"></param>
        private void DoProcessSyntaxWarnings(Document document)
        {
            // This function is expensive for the UI thread.
            // Only do this when the user is not bashing away at the keyboard
            TimeSpan time = DateTime.Now - m_timeLastKey;
            if (time.TotalSeconds < MinTimeAfterKeyBeforeRedraw)
            {
                // Too soon after the last key-press.
                // Queue up this task for later processing.
                ProcessSyntaxWarnings(document);
                return;
            }

            var cu = document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return;

            // Ensure that a syntax error warningLayer is created...
            SpanIndicatorLayer warningLayer = document.SpanIndicatorLayers[WarningLayerId];
            if (warningLayer == null)
            {
                warningLayer = new SpanIndicatorLayer(WarningLayerId, WarningLayerPriority);
                document.SpanIndicatorLayers.Add(warningLayer);
            }

            warningLayer.Clear();

            var warnings = new List<LuatWarning>(cu.Warnings);
            warnings.Sort();

            // Merge overlapping warnings spans
            int count = warnings.Count;
            if (count > 0)
            {
                var spanWarnings = new List<LuatWarning>();
                int start = 0, end = 0;

                foreach (LuatWarning warning in warnings)
                {
                    if (warning.TextRange.StartOffset < end)
                    {
                        // Overlap.
                        end = Math.Max(end, warning.TextRange.StartOffset);
                    }
                    else
                    {
                        // No overlap.
                        // Flush existing spans
                        if (spanWarnings.Count > 0)
                        {
                            warningLayer.Add(new LuatWarningSpanIndicator(WarningStyle, spanWarnings.ToArray()), new TextRange(start, end), false);
                        }

                        // Create new span
                        spanWarnings.Clear();
                        start = warning.TextRange.StartOffset;
                        end = warning.TextRange.EndOffset;
                    }

                    spanWarnings.Add(warning);
                }

                var textRange = new TextRange(start, end);
                warningLayer.Add(new LuatWarningSpanIndicator(WarningStyle, spanWarnings.ToArray()), textRange, false);
            }
        }

        /// <summary>
        /// Returns the quick info for the <see cref="SyntaxEditor"/> at the specified offset.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> to examine.</param>
        /// <param name="offset">The offset to examine.  The offset is updated to the start of the context.</param>
        /// <returns>The quick info for the <see cref="SyntaxEditor"/> at the specified offset.</returns>
        private string GetQuickInfo(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, ref int offset)
        {
            Document document = syntaxEditor.Document;

            // Get the identifier at the offset, if any
            TextStream stream = syntaxEditor.Document.GetTextStream(offset);
            if (!stream.IsAtTokenStart)
                stream.GoToCurrentTokenStart();
            offset = stream.Offset;

            // Check to see if we're over a warning
            SpanIndicatorLayer warningLayer = document.SpanIndicatorLayers[WarningLayerId];
            if (warningLayer != null)
            {
                var sb = new StringBuilder();

                var range = new TextRange(offset, offset + 1);
                SpanIndicator[] spans = warningLayer.GetIndicatorsForTextRange(range);
                foreach (LuatWarningSpanIndicator span in spans)
                {
                    LuatWarning[] warnings = span.Warnings.Filter(warning => warning.TextRange.OverlapsWith(range));

                    var groupedWarnings = warnings.GroupItems(a => a.Message, a => a.Script);

                    foreach (KeyValuePair<string, LuatScript[]> groupedWarning in groupedWarnings)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("<br /><br />");
                        }

                        sb.Append("Context: ");
                        sb.Append("<b>");
                        sb.Append(groupedWarning.Value.ToCommaSeperatedList(a => a.Name));
                        sb.Append("</b>");
                        sb.Append("<br />");

                        sb.Append(groupedWarning.Key);
                    }
                }

                if (sb.Length > 0)
                {
                    return sb.ToString();
                }
            }

            // Get the containing node
            var cu = syntaxEditor.Document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return null;

            var qi = cu.FindNodeRecursive<IQuickInfoProvider>(stream.Offset);
            if (qi == null)
                return null;

            return FormatText(qi.QuickInfo);
        }

        /// <summary>
        /// Gets the formatted text to display in quick info for the specified function call
        /// </summary>
        private string GetQuickInfoForFunctionCall(LuatValue func, int parameterIndex)
        {
            var functionType = func.Type as LuatTypeFunction;

            var sb = new StringBuilder();
            sb.Append("<img src=\"resource:PublicMethod\" align=\"absbottom\"/> ");
            sb.Append("<span style=\"color: blue;\">function</span> ");
            // sb.Append(String.Format("<span style=\"color: teal;\">{0}</span>", IntelliPrompt.EscapeMarkupText(functionDeclaration.Name.Text)));
            sb.Append("(");
            string[] parameters = functionType == null ? new string[0] : functionType.Arguments;
            for (int index = 0; index < parameters.Length; index++)
            {
                if (index > 0)
                    sb.Append(", ");
                if (parameterIndex == index)
                    sb.Append("<b>");
                sb.Append(IntelliPrompt.EscapeMarkupText(parameters[index]));
                if (parameterIndex == index)
                    sb.Append("</b>");
            }
            sb.Append(")<br/>");
            sb.Append(FormatText(func.Description));
            return sb.ToString();
        }

        /// <summary>
        /// Provides the core functionality to show an IntelliPrompt member list based on the current context in a <see cref="SyntaxEditor"/>.
        /// </summary>
        /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> that will display the IntelliPrompt member list.</param>
        /// <param name="completeWord">Whether to complete the word.</param>
        /// <returns>
        /// <c>true</c> if an auto-complete occurred or if an IntelliPrompt member list is displayed; otherwise, <c>false</c>.
        /// </returns>
        private bool ShowIntelliPromptMemberList(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, bool completeWord)
        {
            // Remove any pending tasks to display the IntelliPrompt member list
            m_plugin.TaskQueue.RemoveTasks(m_memberListKey);

            // Add a new tasks
            m_plugin.TaskQueue.AddTask(() =>
            {
                MemberList list = BuildMemberList(syntaxEditor);
                if (null != list)
                {
                    // Queue a task on the UI thread to display the member list
                    m_plugin.TaskQueue.AddTask(() => PresentMemberList(list, syntaxEditor, completeWord), TaskQueue.Thread.UI);
                }
            }, TaskQueue.Thread.Worker, m_memberListKey);

            return false;
        }

        /// <summary>
        /// Presents the parameter info prompt to the user
        /// </summary>
        /// <param name="syntaxEditor"></param>
        /// <returns></returns>
        private bool ShowIntelliPromptParameterInfoImmediate(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            // Initialize the parameter info
            syntaxEditor.IntelliPrompt.ParameterInfo.Hide();
            syntaxEditor.IntelliPrompt.ParameterInfo.Info.Clear();
            syntaxEditor.IntelliPrompt.ParameterInfo.SelectedIndex = 0;

            // Get the compilation unit
            var cu = syntaxEditor.Document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return false;

            // Move back to the last open bracket
            int caret = syntaxEditor.Caret.Offset;
            TextStream stream = syntaxEditor.Document.GetTextStream(caret);
            stream.GoToPreviousTokenWithID(LuatTokenId.OpenParenthesis);

            // Find the argument list the caret is within
            var arguments = cu.FindNodeRecursive<ArgumentList>(stream.Offset);
            ArgumentList next = arguments;
            while (null != next && false == arguments.InsideBrackets(caret))
            {
                arguments = next;
                next = next.FindAncestor<ArgumentList>();
            }

            if (arguments == null)
                return false;

            var call = arguments.ParentNode as FunctionCall;
            if (call == null)
                throw new Exception("ArgumentList does not have FunctionCall as parent");

            // Configure the parameter info
            var textRange = new TextRange(arguments.ListTextRange.StartOffset, arguments.ListTextRange.EndOffset + 1);

            IntelliPromptParameterInfo pi = syntaxEditor.IntelliPrompt.ParameterInfo;
            pi.ValidTextRange = textRange;
            pi.CloseDelimiterCharacter = ')';
            pi.UpdateParameterIndex();
            pi.HideOnParentFormDeactivate = true;

            var functions = new List<LuatValue>();
            foreach (var value in call.ResolvedFunctions)
            {
                if (false == value.Type is LuatTypeFunction)
                    continue;

                functions.Merge(value);

                pi.Info.Add(GetQuickInfoForFunctionCall(value, pi.ParameterIndex));
            }

            // Store the function types in the context
            pi.Context = functions.ToArray();

            // Show the parameter info
            pi.Show(caret);

            return false;
        }

        /// <summary>
        /// Builds a MemberList for the given syntaxEditor
        /// </summary>
        /// <param name="syntaxEditor"></param>
        /// <returns></returns>
        private MemberList BuildMemberList(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor)
        {
            // Get the target text range
            int caret = syntaxEditor.Caret.Offset;
            TextRange targetTextRange = TextRange.Deleted;
            TextStream stream = syntaxEditor.Document.GetTextStream(caret);

            // Get the compilation unit
            var cu = syntaxEditor.Document.SemanticParseData as CompilationUnit;
            if (cu == null)
                return null;

            var itemlist = new Hashtable();

            stream.GoToPreviousNonWhitespaceToken();

            if (stream.Token.IsComment)
            {
                return null;
            }

            var node = cu.FindNodeRecursive<LuatAstNodeBase>(stream.Offset);
            if (null != node)
            {
                foreach (AutoCompleteItem item in m_plugin.Database.GetAutoCompleteList(node, caret))
                {
                    itemlist[item.Name] = new IntelliPromptMemberListItem(item.Name, (int)item.Icon, item.Description);
                }

                targetTextRange = node.GetAutoCompleteTextRange(caret);
            }

            if (itemlist.Count == 0)
            {
                return null;
            }

            var memberlist = new MemberList { List = new IntelliPromptMemberListItem[itemlist.Count] };
            itemlist.Values.CopyTo(memberlist.List, 0);
            memberlist.TargetTextRange = targetTextRange;
            return memberlist;
        }

        /// <summary>
        /// Presents the member list to the user
        /// </summary>
        /// <param name="memberList"></param>
        /// <param name="syntaxEditor"></param>
        /// <param name="completeWord"></param>
        private void PresentMemberList(MemberList memberList, ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, bool completeWord)
        {
            IntelliPromptMemberList prompt = syntaxEditor.IntelliPrompt.MemberList;
            if (prompt.Visible)
                return;

            // Initialize the member list
            prompt.ResetAllowedCharacters();
            prompt.AllowedCharacters.Add(new CharInterval(char.MinValue, char.MaxValue));
            prompt.Clear();
            prompt.ImageList = LuaIntellisenseIcons.GetImageList();
            prompt.AddRange(memberList.List);

            // Show the list
            if (memberList.TargetTextRange.IsDeleted)
                prompt.Show();
            else if (completeWord && memberList.TargetTextRange.Length > 0)
                prompt.CompleteWord(memberList.TargetTextRange.StartOffset, memberList.TargetTextRange.Length);
            else
                prompt.Show(memberList.TargetTextRange.StartOffset, memberList.TargetTextRange.Length);
        }

        private bool UpdateParameterInfoText(IntelliPromptParameterInfo pi)
        {
            var functions = pi.Context as LuatValue[];
            if (functions != null)
                pi.Info[pi.SelectedIndex] = GetQuickInfoForFunctionCall(functions[pi.SelectedIndex], pi.ParameterIndex);
            return true;
        }

        private static string FormatText(string raw)
        {
            if (raw == null)
                return null;

            string formatted =
                raw
                    .Trim(' ', '\t', '\n', '\r')
                    .Replace("\n", "<br/>");

            return formatted;
        }

        #region Private Classes

        /// <summary>
        /// A list of members to be displayed in a IntelliPrompt member list
        /// </summary>
        private class MemberList
        {
            public IntelliPromptMemberListItem[] List { get; set; }
            public TextRange TargetTextRange { get; set; }
        }

        #endregion

        private DateTime m_timeLastKey = DateTime.MinValue;

        private readonly LuaIntellisenseBroker m_plugin;
        private readonly object m_memberListKey = new object();
        private readonly object m_performSemanticParseKey = new object();
        private readonly object m_processSyntaxWarningsKey = new object();
        private readonly LuatLexicalParser m_lexicalParser = new LuatLexicalParser();

        private const string WarningLayerId = "Warnings";
        private const string ReferenceLayerId = "Reference";
        private const string AssignmentLayerId = "Assignment";

        private const float MinTimeAfterKeyBeforeRedraw = 1.0f;

        private const int WarningLayerPriority = 100;
        private const int ReferenceLayerPriority = 10;
        private const int AssignmentLayerPriority = 11;

        // LuaDefinition.xml related
        private const string ReservedWordStyleString = "ReservedWordStyle";
        private const string FunctionStyleString = "FunctionStyle";
        private const string GlobalVariableStyleString = "GlobalVariableStyle";
        private const string OperatorStyleString = "OperatorStyle";
        private const string NumberStyleString = "NumberStyle";
        private const string StringDelimiterStyleString = "StringDelimiterStyle";
        private const string StringDefaultStyleString = "StringDefaultStyle";
        private const string CommentDelimiterStyleString = "CommentDelimiterStyle";
        private const string CommentUrlStyleString = "CommentURLStyle";
        private const string CommentTagStyleString = "CommentTagStyle";
        private const string CommentDefaultStyleString = "CommentDefaultStyle";

        // Custom
        private const string WarningStyleString = "WarningStyle";
        private const string ReferenceStyleString = "ReferenceStyle";
        private const string AssignmentStyleString = "AssignmentStyle";
        
        private HighlightingStyle WarningStyle { get { return HighlightingStyles[WarningStyleString]; } }
        private HighlightingStyle ReferenceStyle { get { return HighlightingStyles[ReferenceStyleString]; } }
        private HighlightingStyle AssignmentStyle { get { return HighlightingStyles[AssignmentStyleString]; } }
    }

    interface IQuickInfoProvider
    {
        string QuickInfo { get; }
    }

    /// <summary>
    /// A single item for the auto-complete list
    /// </summary>
    struct AutoCompleteItem
    {
        public AutoCompleteItem(string name, string description, LuaIntellisenseIconType icon)
        {
            Name = name;
            Description = description;
            Icon = icon;
        }

        public AutoCompleteItem(string name, LuatValue value)
        {
            Name = name;
            Description = value.Description;
            Icon = value.Type.Icon;
        }

        public string Name;
        public string Description;
        public LuaIntellisenseIconType Icon;
    }
}