/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

using Timerz = System.Windows.Forms.Timer;

namespace Sce.Sled
{
    /// <summary>
    /// SledProjectFilesDiskViewEditor Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledProjectFilesDiskViewEditor))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledProjectFilesDiskViewEditor :
        IInitializable,
        IPartImportsSatisfiedNotification,
        IControlHostClient,
        ICommandClient,
        IContextMenuCommandProvider,
        ISourceControlContext,
        IItemView,
        ITreeListView,
        IObservableContext,
        IValidationContext,
        ISelectionContext
    {
        [ImportingConstructor]
        public SledProjectFilesDiskViewEditor(
            MainForm mainForm,
            IContextRegistry contextRegistry,
            ICommandService commandService,
            ISettingsService settingsService,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_tools = new ToolStrip();
            m_host = new UserControl();
            m_editor = new DiskTreeEditor();
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_settingsService = settingsService;
            m_controlHostService = controlHostService;
            m_syncContext = SynchronizationContext.Current;
            
            m_selection.Changing += TheSelectionChanging;
            m_selection.Changed += TheSelectionChanged;

            m_refreshButton = new ToolStripButton { ToolTipText = RefreshButtonText };
            m_showOnlyProjFileFilters = new ToolStripButton { ToolTipText = ShowOnlyProjFilesText };
            m_applyFileFilters = new ToolStripButton { ToolTipText = ApplyFileFiltersText };
            m_txtFileFilters = new ToolStripTextBox();

            m_applyFiltersTimer = new System.Windows.Forms.Timer { Interval = 50 };
            m_applyFiltersTimer.Tick += ApplyFiltersTimerTick;
            m_applyFiltersTimer.Start();
            m_applyFiltersLastTime = DateTime.Now;

            m_syncContext = SynchronizationContext.Current;
            m_isHidden = fsi => (fsi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            m_filePredicate = fi => !m_isHidden(fi);
            m_dirPredicate = di => { return di.Parent == null ? true : !m_isHidden(di); };

            // Stop compiler warning
            if (Reloaded == null) return;
            if (Cancelled == null) return;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            var name = m_editor.TreeListView.Name;

            // Satisfy any MEF imports/exports on the TreeListView
            SledServiceReferenceCompositionContainer.Get.SatisfyImportsOnce(m_editor);

            // Persist column settings (could call m_editor.Initialize() but we want
            // to host more controls than just the TreeListView and the TreeListView's
            // Initialize() function will register the control and we don't want that).
            {
                var owner = string.Format("{0}-{1}-TreeListView-Settings", this, name);

                m_settingsService.RegisterSettings(
                    SledUtil.GuidFromString(owner),
                    new BoundPropertyDescriptor(
                        m_editor.TreeListView,
                        () => m_editor.TreeListView.PersistedSettings,
                        owner,
                        null,
                        owner),
                    new BoundPropertyDescriptor(
                        this,
                        () => ShowOnlyProjectFiles,
                        "ShowOnlyProjectFiles",
                        null,
                        "ShowOnlyProjectFiles"),
                    new BoundPropertyDescriptor(
                        this,
                        () => UseFileFilters,
                        "UseFileFilters",
                        null,
                        "UseFileFilters"),
                    new BoundPropertyDescriptor(
                        this,
                        () => FileFilterString,
                        "FileFilterString",
                        null,
                        "FileFilterString"));
            }

            m_editor.TreeListView.AllowLabelEdit = false;
            m_editor.TreeListView.Control.MouseDown += (s, e) => StartValidating();
            m_editor.TreeListView.Control.MouseUp += (s, e) => EndValidating();
            m_editor.TreeListViewAdapter.ItemLazyLoad += TreeListViewAdapterItemLazyLoad;
            // TODO: Enable at some point
            //m_editor.TreeListViewAdapter.AfterItemLabelEdit += TreeListViewAdapterAfterItemLabelEdit;
            m_editor.TreeListViewAdapter.MouseDoubleClick += TreeListViewAdapterMouseDoubleClick;

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;
            m_projectService.Closed += ProjectServiceClosed;
            m_projectService.SavedAsing += ProjectServiceSavedAsing;
            m_projectService.SavedAsed += ProjectServiceSavedAsed;

            int yOffset;

            {
                m_tools.Location = new Point(0, 0);
                m_tools.Width = m_host.Width;
                m_tools.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                yOffset = m_tools.Height;
            }

            // Add refresh button
            {
                var image = ResourceUtil.GetImageList16().Images[Atf.Resources.DocumentRefreshImage];
                m_refreshButton.Enabled = false;
                m_refreshButton.Image = image;
                m_refreshButton.Click += RefreshButtonClick;
                m_tools.Items.Add(m_refreshButton);
            }

            // Add show only projectfiles button to the toolbar
            {
                var image = ResourceUtil.GetImageList16().Images[Atf.Resources.DataImage];
                m_showOnlyProjFileFilters.Image = image;
                m_showOnlyProjFileFilters.Click += ShowOnlyProjFileFiltersClick;
                m_tools.Items.Add(m_showOnlyProjFileFilters);
            }

            // Add file filters button to the toolbar
            {
                var image = ResourceUtil.GetImageList16().Images[Atf.Resources.ByCategoryImage];
                m_applyFileFilters.Image = image;
                m_applyFileFilters.Click += ApplyFileFiltersOnClick;
                m_tools.Items.Add(m_applyFileFilters);
            }

            // Add filter text box to the toolbar
            {
                m_txtFileFilters.TextChanged += TxtFileFiltersTextChanged;
                m_tools.Items.Add(m_txtFileFilters);
            }

            // Position and set up TreeListView
            {
                m_editor.TreeListView.Control.Location = new Point(0, yOffset);
                m_editor.TreeListView.Control.Width = m_host.Width;
                m_editor.TreeListView.Control.Height = m_host.Height - yOffset;
                m_editor.TreeListView.Control.Anchor =
                    AnchorStyles.Bottom | AnchorStyles.Left |
                    AnchorStyles.Right | AnchorStyles.Top;
            }

            m_host.Controls.Add(m_tools);
            m_host.Controls.Add(m_editor.TreeListView);

            var controlImage = ResourceUtil.GetImageList16().Images[Atf.Resources.ResourceFolderImage];
            var controlInfo = new ControlInfo(name, name, StandardControlGroup.Left, controlImage);
            m_controlHostService.RegisterControl(m_host, controlInfo, this);

            {
                var menu = m_commandService.RegisterMenu(Menu.SledProjectFilesDiskView, Description, Description);

                m_commandService.RegisterCommand(
                    Command.Open,
                    Menu.SledProjectFilesDiskView,
                    Group.File,
                    "Open",
                    "Open a file or folder",
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);

                //m_commandService.RegisterCommand(
                //    Command.Rename,
                //    Menu.SledProjectFilesDiskView,
                //    Group.File,
                //    "Rename",
                //    "Rename a file or folder",
                //    Keys.None,
                //    null,
                //    CommandVisibility.Menu,
                //    this);

                //m_commandService.RegisterCommand(
                //    Command.Delete,
                //    Menu.SledProjectFilesDiskView,
                //    Group.File,
                //    "Delete",
                //    "Delete a file or folder",
                //    Keys.None,
                //    Atf.Resources.DeleteImage,
                //    CommandVisibility.Menu,
                //    this);

                m_commandService.RegisterCommand(
                    Command.ProjectAdd,
                    Menu.SledProjectFilesDiskView,
                    Group.Project,
                    "Project" + "/" + "Add File(s)",
                    "Add a file to the project",
                    Keys.None,
                    SledIcon.ProjectFileAdd,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.ProjectRemove,
                    Menu.SledProjectFilesDiskView,
                    Group.Project,
                    "Project" + "/" + "Remove File(s)",
                    "Remove a file from the project",
                    Keys.None,
                    SledIcon.ProjectFileRemove,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.OpenWith,
                    Menu.SledProjectFilesDiskView,
                    Group.Win32,
                    "Open With",
                    "Open an item using the Open With dialog",
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.Explore,
                    Menu.SledProjectFilesDiskView,
                    Group.Win32,
                    "Explore",
                    "Open a folder",
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.AddRoot,
                    Menu.SledProjectFilesDiskView,
                    Group.Root,
                    "Add Root...",
                    "Add a root directory",
                    Keys.None,
                    Atf.Resources.ResourceFolderImage,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.RemoveRoot,
                    Menu.SledProjectFilesDiskView,
                    Group.Root,
                    "Remove Root",
                    "Remove a root directory",
                    Keys.None,
                    Atf.Resources.ResourceFolderImage,
                    CommandVisibility.Menu,
                    this);

                m_commandService.RegisterCommand(
                    Command.CollapseAll,
                    Menu.SledProjectFilesDiskView,
                    Group.Tree,
                    "Collapse All",
                    "Collapse all",
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);

                menu.GetMenuItem().Visible = false;
            }
        }

        #endregion

        #region IPartImportsSatisfiedNotification Interface

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_sourceControlService == null)
                return;

            m_sourceControlService.StatusChanged += SourceControlServiceStatusChanged;
        }

