/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Project;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    /// <summary>
    /// SledDocumentService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IDocumentService))]
    [Export(typeof(ISledDocumentService))]
    [Export(typeof(ISledDocumentClientService))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(SledDocumentService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDocumentService : StandardFileCommands, ISledDocumentService, ISledDocumentPlugin, IInstancingContext, IHistoryContext, ISourceControlContext
    {
        [ImportingConstructor]
        public SledDocumentService(
            MainForm mainForm,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IFileDialogService fileDialogService,
            IControlHostService controlHostService)
            : base(commandService, documentRegistry, fileDialogService)
        {
            m_mainForm = mainForm;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_fileDialogService = fileDialogService;
            m_controlHostService = controlHostService;

            m_mainForm.Shown += MainFormShown;
            m_mainForm.DragOver += MainFormDragOver;
            m_mainForm.DragDrop += MainFormDragDrop;

            // Relay this event
            m_documentRegistry.ActiveDocumentChanged += DocumentRegistryActiveDocumentChanged;

            // Everything but the copious new & open
            RegisterCommands =
                CommandRegister.FileClose |
                CommandRegister.FileSave |
                CommandRegister.FileSaveAll |
                CommandRegister.FileSaveAs;

            //
            // Register default document types
            //

            m_txtDocumentClient =
                new SledDocumentClient(
                    "Text",
                    ".txt",
                    Atf.Resources.DocumentImage,
                    true,
                    new SledDocumentLanguageSyntaxHighlighter(Languages.Text));

            m_xmlDocumentClient =
                new SledDocumentClient(
                    "Xml",
                    ".xml",
                    Atf.Resources.DocumentImage,
                    new SledDocumentLanguageSyntaxHighlighter(Languages.Xml));

            m_csDocumentClient =
                new SledDocumentClient(
                    "C#",
                    ".cs",
                    Atf.Resources.DocumentImage,
                    new SledDocumentLanguageSyntaxHighlighter(Languages.Csharp));

            m_pyDocumentClient =
                new SledDocumentClient(
                    "Python",
                    ".py",
                    Atf.Resources.DocumentImage,
                    new SledDocumentLanguageSyntaxHighlighter(Languages.Python));

            // One time setup
            SledDocument.ControlHostClient = this;

            // Do not use the system clipboard until further 
            // safety measures can be added. Ron Little, 5/26/2011
            StandardEditCommands.UseSystemClipboard = false;

            // Check if any command line args
            var bFirst = true;
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                // First one is application .exe
                if (bFirst)
                {
                    bFirst = false;
                    continue;
                }

                // Skip non-files
                if (!File.Exists(arg))
                    continue;

                // Skip project files
                if (SledUtil.FileEndsWithExtension(arg, SledProjectService.ProjectExtensions))
                    continue;

                m_lstStartupFiles.Add(arg);
            }
        }
        
        #region IInitializable Interface

        protected override void Initialize()
        {
            m_commandService.RegisterCommand(
                StandardCommand.EditSelectAll,
                CommandVisibility.Menu,
                this);

            m_sledDocumentClients =
                SledServiceInstance.GetAll<ISledDocumentClient>();

            // Create a bunch of "file > new > xxx" entries
            foreach (var client in m_sledDocumentClients)
            {
                var iconName = client.Info.NewIconName;
                m_commandService.RegisterCommand(
                    new FileCommandTag(Command.New, client),
                    StandardMenu.File,
                    StandardCommandGroup.FileNew,
                    Localization.SledFileMenu + Sled.Resources.Resource.MenuSeparator + "New" + Sled.Resources.Resource.MenuSeparator + client.Info.FileType,
                    string.Format("Creates a new {0} document", client.Info.FileType),
                    Keys.None,
                    iconName,
                    CommandVisibility.Menu,
                    this);
            }

            // Create "file > open" entry
            m_commandService.RegisterCommand(
                Command.Open,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledFileMenu + Sled.Resources.Resource.MenuSeparator + Localization.SledOpen,
                Localization.SledCommandFileOpenComment,
                Keys.Control | Keys.O,
                SledIcon.FileOpen,
                CommandVisibility.Menu,
                this);

            // Register goto command
            m_commandService.RegisterCommand(
                Command.Goto,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                Localization.SledCommandGoto,
                Localization.SledCommandGotoComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(StandardCommand.WindowRemoveSplit, CommandVisibility.Menu, this);
            m_commandService.RegisterCommand(StandardCommand.WindowSplitVert, CommandVisibility.Menu, this);
            m_commandService.RegisterCommand(StandardCommand.WindowSplitHoriz, CommandVisibility.Menu, this);

            m_commandService.RegisterCommand(
                Command.CloseAllDocuments,
                StandardMenu.Window,
                StandardCommandGroup.WindowLayout,
                "Close All Documents",
                "Close all opened documents",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region IControlHostClient Interface

        public void Activate(Control control)
        {
            if (!(control.Tag is ISledDocument))
                return;

            var sd = control.Tag as ISledDocument;
            m_contextRegistry.ActiveContext = this;
            m_documentRegistry.ActiveDocument = sd;
            m_commandService.SetActiveClient(this);
        }

        public void Deactivate(Control control)
        {
            if (!(control.Tag is ISledDocument))
                return;

            m_contextRegistry.RemoveContext(this);
        }

        public bool Close(Control control)
        {
            var sd = control.Tag as ISledDocument;
            if (sd != null)
                return Close(sd);

            return true;
        }

        #endregion

        #region ICommandClient Interface

        public override bool CanDoCommand(object commandTag)
        {
            if (base.CanDoCommand(commandTag))
                return true;

            var bEnabled = false;

            if (commandTag is FileCommandTag)
            {
                bEnabled = true;
            }
            else if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditSelectAll:
                        bEnabled = Active;
                        break;

                    case StandardCommand.WindowRemoveSplit:
                        bEnabled = (Active && (ActiveDocument.Editor.VerticalSplitter || ActiveDocument.Editor.HorizontalSplitter));
                        break;

                    case StandardCommand.WindowSplitVert:
                        bEnabled = (Active && !ActiveDocument.Editor.VerticalSplitter);
                        break;

                    case StandardCommand.WindowSplitHoriz:
                        bEnabled = (Active && !ActiveDocument.Editor.HorizontalSplitter);
                        break;
                }
            }
            else if (commandTag is Command)
            {
                switch ((Command) commandTag)
                {
                    case Command.Open:
                        bEnabled = true;
                        break;

                    case Command.Goto:
                        bEnabled = Active;
                        break;

                    case Command.CloseAllDocuments:
                        bEnabled = OpenDocuments.Count > 0;
                        break;
                }
            }

            return bEnabled;
        }

        public override void DoCommand(object commandTag)
        {
            base.DoCommand(commandTag);

            if (commandTag is FileCommandTag)
            {
                var fileCommandTag = (FileCommandTag)commandTag;
                var client = fileCommandTag.Client;

                if (fileCommandTag.Cmd == Command.New)
                    OpenNewDocument(client);
            }
            else if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditSelectAll:
                        ActiveDocument.Editor.SelectAll();
                        break;

                    case StandardCommand.WindowRemoveSplit:
                        ActiveDocument.Editor.HorizontalSplitter = false;
                        ActiveDocument.Editor.VerticalSplitter = false;
                        break;

                    case StandardCommand.WindowSplitVert:
                        ActiveDocument.Editor.VerticalSplitter = true;
                        break;

                    case StandardCommand.WindowSplitHoriz:
                        ActiveDocument.Editor.HorizontalSplitter = true;
                        break;
                }
            }
            else if (commandTag is Command)
            {
                switch ((Command) commandTag)
                {
                    case Command.Open:
                        Open();
                        break;

                    case Command.Goto:
                        ActiveDocument.Editor.ShowGoToLineForm();
                        break;

                    case Command.CloseAllDocuments:
                        CloseAll(null);
                        break;
                }
            }
        }

        #endregion

        #region ISledDocumentService Interface

        public ICollection<ISledDocument> OpenDocuments
        {
            get { return m_documentRegistry.Documents.OfType<ISledDocument>().ToList(); }
        }

        public bool Active
        {
            get { return m_documentRegistry.ActiveDocument != null; }
        }

        public ISledDocument ActiveDocument
        {
            get { return m_documentRegistry.ActiveDocument as ISledDocument; }
        }

        public void Open()
        {
            var filter =
                m_fileExtensionService.Get.FilterString;

            string[] pathNames = null;

            m_fileDialogService.InitialDirectory =
                m_directoryInfoService.Get.ExeDirectory;

            m_fileDialogService.OpenFileNames(ref pathNames, filter);

            if (pathNames == null)
                return;

            foreach (var path in pathNames)
            {
                ISledDocument sd;
                Open(new Uri(path), out sd);
            }
        }

        public bool Open(Uri uri, out ISledDocument sd)
        {
            // Check if document already opened
            if (IsOpen(uri, out sd))
            {
                // It is so jump to the document
                ShowDocument(sd);
                return true;
            }

            // Grab the right client (or default
            // to text client if none found)
            var client = GetClient(uri) ?? m_txtDocumentClient;

            // Try and open document
            var openDoc = OpenExistingDocument(client, uri);
            sd = openDoc as ISledDocument;

            return sd != null;
        }

        public bool IsOpen(Uri uri, out ISledDocument sd)
        {
            sd = null;

            var openDoc = FindOpenDocument(uri);
            if (openDoc != null)
                sd = openDoc as ISledDocument;

            return sd != null;
        }

        public ISledDocumentClient GetDocumentClient(Uri uri)
        {
            if (uri == null)
                return m_txtDocumentClient;

            try
            {
                // Try and find a corresponding client
                var client = GetClient(uri);
                if (client == null)
                    return m_txtDocumentClient;

                // Convert to ISledDocumentClient
                var sledClient =
                    client as ISledDocumentClient;

                return
                    sledClient ?? m_txtDocumentClient;
            }
            catch (Exception)
            {
                // Default ISledDocumentClient to use
                return m_txtDocumentClient;
            }
        }

        public event EventHandler<SledDocumentServiceEventArgs> Opened;

        public event EventHandler ActiveDocumentChanged;

        public event EventHandler<SledDocumentServiceEventArgs> Saving;

        public event EventHandler<SledDocumentServiceEventArgs> Saved;

        public event EventHandler<SledDocumentServiceEventArgs> Closing;

        public event EventHandler<SledDocumentServiceEventArgs> Closed;

        #endregion

        #region ISledDocumentClientService Interface

        public IDocument OpenDocument(Uri uri, ISledDocumentClient client)
        {
            var sd = SledDocument.Create(uri, client);
            sd.Read();

            // Let the control do it itself; it will also
            // supply an icon if the document is readonly.
            sd.RegisterControl();

            if (sd.Editor != null)
                sd.Editor.FileDragDropped += SledDocumentEditorFileDragDropped;

            return sd;
        }

        public void ShowDocument(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd == null)
                return;

            m_controlHostService.Show(sd.Control);
        }

        public void SaveDocument(IDocument document, Uri uri)
        {
            var sd = document as ISledDocument;
            if (sd == null)
                return;

            if (document.IsReadOnly)
                return;

            sd.Write();
        }

        public void CloseDocument(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd == null)
                return;

            if (sd.Editor != null)
                sd.Editor.FileDragDropped -= SledDocumentEditorFileDragDropped;

            // Calls ControlHostService.UnregisterControl() and also
            // calls Dispose() on the underlying ISyntaxEditorControl
            // so as to not leak as it's been shown to do so.
            ((SledDocument)sd).UnregisterControl();
            sd.Dispose();
        }

        #endregion

        #region ISledDocumentPlugin Interface

        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            var commands =
                new List<object>
                    {
                        Command.Goto,
                        null,
                        StandardCommand.EditCut,
                        StandardCommand.EditCopy,
                        StandardCommand.EditPaste,
                        StandardCommand.EditDelete,
                        null,
                        StandardCommand.EditUndo,
                        StandardCommand.EditRedo,
                        StandardCommand.EditSelectAll
                    };
            return commands;
        }

        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return null;
        }

        #endregion

        #region IInstancingContext Interface

        public bool CanInsert(object data)
        {
            var context = ActiveDocument.As<IInstancingContext>();
            return (context != null) && context.CanInsert(data);
        }

        public void Insert(object data)
        {
            var context = ActiveDocument.As<IInstancingContext>();
            if (context == null)
                return;

            context.Insert(data);
        }

        public bool CanCopy()
        {
            var context = ActiveDocument.As<IInstancingContext>();
            return (context != null) && context.CanCopy();
        }

        public object Copy()
        {
            var context = ActiveDocument.As<IInstancingContext>();
            return context == null ? null : context.Copy();
        }

        public bool CanDelete()
        {
            var context = ActiveDocument.As<IInstancingContext>();
            return (context != null) && context.CanDelete();
        }

        public void Delete()
        {
            var context = ActiveDocument.As<IInstancingContext>();
            if (context == null)
                return;

            context.Delete();
        }

        #endregion

        #region IHistoryContext Interface

        public bool CanUndo
        {
            get { return Active && ActiveDocument.Editor.CanUndo; }
        }

        public bool CanRedo
        {
            get { return Active && ActiveDocument.Editor.CanRedo; }
        }

        public string UndoDescription
        {
            get { return string.Empty; }
        }

        public string RedoDescription
        {
            get { return string.Empty; }
        }

        public void Undo()
        {
            if (!Active)
                return;

            ActiveDocument.Editor.Undo();
        }

        public void Redo()
        {
            if (!Active)
                return;

            ActiveDocument.Editor.Redo();
        }

        public bool Dirty
        {
            get { return Active && ActiveDocument.Dirty; }
            set
            {
                ActiveDocument.Dirty = value;
                DirtyChanged.Raise(this, EventArgs.Empty);
            }
        }

        public event EventHandler DirtyChanged;

        #endregion

        #region ISourceControlContext Interface

        public IEnumerable<IResource> Resources
        {
            get
            {
                if (ActiveDocument != null)
                    yield return ActiveDocument;
            }
        }

        #endregion

        #region IEnumerableContext Interface

        //public IEnumerable<object> Items
        //{
        //    get
        //    {
        //        if (!Active)
        //            return s_emptyEnumerable;

        //        return new string[] { ActiveDocument.Editor.Text };
        //    }
        //}

        #endregion

        #region Commands

        enum Command
        {
            New,
            Open,
            Goto,
            CloseAllDocuments,
        }

        #endregion

        #region MainForm Events

        private void MainFormShown(object sender, EventArgs e)
        {
            foreach (var file in m_lstStartupFiles)
            {
                ISledDocument sd;
                Open(new Uri(file), out sd);
            }
        }

        private void MainFormDragOver(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null)
                return;

            e.Effect = DragDropEffects.Copy;
        }

        private void MainFormDragDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null)
                return;

            // Filter paths
            var pathsToOpen =
                from path in paths
                where
                    ((File.GetAttributes(path) & FileAttributes.Directory) == 0) &&
                    !SledUtil.FileEndsWithExtension(path, SledProjectService.ProjectExtensions)
                select path;

            // Open files
            foreach (var path in pathsToOpen)
            {
                ISledDocument sd;
                Open(new Uri(path), out sd);
            }
        }

        #endregion

        #region ISledDocument Events

        private void SledDocumentEditorFileDragDropped(object sender, FileDragDropEventArgs e)
        {
            foreach (var path in e.Paths)
            {
                try
                {
                    var attrs = File.GetAttributes(path);
                    if ((attrs & FileAttributes.Directory) == FileAttributes.Directory)
                        continue;

                    ISledDocument sd;
                    Open(new Uri(path), out sd);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        #endregion

        #region StandardFileCommands Overrides

        protected override void OnDocumentOpened(IDocument document)
        {
            base.OnDocumentOpened(document);

            var sd = document as ISledDocument;
            if (sd == null)
                return;

            Opened.Raise(this, new SledDocumentServiceEventArgs(sd));
        }

        protected override bool OnDocumentSaving(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd != null)
                Saving.Raise(this, new SledDocumentServiceEventArgs(sd));

            return base.OnDocumentSaving(document);
        }

        protected override bool OnDocumentSaved(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd != null)
                Saved.Raise(this, new SledDocumentServiceEventArgs(sd));

            return base.OnDocumentSaved(document);
        }

        protected override void OnDocumentClosing(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd == null)
                return;

            Closing.Raise(this, new SledDocumentServiceEventArgs(sd));
        }

        protected override void OnDocumentClosed(IDocument document)
        {
            var sd = document as ISledDocument;
            if (sd == null)
                return;

            Closed.Raise(this, new SledDocumentServiceEventArgs(sd));
        }

        #endregion

        #region Private Classes

        private struct FileCommandTag
        {
            public FileCommandTag(Command command, ISledDocumentClient client)
            {
                Cmd = command;
                Client = client;
            }

            public readonly Command Cmd;
            public readonly ISledDocumentClient Client;
        }

        #endregion

        #region Member Methods

        private void DocumentRegistryActiveDocumentChanged(object sender, EventArgs e)
        {
            ActiveDocumentChanged.Raise(this, EventArgs.Empty);
        }

        #endregion

        [Export(typeof(IDocumentClient))]
        [Export(typeof(ISledDocumentClient))]
        private readonly SledDocumentClient m_txtDocumentClient;

        [Export(typeof(IDocumentClient))]
        [Export(typeof(ISledDocumentClient))]
        private readonly SledDocumentClient m_xmlDocumentClient;

        [Export(typeof(IDocumentClient))]
        [Export(typeof(ISledDocumentClient))]
        private readonly SledDocumentClient m_csDocumentClient;

        [Export(typeof(IDocumentClient))]
        [Export(typeof(ISledDocumentClient))]
        private readonly SledDocumentClient m_pyDocumentClient;

        private readonly MainForm m_mainForm;
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly IDocumentRegistry m_documentRegistry;
        private readonly IFileDialogService m_fileDialogService;
        private readonly IControlHostService m_controlHostService;

        private readonly List<string> m_lstStartupFiles =
            new List<string>();

        private IEnumerable<ISledDocumentClient> m_sledDocumentClients;

        private readonly SledServiceReference<ISledDirectoryInfoService> m_directoryInfoService =
            new SledServiceReference<ISledDirectoryInfoService>();

        private readonly SledServiceReference<ISledFileExtensionService> m_fileExtensionService =
            new SledServiceReference<ISledFileExtensionService>();
    }
}