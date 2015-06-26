/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.SyntaxEditor;
using Sce.Sled.SyntaxEditor.Intellisense.Lua;

namespace Sce.Sled.Lua.Intellisense
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(LuaIntellisense))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LuaIntellisense : IInitializable, ICommandClient, ISledDocumentPlugin
    {
        [ImportingConstructor]
        public LuaIntellisense(ICommandService commandService)
        {
            m_commandService = commandService;
            m_syncContext = SynchronizationContext.Current;
        }

        #region Implementation of IInitializable

        public void Initialize()
        {
            m_broker = LuaTextEditorFactory.CreateOrGetBroker();
            m_broker.UseNavigationBar = false;
            m_broker.CustomScriptRegistrationHandler = CustomScriptRegistrationHandler;
            m_broker.OpenAndSelectHandler = CustomOpenAndSelectHandler;
            m_broker.Status.Changed += BrokerStatusChanged;

            m_statusService = SledServiceInstance.TryGet<IStatusService>();
            m_gotoService = SledServiceInstance.Get<ISledGotoService>();

            var projectService = SledServiceInstance.Get<ISledProjectService>();
            projectService.Created += ProjectServiceCreated;
            projectService.Opened += ProjectServiceOpened;
            projectService.FileOpened += ProjectServiceFileOpened;
            projectService.FileClosing += ProjectServiceFileClosing;
            projectService.Closed += ProjectServiceClosed;

            m_documentService = SledServiceInstance.Get<ISledDocumentService>();

            // Register the toolbar menu
            var menuInfo = new MenuInfo(Menu.LuaIntellisense, "Lua Intelliense", "Lua Intelliense Menu");
            m_commandService.RegisterMenu(menuInfo);

            // Register the commands
            m_commandService.RegisterCommand(
                Command.GotoDefinition,
                Menu.LuaIntellisense,
                null,
                "Goto &Definition",
                "Jumps to the definition of the selected variable",
                Atf.Input.Keys.Alt | Atf.Input.Keys.G,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.GotoReference,
                Menu.LuaIntellisense,
                null,
                "Goto &References",
                "Jumps to the references of the selected variable",
                Atf.Input.Keys.Alt | Atf.Input.Keys.R,
                null,
                CommandVisibility.Menu,
                this);

            //m_commandService.RegisterCommand(
            //    Command.RenameVariable,
            //    Menu.LuaIntellisense,
            //    Group.LuaIntellisense,
            //    "Rename variable",
            //    "Renames selected variable",
            //    Atf.Input.Keys.Shift | Atf.Input.Keys.Alt | Atf.Input.Keys.R,
            //    null,
            //    CommandVisibility.Menu,
            //    this);

            m_commandService.RegisterCommand(
                Command.MoveToPreviousPosition,
                Menu.LuaIntellisense,
                Group.LuaIntellisense,
                "&Previous visited location",
                "Moves the caret to the previous visited location",
                Atf.Input.Keys.Alt | Atf.Input.Keys.Left,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.MoveToNextPosition,
                Menu.LuaIntellisense,
                Group.LuaIntellisense,
                "&Next visited location",
                "Moves the caret to the next visited location",
                Atf.Input.Keys.Alt | Atf.Input.Keys.Right,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region Implementation of ICommandClient

        public bool CanDoCommand(object commandTag)
        {
            if (!m_customScriptRegistrationWorking)
                return false;

            if (!(commandTag is Command))
                return false;

            var retval = false;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.GotoDefinition:
                    retval = m_broker.CanGotoDefinition(GetDaFromActiveDocument());
                    break;

                case Command.GotoReference:
                    retval = m_broker.CanGotoReference(GetDaFromActiveDocument());
                    break;

                //case Command.RenameVariable:
                //    retval = m_broker.CanRenameVariable(GetDaFromActiveDocument());
                //    break;

                case Command.MoveToNextPosition:
                    retval = m_broker.CanGotoNextPosition;
                    break;

                case Command.MoveToPreviousPosition:
                    retval = m_broker.CanGotoPreviousPosition;
                    break;
            }

            return retval;
        }

        public void DoCommand(object commandTag)
        {
            if (!m_customScriptRegistrationWorking)
                return;

            if (!(commandTag is Command))
                return;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.GotoDefinition: m_broker.GotoDefinition(GetDaFromActiveDocument()); break;
                case Command.GotoReference: m_broker.GotoReference(GetDaFromActiveDocument()); break;
                //case Command.RenameVariable: m_broker.RenameVariable(GetDaFromActiveDocument()); break;
                case Command.MoveToNextPosition: m_broker.GotoNextPosition(); break;
                case Command.MoveToPreviousPosition: m_broker.GotoPreviousPosition(); break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        #region Implementation of ISledDocumentPlugin

        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            if (m_customScriptRegistrationWorking)
            {
                var items = new List<object> { Command.GotoDefinition, Command.GotoReference };
                return items;
            }

            return null;
        }

        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return null;
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            // create before intellisense jobs kick off, as it has to be done on the main thread

            m_project = new ProjectAdapter(e.Project, m_documentService);
            m_broker.ProjectOpened(m_project);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            // create before intellisense jobs kick off, as it has to be done on the main thread

            m_project = new ProjectAdapter(e.Project, m_documentService);
            m_broker.ProjectOpened(m_project);
        }

        private void ProjectServiceFileOpened(object sender, SledProjectServiceFileEventArgs e)
        {
            OpenProjectFile(e.File);
        }

        private void ProjectServiceFileClosing(object sender, SledProjectServiceFileEventArgs e)
        {
            CloseProjectFile(e.File);
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_broker.ProjectClosed();
            m_project = null;
        }

        #endregion

        private void CustomScriptRegistrationHandler(LuatTable stdlibs, ref List<LuatScript> scripts, ref List<ILuaIntellisenseDocument> documents)
        {
            m_customScriptRegistrationWorking = false;

            if (m_project == null)
                return;

            LuaIntellisenseServiceHelpers.RegisterProject(m_project, stdlibs, ref scripts, ref documents);

            m_customScriptRegistrationWorking = true;
        }

        private void CustomOpenAndSelectHandler(string absfilepath, ISyntaxEditorTextRange textrange)
        {
            m_gotoService.GotoFileAndHighlightRange(absfilepath, textrange.StartOffset, textrange.EndOffset);
        }

        private void BrokerStatusChanged(object sender, EventArgs e)
        {
            if (m_statusService == null)
                return;

            string msg =
                m_broker.Status.Active
                    ? string.Format(
                        m_broker.Status.Progress > 0
                            ? "{0}: Lua Intellisense: {1} ({2}%)"
                            : "{0}: Lua Intellisense: {1}",
                        this, m_broker.Status.Action, m_broker.Status.Progress * 100)
                    : "Done";

            lock (m_statusLock)
            {
                if (!string.IsNullOrEmpty(m_statusMsg))
                {
                    m_statusMsg = msg;
                }
                else
                {
                    m_statusMsg = msg;
                    m_syncContext.Post(
                        obj =>
                        {
                            lock (m_statusLock)
                            {
                                m_statusService.ShowStatus(m_statusMsg);
                                m_statusMsg = null;
                            }
                        }, null);
                }
            }
        }

        private void OpenProjectFile(SledProjectFilesFileType file)
        {
            var doc = m_project[file.Uri];
            if (doc != null)
                ((DocumentAdapter)doc).SledDocument = file.SledDocument;

            m_broker.DocumentOpened(doc);
        }

        private void CloseProjectFile(SledProjectFilesFileType file)
        {
            m_broker.DocumentClosed(m_project[file.Uri]);
        }

        private DocumentAdapter GetDaFromActiveDocument()
        {
            if (m_documentService == null)
                return null;

            var activeDocument = m_documentService.ActiveDocument;
            if (activeDocument == null)
                return null;

            if (activeDocument.SledProjectFile == null)
                return null;

            var uri = activeDocument.Uri;

            var document = m_project[uri];
            return document == null ? null : (DocumentAdapter)document;
        }

        private class ProjectAdapter : ILuaIntellisenseProject
        {
            public ProjectAdapter(SledProjectFilesType project, ISledDocumentService documentService)
            {
                m_project = project;
                m_documentService = documentService;
            }

            #region Implementation of ILuaIntellisenseProject

            public ILuaIntellisenseDocument this[Uri uri]
            {
                get
                {
                    DocumentAdapter da;
                    m_documents.TryGetValue(uri, out da);
                    return da;
                }
            }

            public IEnumerable<ILuaIntellisenseDocument> AllDocuments
            {
                get { Build(); return m_documents.Values; }
            }

            public IEnumerable<ILuaIntellisenseDocument> OpenDocuments
            {
                get { Build(); return m_documents.Values.Where(d => d.SledDocument != null); }
            }

            #endregion

            private void Build()
            {
                var allUris = new List<Uri>(m_documents.Keys);

                foreach (var projectFile in m_project.AllFiles)
                {
                    var uri = projectFile.Uri;
                    allUris.Remove(uri);

                    DocumentAdapter da;
                    if (!m_documents.TryGetValue(uri, out da))
                    {
                        da = new DocumentAdapter(uri, this);
                        m_documents.Add(uri, da);
                    }
                }

                // cleanup any lingering files that are no longer in the project
                foreach (var uri in allUris)
                    m_documents.Remove(uri);

                foreach (var document in m_documentService.OpenDocuments)
                {
                    if (document.SledProjectFile == null)
                        continue;

                    DocumentAdapter da;
                    if (m_documents.TryGetValue(document.Uri, out da))
                        da.SledDocument = document;
                    //else
                        //da.SledDocument = null;
                }
            }

            private readonly SledProjectFilesType m_project;

            private readonly ISledDocumentService m_documentService;

            private readonly Dictionary<Uri, DocumentAdapter> m_documents =
                new Dictionary<Uri, DocumentAdapter>();
        }

        private class DocumentAdapter : ILuaIntellisenseDocument
        {
            public DocumentAdapter(Uri uri, ProjectAdapter project)
            {
                Uri = uri;
                Project = project;
                Name = Path.GetFileNameWithoutExtension(Uri.LocalPath);
            }

            #region Implementation of ILuaIntellisenseDocument

            public string Name { get; private set; }

            public string Contents
            {
                get
                {
                    if (SyntaxEditorControl == null)
                    {
                        if (string.IsNullOrEmpty(m_cachedContent))
                            m_cachedContent = GetDocumentContents(Uri);

                        if (string.IsNullOrEmpty(m_cachedContent))
                            m_cachedContent = string.Empty;

                        return m_cachedContent;
                    }

                    m_cachedContent = null;

                    return SyntaxEditorControl.Text;
                }
            }

            public Uri Uri { get; private set; }

            public ISyntaxEditorControl SyntaxEditorControl
            {
                get { return SledDocument == null ? null : SledDocument.Editor; }
            }

            public ILuaIntellisenseProject Project { get; private set; }

            #endregion

            public ISledDocument SledDocument { get; set; }

            private static string GetDocumentContents(Uri uri)
            {
                try
                {
                    using (var document = new StreamReader(uri.LocalPath))
                    {
                        var contents = document.ReadToEnd();
                        return contents;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private string m_cachedContent;
        }

        private enum Command
        {
            GotoDefinition,
            GotoReference,
            //RenameVariable,
            MoveToPreviousPosition,
            MoveToNextPosition,
        }

        private enum Menu
        {
            LuaIntellisense,
        }

        private enum Group
        {
            LuaIntellisense,
        }

        private ProjectAdapter m_project;
        private IStatusService m_statusService;
        private ISledGotoService m_gotoService;
        private ILuaIntellisenseBroker m_broker;
        private ISledDocumentService m_documentService;
        private bool m_customScriptRegistrationWorking;

        private volatile string m_statusMsg;

        private readonly object m_statusLock =
            new object();

        private readonly ICommandService m_commandService;
        private readonly SynchronizationContext m_syncContext;
    }
}