        #endregion

        #region IControlHostClient Interface

        public void Activate(Control control)
        {
            if (!ReferenceEquals(m_tools, control) &&
                !ReferenceEquals(m_editor.TreeListView, control) &&
                !ReferenceEquals(m_host, control))
                return;

            m_contextRegistry.ActiveContext = this;
        }

        public void Deactivate(Control control)
        {
            if (!ReferenceEquals(m_tools, control) &&
                !ReferenceEquals(m_editor.TreeListView, control) &&
                !ReferenceEquals(m_host, control))
                return;

            m_contextRegistry.RemoveContext(this);
        }

        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            return
                m_projectService.Active &&
                CanDoCommand((Command)commandTag, m_editor.SelectionAs<DiskTreeItem>());
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            var selection = m_editor.SelectionAs<DiskTreeItem>();

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.Open:
                    ContextMenuOpen(selection);
                    break;

                //case Command.Rename:
                //    ContextMenuRename(selection.FirstOrDefault());
                //    break;

                //case Command.Delete:
                //    ContextMenuDelete(selection.FirstOrDefault());
                //    break;

                case Command.ProjectAdd:
                    ContextMenuProjectAdd(selection);
                    break;

                case Command.ProjectRemove:
                    ContextMenuProjectRemove(selection);
                    break;

                case Command.OpenWith:
                    ContextMenuOpenWith(selection.FirstOrDefault());
                    break;

                case Command.Explore:
                    ContextMenuExplore(selection.FirstOrDefault());
                    break;

                case Command.AddRoot:
                    ContextMenuAddRoot();
                    break;

                case Command.RemoveRoot:
                    ContextMenuRemoveRoot(selection.FirstOrDefault());
                    break;

                case Command.CollapseAll:
                    m_editor.TreeListView.CollapseAll();
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Interface

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (!ReferenceEquals(context, this))
                yield break;

            if (!m_projectService.Active)
                yield break;

