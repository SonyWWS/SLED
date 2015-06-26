/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using Sce.Sled.Project.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    /// <summary>
    /// SledProjectService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectService))]
    [Export(typeof(ISledLessProjectSerivce))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(SledProjectService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledProjectService : IInitializable, ICommandClient, ISledProjectService, ISledDocumentPlugin, IContextMenuCommandProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [ImportingConstructor]
        public SledProjectService(MainForm mainForm)
        {
            if (SledShared.IsSled)
            {
                m_mainForm = mainForm;

                // Parse command line parameters
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    if (File.Exists(arg) && SledUtil.FileEndsWithExtension(arg, ProjectExtensions))
                    {
                        m_startupProject = arg;
                        break;
                    }
                }

                var saveSettingsTimer = new Timer { Interval = 250 };
                saveSettingsTimer.Tick += SaveSettingsTimerTick;
                saveSettingsTimer.Start();
            }
            else
            {
                // Initialize a few things
                var catalog =
                    new TypeCatalog(
                        typeof(SledSharedSchemaLoader),
                        typeof(SledProjectFileFinderService)
                        );

                var container = new CompositionContainer(catalog);

                var batch = new CompositionBatch();
                batch.AddPart(this);
                container.Compose(batch);

                // Set this one time
                SledServiceReferenceCompositionContainer.SetCompositionContainer(container);

                // Initialize all initializables
                foreach (var initializable in container.GetExportedValues<IInitializable>())
                    initializable.Initialize();
            }
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            //
            // Stuff to do if SLED not running
            //

            if (!SledShared.IsSled)
                return;

            //
            // Stuff to do if SLED running
            //

            // Don't write certain things to the project file
            SledSpfWriter.ExcludeAttribute("expanded");
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledAttributeBaseType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledFunctionBaseType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledProjectFilesBreakpointType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledProjectFilesLanguageType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledProjectFilesWatchType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledSchema.SledProjectFilesUserSettingsType.Type);

            // Sync up files from the project temporary
            // settings file to the main project file
            SledSpfReader.RegisterCopier(new SledProjectFilesFileType.SpfCopier());

            // Sync up folder names/expanded state from the
            // project temporary settings file to the main
            // project file
            SledSpfReader.RegisterCopier(new SledProjectFilesFolderType.SpfCopier());

            RegisterCommands();
            RegisterSettings();
            
            m_mainForm.Loaded += MainFormLoaded;
            m_mainForm.DragOver += MainFormDragOver;
            m_mainForm.DragDrop += MainFormDragDrop;
            m_mainForm.FormClosing += MainFormFormClosing;

            if (DocumentService != null)
            {
                DocumentService.Opened += DocumentServiceOpened;
                DocumentService.Closing += DocumentServiceClosing;
                DocumentService.Closed += DocumentServiceClosed;
            }

            var modifiedProjectFormService = SledServiceInstance.Get<ISledModifiedProjectFormService>();
            modifiedProjectFormService.ChangesDetected += ModifiedProjectFormServiceChangesDetected;
            modifiedProjectFormService.GuiChangesSubmitted += ModifiedProjectFormServiceGuiChangesSubmitted;
        }

        #endregion

        #region Commands & Settings

        private void RegisterCommands()
        {
            if (CommandService == null)
                return;

            // Create a menu for project options
            CommandService.RegisterMenu(
                Menu.Project,
                Localization.SledProjectMenuTitle,
                Localization.SledProjectMenuTitleComment);

            // Register command for new project
            CommandService.RegisterCommand(
                Command.New,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledNew,
                Localization.SledCommandProjectNewComment,
                Keys.Control | Keys.Shift | Keys.N,
                SledIcon.ProjectNew,
                CommandVisibility.Menu,
                this);

            // Register command for opening a project
            CommandService.RegisterCommand(
                Command.Open,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledOpen,
                Localization.SledCommandProjectOpenComment,
                Keys.Control | Keys.Shift | Keys.O,
                SledIcon.ProjectOpen,
                CommandVisibility.Menu,
                this);

            // Register command for saving a project
            CommandService.RegisterCommand(
                Command.Save,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledCommandProjectSave,
                Localization.SledCommandProjectSaveComment,
                Keys.None,
                SledIcon.ProjectSave,
                CommandVisibility.All,
                this);

            // Register command for saving a project
            CommandService.RegisterCommand(
                Command.SaveAs,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledCommandProjectSaveAs,
                Localization.SledCommandProjectSaveAsComment,
                Keys.None,
                SledIcon.ProjectSaveAs,
                CommandVisibility.Menu,
                this);

            // Register command for closing a project
            CommandService.RegisterCommand(
                Command.Close,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledClose,
                Localization.SledCommandProjectCloseComment,
                Keys.None,
                SledIcon.ProjectClose,
                CommandVisibility.Menu,
                this);

            // Add "Add File to Project" option
            CommandService.RegisterCommand(
                Command.AddFile,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledAddFile,
                Localization.SledCommandProjectAddFileComment,
                Keys.None,
                SledIcon.ProjectFileAdd,
                CommandVisibility.Menu,
                this);

            // Add "Remove File from Project" option
            CommandService.RegisterCommand(
                Command.RemoveFile,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                Localization.SledProject + Resources.Resource.MenuSeparator + Localization.SledRemoveFile,
                Localization.SledRemoveFileComment,
                Keys.None,
                SledIcon.ProjectFileAdd,
                CommandVisibility.Menu,
                this);

            // Register all MRU project commands
            for (var i = 0; i < MaxMruCount; i++)
            {
                CommandService.RegisterCommand(
                    new MruCommand(i),
                    StandardMenu.File,
                    CommandGroup.Project,
                    Localization.SledRecentProjects + 
                        Resources.Resource.MenuSeparator + 
                        Localization.SledRecentProject +
                        string.Format("({0})", i),
                    Localization.SledRecentProjectComment,
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);
            }

            // Register clear output window option
            CommandService.RegisterCommand(
                Command.ClearOutputWindow,
                Menu.Project,
                CommandGroup.Project,
                Localization.SledProjectMenuClearOutputWindow,
                Localization.SledProjectMenuClearOutputWindowComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Register change asset directory command
            CommandService.RegisterCommand(
                Command.ChangeAssetDir,
                Menu.Project,
                CommandGroup.Project,
                Localization.SledProjectMenuChangeAssetDir,
                Localization.SledProjectMenuChangeAssetDirComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            CommandService.RegisterCommand(
                Command.ViewProjectPaths,
                Menu.Project,
                CommandGroup.Project,
                Localization.SledProjectMenuViewProjectPaths,
                Localization.SledProjectMenuViewProjectPathsComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            CommandService.RegisterCommand(
                Command.ShowAutoAddFiles,
                Menu.Project,
                CommandGroup.Project,
                Localization.SledProjectMenuShowAutoAddFilesTitle,
                Localization.SledProjectMenuShowAutoAddFilesComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            CommandService.RegisterCommand(
                Command.OpenFileInProject,
                Menu.Project,
                CommandGroup.Project,
                "Open File In Project",
                "Open a file in the project",
                Keys.Alt | Keys.Shift | Keys.O,
                null,
                CommandVisibility.Menu,
                this);
        }

        private void RegisterSettings()
        {
            if (SettingsService == null)
                return;

            // Save the most recently used project list
            SettingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => MruProjects,
                    Resources.Resource.MruProjectListTitle,
                    Resources.Resource.Project,
                    Resources.Resource.MruProjectListComment));

            // Save open-last-project-on-startup value
            SettingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => OpenLastProjectOnStartupToggle,
                    Resources.Resource.OpenLastProjectOnStartupToggleTitle,
                    Resources.Resource.Project,
                    Resources.Resource.OpenLastProjectOnStartupToggleDescription));

            // Add some user settings to edit > preferences
            SettingsService.RegisterUserSettings(
                Resources.Resource.Project,
                new BoundPropertyDescriptor(
                    this,
                    () => OpenLastProjectOnStartupToggle,
                    Resources.Resource.OpenLastProjectOnStartupToggleTitle,
                    null,
                    Resources.Resource.OpenLastProjectOnStartupToggleDescription));
        }

        #endregion

        #region MainForm Events

        private void MainFormLoaded(object sender, EventArgs e)
        {
            // If a project was passed in as an argument, open it. If not and setting
            // to open the last project is set then try and open the last project.
            if (m_startupProject != null)
            {
                Open(m_startupProject);
            }
            else if (OpenLastProjectOnStartupToggle)
            {
                if (m_mruProjects.Count > 0)
                {
                    foreach (var file in m_mruProjects.MostRecentOrder)
                    {
                        Open(file);
                        return;
                    }
                }
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

            // If project file try and open
            foreach (var path in paths)
            {
                // Skip directories
                if ((File.GetAttributes(path) & FileAttributes.Directory) != 0)
                    continue;

                if (!SledUtil.FileEndsWithExtension(path, ProjectExtensions))
                    continue;

                Open(path);
                break;
            }
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // If a project is active, close it first
            if (Active)
                Close();
        }
        
        #endregion

        #region Commands

        enum Command
        {
            New,                // Command to create a new project
            Open,               // Command to open an existing project
            Save,               // Command to save a project
            SaveAs,             // Command to save-as a project
            Close,              // Command to close a project
            AddFile,            // Command to add a file to a project
            RemoveFile,         // Command to remove a file from the project
            ClearOutputWindow,  // Command to clear the output window
            ChangeAssetDir,     // Command to change the asset directory
            ViewProjectPaths,   // Command to view the Project Paths form
            ShowAutoAddFiles,   // Command to bring up the auto add files form
            OpenFileInProject,  // Command to bring up open-file-in-project form
        }

        private class MruCommand
        {
            public MruCommand(int index)
            {
                Index = index;
            }

            public readonly int Index;
        }

        enum Menu
        {
            Project,
        }

        enum CommandGroup
        {
            Project,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.New:
                    case Command.Open:
                        bEnabled = true;
                        break;

                    case Command.Save:
                        bEnabled = Active && Dirty;
                        break;

                    case Command.SaveAs:
                        bEnabled = Active;
                        break;

                    case Command.Close:
                        bEnabled = Active;
                        break;

                    case Command.AddFile:
                    {
                        // Need an active project, an active document, an active document that
                        // exists on disk, and the active document to not be in the project already
                        if (Active &&
                            (DocumentService != null) &&
                            DocumentService.Active &&
                            File.Exists(DocumentService.ActiveDocument.Uri.LocalPath) &&
                            (DocumentService.ActiveDocument.SledProjectFile == null))
                        {
                            bEnabled = true;
                        }
                    }
                    break;

                    case Command.RemoveFile:
                    {
                        // Need an active project, an active document, and active document that
                        // exists on disk, and the active document is in the project
                        if (Active &&
                            (DocumentService != null) &&
                            DocumentService.Active &&
                            File.Exists(DocumentService.ActiveDocument.Uri.LocalPath) &&
                            (DocumentService.ActiveDocument.SledProjectFile != null))
                        {
                            bEnabled = true;
                        }
                    }
                    break;

                    case Command.ClearOutputWindow:
                        bEnabled = true;
                        break;

                    case Command.ChangeAssetDir:
                    case Command.ViewProjectPaths:
                        // TODO: re-enable
                    case Command.ShowAutoAddFiles:
                    case Command.OpenFileInProject:
                        bEnabled = Active;
                        break;
                }
            }
            else if (commandTag is MruCommand)
            {
                var mruCmd = (MruCommand)commandTag;
                var lstMru = GetMruList();
                bEnabled = !string.IsNullOrEmpty(lstMru[mruCmd.Index]);
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                switch ((Command) commandTag)
                {
                    case Command.New:
                        New();
                        break;

                    case Command.Open:
                        Open(null);
                        break;

                    case Command.Save:
                        Save();
                        break;

                    case Command.SaveAs:
                        SaveAs();
                        break;

                    case Command.Close:
                        Close();
                        break;

                    case Command.AddFile:
                        AddFile(DocumentService.ActiveDocument);
                        break;

                    case Command.RemoveFile:
                        RemoveFile(DocumentService.ActiveDocument);
                        break;

                    case Command.ClearOutputWindow:
                        SledOutDevice.Clear();
                        break;

                    case Command.ChangeAssetDir:
                        ChangeAssetDirectory(false);
                        break;

                    case Command.ViewProjectPaths:
                        ShowProjectPaths();
                        break;

                    case Command.ShowAutoAddFiles:
                        ShowAutoAddFilesForm();
                        break;

                    case Command.OpenFileInProject:
                        OpenFileInProject();
                        break;
                }
            }
            else if (commandTag is MruCommand)
            {
                var mruCmd = (MruCommand)commandTag;
                var lstMru = GetMruList();
                Open(lstMru[mruCmd.Index]);
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is MruCommand))
                return;

            var mruCmd = (MruCommand)commandTag;
            var lstMru = GetMruList();
            
            var entry = lstMru[mruCmd.Index];

            state.Text =
                string.IsNullOrEmpty(entry)
                    ? string.Format("{0} ({1})", Localization.SledRecentProject, mruCmd.Index)
                    : entry;
        }

        #endregion

        #region IContextMenuCommandProvider Interface

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (!Active)
                return s_emptyCommands;

            if (target == null)
                return s_emptyCommands;

            if (!(target is DomNode))
                return s_emptyCommands;

            var domNode = target as DomNode;
            if (!ReferenceEquals(domNode.GetRoot(), ActiveProject.DomNode))
                return s_emptyCommands;

            // HACK: this is lame
            if (domNode.Is<SledProjectFilesBreakpointType>())
                return s_emptyCommands;

            var commands =
                new List<object>
                    {
                        // TODO: re-enable
                        Command.ViewProjectPaths/*,
                        Command.ShowAutoAddFiles*/
                    };
            return commands;
        }

        #endregion

        #region IAdaptable Interface

        public object GetAdapter(Type type)
        {
            if ((typeof(SledProjectFilesType).IsAssignableFrom(type)) && Active)
                return ActiveProject;

            return null;
        }

        #endregion

        #region Persisted Settings

        /// <summary>
        /// Save/load the most recently used projects list
        /// </summary>
        public string MruProjects
        {
            get
            {
                // Generate Xml string to contain the Mru project list
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration(Resources.Resource.OnePointZero, Resources.Resource.UtfDashEight, Resources.Resource.YesLower));
                var root = xmlDoc.CreateElement(Resources.Resource.MruProjects);
                xmlDoc.AppendChild(root);

                try
                {
                    foreach (var mru in m_mruProjects.MostRecentOrder)
                    {
                        var elem = xmlDoc.CreateElement(Resources.Resource.MruProject);
                        elem.SetAttribute(Resources.Resource.PathLower, mru);
                        root.AppendChild(elem);
                    }

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    var szSetting = Resources.Resource.MruProjects;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionSavingSetting, szSetting, ex.Message));
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    var nodes = xmlDoc.DocumentElement.SelectNodes(Resources.Resource.MruProject);
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    // Grab all the items
                    IList<string> lstElems =
                        (from XmlElement elem in nodes
                         select elem.GetAttribute(Resources.Resource.PathLower)).ToList();

                    // Add items in reverse order
                    for (var i = lstElems.Count - 1; i >= 0; i--)
                        m_mruProjects.Add(lstElems[i]);
                }
                catch (Exception ex)
                {
                    var szSetting = Resources.Resource.MruProjects;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        /// <summary>
        /// Save/load the user setting
        /// </summary>
        public bool OpenLastProjectOnStartupToggle { get; set; }

        #endregion

        #region ISledProjectService Interface

        /// <summary>
        /// The name of the active project
        /// </summary>
        public string ProjectName
        {
            get { return !Active ? string.Empty : ActiveProject.Name; }
        }

        /// <summary>
        /// The directory the active project is in
        /// </summary>
        public string ProjectDirectory
        {
            get { return !Active ? string.Empty : ActiveProject.ProjectDirectory; }
        }

        /// <summary>
        /// Directory assets are held in (like a scripts directory or something)
        /// </summary>
        public string AssetDirectory
        {
            get { return !Active ? string.Empty : ActiveProject.AssetDirectory; }
        }

        /// <summary>
        /// Project unique identifier
        /// </summary>
        public Guid ProjectGuid
        {
            get { return !Active ? Guid.Empty : ActiveProject.Guid; }

            set
            {
                if (!Active)
                    return;

                var project = ActiveProject;

                var oldGuid = project.Guid;

                // Fire event
                GuidChanging.Raise(this, new SledProjectServiceProjectGuidEventArgs(ActiveProject, oldGuid, value));

                project.Guid = value;

                // Fire event
                GuidChanged.Raise(this, new SledProjectServiceProjectGuidEventArgs(ActiveProject, oldGuid, value));
            }
        }

        /// <summary>
        /// New project created event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> Created;

        /// <summary>
        /// Existing project opened event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> Opened;

        /// <summary>
        /// Project closing event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> Closing;

        /// <summary>
        /// Project closed event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> Closed;

        /// <summary>
        /// Project saved event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> Saved;

        /// <summary>
        /// Project saved-as-ing event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectSaveAsEventArgs> SavedAsing;

        /// <summary>
        /// Project saved-as-ed event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectSaveAsEventArgs> SavedAsed;

        /// <summary>
        /// Project settings saving event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> SavingSettings;

        /// <summary>
        /// Project settings saved event
        /// </summary>
        public event EventHandler<SledProjectServiceProjectEventArgs> SavedSettings;

        /// <summary>
        /// File in the project being renamed
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileRenaming;

        /// <summary>
        /// File in the project renamed
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileRenamed;

        /// <summary>
        /// File added to project event
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileAdded;

        /// <summary>
        /// File being removed from project event
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileRemoving;

        /// <summary>
        /// File removed from project event
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileRemoved;

        /// <summary>
        /// Folder in the project being renamed
        /// </summary>
        public event EventHandler<SledProjectServiceFolderEventArgs> FolderRenaming;

        /// <summary>
        /// Folder in the project renamed
        /// </summary>
        public event EventHandler<SledProjectServiceFolderEventArgs> FolderRenamed;

        /// <summary>
        /// Folder added to project event
        /// </summary>
        public event EventHandler<SledProjectServiceFolderEventArgs> FolderAdded;

        /// <summary>
        /// Folder being removed from project event
        /// </summary>
        public event EventHandler<SledProjectServiceFolderEventArgs> FolderRemoving;

        /// <summary>
        /// Folder removed from project event
        /// </summary>
        public event EventHandler<SledProjectServiceFolderEventArgs> FolderRemoved;

        /// <summary>
        /// Project file opened in the editor
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileOpened;

        /// <summary>
        /// Project file closing in the editor
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileClosing;

        /// <summary>
        /// Project file closed in the editor
        /// </summary>
        public event EventHandler<SledProjectServiceFileEventArgs> FileClosed;

        /// <summary>
        /// Event fired when project name is changing
        /// </summary>
        public event EventHandler<SledProjectServiceProjectNameEventArgs> NameChanging;

        /// <summary>
        /// Event fired when project name changed
        /// </summary>
        public event EventHandler<SledProjectServiceProjectNameEventArgs> NameChanged;

        /// <summary>
        /// Event fired when project guid is changing
        /// </summary>
        public event EventHandler<SledProjectServiceProjectGuidEventArgs> GuidChanging;

        /// <summary>
        /// Event fired when project guid changed
        /// </summary>
        public event EventHandler<SledProjectServiceProjectGuidEventArgs> GuidChanged;

        /// <summary>
        /// Event fired when asset directory is changing
        /// </summary>
        public event EventHandler<SledProjectServiceAssetDirEventArgs> AssetDirChanging;

        /// <summary>
        /// Event fired when asset directory changed
        /// </summary>
        public event EventHandler<SledProjectServiceAssetDirEventArgs> AssetDirChanged;

        /// <summary>
        /// Return true if there is a project open and it has no files in it otherwise return false
        /// </summary>
        public bool Empty
        {
            get
            {
                if (!Active)
                    return true;

                return AllFiles.Count <= 0;
            }
        }

        /// <summary>
        /// Return true if underlying collection is dirty
        /// </summary>
        public bool Dirty
        {
            get
            {
                if (!Active)
                    return false;

                if (m_saveSettingsNeeded)
                    return true;

                var plugins = SledServiceInstance.GetAll<ISledProjectSettingsPlugin>();

                // Determine if any plugins need saving
                var bPluginNeedsSaving = plugins.Any(plugin => plugin.NeedsSaving());

                return ActiveProject.Dirty || bPluginNeedsSaving;
            }

            private set
            {
                if (!Active)
                    return;

                m_saveSettingsNeeded = value;
                ActiveProject.Dirty = value;
            }
        }

        /// <summary>
        /// Returns true if a project is open and false if a project is not open
        /// </summary>
        public bool Active
        {
            get { return ActiveProject != null; }
        }

        /// <summary>
        /// Return the underlying collection as a SLED project (if the underlying collection isn't null)
        /// </summary>
        /// <returns>SLED project</returns>
        [Obsolete("Use ActiveProject instead")]
        public SledProjectFilesType Get()
        {
            return ActiveProject;
        }

        /// <summary>
        /// Returns the active project or null if no active project
        /// </summary>
        public SledProjectFilesType ActiveProject { get; private set; }

        /// <summary>
        /// Return files in the project
        /// </summary>
        public ICollection<SledProjectFilesFileType> AllFiles
        {
            get
            {
                return Active ? ActiveProject.AllFiles : new List<SledProjectFilesFileType>(s_emptyFiles);
            }
        }

        /// <summary>
        /// Create a new project (brings up the new project dialog)
        /// </summary>
        /// <returns>True if project created false if not</returns>
        public bool New()
        {
            // If a previous project is open, close it
            if (Active)
                Close();

            // Create a new project dialog
            using (var hDialog = new SledProjectNewForm())
            {
                // Show the new project dialog
                if (hDialog.ShowDialog(m_mainForm) != DialogResult.OK)
                    return false;

                // Now, create a new project
                return
                    NewShared(
                        hDialog.ProjectFullSpfPath,
                        hDialog.ProjectName,
                        hDialog.ProjectAssetDirectory,
                        Guid.Empty,
                        hDialog.RecursiveAdd);
            }
        }

        /// <summary>
        /// Create a new project with certain criteria
        /// </summary>
        /// <param name="name">Name of project</param>
        /// <param name="projectDir">Project directory</param>
        /// <param name="assetDir">Asset directory</param>
        /// <param name="guid">Guid</param>
        /// <returns>True if project created false it not</returns>
        public bool New(string name, string projectDir, string assetDir, Guid guid)
        {
            var bResult = false;

            // Try and form an absolute path to the project directory
            var szProjectDir =
                PathUtil.IsRelative(projectDir)
                    ? SledUtil.GetAbsolutePath(projectDir, SledShared.DirectoryPath)
                    : projectDir;

            // Try and form an absolute path to the asset directory
            var szAssetDir =
                PathUtil.IsRelative(assetDir)
                    ? SledUtil.GetAbsolutePath(assetDir, szProjectDir)
                    : assetDir;

            // Check that the project directory exists and/or try and create the project directory
            try
            {
                if (!Directory.Exists(szProjectDir))
                {
                    Directory.CreateDirectory(szProjectDir);
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledAutoProjectCreationErrorNotFindProjectDir1, szProjectDir));
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledAutoProjectCreationErrorNotFindProjectDir2, szProjectDir, ex.Message));

                // Abort now
                return false;
            }

            // Check that the asset directory exists and/or try and create the asset directory
            try
            {
                if (!Directory.Exists(szAssetDir))
                {
                    Directory.CreateDirectory(szAssetDir);
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledAutoProjectCreationErrorNotFindAssetDir1, szAssetDir));
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledAutoProjectCreationErrorNotFindAssetDir2, szAssetDir, ex.Message));

                // Abort now
                return false;
            }

            // Strip off project extension from the name if its there
            string szExt;
            var szName =
                SledUtil.StringEndsWithExtension(name, ProjectExtensions, out szExt)
                    ? name.Substring(0, name.Length - szExt.Length)
                    : name;

            // Create a list of paths to project files to look for
            var lstProjFiles = new List<string>();

            if (string.IsNullOrEmpty(szExt))
            {
                // Want to try both .spf & .lpf (favoring .spf)
                lstProjFiles.Add((szProjectDir + Path.DirectorySeparatorChar + szName + Resources.Resource.ProjectFileExtensionSpf));
                lstProjFiles.Add((szProjectDir + Path.DirectorySeparatorChar + szName + Resources.Resource.ProjectFileExtensionLpf));
            }
            else
            {
                // Want to just try version w/ szExt
                lstProjFiles.Add((szProjectDir + Path.DirectorySeparatorChar + szName + szExt));
            }

            // Check if any of the proposed project file(s) exists
            var bFileExists = false;
            foreach (var projFile in lstProjFiles)
            {
                if (File.Exists(projFile))
                    bFileExists = true;
            }

            // Try to open an existing project or create a new one
            if (bFileExists)
            {
                // Try and open an existing project
                for (var i = 0; (i < lstProjFiles.Count) && !bResult; i++)
                {
                    // Make sure this one is actually valid before trying to open it
                    if (!File.Exists(lstProjFiles[i]))
                        continue;

                    bResult = Open(lstProjFiles[i]);
                }
            }
            else
            {
                // Try and create a new project
                NewShared(lstProjFiles[0], szName, szAssetDir, guid, false);
                bResult = true;
            }

            return bResult;
        }

        /// <summary>
        /// Open a project
        /// </summary>
        /// <param name="absPath">Absolute path to project file</param>
        /// <returns>True if project opened false if not</returns>
        public bool Open(string absPath)
        {
            // Going to select a project to open
            var dlg = new OpenFileDialog { Filter = ProjectFilter, CheckFileExists = true };

            if (absPath == null)
            {
                // Show the dialog
                if (dlg.ShowDialog(m_mainForm) != DialogResult.OK)
                    return false;
            }
            else
            {
                dlg.FileName = absPath;
            }

            if (!File.Exists(dlg.FileName))
            {
                var res = 
                    MessageBox.Show(
                        m_mainForm,
                        string.Format("{0}{1}{1}{2}",
                            Localization.SledProjectOpenErrorString1,
                            Environment.NewLine,
                            dlg.FileName),
                        Localization.SledProjectOpenErrorTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                // Remove from MRU project listing if user chose to do so
                if (res == DialogResult.Yes)
                {
                    if (m_mruProjects.Contains(dlg.FileName))
                        m_mruProjects.Remove(dlg.FileName);
                }

                return false;
            }

            // If a project is open then close it
            if (Active)
                Close();

            var bReadOnly = SledUtil.IsFileReadOnly(dlg.FileName);
            var bReadOnlyError = false;

            // Try removing read-only flag
            if (bReadOnly)
            {
                try
                {
                    File.SetAttributes(dlg.FileName, FileAttributes.Normal);
                }
                catch (Exception ex)
                {
                    bReadOnlyError = true;
                    MessageBox.Show(
                        m_mainForm,
                        string.Format(
                            "{0}{1}{1}{2}",
                            Localization.SledProjectFileReadOnlyRemoveError,
                            Environment.NewLine,
                            ex.Message));
                }
            }

            // Make sure namespace is correct
            SledProjectUtilities.CleanupProjectFileNamespace(dlg.FileName);

            // Remove any duplicate files from the project before opening for real
            SledProjectUtilities.CleanupProjectFileDuplicates(dlg.FileName);

            // Try and reset the read-only flag
            if (bReadOnly && !bReadOnlyError)
            {
                try
                {
                    File.SetAttributes(dlg.FileName, FileAttributes.ReadOnly);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        m_mainForm,
                        string.Format(
                            "{0}{1}{1}{2}",
                            Localization.SledProjectFileReadOnlyRemoveError,
                            Environment.NewLine,
                            ex.Message));
                }
            }

            // Add to most recently used project list
            m_mruProjects.Add(dlg.FileName);

            //
            // Load the new project file and set up stuff
            //

            // Set new project directory
            var projectName = Path.GetFileNameWithoutExtension(dlg.FileName);

            // -----------------------------------------------------

            // Create project
            ActiveProject =
                CreateProject(
                    new Uri(dlg.FileName),
                    projectName,
                    null,
                    Guid.Empty,
                    false);

            // Make sure asset directory is valid
            while (!Directory.Exists(AssetDirectory))
            {
                ChangeAssetDirectory(true);
            }

            SubscribeToEvents();

            // -----------------------------------------------------
            
            m_projectFilesUtilityService.Get.SetupFiles(AllFiles);
                       
            // -----------------------------------------------------

            // Fire event
            Opened.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

            SaveSettings();

            return true;
        }

        /// <summary>
        /// Save the project
        /// </summary>
        public void Save()
        {
            if (!Active)
                return;

            if (DocumentService != null)
                DocumentService.SaveAll(false);

            SaveSettings();

            // Fire event
            Saved.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));
        }

        /// <summary>
        /// Save-as the project
        /// </summary>
        public void SaveAs()
        {
            if (!Active)
                return;

            string filename;

            using (var dialog = new SaveFileDialog())
            {
                dialog.DefaultExt = ProjectExtension;
                dialog.Filter = string.Format("SLED Project File (*{0})|*{0}", ProjectExtension);
                dialog.InitialDirectory = ActiveProject.ProjectDirectory;
                dialog.OverwritePrompt = true;
                dialog.SupportMultiDottedExtensions = true;
                dialog.Title = @"SLED - Save Project As...";

                if (dialog.ShowDialog(m_mainForm) != DialogResult.OK)
                    return;

                filename = dialog.FileName;
                if (string.IsNullOrEmpty(filename))
                    return;
            }

            Save();

            try
            {
                var oldName = ActiveProject.Name;
                var newName = Path.GetFileNameWithoutExtension(filename);
                var oldProjDir = ActiveProject.ProjectDirectory;
                var newProjDir = Path.GetDirectoryName(filename);
                var oldAssetDir = ActiveProject.AssetDirectory;
                var newAssetDir = ActiveProject.AssetDirectory; // TODO: just here if needed for future stuff
                
                SavedAsing.Raise(this, new SledProjectServiceProjectSaveAsEventArgs(ActiveProject, oldName, newName, oldProjDir, newProjDir, oldAssetDir, newAssetDir));

                ActiveProject.Name = newName;
                ActiveProject.Uri = new Uri(filename); // also makes the project directory get updated
                ActiveProject.AssetDirectory = newAssetDir;
                m_mruProjects.Add(filename);
                
                SavedAsed.Raise(this, new SledProjectServiceProjectSaveAsEventArgs(ActiveProject, oldName, newName, oldProjDir, newProjDir, oldAssetDir, newAssetDir));
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(SledMessageType.Error, "{0}: Exception in project save-as: {1}", this, ex.Message);
            }
        }

        /// <summary>
        /// Write settings to disk
        /// </summary>
        public void SaveSettings()
        {
            SaveSettings(false);
        }

        /// <summary>
        /// Write settings to disk
        /// </summary>
        /// <param name="canQueue">True if multiple calls can be queued and then some
        /// milliseconds time later be turned into one save settings call.</param>
        public void SaveSettings(bool canQueue)
        {
            if (!Active)
                return;
            
            if (canQueue)
            {
                // Indicate that we need to save later
                m_saveSettingsNeeded = true;
                m_saveSettingsTime = DateTime.Now;

                return;
            }

            try
            {
                var readOnly = SledUtil.IsFileReadOnly(ActiveProject.Uri.LocalPath);

                // Fire event
                SavingSettings.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

                try
                {
                    // Remove read only flag if set
                    if (readOnly)
                        SledUtil.SetReadOnly(ActiveProject.Uri.LocalPath, false);

                    // Write to disk
                    var writer = new SledSpfWriter(SchemaLoader.TypeCollection);
                    writer.Write(ActiveProject.DomNode, ActiveProject.Uri, true);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception encountered when writing " +
                        "SLED project file \"{1}\" to disk: {2}",
                        this, ActiveProject.Uri.LocalPath, ex.Message);
                }
                finally
                {
                    ActiveProject.Dirty = false;

                    // Add back read only flag (if previously set)
                    if (readOnly)
                        SledUtil.SetReadOnly(ActiveProject.Uri.LocalPath, true);
                }
            }
            finally
            {
                // Fire event
                SavedSettings.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

                m_saveSettingsNeeded = false;
                m_saveSettingsTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Close the project
        /// </summary>
        public void Close()
        {
            if (!Active)
                return;

            if (Dirty)
            {
                // Ask if user wants to save before closing
                var res =
                    MessageBox.Show(
                        m_mainForm,
                        Localization.SledProjectDirtyString,
                        Localization.SledProjectDirtyTitle,
                        MessageBoxButtons.YesNo);

                if (res == DialogResult.Yes)
                    Save();
            }

            // Fire event
            Closing.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

            // Close all open project documents
            if (DocumentService != null)
                DocumentService.CloseAll(null);

            UnsubscribeFromEvents();

            // Fire event
            Closed.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

            // Shutdown collection
            ActiveProject = null;

            // Clear output window
            SledOutDevice.Clear();
        }

        /// <summary>
        /// Add file to project
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <param name="file">Handle to file in project</param>
        /// <returns>True if file added (or already in the project) or false if not</returns>
        public bool AddFile(string absPath, out SledProjectFilesFileType file)
        {
            return AddFile(absPath, null, out file);
        }

        /// <summary>
        /// Add file to project with specified folder
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <param name="folder">Folder to add the file to</param>
        /// <param name="file">Handle to file in project</param>
        /// <returns>True if file added (or already in the project) or false if not</returns>
        public bool AddFile(string absPath, SledProjectFilesFolderType folder, out SledProjectFilesFileType file)
        {
            file = m_projectFileFinderService.Get.Find(absPath);
            if (file != null)
                return true;

            var project = ActiveProject;

            // Create new project file (if possible)
            bool bAlreadyInProject;
            file = m_projectFilesUtilityService.Get.CreateFrom(absPath, project, out bAlreadyInProject);
            if (file == null)
                return false;

            if (bAlreadyInProject)
                return true;

            // Add to project (DOM will handle firing the event)
            if (folder == null)
                project.Files.Add(file);
            else
                folder.Files.Add(file);

            return true;
        }

        /// <summary>
        /// Add folder to project
        /// </summary>
        /// <param name="name">Name of the folder</param>
        /// <param name="folder">Handle to folder in project</param>
        /// <returns>True if folder added or false if not</returns>
        public bool AddFolder(string name, out SledProjectFilesFolderType folder)
        {
            return AddFolder(name, null, out folder);
        }

        /// <summary>
        /// Add folder to project
        /// </summary>
        /// <param name="name">Name of the folder</param>
        /// <param name="parent">Folder to add the new folder to</param>
        /// <param name="folder">Handle to folder in project</param>
        /// <returns>True if folder added or false if not</returns>
        public bool AddFolder(string name, SledProjectFilesFolderType parent, out SledProjectFilesFolderType folder)
        {
            folder = null;

            if (string.IsNullOrEmpty(name))
                return false;

            if (!Active)
                return false;

            var domNode = new DomNode(SledSchema.SledProjectFilesFolderType.Type);
            
            folder = domNode.As<SledProjectFilesFolderType>();
            folder.Name = name;

            if (parent == null)
                ActiveProject.Folders.Add(folder);
            else
                parent.Folders.Add(folder);

            return true;
        }

        /// <summary>
        /// Remove a file from the project
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <returns>True if file removed or false if not in project and therefore not removed</returns>
        public bool RemoveFile(string absPath)
        {
            var file = m_projectFileFinderService.Get.Find(absPath);
            if (file == null)
                return false;
            
            var domParent = file.DomNode.Parent;
            if (domParent == null)
                return false;

            if (!domParent.Is<SledProjectFilesFolderType>())
                return false;

            var folder = domParent.As<SledProjectFilesFolderType>();

            folder.Files.Remove(file);
            return true;
        }

        #endregion

        #region ISledLessProjectSerivce Interface

        public bool CreateProject(string name, string projDir, string assetDir, IEnumerable<string> absPathFiles)
        {
            bool bRetval;

            try
            {
                var szAbsSpfPath = Path.Combine(projDir + Path.DirectorySeparatorChar, name + ".spf");

                var uri = new Uri(szAbsSpfPath);
                var guid = GetExistingProjectGuid(uri, SchemaLoader);

                // Does this exist?
                var projectFileFinderService =
                    SledServiceInstance.TryGet<ISledProjectFileFinderService>();

                var domNode =
                    new DomNode(
                        SledSchema.SledProjectFilesType.Type,
                        SledSchema.SledProjectFilesRootElement);

                // Create project
                var tempProject = domNode.As<SledProjectFilesType>();
                tempProject.Name = name;
                tempProject.Uri = uri;
                tempProject.AssetDirectory = assetDir;
                tempProject.Guid = guid;

                // Add each file to the project
                foreach (var file in absPathFiles)
                {
                    var projFile = SledProjectFilesFileType.Create(file, tempProject);
                    if (projFile == null)
                        continue;

                    // Check for duplicates
                    if (projectFileFinderService != null)
                    {
                        // Check if this file already exists in the project
                        var projFoundFile = projectFileFinderService.Find(projFile, tempProject);

                        // Actual file that projFile represents is in the
                        // project so this is a duplicate
                        if (projFoundFile != null)
                            continue;
                    }

                    // Add file to project
                    tempProject.Files.Add(projFile);
                }

                // Write .spf to disk
                var writer = new SledSpfWriter(SchemaLoader.TypeCollection);
                writer.Write(tempProject.DomNode, uri, false);

                bRetval = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                bRetval = false;
            }

            return bRetval;
        }

        private static Guid GetExistingProjectGuid(Uri uri, XmlSchemaTypeLoader schemaLoader)
        {
            // No file so make up guid
            if (!File.Exists(uri.LocalPath))
                return SledUtil.MakeXmlSafeGuid();

            // Look for guid
            var retval = Guid.Empty;

            try
            {
                var reader = new SledSpfReader(schemaLoader);
                var root = reader.Read(uri, false);
                var project = root.As<SledProjectFilesType>();

                retval = project.Guid;
            }
            catch (Exception ex)
            {
                ex.ToString();
                retval = Guid.Empty;
            }
            finally
            {
                if (retval == Guid.Empty)
                    retval = SledUtil.MakeXmlSafeGuid();
            }

            return retval;
        }

        #endregion

        #region ISledDocumentPlugin Interface

        /// <summary>
        /// Gets context menu command tags for the target SledDocument
        /// </summary>
        /// <param name="args">Arguments (document, region clicked, line number clicked)</param>
        /// <returns>List of context menu command tags for the target SledDocument</returns>
        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            var commands =
                new List<object>
                    {
                        args.Document.SledProjectFile == null
                            ? Command.AddFile
                            : Command.RemoveFile
                    };

            return commands;
        }

        /// <summary>
        /// Gets values for hovered over tokens
        /// </summary>
        /// <param name="args">Arguments (document, token, line number)</param>
        /// <returns>List of strings representing possible values for the hovered over token</returns>
        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return null;
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            // Determine if this document is in the project or not
            // and set up references

            if (!Active)
                return;

            var sd = e.Document;

            // Search for file
            var projFile = m_projectFileFinderService.Get.Find(sd);
            if (projFile == null)
                return;

            // Set up references
            projFile.SledDocument = sd;
            sd.SledProjectFile = projFile;

            sd.UriChanged += SledDocumentUriChanged;

            // Fire event
            FileOpened.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, projFile));
        }

        private void SledDocumentUriChanged(object sender, UriChangedEventArgs e)
        {
            var sd = sender as ISledDocument;
            if (sd == null)
                return;

            var file = sd.SledProjectFile;
            if (file == null)
                return;

            // Update name & path since this is a project file
            file.Name = Path.GetFileName(sd.Uri.LocalPath);
            file.Path = SledUtil.GetRelativePath(sd.Uri.LocalPath, AssetDirectory);
            file.Uri = sd.Uri;
        }

        private void DocumentServiceClosing(object sender, SledDocumentServiceEventArgs e)
        {
            if (!Active)
                return;

            var projFile = e.Document.SledProjectFile;

            // File not in project so we don't care
            if (projFile == null)
                return;

            // Fire event
            FileClosing.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, projFile));
        }

        private void DocumentServiceClosed(object sender, SledDocumentServiceEventArgs e)
        {
            if (!Active)
                return;

            var projFile = e.Document.SledProjectFile;

            // File not in project so we don't care
            if (projFile == null)
                return;

            // Fire event
            FileClosed.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, projFile));

            e.Document.UriChanged -= SledDocumentUriChanged;

            // Remove references
            projFile.SledDocument = null;
            e.Document.SledProjectFile = null;            
        }

        #endregion

        #region ISledModifiedProjectFormService Events

        private void ModifiedProjectFormServiceChangesDetected(object sender, SledModifiedProjectChangesDetectedEventArgs e)
        {
            if (e.ShowingGui)
                return;

            //
            // If the GUI isn't going to be shown we want to make
            // sure we save the project settings so that any
            // in-memory stuff that may have gotten removed during
            // the other program overwriting the SLED project file
            // can be preserved. This also handles when changes have
            // been reverted and the project needs to be saved.
            //

            SaveSettings();
        }

        private void ModifiedProjectFormServiceGuiChangesSubmitted(object sender, SledModifiedProjectChangesEventArgs e)
        {
            foreach (var change in e.AcceptedChanges)
            {
                switch (change.ChangeType)
                {
                    case SledModifiedProjectChangeType.Name:
                    {
                        var changeName = (SledModifiedProjectNameChange)change;
                        ActiveProject.Name = changeName.NewName;
                    }
                    break;

                    case SledModifiedProjectChangeType.Guid:
                    {
                        var changeGuid = (SledModifiedProjectGuidChange)change;
                        ActiveProject.Guid = changeGuid.NewGuid;
                    }
                    break;

                    case SledModifiedProjectChangeType.AssetDir:
                    {
                        var changeAssetDir = (SledModifiedProjectAssetDirChange)change;
                        ChangeAssetDirectoryNoGui(changeAssetDir.OldDirectory, changeAssetDir.NewDirectory, true, false);
                    }
                    break;

                    case SledModifiedProjectChangeType.FileAdded:
                    {
                        var changeFileAdded = (SledModifiedProjectFileAddedChange)change;

                        SledProjectFilesFileType projFile;
                        AddFile(changeFileAdded.AbsolutePath, out projFile);
                    }
                    break;

                    case SledModifiedProjectChangeType.FileRemoved:
                    {
                        var changeFileRemoved = (SledModifiedProjectFileRemovedChange)change;
                        RemoveFile(changeFileRemoved.AbsolutePath);
                    }
                    break;
                }
            }

            SaveSettings();
        }

        #endregion

        #region DomNode Events

        private void DomNodeAttributeChanging(object sender, AttributeEventArgs e)
        {
            if (e.DomNode.Type == SledSchema.SledProjectFilesType.Type)
            {
                if ((e.AttributeInfo == SledSchema.SledProjectFilesBaseType.nameAttribute) ||
                    (e.AttributeInfo == SledSchema.SledProjectFilesFolderType.nameAttribute) ||
                    (e.AttributeInfo == SledSchema.SledProjectFilesType.nameAttribute))
                {
                    var oldName = string.IsNullOrEmpty(e.OldValue as string) ? string.Empty : e.OldValue.ToString();
                    var newName = string.IsNullOrEmpty(e.NewValue as string) ? string.Empty :  e.NewValue.ToString();

                    NameChanging.Raise(this, new SledProjectServiceProjectNameEventArgs(ActiveProject, oldName, newName));
                }
                else if (e.AttributeInfo == SledSchema.SledProjectFilesType.guidAttribute)
                {
                    var oldGuid = string.IsNullOrEmpty(e.OldValue as string) ? Guid.Empty : new Guid(e.OldValue.ToString());
                    var newGuid = string.IsNullOrEmpty(e.NewValue as string) ? Guid.Empty : new Guid(e.NewValue.ToString());

                    GuidChanging.Raise(this, new SledProjectServiceProjectGuidEventArgs(ActiveProject, oldGuid, newGuid));
                }
                else if (e.AttributeInfo == SledSchema.SledProjectFilesType.assetdirectoryAttribute)
                {
                    var oldAssetDir = string.IsNullOrEmpty(e.OldValue as string) ? string.Empty : e.OldValue.ToString();
                    var newAssetDir = string.IsNullOrEmpty(e.NewValue as string) ? string.Empty : e.NewValue.ToString();

                    AssetDirChanging.Raise(this, new SledProjectServiceAssetDirEventArgs(ActiveProject, oldAssetDir, newAssetDir));
                }
            }

            if (e.DomNode.Type == SledSchema.SledProjectFilesFileType.Type)
            {
                if (e.AttributeInfo == SledSchema.SledProjectFilesFileType.pathAttribute)
                {
                    var file = e.DomNode.As<SledProjectFilesFileType>();

                    var assetDir = AssetDirectory;

                    var oldPath = e.OldValue as string;
                    oldPath = SledUtil.GetAbsolutePath(oldPath, assetDir);

                    var newPath = e.NewValue as string;
                    newPath = SledUtil.GetAbsolutePath(newPath, assetDir);

                    // Fire event
                    FileRenaming.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, file, oldPath, newPath));

                    // Update path
                    if (file.SledDocument != null)
                        file.SledDocument.Uri = new Uri(newPath);

                    Dirty = true;
                }
            }
            else if (e.DomNode.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                if (e.AttributeInfo == SledSchema.SledProjectFilesBaseType.nameAttribute)
                {
                    var folder = e.DomNode.As<SledProjectFilesFolderType>();

                    var oldName = e.OldValue as string;
                    var newName = e.NewValue as string;

                    // Fire event
                    FolderRenaming.Raise(this, new SledProjectServiceFolderEventArgs(ActiveProject, folder, oldName, newName));

                    Dirty = true;
                }
            }
        }

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (e.DomNode.Type == SledSchema.SledProjectFilesType.Type)
            {
                if ((e.AttributeInfo == SledSchema.SledProjectFilesBaseType.nameAttribute) ||
                    (e.AttributeInfo == SledSchema.SledProjectFilesFolderType.nameAttribute) ||
                    (e.AttributeInfo == SledSchema.SledProjectFilesType.nameAttribute))
                {
                    var oldName = string.IsNullOrEmpty(e.OldValue as string) ? string.Empty : e.OldValue.ToString();
                    var newName = string.IsNullOrEmpty(e.NewValue as string) ? string.Empty : e.NewValue.ToString();

                    NameChanged.Raise(this, new SledProjectServiceProjectNameEventArgs(ActiveProject, oldName, newName));
                }
                else if (e.AttributeInfo == SledSchema.SledProjectFilesType.guidAttribute)
                {
                    var oldGuid = string.IsNullOrEmpty(e.OldValue as string) ? Guid.Empty : new Guid(e.OldValue.ToString());
                    var newGuid = string.IsNullOrEmpty(e.NewValue as string) ? Guid.Empty : new Guid(e.NewValue.ToString());

                    GuidChanged.Raise(this, new SledProjectServiceProjectGuidEventArgs(ActiveProject, oldGuid, newGuid));
                }
                else if (e.AttributeInfo == SledSchema.SledProjectFilesType.assetdirectoryAttribute)
                {
                    var oldAssetDir = string.IsNullOrEmpty(e.OldValue as string) ? string.Empty : e.OldValue.ToString();
                    var newAssetDir = string.IsNullOrEmpty(e.NewValue as string) ? string.Empty : e.NewValue.ToString();

                    AssetDirChanged.Raise(this, new SledProjectServiceAssetDirEventArgs(ActiveProject, oldAssetDir, newAssetDir));
                }
            }

            if (e.DomNode.Type == SledSchema.SledProjectFilesFileType.Type)
            {
                if (e.AttributeInfo == SledSchema.SledProjectFilesFileType.pathAttribute)
                {
                    var file = e.DomNode.As<SledProjectFilesFileType>();

                    var assetDir = AssetDirectory;

                    var oldPath = e.OldValue as string;
                    oldPath = SledUtil.GetAbsolutePath(oldPath, assetDir);

                    var newPath = e.NewValue as string;
                    newPath = SledUtil.GetAbsolutePath(newPath, assetDir);

                    // Fire event
                    FileRenamed.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, file, oldPath, newPath));

                    Dirty = true;
                }
            }
            else if (e.DomNode.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                if (e.AttributeInfo == SledSchema.SledProjectFilesBaseType.nameAttribute)
                {
                    var folder = e.DomNode.As<SledProjectFilesFolderType>();

                    var oldName = e.OldValue as string;
                    var newName = e.NewValue as string;

                    // Fire event
                    FolderRenamed.Raise(this, new SledProjectServiceFolderEventArgs(ActiveProject, folder, oldName, newName));

                    Dirty = true;
                }
            }
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (e.Child.Type == SledSchema.SledProjectFilesFileType.Type)
            {
                var file = e.Child.As<SledProjectFilesFileType>();

                // Check if any open documents correspond to this file
                ISledDocument sd;
                if (DocumentService.IsOpen(new Uri(file.AbsolutePath), out sd))
                {
                    // Set up references
                    file.SledDocument = sd;
                    sd.SledProjectFile = file;
                }

                // Fire event
                FileAdded.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, file));

                Dirty = true;
            }
            else if (e.Child.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                var folder = e.Child.As<SledProjectFilesFolderType>();

                // Fire event
                FolderAdded.Raise(this, new SledProjectServiceFolderEventArgs(ActiveProject, folder));

                Dirty = true;
            }
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (e.Child.Type == SledSchema.SledProjectFilesFileType.Type)
            {
                var file = e.Child.As<SledProjectFilesFileType>();

                // Fire event
                FileRemoving.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, file));
            }
            else if (e.Child.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                var folder = e.Child.As<SledProjectFilesFolderType>();

                // Fire event
                FolderRemoving.Raise(this, new SledProjectServiceFolderEventArgs(ActiveProject, folder));
            }
        }

        private void DomNodeChildRemoved(object sender, ChildEventArgs e)
        {
            if (e.Child.Type == SledSchema.SledProjectFilesFileType.Type)
            {
                var file = e.Child.As<SledProjectFilesFileType>();

                // Fire event
                FileRemoved.Raise(this, new SledProjectServiceFileEventArgs(ActiveProject, file));

                // Set references
                if (file.SledDocument != null)
                {
                    file.SledDocument.SledProjectFile = null;
                    file.SledDocument = null;
                }

                Dirty = true;
            }
            else if (e.Child.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                var folder = e.Child.As<SledProjectFilesFolderType>();

                // Fire event
                FolderRemoved.Raise(this, new SledProjectServiceFolderEventArgs(ActiveProject, folder));

                Dirty = true;
            }
        }

        #endregion

        #region Member Methods

        private bool NewShared(string szAbsSpfPath, string szName, string szAbsAssetDir, Guid guid, bool bRecursiveFileAdd)
        {
            // Add to most recently used project list
            m_mruProjects.Add(szAbsSpfPath);

            // -----------------------------------------------------

            ActiveProject =
                CreateProject(
                    new Uri(szAbsSpfPath),
                    szName,
                    szAbsAssetDir,
                    guid,
                    true);

            SubscribeToEvents();

            // -----------------------------------------------------

            // Add in any files if told to auto-add stuff
            if (bRecursiveFileAdd)
                ShowAutoAddFilesForm();

            m_projectFilesUtilityService.Get.SetupFiles(AllFiles);

            // -----------------------------------------------------

            // Fire event
            Created.Raise(this, new SledProjectServiceProjectEventArgs(ActiveProject));

            SaveSettings();

            return true;
        }

        private void AddFile(ISledDocument sd)
        {
            var project = ActiveProject;

            bool bAlreadyInProject;
            var file =
                m_projectFilesUtilityService.Get.CreateFrom(sd, project, out bAlreadyInProject);
            
            if (bAlreadyInProject)
                return;

            // Add to project (MasterCollection_ChildInserted will handle firing the event)
            project.Files.Add(file);

            SaveSettings();
        }

        private void RemoveFile(ISledDocument sd)
        {
            // Grab project file version
            var file = sd.SledProjectFile;

            // Grab parent folder
            var folder =
                file.DomNode.Parent.As<SledProjectFilesFolderType>();

            // Remove file from project (events (removing & removed) get fired
            // from MasterCollection_ChildRemoving & MasterCollection_ChildRemoved)
            folder.Files.Remove(file);

            SaveSettings();
        }

        private void SubscribeToEvents()
        {
            if (ActiveProject == null)
                return;

            if (ActiveProject.DomNode == null)
                return;

            ActiveProject.DomNode.AttributeChanging += DomNodeAttributeChanging;
            ActiveProject.DomNode.AttributeChanged += DomNodeAttributeChanged;
            ActiveProject.DomNode.ChildInserted += DomNodeChildInserted;
            ActiveProject.DomNode.ChildRemoving += DomNodeChildRemoving;
            ActiveProject.DomNode.ChildRemoved += DomNodeChildRemoved;
        }

        private void UnsubscribeFromEvents()
        {
            if (ActiveProject == null)
                return;

            if (ActiveProject.DomNode == null)
                return;

            ActiveProject.DomNode.AttributeChanging -= DomNodeAttributeChanging;
            ActiveProject.DomNode.AttributeChanged -= DomNodeAttributeChanged;
            ActiveProject.DomNode.ChildInserted -= DomNodeChildInserted;
            ActiveProject.DomNode.ChildRemoving -= DomNodeChildRemoving;
            ActiveProject.DomNode.ChildRemoved -= DomNodeChildRemoved;
        }

        private void ShowAutoAddFilesForm()
        {
            using (var dlg = new SledProjectAutoFilesAddForm())
            {
                // Look in these directories by default
                dlg.AddDirectory(ProjectDirectory);
                dlg.AddDirectory(AssetDirectory);

                // Gather extensions to be included and checked by default
                var fileExtensionService = SledServiceInstance.TryGet<ISledFileExtensionService>();
                if (fileExtensionService != null)
                    dlg.Extensions = fileExtensionService.AllExtensions;

                // If OK, add selected files to the project
                var res = dlg.ShowDialog(m_mainForm);
                if (res != DialogResult.OK)
                    return;

                // Add files to project
                foreach (string file in dlg.CheckedFiles)
                {
                    SledProjectFilesFileType projFile;
                    AddFile(file, out projFile);
                }
            }
        }

        private void ShowProjectPaths()
        {
            using (var dlg = new SledViewProjectPathsForm())
            {
                dlg.ProjectName = ProjectName;
                dlg.ProjectDirectory = ProjectDirectory;
                dlg.AssetDirectory = AssetDirectory;
                dlg.ProjectGuid = ProjectGuid.ToString();
                dlg.ProjectFiles = AllFiles;
                dlg.ShowDialog(m_mainForm);
            }
        }

        private void OpenFileInProject()
        {
            using (var dialog = new SledProjectOpenFile())
            {
                dialog.Files = AllFiles;
                if (dialog.ShowDialog(m_mainForm) != DialogResult.OK)
                    return;

                foreach (var file in dialog.SelectedFiles)
                {
                    ISledDocument sd;
                    DocumentService.Open(file.Uri, out sd);
                }
            }
        }

        private void ChangeAssetDirectory(bool bWarn)
        {
            if (bWarn)
            {
                // Ask if user wants to change
                var res =
                    MessageBox.Show(
                        m_mainForm,
                        Localization.SledAssetDirectoryErrorDoesNotExist,
                        Localization.SledAssetDirectoryErrorTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                // Get out if user doesn't want to change
                if (res != DialogResult.Yes)
                    return;
            }

            var assetDirOld = AssetDirectory;
            string assetDirNew;

            // Search for a new asset directory
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = Localization.SledAssetDirectorySelect;
                dlg.ShowNewFolderButton = true;
                dlg.RootFolder = Environment.SpecialFolder.Desktop;

                // Start in original asset directory if it exists
                if (Directory.Exists(assetDirOld))
                    dlg.SelectedPath = assetDirOld;

                // Show dialog
                if (dlg.ShowDialog(m_mainForm) == DialogResult.OK)
                {
                    // Add directory separator character to the end if not there
                    assetDirNew = dlg.SelectedPath;
                    if (!assetDirNew.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        assetDirNew = assetDirNew + Path.DirectorySeparatorChar;
                }
                else
                {
                    // User doesn't care about changing; bail
                    if (bWarn)
                        ShowInvalidAssetDirectoryWarning();

                    return;
                }
            }

            // Default value is to generate new relative
            // paths from asset directory to scripts
            var bOption1 = true;

            var bShowResolutionGui = AllFiles.Count > 0;

            // Show change asset directory resolution form
            if (bShowResolutionGui)
            {
                using (var dlg = new SledChangeAssetDirectoryForm())
                {
                    dlg.OldAssetDirectory = assetDirOld;
                    dlg.NewAssetDirectory = assetDirNew;
                    dlg.Files = AllFiles;

                    // Show dialog
                    if (dlg.ShowDialog(m_mainForm) != DialogResult.OK)
                    {
                        // User doesn't care about changing; bail
                        if (bWarn)
                            ShowInvalidAssetDirectoryWarning();

                        return;
                    }

                    bOption1 = dlg.Option1;
                }
            }

            // Do the rest of the work
            ChangeAssetDirectoryNoGui(assetDirOld, assetDirNew, bOption1, true);
        }

        private void ChangeAssetDirectoryNoGui(string oldAssetDir, string newAssetDir, bool bOption1, bool bSaveSettings)
        {
            if (!Active)
                return;

            // Fire event
            AssetDirChanging.Raise(this, new SledProjectServiceAssetDirEventArgs(ActiveProject, oldAssetDir, newAssetDir));

            // Update asset directory
            ActiveProject.AssetDirectory = newAssetDir;

            // Make change to files based on what the user selected
            ChangeAssetDirectoryUpdateFilePaths(oldAssetDir, newAssetDir, AllFiles, bOption1);

            // Fire event
            AssetDirChanged.Raise(this, new SledProjectServiceAssetDirEventArgs(ActiveProject, oldAssetDir, newAssetDir));

            // Save changes to disk
            if (bSaveSettings)
                SaveSettings();
        }

        private static void ChangeAssetDirectoryUpdateFilePaths(string oldAssetDir, string newAssetDir, IEnumerable<SledProjectFilesFileType> files, bool bOption1)
        {
            //
            // bOption1 == true means the files haven't
            // moved on disk and relative paths to them
            // should be created from the new asset directory.
            //
            // bOption1 == false means the files have moved
            // on disk and now live under the new asset
            // directory.
            //

            // Update file paths to reflect asset directory change
            foreach (var file in files)
            {
                var absPath =
                    bOption1
                        ? SledUtil.GetAbsolutePath(file.Path, oldAssetDir)
                        : newAssetDir + file.Path;

                // Be sure to update
                file.Uri = new Uri(absPath);
                file.Path = SledUtil.GetRelativePath(absPath, newAssetDir);
            }
        }

        private void ShowInvalidAssetDirectoryWarning()
        {
            // Show message indicating asset directory is still invalid
            MessageBox.Show(
                m_mainForm,
                Localization.SledAssetDirectoryErrorNotSet,
                Localization.SledAssetDirectoryErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public SledProjectFilesType CreateProject(Uri uri, string name, string assetDirectory, Guid guid, bool bNew)
        {
            SledProjectFilesType project;

            if (bNew)
            {
                var root =
                    new DomNode(
                        SledSchema.SledProjectFilesType.Type,
                        SledSchema.SledProjectFilesRootElement);

                project = root.As<SledProjectFilesType>();
                project.Uri = uri;
                project.Name = name;
                project.AssetDirectory = assetDirectory;
                project.Guid = guid;
                project.Dirty = true;
            }
            else
            {
                var reader = new SledSpfReader(SchemaLoader);
                var root = reader.Read(uri, true);

                project = root.As<SledProjectFilesType>();
                project.Uri = uri;
                project.Dirty = false;
            }

            return project;
        }

        private List<string> GetMruList()
        {
            var lstMru =
                new List<string>(
                    m_mruProjects.MostRecentOrder);

            var padding = MaxMruCount - lstMru.Count;
            for (var i = 0; i < padding; i++)
                lstMru.Add(string.Empty);

            return lstMru;
        }

        private void SaveSettingsTimerTick(object sender, EventArgs e)
        {
            if (!m_saveSettingsNeeded)
                return;

            var span = DateTime.Now.Subtract(m_saveSettingsTime);
            if (span.Milliseconds < 250)
                return;

            try
            {
                SaveSettings();
            }
            finally
            {
                m_saveSettingsNeeded = false;
            }
        }

        #endregion

        public const string ProjectFilter = "SLED Projects (*.spf, *.lpf)|*.spf;*.lpf";

        public static readonly string ProjectExtension = Resources.Resource.ProjectFileExtensionSpf;

        public static readonly string[] ProjectExtensions =
        {
            Resources.Resource.ProjectFileExtensionLpf,
            Resources.Resource.ProjectFileExtensionSpf
        };

        private bool m_saveSettingsNeeded;
        private bool m_bOpenLastProjectOnStartup;
        private DateTime m_saveSettingsTime;
        
        private readonly MainForm m_mainForm;
        private readonly string m_startupProject;

        [Import(AllowDefault = true)]
        protected ISledDocumentService DocumentService;

        [Import(AllowDefault = true)]
        protected ICommandService CommandService;

        [Import(AllowDefault = true)]
        protected ISettingsService SettingsService;

        [Import]
        protected SledSharedSchemaLoader SchemaLoader;

        private const int MaxMruCount = 8;

        private readonly ActiveCollection<string> m_mruProjects =
            new ActiveCollection<string>(MaxMruCount);

        private static readonly IEnumerable<object> s_emptyCommands =
            EmptyEnumerable<object>.Instance;

        private static readonly IEnumerable<SledProjectFilesFileType> s_emptyFiles =
            EmptyEnumerable<SledProjectFilesFileType>.Instance;

        private readonly SledServiceReference<ISledProjectFileFinderService> m_projectFileFinderService =
            new SledServiceReference<ISledProjectFileFinderService>();

        private readonly SledServiceReference<SledProjectFilesUtilityService> m_projectFilesUtilityService =
            new SledServiceReference<SledProjectFilesUtilityService>();
    }
}
