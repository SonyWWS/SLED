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
using Sce.Atf.Controls;
using Sce.Atf.Dom;

using Sce.Sled.Resources;
using Sce.Sled.Project;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledProjectFilesTreeEditor Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledProjectFilesTreeEditor))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledProjectFilesTreeEditor : TreeControlEditor, IInitializable, IPartImportsSatisfiedNotification, IControlHostClient, ICommandClient, IContextMenuCommandProvider, ISourceControlContext
    {
        [ImportingConstructor]
        public SledProjectFilesTreeEditor(
            MainForm mainForm,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IControlHostService controlHostService)
            : base(commandService)
        {
            m_mainForm = mainForm;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;

            TreeControl.Name = Localization.SledProjectFilesTitle;
            TreeControl.AllowDrop = true;
            TreeControl.ShowRoot = true;
            TreeControl.MouseDown += TreeControlMouseDown;
            TreeControl.MouseUp += TreeControlMouseUp;
            TreeControl.MouseDoubleClick += TreeControlMouseDoubleClick;
            TreeControl.NodeExpandedChanging += TreeControlNodeExpandedChanging;
            TreeControlAdapter.LastHitChanged += TreeControlAdapterLastHitChanged;

            m_domTreeAdapter = new DomTreeAdapter(this);
            m_domTreeAdapter.Beginning += DomTreeAdapterBeginning;
            m_domTreeAdapter.Ended += DomTreeAdapterEnded;
            SledServiceReferenceCompositionContainer.Get.ComposeParts(m_domTreeAdapter);

            m_emptyTreeAdapter = new EmptyTreeAdapter();
            SledServiceReferenceCompositionContainer.Get.ComposeParts(m_emptyTreeAdapter);

            var image = ResourceUtil.GetImage(Atf.Resources.FolderImage);

            m_controlInfo =
                new ControlInfo(
                    TreeControl.Name,
                    TreeControl.Name,
                    StandardControlGroup.Left,
                    image);

            controlHostService.RegisterControl(TreeControl, m_controlInfo, this);

            // Progress bar for loading/parsing files
            {
                m_progressBar =
                    new ProgressBar
                        {
                            Width = TreeControl.Width,
                            Style = ProgressBarStyle.Marquee,
                            Dock = DockStyle.Bottom
                        };
                m_progressBar.Hide();
                TreeControl.Controls.Add(m_progressBar);
            }

            // Project files tree editor menu commands

            // This is a hack to avoid duplicate command exceptions from being thrown.
            // The commands below don't need a menu but because other CommandPlugin's
            // register commands with no menu and the same "Text" CommandService thinks
            // there are duplicates and throws an exception. So, we'll toss the commands
            // into a fake menu and hide the menu once commands are registered.
            var menuInfo =
                m_commandService.RegisterMenu(
                    Menu.ProjectFilesTreeEditor,
                    "Project Files Tree Editor",
                    "Project Files Tree Editor Commands");

            m_commandService.RegisterCommand(
                Command.Open,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeOpen,
                Localization.SledProjectFilesTreeOpenComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.Rename,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeRename,
                Localization.SledProjectFilesTreeRenameComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.Remove,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeRemove,
                Localization.SledProjectFilesTreeRemoveComment,
                Keys.None,
                Atf.Resources.DeleteImage,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.NewFolder,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeNewFolder,
                Localization.SledProjectFilesTreeNewFolderComment,
                Keys.None,
                Atf.Resources.FolderImage,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.OpenWith,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeOpenWith,
                Localization.SledProjectFilesTreeOpenWithComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.Explore,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeExplore,
                Localization.SledProjectFilesTreeExploreComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                Command.Goto,
                Menu.ProjectFilesTreeEditor,
                CommandGroup.ProjectFilesTreeEditor,
                Localization.SledProjectFilesTreeGoto,
                Localization.SledProjectFilesTreeGotoComment,
                Keys.None,
                Atf.Resources.ZoomInImage,
                CommandVisibility.Menu,
                this);

            // Hide the menu now that items are added
            menuInfo.GetMenuItem().Visible = false;
        }

        public DomNode Root
        {
            get { return m_domTreeAdapter.RootNode; }
            set
            {
                if (value != null)
                {
                    m_domTreeAdapter.RootNode = value;
                    TreeControlAdapter.TreeView = m_domTreeAdapter;

                    try
                    {
                        m_bRestoringExpandState = true;

                        var lstExpandedViewables = new List<ISledProjectFilesTreeViewable>();
                        FindExpandedViewables(m_domTreeAdapter.RootNode.As<ISledProjectFilesTreeViewable>(), ref lstExpandedViewables);

                        foreach (var viewable in lstExpandedViewables)
                            TreeControlAdapter.Expand(viewable);
                    }
                    finally
                    {
                        m_bRestoringExpandState = false;
                    }

                    m_domTreeAdapter.RootNode.ChildInserted += RootChildInserted;
                    m_domTreeAdapter.RootNode.ChildRemoving += RootChildRemoving;
                }
                else
                {
                    if (m_domTreeAdapter.RootNode != null)
                    {
                        m_domTreeAdapter.RootNode.ChildInserted -= RootChildInserted;
                        m_domTreeAdapter.RootNode.ChildRemoving -= RootChildRemoving;
                    }

                    TreeControlAdapter.TreeView = null;
                    m_domTreeAdapter.RootNode = null;
                }
            }
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Grab project service and monitor some events
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closed += ProjectServiceClosed;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRenamed += ProjectServiceFileRenamed;
            m_projectService.FileRemoved += ProjectServiceFileRemoved;
            m_projectService.FolderAdded += ProjectServiceFolderAdded;
            m_projectService.FolderRenamed += ProjectServiceFolderRenamed;
            m_projectService.FolderRemoved += ProjectServiceFolderRemoved;
            m_projectService.NameChanged += ProjectServiceNameChanged;

            // Start expand timer
            m_hNeedExpandTimer = new Timer { Interval = 50 };
            m_hNeedExpandTimer.Tick += NeedExpandTimerTick;

            m_refreshTimer = new Timer { Interval = 500 };
            m_refreshTimer.Tick += RefreshTimerTick;

            TreeControlAdapter.TreeView = m_emptyTreeAdapter;
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
            if (!ReferenceEquals(control, TreeControl))
                return;

            m_commandService.SetActiveClient(this);
            m_contextRegistry.ActiveContext = this;
        }

        public void Deactivate(Control control)
        {
            if (!ReferenceEquals(control, TreeControl))
                return;

            m_contextRegistry.RemoveContext(this);
        }

        public bool Close(Control control)
        {
            return true;
        }

        #endregion 

        #region Commands

        enum Command
        {
            Open,
            Rename,
            Remove,
            NewFolder,
            OpenWith,
            Explore,
            Goto,
        }

        enum Menu
        {
            ProjectFilesTreeEditor,
        }

        enum CommandGroup
        {
            ProjectFilesTreeEditor,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            var selection = SelectionAs<ISledProjectFilesTreeViewable>();
            var count = selection.Count();
            
            // Count all items in the selection at once
            int numRoot, numFiles, numFolders, numFunctions;
            CountSelection(selection, out numRoot, out numFiles, out numFolders, out numFunctions);

            // Figure out some conditions
            var bAllRoot = count == numRoot;
            var bAllFiles = count == numFiles;
            var bAllFolders = count == numFolders;
            var bAllFunctions = count == numFunctions;
            var bAnyRoot = numRoot != 0;
            var bAnyFiles = numFiles != 0;
            var bAnyFolders = numFolders != 0;
            var bOnly1 = count == 1;

            switch ((Command)commandTag)
            {
                case Command.Open:      return bAnyFiles && !bAnyRoot;
                case Command.Rename:    return bOnly1 && (bAllFiles || bAllFolders);
                case Command.Remove:    return (bAnyFiles || bAnyFolders) && !bAnyRoot;
                case Command.NewFolder: return bOnly1 && (bAllFolders || bAllRoot);
                case Command.OpenWith:  return bOnly1 && bAllFiles;
                case Command.Explore:   return bOnly1 && (bAllRoot || bAllFiles);
                case Command.Goto:      return bOnly1 && bAllFunctions;
            }

            return false;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            var selection = SelectionAs<ISledProjectFilesTreeViewable>();

            switch ((Command)commandTag)
            {
                case Command.Open:
                    Open(selection);
                    break;

                case Command.Rename:
                    Rename(selection.ElementAt(0));
                    break;

                case Command.Remove:
                    Remove(selection);
                    break;

                case Command.NewFolder:
                    NewFolder(selection.FirstOrDefault());
                    break;

                case Command.OpenWith:
                    OpenWith(selection.FirstOrDefault());
                    break;

                case Command.Explore:
                    Explore(selection.FirstOrDefault());
                    break;

                case Command.Goto:
                    GotoFunction(new[] { selection.ElementAt(0) });
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            Root = e.Project.DomNode;
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            Root = e.Project.DomNode;
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            Root = null;

            m_needsRefresh.Clear();
            if (m_refreshTimer.Enabled)
                m_refreshTimer.Stop();

            TreeControlAdapter.TreeView = m_emptyTreeAdapter;
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            RefreshParentNode(e.File);
        }

        private void ProjectServiceFileRenamed(object sender, SledProjectServiceFileEventArgs e)
        {
            RefreshParentNode(e.File);
        }

        private void ProjectServiceFileRemoved(object sender, SledProjectServiceFileEventArgs e)
        {
            RefreshNode(Root.As<ISledProjectFilesTreeViewable>());
        }

        private void ProjectServiceFolderAdded(object sender, SledProjectServiceFolderEventArgs e)
        {
            RefreshParentNode(e.Folder);
            ExpandParentNode(e.Folder);
        }

        private void ProjectServiceFolderRenamed(object sender, SledProjectServiceFolderEventArgs e)
        {
            RefreshParentNode(e.Folder);
        }

        private void ProjectServiceFolderRemoved(object sender, SledProjectServiceFolderEventArgs e)
        {
            RefreshNode(Root.As<ISledProjectFilesTreeViewable>());
        }

        private void ProjectServiceNameChanged(object sender, SledProjectServiceProjectNameEventArgs e)
        {
            RefreshNode(TreeControl.Root.Tag.As<ISledProjectFilesTreeViewable>());
        }

        #endregion

        #region IContextMenuCommandProvider Interface

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (!(context is DomTreeAdapter))
                return s_emptyCommands;

            if (m_projectService == null)
                return s_emptyCommands;

            // Nothing to do if project isn't open
            if (!m_projectService.Active)
                return s_emptyCommands;

            var selection = SelectionAs<ISledProjectFilesTreeViewable>().ToList();

            // If clicked on item isn't in selection add it
            var itemClicked = target.As<ISledProjectFilesTreeViewable>();
            if ((itemClicked != null) && !selection.Contains(itemClicked))
                selection.Add(itemClicked);

            // Count items
            var count = selection.Count;
            int numRoot, numFiles, numFolders, numFunctions;
            CountSelection(selection, out numRoot, out numFiles, out numFolders, out numFunctions);

            // List of commands that are possible
            var commands = new List<object>();

            // Figure out some conditions
            var bAllRoot = count == numRoot;
            var bAllFiles = count == numFiles;
            var bAllFolders = count == numFolders;
            var bAllFunctions = count == numFunctions;
            var bAnyRoot = numRoot != 0;
            var bAnyFiles = numFiles != 0;
            var bAnyFolders = numFolders != 0;

            if (count <= 0)
            {
                // No items selected and no item clicked on
                commands.AddRange(
                    new object[]
                    {
                        Command.Explore,
                        Command.NewFolder
                    });
            }
            else if (count == 1)
            {
                // One item in selection

                // Just the root node clicked
                if (bAllRoot)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Explore,
                            null,
                            Command.NewFolder
                        });

                // Just a single file clicked
                if (bAllFiles)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Open,
                            Command.Rename,
                            Command.Remove,
                            null,
                            Command.OpenWith,
                            Command.Explore
                        });

                // Just a single folder clicked
                if (bAllFolders)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Rename,
                            Command.Remove,
                            null,
                            Command.NewFolder
                        });

                // Just a single function clicked
                if (bAllFunctions)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Goto
                        });
            }
            else
            {
                // Many items in selection

                if (bAnyFiles && !bAnyRoot)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Open,
                            Command.Remove
                        });

                if (bAnyFolders && !bAnyRoot)
                    commands.AddRange(
                        new object[]
                        {
                            Command.Remove
                        });
            }

            return commands.Distinct();
        }

        #endregion

        #region ISourceControlContext Interface

        public IEnumerable<IResource> Resources
        {
            get
            {
                if (TreeControlAdapter == null)
                    yield break;

                foreach (var selected in TreeControlAdapter.GetSelectedItems())
                {
                    if (selected.Is<IResource>())
                        yield return selected.As<IResource>();
                }
            }
        }

        #endregion

        #region DomNode Events

        private void RootChildInserted(object sender, ChildEventArgs e)
        {
            RefreshNode(e.Parent.As<ISledProjectFilesTreeViewable>());
        }

        private void RootChildRemoving(object sender, ChildEventArgs e)
        {
            // When removing a folder (and not drag dropping) we want to
            // remove everything under that folder, too.
            if (e.Child.Type == SledSchema.SledProjectFilesFolderType.Type)
            {
                if (!m_domTreeAdapter.DragDropping)
                {
                    var folder = e.Child.As<SledProjectFilesFolderType>();

                    // Remove files from this folder
                    var lstFiles = new List<SledProjectFilesFileType>(folder.Files);

                    while (lstFiles.Count > 0)
                    {
                        folder.Files.Remove(lstFiles[0]);
                        lstFiles.RemoveAt(0);
                    }

                    // Remove folders from this folder
                    var lstFolders = new List<SledProjectFilesFolderType>(folder.Folders);
                    while (lstFolders.Count > 0)
                    {
                        folder.Folders.Remove(lstFolders[0]);
                        lstFolders.RemoveAt(0);
                    }
                }
            }

            // Refresh when a function is removed
            RefreshNode(e.Parent.As<ISledProjectFilesTreeViewable>());
        }

        #endregion

        #region Mouse Events

        private void TreeControlMouseDown(object sender, MouseEventArgs e)
        {
            // Watch double click state
            m_bDoubleClick = (e.Clicks == 2);
        }

        private void TreeControlMouseUp(object sender, MouseEventArgs e)
        {
            // Reset
            m_bDoubleClick = false;
        }

        private void TreeControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (LastHit == null)
                return;

            var viewable = LastHit.As<ISledProjectFilesTreeViewable>();
            if (viewable == null)
            {
                var projectFilesEmptyType = LastHit.As<SledProjectFilesEmptyType>();
                if (projectFilesEmptyType != null)
                    m_projectService.Open(null);

                return;
            }

            // Will handle items that aren't able to be opened
            Open(new[] { viewable });

            // Will handle items that aren't functions
            GotoFunction(new[] { viewable });
        }

        #endregion

        #region Node Event Handling

        private void TreeControlNodeExpandedChanging(object sender, TreeControl.CancelNodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (e.Node.Tag == null)
                return;

            var viewable = e.Node.Tag.As<ISledProjectFilesTreeViewable>();
            if (viewable == null)
                return;

            // This event is happening before the item is expanded or collapsed
            var bExpanding = !e.Node.Expanded;
                       
            if (bExpanding)
            {
                if (m_bDoubleClick)
                {
                    if (viewable.Is<SledProjectFilesFileType>())
                    {
                        var file = viewable.As<SledProjectFilesFileType>();
                        if (file.Functions.Count > 0)
                            e.Cancel = true;
                    }
                }

                if (!m_bRestoringExpandState)
                {
                    viewable.Expanded = !e.Cancel;
                    m_hNeedExpandTimer.Start();
                }
            }
            else
            {
                if (m_bDoubleClick)
                {
                    if (viewable.Is<SledProjectFilesFileType>())
                    {
                        var file = viewable.As<SledProjectFilesFileType>();
                        if (file.Functions.Count > 0)
                            e.Cancel = true;
                    }
                }

                if (!m_bRestoringExpandState)
                {
                    viewable.Expanded = e.Cancel;

                    // Write updated expanded state to disk
                    m_projectService.SaveSettings(true);
                }
            }
        }

        private void NeedExpandTimerTick(object sender, EventArgs e)
        {
            if (Root == null)
                return;

            var bMoreWork = false;

            var lstViewables = new List<ISledProjectFilesTreeViewable>();
            FindExpandedViewables(Root.As<ISledProjectFilesTreeViewable>(), ref lstViewables);

            try
            {
                m_bRestoringExpandState = true;

                foreach (var viewable in lstViewables)
                    TreeControlAdapter.Expand(viewable);
            }
            finally
            {
                m_bRestoringExpandState = false;
            }

            for (var i = 0; (i < lstViewables.Count) && !bMoreWork; i++)
            {
                if (!TreeControlAdapter.IsExpanded(lstViewables[i]) &&
                    TreeControlAdapter.IsVisible(lstViewables[i]))
                {
                    bMoreWork = true;
                }
            }

            if (bMoreWork)
                return;

            m_hNeedExpandTimer.Stop();
            m_projectService.SaveSettings(true);
        }

        private static void FindExpandedViewables(ISledProjectFilesTreeViewable viewable, ref List<ISledProjectFilesTreeViewable> lstViewables)
        {
            if (viewable.Expanded)
                lstViewables.Add(viewable);

            foreach (var child in viewable.Children)
                FindExpandedViewables(child, ref lstViewables);
        }

        private void TreeControlAdapterLastHitChanged(object sender, EventArgs e)
        {
            if (TreeControlAdapter == null)
            {
                m_domTreeAdapter.LastHit = null;
                return;
            }

            var domNode = TreeControlAdapter.LastHit.As<DomNode>() ?? m_domTreeAdapter.RootNode;
            m_domTreeAdapter.LastHit = domNode;
        }

        #endregion

        #region SledSourceControlService Events

        private void SourceControlServiceStatusChanged(object sender, SourceControlEventArgs e)
        {
            if (m_projectService == null)
                return;

            var fileInProject = m_projectFileFinderService.Find(e.Uri);

            if (fileInProject != null)
                TreeControlAdapter.Refresh(fileInProject);
            else
            {
                var project = m_projectService.As<SledProjectFilesType>();
                if ((project != null) && project.Uri.Equals(e.Uri))
                {
                    // When refreshing the root synchronously from this function - and
                    // during a project load - multiple items will be added to the tree
                    // So this is a hack to get around that for now.
                    var syncContext = System.Threading.SynchronizationContext.Current;
                    if (syncContext != null)
                        syncContext.Post(obj => RefreshRoot(), null);
                }
            }
        }

        private void RefreshRoot()
        {
            if (Root == null)
                return;

            if (TreeControlAdapter == null)
                return;

            TreeControlAdapter.Refresh(Root);
        }

        #endregion

        #region DomTreeAdapter Events

        private void DomTreeAdapterBeginning(object sender, EventArgs e)
        {
            TreeControl.Enabled = false;
            m_progressBar.Show();
        }

        private void DomTreeAdapterEnded(object sender, EventArgs e)
        {
            m_progressBar.Hide();
            TreeControl.Enabled = true;
            m_hNeedExpandTimer.Start();
            TreeControl.Invalidate(true);
        }

        #endregion

        #region Private Classes

        private class DomTreeAdapter : ITreeView, IItemView, IObservableContext, IInstancingContext, INamingContext, IAdaptable, IValidationContext
        {
            public DomTreeAdapter(SledProjectFilesTreeEditor owner)
            {
                m_owner = owner;
                m_timer = new Timer { Interval = 250 };
                m_timer.Tick += TimerTick;
                m_timer.Start();

                if (Cancelled == null) return;
            }

            public DomNode RootNode
            {
                get { return m_root; }
                set
                {
                    if (m_root != null)
                    {
                        m_root.ChildInserted -= RootChildInserted;
                        m_root.AttributeChanged -= RootAttributeChanged;
                        m_root.ChildRemoved -= RootChildRemoved;
                    }

                    Beginning.Raise(this, EventArgs.Empty);

                    m_root = value;

                    Ending.Raise(this, EventArgs.Empty);
                    Ended.Raise(this, EventArgs.Empty);

                    if (m_root != null)
                    {
                        m_root.ChildInserted += RootChildInserted;
                        m_root.AttributeChanged += RootAttributeChanged;
                        m_root.ChildRemoved += RootChildRemoved;
                    }

                    Reloaded.Raise(this, EventArgs.Empty);
                }
            }

            public DomNode LastHit { private get; set; }

            public bool DragDropping { get; private set; }

            public bool Validating { get; private set; }

            #region ITreeView Interface

            public object Root
            {
                get { return m_root; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                if (parent == null)
                    yield break;

                var viewable = parent.As<ISledProjectFilesTreeViewable>();
                if (viewable == null)
                    yield break;

                var children = viewable.Children.ToList();
                if (children.Count <= 0)
                    yield break;

                children.Sort(m_comparer);

                foreach (var child in children)
                    yield return child;
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

            #region IObservableContext Interface

            /// <summary>
            /// Event that is raised when a tree item is inserted</summary>
            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            /// <summary>
            /// Event that is raised when a tree item is removed</summary>
            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            /// <summary>
            /// Event that is raised when a tree item is changed</summary>
            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            /// <summary>
            /// Event that is raised when the tree is reloaded</summary>
            public event EventHandler Reloaded;

            #endregion

            #region IInstancingContext Interface

            public bool CanCopy()
            {
                return false;
            }

            public object Copy()
            {
                return null;
            }

            public bool CanInsert(object item)
            {
                var dataObject = item.As<IDataObject>();
                if (dataObject == null)
                    return false;

                // Always allow file drops
                if (dataObject.GetDataPresent(DataFormats.FileDrop))
                    return true;

                // Check for other drops
                var items = dataObject.GetData(typeof(object[])) as object[];
                if (items == null)
                    return false;

                if (items.Length <= 0)
                    return false;

                var childNodes = items.AsIEnumerable<DomNode>();

                var parent = LastHit.As<DomNode>() ?? RootNode;
                if (parent == null)
                    return false;

                if (!parent.Is<SledProjectFilesFolderType>())
                    return false;

                foreach (var child in childNodes)
                {
                    //foreach (var ancestor in parent.Lineage)
                    //{
                    //    if (ancestor == child)
                    //        return false;
                    //}

                    var child1 = child;
                    if (parent.Lineage.Any(ancestor => ancestor == child1))
                        return false;

                    if (parent == child.Parent)
                        return false;

                    if (!CanParent(parent, child.Type))
                        return false;
                }

                return true;
            }

            public void Insert(object item)
            {
                var dataObject = item.As<IDataObject>();
                if (dataObject == null)
                    return;

                if (dataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    HandleFileInsert(dataObject);
                    return;
                }

                var items = dataObject.GetData(typeof(object[])) as object[];
                if (items == null)
                    return;

                if (items.Length <= 0)
                    return;

                var childNodes = items.AsIEnumerable<DomNode>();

                var parent = LastHit.As<DomNode>() ?? RootNode;
                if (parent == null)
                    return;

                try
                {
                    DragDropping = true;

                    foreach (var child in childNodes)
                    {
                        var childInfo = GetChildInfo(parent, child.Type);
                        if (childInfo == null)
                            continue;

                        if (childInfo.IsList)
                        {
                            var list = parent.GetChildList(childInfo);
                            list.Add(child);
                        }
                        else
                        {
                            parent.SetChild(childInfo, child);
                        }
                    }
                }
                finally
                {
                    DragDropping = false;
                }
            }

            public bool CanDelete()
            {
                return false;
            }

            public void Delete()
            {
            }

            #endregion

            #region INamingContext Interface

            public string GetName(object item)
            {
                var node = item.As<DomNode>();
                if (node == null)
                    return null;

                var viewable =
                    node.As<ISledProjectFilesTreeViewable>();

                return
                    viewable == null
                        ? null
                        : viewable.Name;
            }

            public bool CanSetName(object item)
            {
                var node = item.As<DomNode>();
                if (node == null)
                    return false;

                if (node.Is<SledProjectFilesType>())
                    return false;

                if (node.Is<SledProjectFilesFolderType>())
                    return true;

                if (!node.Is<SledProjectFilesFileType>())
                    return false;

                var file =
                    node.As<SledProjectFilesFileType>();

                return
                    !SledUtil.IsFileReadOnly(file.AbsolutePath);
            }

            public void SetName(object item, string name)
            {
                var node = item.As<DomNode>();
                if (node == null)
                    return;

                if (node.Is<SledProjectFilesFolderType>())
                {
                    var folder =
                        node.As<SledProjectFilesFolderType>();

                    folder.Name = name;
                }
                else if (node.Is<SledProjectFilesFileType>())
                {
                    var file =
                        node.As<SledProjectFilesFileType>();

                    var szOldPath = file.AbsolutePath;
                    var szNewPath =
                        Path.GetDirectoryName(szOldPath) +
                        Path.DirectorySeparatorChar +
                        name;

                    try
                    {
                        File.Move(szOldPath, szNewPath);

                        file.Uri = new Uri(szNewPath);
                        file.Name = name;
                        file.Path = SledUtil.GetRelativePath(szNewPath, file.Project.AssetDirectory);
                    }
                    catch (Exception ex2)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            SledUtil.TransSub(Localization.SledProjectFilesTreeErrorRenamingFile, file.Name, ex2.Message));

                        throw;
                    }
                }
            }

            #endregion

            #region IAdaptable Interface

            public object GetAdapter(Type type)
            {
                return type.Equals(typeof(ISourceControlContext)) ? m_owner : null;
            }

            #endregion

            #region IValidationContext Interface

            public event EventHandler Beginning;

            public event EventHandler Cancelled;

            public event EventHandler Ending;

            public event EventHandler Ended;

            #endregion

            #region DomNode Events

            private void RootChildInserted(object sender, ChildEventArgs e)
            {
                var viewable = e.Child.Is<ISledProjectFilesTreeViewable>();

                if (e.Child.Is<SledProjectFilesFileType>())
                    BeginValidating();

                if (viewable)
                    ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, e.Child, e.Parent));
            }

            private void RootAttributeChanged(object sender, AttributeEventArgs e)
            {
                if (!e.DomNode.Is<ISledProjectFilesTreeViewable>())
                    return;

                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            }

            private void RootChildRemoved(object sender, ChildEventArgs e)
            {
                if (!e.Child.Is<ISledProjectFilesTreeViewable>())
                    return;

                BeginValidating();
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, e.Child, e.Parent));
            }

            #endregion

            #region Private Classes

            private class SledProjectFilesTreeViewableComparer : IComparer<ISledProjectFilesTreeViewable>
            {
                public int Compare(ISledProjectFilesTreeViewable obj1, ISledProjectFilesTreeViewable obj2)
                {
                    if ((obj1 == null) && (obj2 == null))
                        return 0;

                    if (obj1 == null)
                        return 1;

                    if (obj2 == null)
                        return -1;

                    if (ReferenceEquals(obj1, obj2))
                        return 0;

                    var item1IsFolder = obj1.Is<SledProjectFilesFolderType>();
                    var item2IsFolder = obj2.Is<SledProjectFilesFolderType>();

                    if (item1IsFolder && item2IsFolder)
                        return m_comparer.Compare(obj1.Name, obj2.Name);

                    if (item1IsFolder)
                        return -1;

                    return item2IsFolder ? 1 : m_comparer.Compare(obj1.Name, obj2.Name);
                }

                private readonly StringComparer m_comparer =
                    StringComparer.CurrentCultureIgnoreCase;
            }

            #endregion

            #region Member Methods

            private void HandleFileInsert(IDataObject dataObject)
            {
                // Grab files
                var files = (string[])dataObject.GetData(DataFormats.FileDrop);
                if (files == null)
                    return;

                if (files.Length <= 0)
                    return;

                if (m_projectService == null)
                    return;

                var domHit = LastHit ?? RootNode;

                // Folder parent to use
                var folder = 
                    GetFirstAncestorOfType<SledProjectFilesFolderType>(domHit) ??
                    RootNode.As<SledProjectFilesFolderType>();

                var bAdded = false;

                string projectFileDropped = null;

                // Go through adding files to the project
                foreach (var file in files)
                {
                    if ((File.GetAttributes(file) & FileAttributes.Directory) != 0)
                        continue;

                    try
                    {
                        var fileIsProjFile = false;
                        var extension = Path.GetExtension(file);
                        foreach (var projExt in SledProjectService.ProjectExtensions)
                        {
                            if (string.Compare(extension, projExt, true) != 0)
                                continue;

                            fileIsProjFile = true;
                            projectFileDropped = file;
                            break;
                        }

                        // skip adding project files to the project
                        if (fileIsProjFile)
                            continue;
                    }
                    catch (Exception)
                    {
                        projectFileDropped = null;
                    }

                    
                    bool bAlreadyInProject;
                    var projFile =
                        m_projectFilesUtilityService.CreateFrom(
                            file,
                            m_projectService.ActiveProject,
                            out bAlreadyInProject);

                    // Skip files already in the project
                    if (bAlreadyInProject)
                        continue;

                    // Add file to the project
                    folder.Files.Add(projFile);
                    bAdded = true;
                }

                if (!bAdded && string.IsNullOrEmpty(projectFileDropped))
                    return;

                // Save project settings file
                m_projectService.SaveSettings(true);

                if (!string.IsNullOrEmpty(projectFileDropped))
                    m_projectService.Open(projectFileDropped);
            }

            private void TimerTick(object sender, EventArgs e)
            {
                var span = DateTime.Now.Subtract(m_lastUpdate);
                if (span.Milliseconds < 250)
                    return;

                EndValidating();
            }

            private void BeginValidating()
            {
                m_lastUpdate = DateTime.Now;

                if (Validating)
                    return;

                try
                {
                    Beginning.Raise(this, EventArgs.Empty);
                }
                finally
                {
                    Validating = true;
                }
            }

            private void EndValidating()
            {
                if (!Validating)
                    return;

                try
                {
                    Ending.Raise(this, EventArgs.Empty);
                    Ended.Raise(this, EventArgs.Empty);
                    Reloaded.Raise(this, EventArgs.Empty);
                }
                finally
                {
                    Validating = false;
                }
            }

            private static bool CanParent(DomNode parent, DomNodeType childType)
            {
                return GetChildInfo(parent, childType) != null;
            }

            private static ChildInfo GetChildInfo(DomNode parent, DomNodeType childType)
            {
                return
                    parent.Type.Children.FirstOrDefault(
                        childInfo => childInfo.Type.IsAssignableFrom(childType));
            }

            private static T GetFirstAncestorOfType<T>(DomNode domNode) where T : class
            {
                return domNode.Lineage.FirstOrDefault(l => l.Is<T>()).As<T>();
            }

            #endregion

            private DomNode m_root;
            private DateTime m_lastUpdate;

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

            [Import]
            private ISledProjectService m_projectService;

            [Import]
            private SledProjectFilesUtilityService m_projectFilesUtilityService;