            var selection = m_editor.SelectionAs<DiskTreeItem>();
            foreach (Command cmd in Enum.GetValues(typeof(Command)))
            {
                if (CanDoCommand(cmd, selection))
                    yield return cmd;
            }
        }

        #endregion

        #region ISourceControlContext Interface

        public IEnumerable<IResource> Resources
        {
            get
            {
                return m_editor.TreeListViewAdapter.Selection
                    .Where(r => r.Is<IResource>())
                    .Select(r => r.As<IResource>());
            }
        }

        #endregion

        #region IItemView Interface

        public void GetInfo(object item, ItemInfo info)
        {
            var itemView = item.As<IItemView>();
            if (itemView == null)
                return;

            itemView.GetInfo(item, info);
        }

        #endregion

        #region ITreeListView Interface

        public IEnumerable<object> Roots
        {
            get { return m_roots.Where(i => i.Visible).Cast<object>(); }
        }

        public IEnumerable<object> GetChildren(object parent)
        {
            if (parent == null)
                yield break;

            var diskTreeItem = parent.As<DiskTreeItem>();
            if (diskTreeItem == null)
                yield break;

            foreach (var child in diskTreeItem.Children.Where(i => i.Visible))
                yield return child;
        }

        public string[] ColumnNames
        {
            get { return s_columns; }
        }

        #endregion

        #region IObservableContext Interface

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public event EventHandler Reloaded;

        #endregion

        #region IValidationContext Interface

        public event EventHandler Beginning;

        public event EventHandler Cancelled;

        public event EventHandler Ending;

        public event EventHandler Ended;

        #endregion

        #region ISelectionContext Interface

        public IEnumerable<object> Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        public IEnumerable<T> GetSelection<T>() where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        public T GetLastSelected<T>() where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        public bool SelectionContains(object item)
        {
            return m_selection.Contains(item);
        }

        public int SelectionCount
        {
            get { return m_selection.Count; }
        }

        public event EventHandler SelectionChanging;

        public event EventHandler SelectionChanged;

        #endregion

        #region Command Stuff

        private enum Command
        {
            Open,
            //Rename,
            //Delete,

            ProjectAdd,
            ProjectRemove,

            OpenWith,
            Explore,

            AddRoot,
            RemoveRoot,

            CollapseAll,
        }

        private enum Menu
        {
            SledProjectFilesDiskView,
        }

        private enum Group
        {
            File,
            Project,
            Win32,
            Root,
            Tree,
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            BindToProject(e.Project, ProjectEvent.Created);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            BindToProject(e.Project, ProjectEvent.Opened);
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            try
            {
                StartValidating();

                // Find and update the DiskTreeItem(s) that correspond to this project file

                List<DiskTreeItem> lstItems;
                if (m_fileMappings.TryGetValue(e.File.AbsolutePath, out lstItems))
                {
                    foreach (var diskTreeItem in lstItems)
                    {
                        diskTreeItem.ProjectFile = e.File;
                        diskTreeItem.Visible = IsVisible(diskTreeItem, m_showOnlyProjFiles, m_filters);
                    }
                }
            }
            finally
            {
                m_applyFiltersNeeded = true;
                m_applyFiltersLastTime = DateTime.Now;
            }
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            try
            {
                StartValidating();

                // Find and update the DiskTreeItem(s) that correspond to this project file

                List<DiskTreeItem> lstItems;
                if (m_fileMappings.TryGetValue(e.File.AbsolutePath, out lstItems))
                {
                    foreach (var diskTreeItem in lstItems)
                    {
                        diskTreeItem.ProjectFile = null;
                        diskTreeItem.Visible = IsVisible(diskTreeItem, m_showOnlyProjFiles, m_filters);
                    }
                }
            }
            finally
            {
                m_applyFiltersNeeded = true;
                m_applyFiltersLastTime = DateTime.Now;
            }
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            BindToProject(null, ProjectEvent.Closed);
            m_refreshButton.Enabled = false;
        }

        private void ProjectServiceSavedAsing(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            m_savedRootsForProjectSaveAs = new List<string>();
            foreach (var root in e.Project.Roots)
                m_savedRootsForProjectSaveAs.Add(root.Directory);

            e.Project.Roots.Clear();
        }

        private void ProjectServiceSavedAsed(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            if (m_savedRootsForProjectSaveAs == null)
                return;

            try
            {
                if (!m_savedRootsForProjectSaveAs.Contains(e.NewProjectDir, StringComparer.CurrentCultureIgnoreCase))
                    m_savedRootsForProjectSaveAs.Add(e.NewProjectDir);

                if (!m_savedRootsForProjectSaveAs.Contains(e.NewAssetDir, StringComparer.CurrentCultureIgnoreCase))
                    m_savedRootsForProjectSaveAs.Add(e.NewAssetDir);

                foreach (var savedAbsRootPath in m_savedRootsForProjectSaveAs.Distinct())
                {
                    var root =
                        new DomNode(SledSchema.SledProjectFilesRootType.Type)
                        .As<SledProjectFilesRootType>();

                    root.Directory = savedAbsRootPath;
                    e.Project.Roots.Add(root);
                }

                CompletelyReload();
            }
            finally
            {
                m_savedRootsForProjectSaveAs = null;
            }
        }

        #endregion

        #region SledSourceControlService Events

        private void SourceControlServiceStatusChanged(object sender, SourceControlEventArgs e)
        {
            // Find item and update its status

            DiskTreeItem diskTreeItem = null;

            foreach (var root in m_roots)
            {
                diskTreeItem =
                    root.FlattenedChildren.FirstOrDefault(
                        dti => string.Compare(dti.FullPath, e.Uri.LocalPath, true) == 0);

                if (diskTreeItem != null)
                    break;
            }

            if (diskTreeItem == null)
                return;

            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(diskTreeItem));
        }

        #endregion

        #region Persisted Settings

        public bool ShowOnlyProjectFiles
        {
            get { return m_showOnlyProjFileFilters.Checked; }
            set
            {
                var changed = value != m_showOnlyProjFileFilters.Checked;

                m_showOnlyProjFileFilters.Checked = value;

                if (changed)
                    Rebind();
            }
        }

        public bool UseFileFilters
        {
            get { return m_applyFileFilters.Checked; }
            set
            {
                var changed = value != m_applyFileFilters.Checked;

                m_applyFileFilters.Checked = value;

                if (changed)
                    Rebind();
            }
        }

        public string FileFilterString
        {
            get { return m_txtFileFilters.Text; }
            set { m_txtFileFilters.Text = value; }
        }

        #endregion

        #region Control Related Events

        private void TreeListViewAdapterItemLazyLoad(object sender, TreeListViewAdapter.ItemLazyLoadEventArgs e)
        {
            Lookup(e.Item.As<DiskTreeItem>(), false);
        }

        //private void TreeListViewAdapterAfterItemLabelEdit(object sender, TreeListViewAdapter.ItemLabelEditEventArgs e)
        //{
        //    //e.CancelEdit = true;

        //    //if (string.IsNullOrEmpty(e.Label))
        //    //    return;

        //    //var diskTreeItem = e.Item.As<DiskTreeItem>();
        //    //if (diskTreeItem == null)
        //    //    return;

        //    //e.CancelEdit = !diskTreeItem.Rename(e.Label);
        //}

        private void TreeListViewAdapterMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var adapter = sender.As<TreeListViewAdapter>();
            if (adapter == null)
                return;

            ContextMenuOpen(new[] { adapter.LastHit.As<DiskTreeItem>() });
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            CompletelyReload();
        }

        private void ShowOnlyProjFileFiltersClick(object sender, EventArgs e)
        {
            ShowOnlyProjectFiles = !ShowOnlyProjectFiles;
        }

        private void ApplyFileFiltersOnClick(object sender, EventArgs e)
        {
            UseFileFilters = !UseFileFilters;
        }

        private void TxtFileFiltersTextChanged(object sender, EventArgs e)
        {
            if (!UseFileFilters)
                return;

            Rebind();
        }

        #endregion

        #region Member Methods (Context Menu)

        private void ContextMenuOpen(IEnumerable<DiskTreeItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items.Where(i => (i != null) && !i.IsDirectory))
            {
                try
                {
                    ISledDocument sd;
                    m_documentService.Open(new Uri(item.FullPath), out sd);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception trying to open file \"{1}\": {2}",
                        this, item.FullPath, ex.Message);
                }
            }
        }

        //private void ContextMenuRename(DiskTreeItem item)
        //{
        //    if (item == null)
        //        return;

        //    // TODO: Enable at some point
        //    //m_editor.TreeListViewAdapter.BeginLabelEdit(item);
        //}

        //private void ContextMenuDelete(DiskTreeItem item)
        //{
        //    if (item == null)
        //        return;

        //    var result =
        //        MessageBox.Show(
        //            m_mainForm,
        //            @"Are you sure? If this is a folder all files under it will also be deleted!",
        //            @"Delete",
        //            MessageBoxButtons.YesNo,
        //            MessageBoxIcon.Question);

        //    if (result != DialogResult.Yes)
        //        return;

        //    try
        //    {
        //        if (item.IsDirectory)
        //            Directory.Delete(item.FullPath, true);
        //        else
        //            File.Delete(item.FullPath);

        //        DeleteItem(item);
        //    }
        //    catch (Exception ex)
        //    {
        //        SledOutDevice.OutLine(
        //            SledMessageType.Error,
        //            "{0}: Exception deleting file \"{1}\": {2}",
        //            this, item.FullPath, ex.Message);
        //    }
        //}

        private void ContextMenuProjectAdd(IEnumerable<DiskTreeItem> items)
        {
            if (items == null)
                return;

            var allFiles = new List<string>();
            var validItems = items.Where(i => i != null);

            var directories = validItems.Where(i => i.IsDirectory);
            if (directories.Any())
            {
                // Find all the files recursively in each selected directory
                var files = GatherFiles(m_mainForm, directories, FileFilters);
                
                // Add files found recursively
                allFiles.AddRange(files);
            }

            // Add the remaining user selected non project files
            allFiles.AddRange(
                validItems
                    .Where(dti => !dti.IsProjectFile && !dti.IsDirectory)
                    .Select(dti => dti.FullPath));

            foreach (var file in allFiles.Distinct())
            {
                SledProjectFilesFileType projFile;
                m_projectService.AddFile(file, out projFile);
            }
        }

        private void ContextMenuProjectRemove(IEnumerable<DiskTreeItem> items)
        {
            if (items == null)
                return;

            var validItems = items.Where(i => i != null);

            var directories = validItems.Where(i => i.IsDirectory);
            if (directories.Any())
            {
                // Find all the files recursively in each selected directory
                var files = GatherFiles(m_mainForm, directories, FileFilters);

                // Remove the recursively found files from the project
                foreach (var file in files)
                    m_projectService.RemoveFile(file);
            }

            // Remove the remaining user selected project files
            foreach (var item in validItems.Where(i => i.IsProjectFile))
                m_projectService.RemoveFile(item.FullPath);
        }

        private void ContextMenuOpenWith(DiskTreeItem item)
        {
            try
            {
                SledUtil.ShellOpenWith(item.FullPath);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception opening path \"{1}\": {2}",
                    this, item.FullPath, ex.Message);
            }
        }

        private void ContextMenuExplore(DiskTreeItem item)
        {
            try
            {
                SledUtil.ShellOpenExplorerPath(item.FullPath);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception opening path \"{1}\": {2}",
                    this, item.FullPath, ex.Message);
            }
        }

        private void ContextMenuAddRoot()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = @"Select folder...";
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                
                if (!string.IsNullOrEmpty(m_lastRootStartDir) && Directory.Exists(m_lastRootStartDir))
                    dialog.SelectedPath = m_lastRootStartDir;

                if (dialog.ShowDialog(m_mainForm) != DialogResult.OK)
                    return;

                var di = SledUtil.CreateDirectoryInfo(dialog.SelectedPath, true);
                if (di == null)
                    return;

                m_lastRootStartDir = dialog.SelectedPath;
                AddRoot(new DiskTreeItem(di));
            }
        }

        private void ContextMenuRemoveRoot(DiskTreeItem item)
        {
            RemoveRoot(item);
            m_projectService.SaveSettings();
        }

        #endregion

        #region Member Methods

        private void BindToProject(SledProjectFilesType project, ProjectEvent projEvent)
        {
            StopThread();

            DiskTreeItem.ProjectDirectory = null;
            DiskTreeItem.AssetDirectory = null;
            
            m_fileMappings.Clear();
            m_roots.Clear();
            m_project = project;

            m_lastRootStartDir = null;
            m_editor.View = null;

            // Bail if closing
            if (projEvent == ProjectEvent.Closed)
                return;

            StartThread();

            m_editor.View = this;
            m_lastRootStartDir = m_project.AssetDirectory;

            DiskTreeItem.ProjectDirectory = m_project.ProjectDirectory;
            DiskTreeItem.AssetDirectory = m_project.AssetDirectory;

            var addedAtLeastOne = false;

            // Scan for saved roots in existing projects
            if (projEvent == ProjectEvent.Opened)
            {
                foreach (var projRoot in project.Roots)
                {
                    var di = SledUtil.CreateDirectoryInfo(projRoot.Directory, true);
                    if (di == null)
                        continue;

                    AddRoot(new DiskTreeItem(di));
                    addedAtLeastOne = true;
                }

                if (addedAtLeastOne)
                    return;
            }

            // Try and add some default roots
            var projDir = SledUtil.CreateDirectoryInfo(project.ProjectDirectory, true);
            if (projDir != null)
            {
                AddRoot(new DiskTreeItem(projDir));
                addedAtLeastOne = true;
            }

            if ((string.Compare(project.ProjectDirectory, project.AssetDirectory, true) == 0) && addedAtLeastOne)
                return;

            var assetDir = SledUtil.CreateDirectoryInfo(project.AssetDirectory, true);
            if (assetDir != null)
                AddRoot(new DiskTreeItem(assetDir));
        }

        private void CompletelyReload()
        {
            if (m_project == null)
                return;

            var project = m_project;
            BindToProject(null, ProjectEvent.Closed);
            BindToProject(project, ProjectEvent.Opened);
        }

        private IEnumerable<string> FileFilters
        {
            get
            {
                if (!UseFileFilters)
                    yield break;

                var separator = new[] { ", " };

                IEnumerable<string> filters;
                try
                {
                    var filterText = m_txtFileFilters.Text.Trim();
                    filters = filterText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    filters = EmptyEnumerable<string>.Instance;
                }

                foreach (var filter in filters)
                    yield return filter;
            }
        }

        private void Rebind()
        {
            try
            {
                StartValidating();
                m_editor.SaveState();
                m_editor.View = null;

                ApplyFilters(ShowOnlyProjectFiles, FileFilters);
            }
            finally
            {
                m_editor.View = this;
                m_editor.RestoreState();
                EndValidating();
            }
        }

        private void ApplyFilters(bool showOnlyProjFiles, IEnumerable<string> filters)
        {
            m_showOnlyProjFiles = showOnlyProjFiles;
            m_filters = filters.ToList();

            foreach (var root in m_roots)
            {
                foreach (var item in root.FlattenedChildren)
                    item.Visible = IsVisible(item, m_showOnlyProjFiles, m_filters);
            }
        }

        private void ApplyFiltersTimerTick(object sender, EventArgs e)
        {
            if (!m_applyFiltersNeeded)
                return;

            var span = DateTime.Now.Subtract(m_applyFiltersLastTime);
            if (span.Milliseconds < 250)
                return;

            Rebind();

            m_applyFiltersNeeded = false;
        }

        private void AddRoot(DiskTreeItem diskTreeItem)
        {
            Lookup(diskTreeItem, true);
        }

        private void RemoveRoot(DiskTreeItem diskTreeItem)
        {
            if (diskTreeItem == null)
                return;

            if (!m_roots.Remove(diskTreeItem))
                return;

            if (m_project != null)
            {
                var projRoots =
                    m_project.Roots.Where(
                        r => string.Compare(r.Directory, diskTreeItem.FullPath, true) == 0).ToList();

                while (projRoots.Count > 0)
                {
                    m_project.Roots.Remove(projRoots[0]);
                    projRoots.RemoveAt(0);
                }
            }

            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, diskTreeItem));
        }

        private void Lookup(DiskTreeItem diskTreeItem, bool root)
        {
            if (diskTreeItem == null)
                return;

            if ((m_thread == null) || (m_project == null))
                return;

            if (root)
            {
                var projectRoots = m_project.Roots.Select(r => r.Directory);

                var duplicate =
                    projectRoots.Any(
                        dir => string.Compare(dir, diskTreeItem.FullPath, true) == 0);

                if (!duplicate)
                {
                    var projRoot =
                        new DomNode(SledSchema.SledProjectFilesRootType.Type)
                        .As<SledProjectFilesRootType>();

                    projRoot.Directory = diskTreeItem.FullPath;
                    m_project.Roots.Add(projRoot);
                }
            }

            try
            {
                lock (m_lock)
                    m_toResolve.Enqueue(new Pair<DiskTreeItem, bool>(diskTreeItem, root));
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception looking up directory: {1}",
                    this, ex.Message);
            }
        }

        private void StartThread()
        {
            m_thread =
                new Thread(ThreadRun)
                    {
                        Name = "Project Files - Disk View Thread",
                        IsBackground = true,
                        CurrentCulture = Thread.CurrentThread.CurrentCulture,
                        CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                    };
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        private void StopThread()
        {
            if (m_thread == null)
                return;

            m_thread.Abort();
            m_thread.Join();
            m_thread = null;
        }

        private void ThreadRun()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(50);

                    Pair<DiskTreeItem, bool> pair;

                    lock (m_lock)
                    {
                        if (m_toResolve.Count <= 0)
                            continue;

                        pair = m_toResolve.Dequeue();
                    }

                    var diskTreeItem = pair.First;
                    var root = pair.Second;

                    var di = SledUtil.CreateDirectoryInfo(diskTreeItem.FullPath, true);
                    if (di == null)
                        continue;

                    // Add the root - special case to AddItem where both items are the same
                    if (root)
                        m_syncContext.Post(obj => AddItem(diskTreeItem, diskTreeItem), null);

                    // Find children
                    var files = di.GetFilesAndDirectories(SearchOption.TopDirectoryOnly, m_filePredicate, m_dirPredicate);

                    // Skip first directory as it's already been added if it's a root
                    // and if it's not a root we don't care about it
                    foreach (var fsi in files.Skip(1))
                    {
                        var theFsi = fsi;
                        var child = new DiskTreeItem(theFsi);

                        m_syncContext.Post(
                            obj => AddItem(child, diskTreeItem), null);
                    }

                    m_syncContext.Post(obj => AddItemFinished(), null);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Exception in \"Project Files - Disk View\" file resolving: {0}",
                        ex.Message);
                }
            }
        }

        private void AddItem(DiskTreeItem item, DiskTreeItem parent)
        {
            if ((item == null) || (parent == null))
                return;

            if ((m_thread == null) || (m_project == null))
                return;

            StartValidating();

            // Check for root
            var root = item == parent;

            // Associate item with project file (if any)
            item.ProjectFile = m_projectFileFinderService.Find(item.FullPath);

            item.Visible = IsVisible(item, m_showOnlyProjFiles, m_filters);

            // Keep mapping of absolute paths to DiskTreeItems in sync
            List<DiskTreeItem> lstItems;
            if (m_fileMappings.TryGetValue(item.FullPath, out lstItems))
            {
                lstItems.Add(item);
            }
            else
            {
                lstItems = new List<DiskTreeItem> { item };
                m_fileMappings.Add(item.FullPath, lstItems);
            }

            if (root)
            {
                item.LookedUp = true;
                m_roots.Add(item);
            }
            else
            {
                item.Parent = parent;

                parent.LookedUp = true;
                parent.Children.Add(item);
                parent.Expanded = true;
            }

            // Don't add to tree if not visible
            if (!item.Visible)
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(-1, item, root ? null : parent));
        }

        private void AddItemFinished()
        {
            EndValidating();
        }

        //private void DeleteItem(DiskTreeItem item)
        //{
        //    ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, item));
        //}

        private void StartValidating()
        {
            if (m_validating)
                return;

            try
            {
                m_refreshButton.Enabled = false;
                Beginning.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = true;
            }
        }

        private void EndValidating()
        {
            if (!m_validating)
                return;

            try
            {
                Ending.Raise(this, EventArgs.Empty);
                Ended.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = false;
                m_refreshButton.Enabled = true;
            }
        }

        private void TheSelectionChanging(object sender, EventArgs e)
        {
            SelectionChanging.Raise(this, EventArgs.Empty);
        }

        private void TheSelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged.Raise(this, EventArgs.Empty);
        }

        private static bool CanDoCommand(Command cmd, IEnumerable<DiskTreeItem> selection)
        {
            var retval = false;
            var count = (selection == null) ? 0 : selection.Count();

            var empty = count == 0;
            var single = count == 1;
            var anyDirectories = (selection != null) && selection.Any(i => i.IsDirectory);
            //var allDirectories = (selection != null) && selection.All(i => i.IsDirectory);
            var anyFiles = (selection != null) && selection.Any(i => !i.IsDirectory);
            var allFiles = (selection != null) && selection.All(i => !i.IsDirectory);
            var anyRoots = (selection != null) && selection.Any(i => i.IsRoot);

            //var anyProjFiles = (selection != null) && selection.Any(i => i.IsProjectFile);
            //var anyNonProjFiles = (selection != null) && selection.Any(i => !i.IsProjectFile);

            switch (cmd)
            {
                case Command.Open:
                    retval = anyFiles;
                    break;

                //case Command.Rename:
                //case Command.Delete:
                //    retval = single && (anyFiles || anyDirectories);
                //    break;

                case Command.ProjectAdd:
                case Command.ProjectRemove:
                    retval = anyFiles || anyDirectories;
                    break;

                case Command.OpenWith:
                    retval = single && allFiles;
                    break;

                case Command.Explore:
                    retval = single && (anyFiles || anyDirectories);
                    break;

                case Command.AddRoot:
                    retval = empty;
                    break;

                case Command.RemoveRoot:
                    retval = single && anyRoots;
                    break;

                case Command.CollapseAll:
                    retval = anyFiles || anyDirectories;
                    break;
            }

            return retval;
        }
        
        private static bool IsVisible(DiskTreeItem item, bool showOnlyProjFiles, IEnumerable<string> filters)
        {
            if (item.IsDirectory)
                return true;

            // Combine project only filter with any file filters

            var projResult = showOnlyProjFiles ? item.IsProjectFile : true;

            bool fileResult;
            try
            {
                var fi = new FileInfo(item.FullPath);

                if (filters == null)
                    fileResult = true;
                else
                {
                    fileResult =
                        filters.All(ext => string.IsNullOrEmpty(ext)) ||
                        filters.Any(ext => string.Compare(ext, fi.Extension, true) == 0);
                }
            }
            catch (Exception)
            {
                fileResult = false;
            }

            return projResult && fileResult;
        }

        private static IEnumerable<string> GatherFiles(IWin32Window mainForm, IEnumerable<DiskTreeItem> directories, IEnumerable<string> extensions)
        {
            var allPaths = new List<string>();

            // Run the queries now to avoid cross-thread exceptions later when the task is run
            var localDirectories = directories == null ? new List<DiskTreeItem>() : directories.ToList();
            var localExtensions = extensions == null ? EmptyEnumerable<string>.Instance : extensions.ToList();

            Func<FileSystemInfo, bool> isHidden =
                fsi => (fsi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

            using (var dialog = new SledAsyncTaskForm())
            {
                const string scanning = "Scanning for files and folders...";

                // This task runs in a separate thread so make sure to avoid
                // cross-thread LINQ queries or exceptions get thrown
                Func<SledUtil.BoolWrapper, bool> asyncTask =
                    cancel =>
                    {
                        var anyExtensions = localExtensions.Any();

                        Func<FileInfo, bool> filePredicate =
                            fi =>
                            {
                                if (isHidden(fi))
                                    return false;

                                return
                                    !anyExtensions ||
                                    localExtensions.Any(ext => string.Compare(fi.Extension, ext, true) == 0);
                            };

                        Func<DirectoryInfo, bool> dirPredicate =
                            di =>
                            {
                                // Sometimes the root directory can show up
                                // as hidden... depending on what path is
                                // passed into the DirectoryInfo upon creation
                                if (di.Parent == null)
                                    return true;

                                var hidden = isHidden(di);

                                if (!hidden)
                                    dialog.Label = string.Format("{0} ({1})", scanning, di.Name);

                                return !hidden;
                            };

                        foreach (var directory in localDirectories)
                        {
                            try
                            {
                                // Gather files and folders recursively from the directory
                                var info = new DirectoryInfo(directory.FullPath);
                                var tree = info.GetFilesAndDirectoriesTree(SearchOption.AllDirectories, filePredicate, dirPredicate, cancel);
                                if (tree == null)
                                    continue;

                                // Ignore directories
                                var filesOnly = tree.PreOrder.Where(t => !t.Value.IsDirectory());

                                // Add to master list
                                allPaths.AddRange(filesOnly.Select(t => t.Value.FullName));
                            }
                            catch (Exception ex)
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    "{0}: Exception enumerating directory \"{1}\": {2}",
                                    typeof(SledProjectFilesDiskViewEditor), directory.FullPath, ex.Message);

                                continue;
                            }
                        }

                        return true;
                    };


                dialog.Task = asyncTask;
                dialog.Text = string.Format("SLED - {0}", scanning);
                dialog.Label = scanning;

                // 'DialogResult.Yes' means task ran to completion and returned 'true'
                if (dialog.ShowDialog(mainForm) != DialogResult.Yes)
                    yield break;
            }

            foreach (var path in allPaths.Distinct())
                yield return path;
        }

        #endregion

        private Thread m_thread;
        private bool m_validating;
        private bool m_showOnlyProjFiles;
        private bool m_applyFiltersNeeded;
        private string m_lastRootStartDir;
        private IEnumerable<string> m_filters;
        private SledProjectFilesType m_project;
        private DateTime m_applyFiltersLastTime;
        private List<string> m_savedRootsForProjectSaveAs;

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledDocumentService m_documentService;

        [Import]
        private SledSourceControlService m_sourceControlService;

        [Import]
        private ISledProjectFileFinderService m_projectFileFinderService;

