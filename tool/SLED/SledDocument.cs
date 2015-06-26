/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    /// <summary>
    /// Class representing an editable file
    /// </summary>
    public class SledDocument : ISledDocument
    {
        public static SledDocument CreateHidden(Uri uri, ISledDocumentClient client)
        {
            return new SledDocument(uri, client, true);
        }

        public static SledDocument Create(Uri uri, ISledDocumentClient client)
        {
            return new SledDocument(uri, client, false);
        }

        private SledDocument(Uri uri, ISledDocumentClient client, bool bHidden)
        {
            m_uri = uri;
            m_type = client == null ? "SledDocument" : client.Info.FileType;
            m_bHiddenDoc = bHidden;

            var filePath = uri.LocalPath;
            var fileName = Path.GetFileName(filePath);
            var ext = Path.GetExtension(filePath);

            // Set up language plugin (if any)
            LanguagePlugin = null;
            if (!m_bHiddenDoc)
            {
                if (s_languagePluginService.Get != null)
                    LanguagePlugin = s_languagePluginService.Get.GetPluginForExtension(ext);
            }

            // Create new syntax editor
            m_editor = TextEditorFactory.CreateSyntaxHighlightingEditor();
            m_editor.Control.Tag = this;
            m_editor.Control.Name = fileName;

            // Grab reference to image
            if (s_imgLock == null)
                s_imgLock = GetLockImage();

            // Check if readonly
            if (!m_bHiddenDoc)
            {
                m_bReadOnly = IsReadOnly;

                // Create hosting control
                m_control =
                    new SledDocumentControl(m_editor, this, client == null ? null : client.EmbeddedTypes)
                        {
                            Name = fileName,
                            Tag = this
                        };
            }

            // Create control info
            m_controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center, !m_bReadOnly ? null : s_imgLock);
            if (!m_bHiddenDoc)
                m_controlInfo.IsDocument = true;

            // Using a custom context menu, so disable the default
            m_editor.DefaultContextMenuEnabled = false;
            m_editor.EditorTextChanged += EditorEditorTextChanged;
            m_editor.KeyPress += EditorKeyPress;
            m_editor.ShowContextMenu += EditorShowContextMenu;
            m_editor.MouseHoveringOverToken += EditorMouseHoveringOverToken;

            if (client == null)
                return;

            // Set syntax highlighting for document
            SledDocumentSyntaxHighlighter.FeedHighlighterToSyntaxEditor(client.SyntaxHighlighter, m_editor);
        }

        #region IDisposable Interface

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (m_control != null)
                m_control.Dispose();

            if (m_editor != null)
                m_editor.Dispose();
        }

        #endregion

        #region IDocument Interface

        public bool Dirty
        {
            get { return m_bDirty; }
            set
            {
                if (value == m_bDirty)
                    return;

                m_bDirty = value;

                // ReSharper disable RedundantCheckBeforeAssignment
                if (value != m_editor.Dirty)
                    m_editor.Dirty = value;
                // ReSharper restore RedundantCheckBeforeAssignment

                UpdateControlInfo();

                DirtyChanged.Raise(this, EventArgs.Empty);
            }
        }

        public bool IsReadOnly
        {
            get { return (m_editor.ReadOnly) || m_bReadOnly || SledUtil.IsFileReadOnly(Uri.LocalPath); }

            internal set
            {
                // Actual read only status depends on if the file is read only or not
                m_bReadOnly = (value || SledUtil.IsFileReadOnly(Uri.LocalPath));

                // Keep SyntaxEditor up to date with actual read only status
                m_editor.ReadOnly = m_bReadOnly;

                // This should take care of the icon being added/removed from the tab
                UpdateControlInfo();
            }
        }

        public event EventHandler DirtyChanged;

        #endregion

        #region IResource Interface

        public string Type
        {
            get { return m_type; }
        }

        public Uri Uri
        {
            get { return m_uri; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value == m_uri)
                    return;

                var oldUri = m_uri;
                m_uri = value;

                UpdateControlInfo();

                UriChanged.Raise(this, new UriChangedEventArgs(oldUri));
            }
        }

        public event EventHandler<UriChangedEventArgs> UriChanged;

        #endregion

        #region IInstancingContext Interface

        public bool CanInsert(object data)
        {
            var dataObject = data.As<IDataObject>();
            if (dataObject == null)
                return false;

            return
                (dataObject.GetDataPresent(DataFormats.Text) ||
                dataObject.GetDataPresent(DataFormats.UnicodeText));
        }

        public void Insert(object data)
        {
            if (IsReadOnly)
            {
                if (!CanGetSourceControlToCheckOut(this))
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Document \"{1}\" is readonly",
                        this, Uri.LocalPath);

                    return;
                }

                // Override for the paste operation
                Editor.ReadOnly = false;
            }

            // Windows _system clipboard_ must contain 'data' first
            Editor.Paste();
        }

        public bool CanCopy()
        {
            return Editor.HasSelection;
        }

        public object Copy()
        {
            var selectedText = Editor.Selection;

            // Since our Insert() method does not use the data object,
            // but instead requires the Windows system clipboard to be
            // set, we need to set the system clipboard ourselves
            // if StandardEditCommands does not.
            try
            {
                if (!StandardEditCommands.UseSystemClipboard)
                    Clipboard.SetText(selectedText);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Clipboard error copying text from \"{1}\": {2}",
                    this, Uri.LocalPath, ex.Message);
            }

            var data = new DataObject(selectedText);
            return data;
        }

        public bool CanDelete()
        {
            return Editor.HasSelection;
        }

        public void Delete()
        {
            if (IsReadOnly)
            {
                if (!CanGetSourceControlToCheckOut(this))
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Document \"{1}\" is readonly",
                        this, Uri.LocalPath);

                    return;
                }

                // Override for the delete operation
                Editor.ReadOnly = false;
            }

            Editor.Delete();
        }

        #endregion

        #region ISledDocument Interface

        public ISyntaxEditorControl Editor
        {
            get { return m_editor; }
        }

        /// <summary>
        /// If not null then this is the project file that corresponds to this SledDocument
        /// </summary>
        public SledProjectFilesFileType SledProjectFile { get; set; }

        public ISledLanguagePlugin LanguagePlugin { get; set; }

        public Control Control
        {
            get { return m_control; }
        }

        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        public bool HasSelection
        {
            get { return !string.IsNullOrEmpty(m_editor.Selection); }
        }

        public string Selection
        {
            get { return !HasSelection ? string.Empty : m_editor.Selection; }
        }

        public void Read()
        {
            try
            {
                m_bLoading = true;

                var filePath = m_uri.LocalPath;
                if (File.Exists(filePath))
                {
                    // Store whether file has byte order mark
                    m_bHasBom = SledUtil.FileHasBom(filePath);

                    using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new StreamReader(stream, true))
                        {
                            // Store encoding
                            m_encoding = reader.CurrentEncoding;

                            // Read file and update editor
                            m_editor.Text = reader.ReadToEnd();
                            m_editor.Dirty = false;
                        }
                    }
                }
            }
            finally
            {
                m_bLoading = false;
            }
        }

        public void Write()
        {
            var filePath = m_uri.LocalPath;

            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                // Some UTF8 files might not have a BOM so we don't want to
                // forcibly create one and potentially mess up the file for
                // the user therefore we'll use the default constructor of 
                // stream writer to write UTF8 w/o a BOM and for everything
                // else use the encoding the stream reader detected when the
                // file was initially opened.
                StreamWriter writer;

                if ((m_encoding == Encoding.UTF8) && !m_bHasBom)
                    writer = new StreamWriter(stream);
                else
                    writer = new StreamWriter(stream, m_encoding);

                using (writer)
                {
                    writer.Write(m_editor.Text);
                    m_editor.Dirty = false;
                }
            }
        }

        public bool IsValidLine(int lineNumber)
        {
            return ((lineNumber >= 1) && (lineNumber <= m_editor.DocumentLineCount));
        }

        public bool IsBreakpointSet(int lineNumber)
        {
            if (IsValidLine(lineNumber))
            {
                var bp = m_editor.GetBreakpoint(lineNumber);
                if (bp != null)
                    return true;
            }

            return false;
        }

        public bool IsBreakpointEnabled(int lineNumber)
        {
            if (IsValidLine(lineNumber))
            {
                var bp = m_editor.GetBreakpoint(lineNumber);
                if (bp != null)
                    return bp.Enabled;
            }

            return false;
        }

        public event EventHandler<SledDocumentLineCountChangedArgs> DocumentLineCountChanged;

        #endregion

        #region Private Classes

        private sealed class SledDocumentControl : UserControl
        {
            public SledDocumentControl(ISyntaxEditorControl editor, ISledDocument document, IEnumerable<SledDocumentEmbeddedTypeInfo> embeddedTypes)
            {
                Dock = DockStyle.Fill;

                m_syntaxEditorControl = editor;
                m_document = document;
                
                var x = 0;
                var y = 0;
                var width = Width;
                var height = Height;

                // Add any embedded types
                AddEmbeddedTypes(
                    embeddedTypes,
                    this,
                    m_document,
                    ref x,
                    ref y,
                    ref width,
                    ref height,
                    m_lstEmbeddedTypes);

                // Set up syntax editor
                m_syntaxEditorControl = editor;
                m_syntaxEditorControl.Control.Location = new Point(x, y);
                m_syntaxEditorControl.Control.Width = width;
                m_syntaxEditorControl.Control.Height = height;
                m_syntaxEditorControl.Control.Anchor =
                    AnchorStyles.Left | AnchorStyles.Right |
                    AnchorStyles.Top | AnchorStyles.Bottom;
                
                // Add syntax editor
                Controls.Add(m_syntaxEditorControl.Control);
            }

            public void Shown()
            {
                if (m_bShown)
                    return;

                try
                {
                    foreach (var embeddedType in m_lstEmbeddedTypes)
                        embeddedType.Shown();
                }
                finally
                {
                    m_bShown = true;
                }
            }

            public void Closing()
            {
                foreach (var embeddedType in m_lstEmbeddedTypes)
                    embeddedType.Closing();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (var cntrl in m_lstEmbeddedTypes.Cast<Control>())
                        cntrl.Dispose();

                    m_lstEmbeddedTypes.Clear();
                }

                base.Dispose(disposing);
            }

            private static void AddEmbeddedTypes(
                IEnumerable<SledDocumentEmbeddedTypeInfo> embeddedTypes,
                Control parent,
                ISledDocument sd,
                ref int x,
                ref int y,
                ref int width,
                ref int height,
                ICollection<ISledDocumentEmbeddedType> lstEmbeddedTypes)
            {
                if (embeddedTypes == null)
                    return;

                // Embedded types must be assignable to
                // each of these types
                var ifaces =
                    new[]
                    {
                        typeof(Control),
                        typeof(ISledDocumentEmbeddedType)
                    };

                foreach (var embeddedType in embeddedTypes)
                {
                    var type = embeddedType;
                    if (!ifaces.All(t => t.IsAssignableFrom(type.Type)))
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            "Cannot create embedded " +
                            "SledDocument type \"{0}\"",
                            type);

                        continue;
                    }

                    try
                    {
                        // Create embedded type
                        var cntrl =
                            (Control)Activator.CreateInstance(
                                embeddedType.Type);

                        var cntrlEmbed =
                            cntrl.As<ISledDocumentEmbeddedType>();

                        // Initialize
                        cntrlEmbed.Initialize(sd);

                        // Add to list
                        lstEmbeddedTypes.Add(cntrlEmbed);

                        // Adjust positioning
                        switch (embeddedType.Position)
                        {
                            case SledDocumentEmbeddedTypePosition.Left:
                            {
                                cntrl.Location = new Point(x, y);
                                cntrl.Height = height;

                                x += cntrl.Width;
                                width -= cntrl.Width;

                                cntrl.Anchor =
                                    AnchorStyles.Left |
                                    AnchorStyles.Top |
                                    AnchorStyles.Bottom;
                            }
                            break;

                            case SledDocumentEmbeddedTypePosition.Top:
                            {
                                cntrl.Location = new Point(x, y);
                                cntrl.Width = width;

                                y += cntrl.Height;
                                height -= cntrl.Height;

                                cntrl.Anchor =
                                    AnchorStyles.Left |
                                    AnchorStyles.Top |
                                    AnchorStyles.Right;
                            }
                            break;

                            case SledDocumentEmbeddedTypePosition.Right:
                            {
                                cntrl.Location = new Point(width - cntrl.Width, y);
                                cntrl.Height = height;

                                width -= cntrl.Width;

                                cntrl.Anchor =
                                    AnchorStyles.Top |
                                    AnchorStyles.Right |
                                    AnchorStyles.Bottom;
                            }
                            break;

                            case SledDocumentEmbeddedTypePosition.Bottom:
                            {
                                cntrl.Location = new Point(x, height - cntrl.Height);
                                cntrl.Width = width;

                                height -= cntrl.Height;

                                cntrl.Anchor =
                                    AnchorStyles.Left |
                                    AnchorStyles.Right |
                                    AnchorStyles.Bottom;
                            }
                            break;
                        }

                        parent.Controls.Add(cntrl);
                    }
                    catch (Exception ex)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            "Cannot create embedded " +
                            "SledDocument type \"{0}\": {1}",
                            type, ex.Message);
                    }
                }
            }

            private bool m_bShown;

            private readonly ISledDocument m_document;
            private readonly ISyntaxEditorControl m_syntaxEditorControl;
            private readonly List<ISledDocumentEmbeddedType> m_lstEmbeddedTypes =
                new List<ISledDocumentEmbeddedType>();
        }

        private sealed class SledDocumentSourceControlContext : ISourceControlContext
        {
            public SledDocumentSourceControlContext(SledDocument sd)
            {
                m_sd = sd;
            }

            public IEnumerable<IResource> Resources
            {
                get { yield return m_sd; }
            }

            private readonly SledDocument m_sd;
        }

        #endregion

        #region Member Methods

        public static IControlHostClient ControlHostClient
        {
            get { return s_controlHostClient; }
            set { if (s_controlHostClient == null) s_controlHostClient = value; }
        }

        public List<int> SavedBreakpoints
        {
            get { return m_savedBreakpoints; }
        }

        public static bool BreakpointState
        {
            get { return s_breakpointState; }
            set { s_breakpointState = value; }
        }

        private void UpdateControlInfo()
        {
            var filePath = Uri.LocalPath;
            var fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            m_controlInfo.Name = fileName;
            m_controlInfo.Description = filePath;
            m_controlInfo.Image = !m_bReadOnly ? null : s_imgLock;
        }

        internal bool Hidden
        {
            get { return m_bHiddenDoc; }
        }

        private void EditorKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!m_editor.ReadOnly)
                return;

            if ((s_debugService.Get != null) && s_debugService.Get.IsDebugging)
            {
                MessageBox.Show(
                    s_mainForm.Get,
                    Localization.SledCannotMakeChanges,
                    Localization.SledEditorLocked,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                // Quick hack for a feature Zindagi requested
                if (CanGetSourceControlToCheckOut(this))
                    return;

                // Otherwise show normal GUI
                var res =
                    MessageBox.Show(
                        s_mainForm.Get,
                        string.Format(
                            "{0}{1}{1}{2}",
                            Localization.SledFileReadOnly,
                            Environment.NewLine,
                            Localization.SledAttemptChangeFilePermissions),
                        Localization.SledFileReadOnly,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                if (res == DialogResult.Yes)
                {
                    try
                    {
                        File.SetAttributes(Uri.LocalPath, FileAttributes.Normal);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            s_mainForm.Get,
                            string.Format(
                                "{0}{1}{1}{2}",
                                Localization.SledFailChangePermissions,
                                Environment.NewLine,
                                SledUtil.TransSub(Localization.SledErrorWas, ex.Message)),
                            Localization.SledFileReadOnlyError,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void EditorEditorTextChanged(object sender, EditorTextChangedEventArgs e)
        {
            if (m_bLoading)
                return;

            var dirty = m_editor.Dirty;
            if (dirty != m_bDirty)
                Dirty = dirty;

            // Check line count for a change
            if (m_iLineCount != m_editor.DocumentLineCount)
            {
                DocumentLineCountChanged.Raise(
                    this,
                    new SledDocumentLineCountChangedArgs(
                        m_iLineCount,
                        m_editor.DocumentLineCount));
            }

            // Update line count
            m_iLineCount = m_editor.DocumentLineCount;
        }

        private static IEnumerable<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            // Grab all SLED document plugins
            var docPlugins = SledServiceInstance.GetAll<ISledDocumentPlugin>();

            // Collect popup commands from SLED document plugins
            var commandTags = new List<object>();
            foreach (var docPlugin in docPlugins)
            {
                var lstCommandTags = docPlugin.GetPopupCommandTags(args);
                if (lstCommandTags == null)
                    continue;

                commandTags.AddRange(lstCommandTags);
                commandTags.Add(null);
            }

            if (commandTags.Count > 0)
                commandTags.RemoveAt(commandTags.Count - 1); // trim last null

            return commandTags;
        }

        private void EditorShowContextMenu(object sender, ShowContextMenuEventArg e)
        {
            var dcma =
                new SledDocumentContextMenuArgs(
                    this,
                    ToSledDocumentRegion(m_editor.GetRegion(e.MouseLocation)),
                    e.LineNumber);

            // Find objects to add to the context menu
            var commands = new List<object>(GetPopupCommandTags(dcma));

            // Point on screen that was clicked
            var screenPoint = Control.PointToScreen(e.MouseLocation);

            // Show context menu if any commands
            if (commands.Count > 0)
                s_commandService.Get.RunContextMenu(commands, screenPoint);
        }

        private void EditorMouseHoveringOverToken(object sender, MouseHoverOverTokenEventArgs e)
        {
            // Collect values to display, if any
            var toolTips = new List<string>();

            var docPlugins = SledServiceInstance.GetAll<ISledDocumentPlugin>();

            var ea = new SledDocumentHoverOverTokenArgs(this, e);

            // Gather all tooltips from plugins
            foreach (var docPlugin in docPlugins)
            {
                var lstToolTips = docPlugin.GetMouseHoverOverTokenValues(ea);

                if (lstToolTips == null)
                    continue;

                toolTips.AddRange(lstToolTips);
            }

            // If no tooltips then abort
            if (toolTips.Count <= 0)
                return;

            var bFirst = true;
            m_sbToolTip.Remove(0, m_sbToolTip.Length);

            // Concatenate uber tooltip
            foreach (var toolTip in toolTips)
            {
                if (!bFirst)
                    m_sbToolTip.Append("<br/>");

                m_sbToolTip.Append(toolTip);

                if (bFirst)
                    bFirst = false;
            }

            // Show uber tooltip
            e.TooltipText = m_sbToolTip.ToString();
        }

        private static SledDocumentRegions ToSledDocumentRegion(SyntaxEditorRegions region)
        {
            switch (region)
            {
                case SyntaxEditorRegions.IndicatorMargin:
                    return SledDocumentRegions.Breakpoint;

                case SyntaxEditorRegions.LineNumberMargin:
                    return SledDocumentRegions.LineNumber;

                case SyntaxEditorRegions.TextArea:
                    return SledDocumentRegions.TextArea;
            }

            return SledDocumentRegions.EverythingElse;
        }

        internal void RegisterControl()
        {
            // Check/set read only aspect
            var bReadOnly = IsReadOnly;
            m_editor.ReadOnly = bReadOnly;

            // Adjust image if readonly
            UpdateControlInfo();

            // Show document
            s_controlHostService.Get.RegisterControl(
                Control,
                ControlInfo,
                s_controlHostClient);

            // Notify embedded controls of being shown (only
            // fires once even-if/when RegisterControl() is
            // called multiple times)
            m_control.Shown();
        }

        internal void UnregisterControl()
        {
            m_control.Closing();
            s_controlHostService.Get.UnregisterControl(Control);
            m_editor.Dispose();
        }

        private static bool CanGetSourceControlToCheckOut(SledDocument sd)
        {
            if (sd == null)
                return false;

            var sourceControlService = SledServiceInstance.TryGet<SledSourceControlService>();
            if (sourceControlService == null)
                return false;

            var context = new SledDocumentSourceControlContext(sd);
            return
                sourceControlService.CanDoCommand(SledSourceControlCommand.CheckOut, context) &&
                sourceControlService.DoCommand(SledSourceControlCommand.CheckOut, context);
        }

        private static Image GetLockImage()
        {
            try
            {
                var image = ResourceUtil.GetImage(Atf.Resources.LockImage);
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        private Uri m_uri;
        private bool m_bDirty;

        private bool m_bHasBom;
        private bool m_bLoading;
        private int m_iLineCount;
        private bool m_bReadOnly;
        private Encoding m_encoding = Encoding.ASCII;
        private readonly SledDocumentControl m_control;

        private readonly StringBuilder m_sbToolTip =
            new StringBuilder();

        private readonly string m_type;
        private readonly bool m_bHiddenDoc;
        private readonly ControlInfo m_controlInfo;
        private readonly ISyntaxEditorControl m_editor;
        
        private static Image s_imgLock;
        private static IControlHostClient s_controlHostClient;

        private static bool s_breakpointState = true;
        private readonly List<int> m_savedBreakpoints =
            new List<int>();

        private static readonly SledServiceReference<MainForm> s_mainForm =
            new SledServiceReference<MainForm>();

        private static readonly SledServiceReference<ICommandService> s_commandService =
            new SledServiceReference<ICommandService>();

        private static readonly SledServiceReference<ISledDebugService> s_debugService =
            new SledServiceReference<ISledDebugService>();

        private static readonly SledServiceReference<IControlHostService> s_controlHostService =
            new SledServiceReference<IControlHostService>();

        private static readonly SledServiceReference<ISledLanguagePluginService> s_languagePluginService =
            new SledServiceReference<ISledLanguagePluginService>();
    }

    /// <summary>
    /// SledDocumentComparer Class
    /// </summary>
    public class SledDocumentComparer : IEqualityComparer<SledDocument>
    {
        /// <summary>
        /// See if two SledDocuments are equal
        /// </summary>
        /// <param name="sd1">document to compare</param>
        /// <param name="sd2">document to compare</param>
        /// <returns>true if documents are equal</returns>
        public bool Equals(SledDocument sd1, SledDocument sd2)
        {
            return m_comparer.Equals(sd1, sd2);
        }

        /// <summary>
        /// Get a hashcode for a SledDocument
        /// </summary>
        /// <param name="sd">document</param>
        /// <returns>hashcode of document's pathname property</returns>
        public int GetHashCode(SledDocument sd)
        {
            return m_comparer.GetHashCode(sd);
        }

        // Piggy back existing implementation
        private readonly ISledDocumentComparer m_comparer =
            new ISledDocumentComparer();
    }
}
