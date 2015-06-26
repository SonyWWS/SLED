/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using Sce.Sled.Shared;
using Sce.Sled.Project.Resources;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    public partial class SledProjectAutoFilesAddForm : Form
    {
        public SledProjectAutoFilesAddForm()
        {
            InitializeComponent();

            m_treeCheckedFiles.ImageList = Atf.ResourceUtil.GetImageList16();
            m_treeCheckedFiles.RecursiveCheckBoxes = true;
            m_treeCheckedFiles.ShowNodeToolTips = true;

            s_folderImageIdx = m_treeCheckedFiles.ImageList.Images.IndexOfKey(Atf.Resources.FolderImage);
        }

        public IEnumerable<string> Extensions
        {
            get { return m_txtFileExtsAutoCheck.Text.FromCommaSeparatedValues(); }
            set
            {
                if (value == null)
                    m_txtFileExtsAutoCheck.Clear();
                else
                    m_txtFileExtsAutoCheck.Text = value.ToCommaSeparatedValues(true);
            }
        }

        public IEnumerable<string> CheckedFiles
        {
            get
            {
                foreach (TreeNode node in m_treeCheckedFiles.Nodes)
                {
                    foreach (var absPath in GetAllCheckedFiles(node))
                        yield return absPath;
                }
            }
        }

        public void AddDirectory(string absDirPath)
        {
            var di = SledUtil.CreateDirectoryInfo(absDirPath, true);
            if (di == null)
                return;

            AddDirectory(di);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            m_shown = true;

            ScanDirectories(m_enqueuedDirs);
            m_enqueuedDirs.Clear();
        }

        private void ChkAutoFilesCheckedChanged(object sender, EventArgs e)
        {
            if (!m_chkAutoFiles.Checked)
                return;

            try
            {
                m_treeCheckedFiles.BeginUpdate();
                
                var localExtensions = new List<string>(Extensions);
                if (!localExtensions.Any())
                    return;

                const string starDotStar = "*.*";
                var hasStarDotStar = localExtensions.Any(ext => string.Compare(ext, starDotStar, StringComparison.Ordinal) == 0);

                Func<FileInfo, bool> filePredicate =
                    fi => localExtensions.Any(extension => hasStarDotStar || string.Compare(extension, fi.Extension, StringComparison.OrdinalIgnoreCase) == 0);

                foreach (TreeNode node in m_treeCheckedFiles.Nodes)
                    CheckMarkMatchingFiles(node, filePredicate);
            }
            finally
            {
                m_treeCheckedFiles.EndUpdate();
            }
        }

        private void BtnDirAddClick(object sender, EventArgs e)
        {
            var hDialog =
                new FolderBrowserDialog
                    {
                        Description = Localization.SledProjectAutoAddFilesSelectDirTitle,
                        RootFolder = Environment.SpecialFolder.Desktop
                    };

            if (hDialog.ShowDialog(this) != DialogResult.OK)
                return;

            AddDirectory(hDialog.SelectedPath);
        }

        private void BtnRemoveDirClick(object sender, EventArgs e)
        {
            var wrapper = m_lstDirUsed.SelectedItem.As<Wrapper>();
            if (wrapper == null)
                return;

            m_lstDirUsed.Items.Remove(wrapper);
            m_wrappers.Remove(wrapper);

            TreeNode node;
            if (!m_wrapperRootNodes.TryGetValue(wrapper, out node))
                return;

            m_wrapperRootNodes.Remove(wrapper);
            m_treeCheckedFiles.Nodes.Remove(node);
        }

        private void LstDirUsedSelectedIndexChanged(object sender, EventArgs e)
        {
            var isItemSelected = m_lstDirUsed.SelectedItem != null;
            m_btnRemoveDir.Enabled = isItemSelected;
        }
        
        private void AddDirectory(DirectoryInfo di)
        {
            if (m_shown)
            {
                ScanDirectories(new[] { di });
            }
            else
            {
                if (!m_enqueuedDirs.Any(d => string.Compare(d.FullName, di.FullName, StringComparison.Ordinal) == 0))
                    m_enqueuedDirs.Add(di);
            }
        }

        private void ScanDirectories(IEnumerable<DirectoryInfo> dirInfos)
        {
            var localDirectories = new List<DirectoryInfo>();
            foreach (var dirInfo in dirInfos)
            {
                var localDirInfo = dirInfo;

                var duplicate =
                    m_wrappers.FirstOrDefault(
                        wrapper => string.Compare(wrapper.Directory.FullName, localDirInfo.FullName, StringComparison.OrdinalIgnoreCase) == 0);

                if (duplicate == null)
                    localDirectories.Add(dirInfo);
            }

            if (!localDirectories.Any())
                return;

            var localMappings = new Dictionary<DirectoryInfo, Atf.Tree<FileSystemInfo>>();

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
                        Func<FileInfo, bool> filePredicate = fi => !isHidden(fi);

                        Func<DirectoryInfo, bool> dirPredicate =
                            di =>
                            {
                                // Sometimes the root directory can show up
                                // as hidden... depending on what path is
                                // passed into the DirectoryInfo upon creation
                                if (di.Parent == null)
                                    return true;

                                var hidden = isHidden(di);

                                // ReSharper disable AccessToDisposedClosure
                                if (!hidden)
                                    dialog.Label = string.Format("{0} ({1})", scanning, di.Name);
                                // ReSharper restore AccessToDisposedClosure

                                return !hidden;
                            };

                        foreach (var di in localDirectories)
                        {
                            try
                            {
                                // Gather files and folders recursively from the directory
                                var tree = di.GetFilesAndDirectoriesTree(SearchOption.AllDirectories, filePredicate, dirPredicate, cancel);
                                if (tree == null)
                                    continue;

                                localMappings.Add(di, tree);
                            }
                            catch (Exception ex)
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    "{0}: Exception enumerating directory \"{1}\": {2}",
                                    this, di.FullName, ex.Message);
                            }
                        }

                        return true;
                    };

                dialog.Task = asyncTask;
                dialog.Text = string.Format("SLED - {0}", scanning);
                dialog.Label = scanning;

                // 'DialogResult.Yes' means task ran to completion and returned 'true'
                if (dialog.ShowDialog(this) != DialogResult.Yes)
                    return;

                var newWrappers = new List<Wrapper>();
                foreach (var kv in localMappings)
                {
                    var wrapper = new Wrapper(kv.Key, kv.Value);
                    newWrappers.Add(wrapper);
                    m_wrappers.Add(wrapper);
                    m_lstDirUsed.Items.Add(wrapper);
                }

                try
                {
                    m_treeCheckedFiles.BeginUpdate();

                    foreach (var wrapper in newWrappers)
                    {
                        var rootDirNode =
                            new TreeNode(wrapper.ToString())
                            {
                                Tag = wrapper.FilesAndFolders.Value,
                                ToolTipText = wrapper.FilesAndFolders.Value.FullName
                            };
                        m_treeCheckedFiles.Nodes.Add(rootDirNode);
                        m_wrapperRootNodes.Add(wrapper, rootDirNode);

                        AddFilesAndFoldersToRoot(rootDirNode, wrapper.FilesAndFolders);
                        rootDirNode.ImageIndex = s_folderImageIdx;
                        rootDirNode.SelectedImageIndex = rootDirNode.ImageIndex;
                    }
                }
                finally
                {
                    m_treeCheckedFiles.EndUpdate();
                }
            }
        }

        private static void AddFilesAndFoldersToRoot(TreeNode rootNode, Atf.Tree<FileSystemInfo> filesAndFolders)
        {
            foreach (var child in filesAndFolders.Children)
            {
                var childNode =
                    new TreeNode(child.Value.Name)
                        {
                            Tag = child.Value,
                            ToolTipText = child.Value.FullName
                        };
                rootNode.Nodes.Add(childNode);

                if (child.Value.IsDirectory())
                {
                    childNode.ImageIndex = s_folderImageIdx;
                    childNode.SelectedImageIndex = childNode.ImageIndex;
                }
                else
                {
                    childNode.ImageKey = GetImageKeyFromExtension(child.Value.Extension);
                    childNode.SelectedImageKey = childNode.ImageKey;
                }

                AddFilesAndFoldersToRoot(childNode, child);
            }
        }

        private static Dictionary<string, string> CreateExtensionToImageKeyMapping()
        {
            var dictionary = new Dictionary<string, string>();

            var documentClients = SledServiceInstance.GetAll<IDocumentClient>();
            foreach (var documentClient in documentClients)
            {
                foreach (var ext in documentClient.Info.Extensions)
                {
                    // if multiple document clients use the same extension, only the first is chosen
                    if (dictionary.ContainsKey(ext))
                        continue;

                    dictionary.Add(ext, documentClient.Info.NewIconName);
                }
            }

            return dictionary;
        }

        private static string GetImageKeyFromExtension(string extension)
        {
            if (s_extensionsToImageKeys == null)
                s_extensionsToImageKeys = CreateExtensionToImageKeyMapping();

            string imageKey;
            if (!s_extensionsToImageKeys.TryGetValue(extension, out imageKey))
                imageKey = Atf.Resources.DataImage;

            return imageKey;
        }

        private static void CheckMarkMatchingFiles(TreeNode rootNode, Func<FileInfo, bool> filePredicate)
        {
            var info = rootNode.Tag.As<FileInfo>();
            if ((info != null) && filePredicate(info))
                rootNode.Checked = true;

            foreach (TreeNode node in rootNode.Nodes)
                CheckMarkMatchingFiles(node, filePredicate);
        }

        private static bool IsACheckedFile(TreeNode node, out string absPathToFile)
        {
            var info = node.Tag.As<FileInfo>();

            absPathToFile = info == null ? null : node.Checked ? info.FullName : null;

            return absPathToFile != null;
        }

        private static IEnumerable<string> GetAllCheckedFiles(TreeNode rootNode)
        {
            string absPath;
            if (IsACheckedFile(rootNode, out absPath))
                yield return absPath;

            foreach (TreeNode childNode in rootNode.Nodes)
            {
                foreach (string anotherAbsPath in GetAllCheckedFiles(childNode))
                    yield return anotherAbsPath;
            }
        }

        private class Wrapper
        {
            public Wrapper(DirectoryInfo di, Atf.Tree<FileSystemInfo> filesAndFolders)
            {
                Directory = di;
                FilesAndFolders = filesAndFolders;
            }

            public override string ToString()
            {
                return Directory.FullName;
            }

            public DirectoryInfo Directory { get; private set; }

            public Atf.Tree<FileSystemInfo> FilesAndFolders { get; private set; }
        }

        private bool m_shown;

        private readonly List<Wrapper> m_wrappers =
            new List<Wrapper>();

        private readonly List<DirectoryInfo> m_enqueuedDirs =
            new List<DirectoryInfo>();

        private readonly Dictionary<Wrapper, TreeNode> m_wrapperRootNodes =
            new Dictionary<Wrapper, TreeNode>();

        private static int s_folderImageIdx = -1;

        private static Dictionary<string, string> s_extensionsToImageKeys;
    }
}