#pragma warning restore 649

        private readonly MainForm m_mainForm;
        private readonly ToolStrip m_tools;
        private readonly UserControl m_host;
        private readonly DiskTreeEditor m_editor;
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly ISettingsService m_settingsService;
        private readonly IControlHostService m_controlHostService;
        private readonly Timerz m_applyFiltersTimer;
        private readonly SynchronizationContext m_syncContext;
        private readonly Func<FileSystemInfo, bool> m_isHidden;
        private readonly Func<FileInfo, bool> m_filePredicate;
        private readonly Func<DirectoryInfo, bool> m_dirPredicate;

        private readonly ToolStripButton m_refreshButton;
        private readonly ToolStripButton m_showOnlyProjFileFilters;
        private readonly ToolStripButton m_applyFileFilters;
        private readonly ToolStripTextBox m_txtFileFilters;

        private volatile object m_lock =
            new object();

        private readonly List<DiskTreeItem> m_roots =
            new List<DiskTreeItem>();

        private readonly Selection<object> m_selection =
            new Selection<object>();

        private readonly Queue<Pair<DiskTreeItem, bool>> m_toResolve =
            new Queue<Pair<DiskTreeItem, bool>>();

        private readonly Dictionary<string, List<DiskTreeItem>> m_fileMappings =
            new Dictionary<string, List<DiskTreeItem>>(StringComparer.CurrentCultureIgnoreCase);

        private const string Description = "Project Files - Disk View";
        private const string RefreshButtonText = "Refresh";
        private const string ShowOnlyProjFilesText = "Show only project files";
        private const string ApplyFileFiltersText = "Apply file filters";
        private static readonly string[] s_columns = new[] { "Roots" };

        #region Private Classes

        private enum ProjectEvent
        {
            Created,
            Opened,
            Closed
        }

        private class DiskTreeItem : IResource, IItemView
        {
            public DiskTreeItem(FileSystemInfo fileSystemInfo)
            {
                m_fileSystemInfo = fileSystemInfo;

                Guid = Guid.NewGuid();
                Visible = true;
                Expanded = false;
                RootType = SpecialRootType.None;
                Children = new List<DiskTreeItem>();
                Uri = new Uri(m_fileSystemInfo.FullName);

                var areSame = string.Compare(ProjectDirectory, AssetDirectory, true) == 0;

                if (string.Compare(ProjectDirectory, m_fileSystemInfo.FullName, true) == 0)
                    RootType = areSame ? SpecialRootType.ProjectAndAssetDirectory : SpecialRootType.ProjectDirectory;
                else if (string.Compare(AssetDirectory, m_fileSystemInfo.FullName, true) == 0)
                    RootType = areSame ? SpecialRootType.ProjectAndAssetDirectory : SpecialRootType.AssetDirectory;

                // Stop compiler warning; Uri can never change
                if (UriChanged == null) return;
            }

            #region IResource Interface

            public string Type
            {
                get { return GetType().ToString(); }
            }

            public Uri Uri { get; set; }

            public event EventHandler<UriChangedEventArgs> UriChanged;

            #endregion

            #region IItemView Interface

            public void GetInfo(object item, ItemInfo info)
            {
                info.AllowLabelEdit = false;

                var label = m_fileSystemInfo.Name;
                switch (RootType)
                {
                    case SpecialRootType.ProjectAndAssetDirectory:
                        info.Label = string.Format("{0} ({1})", label, AssetProjectDirectoryString);
                        break;

                    case SpecialRootType.AssetDirectory:
                        info.Label = string.Format("{0} ({1})", label, AssetDirectoryString);
                        break;

                    case SpecialRootType.ProjectDirectory:
                        info.Label = string.Format("{0} ({1})", label, ProjectDirectoryString);
                        break;

                    default: info.Label = label; break;
                }

                info.IsExpandedInView = Expanded;

                var isDir = IsDirectory;
                if (isDir)
                {
                    info.ImageIndex =
                        info.GetImageIndex(
                            !IsRoot
                                ? Atf.Resources.FolderImage
                                : Atf.Resources.ResourceFolderImage);

                    info.IsLeaf = LookedUp ? !VisibleChildren.Any() : false;
                }
                else
                {
                    var imageName = Atf.Resources.DataImage;

                    {
                        if (s_documentService == null)
                            s_documentService = SledServiceInstance.Get<ISledDocumentService>();

                        var documentClient = s_documentService.GetDocumentClient(Uri);
                        if ((documentClient != null) &&
                            (documentClient.Info != null) &&
                            !string.IsNullOrEmpty(documentClient.Info.OpenIconName))
                        {
                            imageName = documentClient.Info.OpenIconName;
                        }
                    }

                    info.ImageIndex = info.GetImageIndex(imageName);
                    info.IsLeaf = true;
                }

                // Set source control status
                {
                    if (s_sourceControlService == null)
                        s_sourceControlService = SledServiceInstance.TryGet<SledSourceControlService>();

                    if (s_sourceControlService == null)
                        return;

                    if (!s_sourceControlService.CanUseSourceControl)
                        return;

                    var sourceControlStatus = s_sourceControlService.GetStatus(this);
                    switch (sourceControlStatus)
                    {
                        case SourceControlStatus.CheckedOut:
                            info.StateImageIndex = info.GetImageIndex(Atf.Resources.DocumentCheckOutImage);
                            break;

                        default:
                            info.StateImageIndex = TreeListView.InvalidImageIndex;
                            break;
                    }
                }
            }

            #endregion

            public Guid Guid { get; private set; }

            public string FullPath
            {
                get { return m_fileSystemInfo.FullName; }
            }

            public bool Visible { get; set; }

            public bool Expanded { private get; set; }

            public bool IsDirectory
            {
                get { return m_fileSystemInfo.Is<DirectoryInfo>(); }
            }

            public bool IsProjectFile
            {
                get { return ProjectFile != null; }
            }

            public bool IsRoot
            {
                get { return Parent == null; }
            }

            public bool LookedUp { private get; set; }

            public SledProjectFilesFileType ProjectFile { private get; set; }

            public DiskTreeItem Parent { private get; set; }

            public IList<DiskTreeItem> Children { get; private set; }

            public IEnumerable<DiskTreeItem> FlattenedChildren
            {
                get
                {
                    foreach (var child in Children)
                    {
                        yield return child;

                        foreach (var grandChild in child.FlattenedChildren)
                            yield return grandChild;
                    }
                }
            }

            //public bool Rename(string newName)
            //{
            //    try
            //    {
            //        string newPath = null;

            //        if (IsDirectory)
            //        {
            //            // TODO:
            //        }
            //        else
            //        {
            //            newPath = Path.Combine(Path.GetDirectoryName(FullPath) + Path.DirectorySeparatorChar, newName);
            //        }

            //        if (string.IsNullOrEmpty(newPath))
            //            return false;

            //        if (IsDirectory)
            //        {
            //            Directory.Move(FullPath, newPath);
            //            m_fileSystemInfo = new DirectoryInfo(newPath);
            //        }
            //        else
            //        {
            //            File.Move(FullPath, newPath);
            //            m_fileSystemInfo = new FileInfo(newPath);
            //        }

            //        Uri = new Uri(m_fileSystemInfo.FullName);

            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        return false;
            //    }
            //}

            private SpecialRootType RootType { get; set; }

            private IEnumerable<DiskTreeItem> VisibleChildren
            {
                get
                {
                    foreach (var child in Children.Where(i => i.Visible))
                    {
                        yield return child;

                        foreach (var grandChild in child.VisibleChildren)
                            yield return grandChild;
                    }
                }
            }

            private enum SpecialRootType
            {
                None,
                ProjectDirectory,
                AssetDirectory,
                ProjectAndAssetDirectory,
            }

            public static string ProjectDirectory { private get; set; }
            public static string AssetDirectory { private get; set; }

            private readonly FileSystemInfo m_fileSystemInfo;

            private static ISledDocumentService s_documentService;
            private static ISledSourceControlService s_sourceControlService;

            private const string AssetDirectoryString = "Asset";
            private const string ProjectDirectoryString = "Project";
            private const string AssetProjectDirectoryString = "Asset/Project";
        }

        private class DiskTreeEditor : SledTreeListViewEditor
        {
            public DiskTreeEditor()
                : base(
                    Description,
                    null,
                    s_columns,
                    TreeListView.Style.TreeList,
                    StandardControlGroup.Left)
            {
                TreeListView.NodeSorter = new DiskTreeItemSorter();
                TreeListView.Renderer = new DiskTreeRenderer(TreeListView);
                TreeListView.NodeExpandedChanged += TreeListViewNodeExpandedChanged;

                TreeListView.GridLines = false;
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

            private void TreeListViewNodeExpandedChanged(object sender, TreeListView.NodeEventArgs e)
            {
                if (View == null)
                    return;

                var item = e.Node.As<DiskTreeItem>();
                if (item == null)
                    return;

                item.Expanded = e.Node.Expanded;
            }

            private void SaveTopItem()
            {
                m_topItem = null;

                var diskTreeItem = TreeListViewAdapter.TopItem.As<DiskTreeItem>();
                if (diskTreeItem == null)
                    return;

                m_topItem = diskTreeItem;
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
                }
            }

            private void SaveSelection()
            {
                m_lstSelection.Clear();
                m_lstSelection.AddRange(TreeListViewAdapter.Selection.Where(i => i.Is<DiskTreeItem>()));
            }

            private void RestoreSelection()
            {
                if (m_lstSelection.Count <= 0)
                    return;

                Selection = m_lstSelection;
            }

            private object m_topItem;

            private readonly List<object> m_lstSelection =
                new List<object>();

            #region Private Classes

            private class DiskTreeItemSorter : IComparer<TreeListView.Node>
            {
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

                    var dti1 = x.Tag.As<DiskTreeItem>();
                    var dti2 = y.Tag.As<DiskTreeItem>();

                    var xIsDir = dti1.IsDirectory;
                    var yIsDir = dti2.IsDirectory;

                    if ((!xIsDir && !yIsDir) || (xIsDir && yIsDir))
                    {
                        var result = string.Compare(dti1.FullPath, dti2.FullPath);
                        if (result == 0)
                            result = dti1.Guid.CompareTo(dti2.Guid);

                        return result;
                    }

                    return xIsDir ? -1 : 1;
                }
            }

            private class DiskTreeRenderer : TreeListView.NodeRenderer
            {
                public DiskTreeRenderer(TreeListView owner)
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

                    var flags = TextFormatFlags.VerticalCenter;

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

                    var isProjFileOrDir = IsProjectFileOrDirectory(node);

                    var textColor =
                        node.Selected
                            ? Owner.HighlightTextColor
                            : isProjFileOrDir
                                ? Owner.TextColor
                                : Owner.DisabledTextColor;

                    if (!Owner.Control.Enabled)
                        textColor = Owner.DisabledTextColor;

                    TextRenderer.DrawText(gfx, text, Owner.Control.Font, bounds, textColor, flags);
                }

                public override void DrawImage(TreeListView.Node node, Graphics gfx, Rectangle bounds)
                {
                    // No image to draw
                    if (node.ImageIndex == -1)
                        return;

                    if (Owner.ImageList == null)
                        return;

                    if (node.ImageIndex >= Owner.ImageList.Images.Count)
                        return;

                    if (Owner.Control.Enabled && IsProjectFileOrDirectory(node))
                    {
                        Owner.ImageList.Draw(gfx, bounds.X, bounds.Y, bounds.Width, bounds.Height, node.ImageIndex);
                    }
                    else
                    {
                        using (var image = Owner.ImageList.Images[node.ImageIndex])
                        {
                            ControlPaint.DrawImageDisabled(gfx, image, bounds.X, bounds.Y, Owner.DisabledBackColor);
                        }
                    }
                }

                private static string GetObjectString(object value)
                {
                    var formattable = value as IFormattable;

                    return
                        formattable != null
                            ? formattable.ToString(null, null)
                            : value.ToString();
                }

                private static bool IsProjectFileOrDirectory(TreeListView.Node node)
                {
                    if (node == null)
                        return false;

                    if (node.Tag == null)
                        return false;

                    var diskTreeItem = node.Tag.As<DiskTreeItem>();
                    if (diskTreeItem == null)
                        return false;

                    return diskTreeItem.IsDirectory || diskTreeItem.IsProjectFile;
                }

            }

            #endregion
        }

        #endregion
    }
}