#pragma warning restore 649

            private readonly Timer m_timer;
            private readonly SledProjectFilesTreeEditor m_owner;

            private readonly SledProjectFilesTreeViewableComparer m_comparer =
                new SledProjectFilesTreeViewableComparer();
        }

        private class EmptyTreeAdapter : ITreeView, IItemView, IInstancingContext
        {
            public EmptyTreeAdapter()
            {
                Root = new DomNode(SledSchema.SledProjectFilesEmptyType.Type);

                var emptyProj = ((DomNode)Root).As<SledProjectFilesEmptyType>();
                emptyProj.Name = @"Open a project";
            }

            #region Implementation of ITreeView

            public object Root { get; private set; }

            public IEnumerable<object> GetChildren(object parent)
            {
                return EmptyEnumerable<object>.Instance;
            }

            #endregion

            #region Implementation of IItemView

            public void GetInfo(object item, ItemInfo info)
            {
                var itemView = item.As<IItemView>();
                if (itemView == null)
                    return;

                itemView.GetInfo(item, info);
            }

            #endregion

            #region Implementation of IInstancingContext

            public bool CanCopy()
            {
                return false;
            }

            public object Copy()
            {
                return null;
            }

            public bool CanInsert(object item)
            {
                var dataObject = item.As<IDataObject>();
                if (dataObject == null)
                    return false;

                if (!dataObject.GetDataPresent(DataFormats.FileDrop))
                    return false;

                var files = dataObject.GetData(DataFormats.FileDrop) as string[];
                if (files == null)
                    return false;

                if (files.Length <= 0)
                    return false;

                foreach (var file in files)
                {
                    try
                    {
                        var extension = Path.GetExtension(file);
                        if (SledProjectService.ProjectExtensions.Any(ext => string.Compare(ext, extension, true) == 0))
                            return true;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return false;
            }

            public void Insert(object item)
            {
                var dataObject = item.As<IDataObject>();
                if (dataObject == null)
                    return;

                if (!dataObject.GetDataPresent(DataFormats.FileDrop))
                    return;

                var files = dataObject.GetData(DataFormats.FileDrop) as string[];
                if (files == null)
                    return;

                if (files.Length <= 0)
                    return;

                foreach (var file in files)
                {
                    try
                    {
                        var extension = Path.GetExtension(file);
                        if (SledProjectService.ProjectExtensions.Any(ext => string.Compare(ext, extension, true) == 0))
                            m_projectService.Open(file);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            public bool CanDelete()
            {
                return false;
            }

            public void Delete()
            {
            }

            #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

            [Import]
            private ISledProjectService m_projectService;

#pragma warning restore 649
        }

        #endregion

        #region Member Methods

        private IEnumerable<T> SelectionAs<T>() where T : class
        {
            if (TreeControl == null)
                yield break;

            var nodes = TreeControl.SelectedNodes;
            var nodesAreT = nodes.Where(n => n.Tag.Is<T>());

            foreach (var node in nodesAreT)
                yield return node.Tag.As<T>();
        }

        private void RefreshNode(ISledProjectFilesTreeViewable viewable)
        {
            if (m_domTreeAdapter.Validating)
                return;

            if (TreeControlAdapter == null)
                return;

            if (viewable == null)
                return;

            EnqueueRefresh(viewable);
        }

        private void RefreshParentNode(ISledProjectFilesTreeViewable viewable)
        {
            if (m_domTreeAdapter.Validating)
                return;

            if (TreeControlAdapter == null)
                return;

            if (viewable == null)
                return;

            if (viewable.Parent == null)
                return;

            EnqueueRefresh(viewable.Parent);
        }

        private void EnqueueRefresh(ISledProjectFilesTreeViewable viewable)
        {
            if (!m_refreshTimer.Enabled)
                m_refreshTimer.Start();

            m_lastRefreshTime = DateTime.Now;

            if (viewable == null)
                return;

            if (m_needsRefresh.Contains(viewable))
                return;

            m_needsRefresh.Add(viewable);
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            var span = DateTime.Now.Subtract(m_lastRefreshTime);
            if (span.Milliseconds < 500)
                return;

            try
            {
                foreach (var viewable in m_needsRefresh)
                {
                    TreeControlAdapter.Refresh(viewable);
                }
            }
            finally
            {
                m_needsRefresh.Clear();
                m_refreshTimer.Stop();
            }
        }

        private void ExpandParentNode(ISledProjectFilesTreeViewable viewable)
        {
            if (TreeControlAdapter == null)
                return;

            if (viewable == null)
                return;

            if (viewable.Parent == null)
                return;

            TreeControlAdapter.Expand(viewable.Parent);
        }

        private void Open(IEnumerable<ISledProjectFilesTreeViewable> viewables)
        {
            if (viewables == null)
                return;

            var files = viewables
                .Where(i => i.Is<SledProjectFilesFileType>())
                .Select(i => i.As<SledProjectFilesFileType>());

            foreach (var file in files)
            {
                ISledDocument sd;
                if (!m_documentService.Open(file.Uri, out sd))
                {
                    MessageBox.Show(
                        m_mainForm,
                        string.Format(
                            "{0}{1}{1}{2}",
                            Localization.SledProjectFilesTreeFileNotExist,
                            Environment.NewLine,
                            file.AbsolutePath));
                }
            }
        }

        private void Rename(ISledProjectFilesTreeViewable viewable)
        {
            if (viewable == null)
                return;

            var lstPaths = new List<Path<object>>(TreeControlAdapter.GetPaths(viewable));
            if (lstPaths.Count <= 0)
                return;

            TreeControlAdapter.BeginLabelEdit(lstPaths[0]);
        }

        private void Remove(IEnumerable<ISledProjectFilesTreeViewable> viewables)
        {
            if (viewables == null)
                return;

            var bDidSomething = false;

            foreach (var viewable in viewables)
            {
                var lstPaths = new List<Path<object>>(TreeControlAdapter.GetPaths(viewable));
                if (lstPaths.Count <= 0)
                    continue;

                if (viewable.Is<SledProjectFilesFileType>())
                {
                    var file = viewable.As<SledProjectFilesFileType>();
                    var folder = viewable.Parent.As<SledProjectFilesFolderType>();

                    folder.Files.Remove(file);
                    bDidSomething = true;
                }
                else if (viewable.Is<SledProjectFilesFolderType>())
                {
                    var folder = viewable.As<SledProjectFilesFolderType>();
                    var parentFolder = viewable.Parent.As<SledProjectFilesFolderType>();

                    parentFolder.Folders.Remove(folder);
                    bDidSomething = true;
                }
            }

            if (bDidSomething)
                m_projectService.SaveSettings(true);
        }

        private void NewFolder(ISledProjectFilesTreeViewable viewable)
        {
            if (viewable == null)
                return;

            var domNode = viewable.As<DomNode>();
            if (domNode == null)
                return;

            // Navigate up tree to find folder to place the new folder in
            while ((domNode != null) &&
                   !domNode.Is<SledProjectFilesFolderType>())
            {
                domNode = domNode.Parent;
            }

            if (domNode == null)
                return;

            if (!domNode.Is<SledProjectFilesFolderType>())
                return;

            var parentFolder = domNode.As<SledProjectFilesFolderType>();
            var domNodeFolder = new DomNode(SledSchema.SledProjectFilesFolderType.Type);

            var folder = domNodeFolder.As<SledProjectFilesFolderType>();
            folder.Name = NewFolderString;

            parentFolder.Folders.Add(folder);
            m_projectService.SaveSettings(true);
        }

        private static void OpenWith(ISledProjectFilesTreeViewable viewable)
        {
            if (viewable == null)
                return;
            
            var file = viewable.As<SledProjectFilesFileType>();
            if (file == null)
                return;

            SledUtil.ShellOpenWith(file.AbsolutePath);
        }

        private void Explore(ISledProjectFilesTreeViewable viewable)
        {
            if (viewable == null)
                return;

            if (viewable.Is<SledProjectFilesType>())
            {
                SledUtil.ShellOpenExplorerPath(m_projectService.ProjectDirectory);
            }
            else if (viewable.Is<SledProjectFilesFileType>())
            {
                var file = viewable.As<SledProjectFilesFileType>();
                SledUtil.ShellOpenExplorerPath(file.AbsolutePath);
            }
        }

        private void GotoFunction(IEnumerable<ISledProjectFilesTreeViewable> viewables)
        {
            if (viewables == null)
                return;

            var functions = viewables
                .Where(i => i.Is<SledFunctionBaseType>())
                .Select(i => i.As<SledFunctionBaseType>());

            foreach (var function in functions.Where(func => func.File != null))
            {
                m_gotoService.GotoLine(
                    function.File.AbsolutePath,
                    function.LineDefined,
                    false);
            }
        }

        private static void CountSelection(
            IEnumerable<ISledProjectFilesTreeViewable> selection,
            out int root, out int files, out int folders, out int functions)
        {
            root = files = folders = functions = 0;

            foreach (var item in selection)
            {
                if (item.Is<SledProjectFilesType>())
                {
                    ++root;
                    continue;
                }

                if (item.Is<SledProjectFilesFileType>())
                {
                    ++files;
                    continue;
                }

                if (item.Is<SledProjectFilesFolderType>())
                {
                    ++folders;
                    continue;
                }

                if (item.Is<SledFunctionBaseType>())
                    ++functions;
            }
        }

        #endregion

        private bool m_bDoubleClick;
        private Timer m_refreshTimer;
        private Timer m_hNeedExpandTimer;
        private bool m_bRestoringExpandState;
        private DateTime m_lastRefreshTime = DateTime.Now;

        private const string NewFolderString = "New Folder";

        private readonly MainForm m_mainForm;
        private readonly ControlInfo m_controlInfo;
        private readonly DomTreeAdapter m_domTreeAdapter;
        private readonly EmptyTreeAdapter m_emptyTreeAdapter;
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly ProgressBar m_progressBar;

        private readonly List<ISledProjectFilesTreeViewable> m_needsRefresh =
            new List<ISledProjectFilesTreeViewable>();

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledGotoService m_gotoService;

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledDocumentService m_documentService;

        [Import(AllowDefault = true)]
        private SledSourceControlService m_sourceControlService;

        [Import]
        private ISledProjectFileFinderService m_projectFileFinderService;

#pragma warning restore 649

        private static readonly IEnumerable<object> s_emptyCommands =
            EmptyEnumerable<object>.Instance;
    }
}
