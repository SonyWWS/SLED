/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Sce.Atf;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    /// <summary>
    /// SledProjectFileAdderService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectFileAdderService))]
    [Export(typeof(SledProjectFileAdderService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledProjectFileAdderService : IInitializable, ISledProjectFileAdderService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectService">Project service</param>
        [ImportingConstructor]
        public SledProjectFileAdderService(ISledProjectService projectService)
        {
            m_projectService = projectService;

            m_isHidden = fsi => (fsi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            m_dirPredicate = di => !m_isHidden(di);
        }

        #region IInitializable Interface

        /// <summary>
        /// Initialize
        /// </summary>
        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledProjectFileAdderService Interface

        /// <summary>
        /// Add the files to the project
        /// </summary>
        /// <param name="absPaths">List of absolute paths to files to add to the project</param>
        public void AddFiles(IEnumerable<string> absPaths)
        {
            AddFiles(absPaths, null);
        }

        /// <summary>
        /// Add the files to the project
        /// </summary>
        /// <param name="uris">List of uris to add to the project</param>
        public void AddFiles(IEnumerable<Uri> uris)
        {
            AddFiles(uris, null);
        }

        /// <summary>
        /// Add the files to the project
        /// </summary>
        /// <param name="absPaths">List of absolute paths to files to add to the project</param>
        /// <param name="folder">Folder that should contain the files in the project</param>
        public void AddFiles(IEnumerable<string> absPaths, SledProjectFilesFolderType folder)
        {
            foreach (var absPath in absPaths.Where(File.Exists))
            {
                SledProjectFilesFileType file;
                m_projectService.AddFile(absPath, folder, out file);
            }
        }

        /// <summary>
        /// Add the files to the project
        /// </summary>
        /// <param name="uris">List of uris to add to the project</param>
        /// <param name="folder">Folder that should contain the files in the project</param>
        public void AddFiles(IEnumerable<Uri> uris, SledProjectFilesFolderType folder)
        {
            AddFiles(uris.Select(u => u.AbsolutePath), folder);
        }

        /// <summary>
        /// Add the files from the specified folders to the project
        /// </summary>
        /// <param name="absFolderPaths">Folders to search in</param>
        /// <param name="searchOption">Whether to recursively search or not</param>
        /// <param name="extensions">File extensions to grab</param>
        public void AddFilesInFolders(IEnumerable<string> absFolderPaths, SearchOption searchOption, IEnumerable<string> extensions)
        {
            AddFilesInFolders(absFolderPaths, searchOption, extensions, null);
        }

        /// <summary>
        /// Add the files from the specified folders to the project
        /// </summary>
        /// <param name="absFolderPaths">Folders to search in</param>
        /// <param name="searchOption">Whether to recursively search or not</param>
        /// <param name="extensions">File extensions to grab</param>
        /// <param name="folder">Folder that should contain the files</param>
        public void AddFilesInFolders(IEnumerable<string> absFolderPaths, SearchOption searchOption, IEnumerable<string> extensions, SledProjectFilesFolderType folder)
        {
            foreach (var absFolderPath in absFolderPaths)
            {
                try
                {
                    var dir = new DirectoryInfo(absFolderPath);
                    if (!dir.Exists)
                        continue;

                    Func<FileInfo, bool> filePredicate =
                        fi =>
                        {
                            if (m_isHidden(fi))
                                return false;

                            return extensions == null || extensions.Any(ext => string.Compare(ext, fi.Extension, StringComparison.OrdinalIgnoreCase) == 0);
                        };

                    var tree = dir.GetFilesAndDirectoriesTree(searchOption, filePredicate, m_dirPredicate);
                    AddTree(m_projectService, folder, tree, null);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception finding files in directory {1}: {2}",
                        this, absFolderPaths, ex.Message);
                }
            }
        }

        /// <summary>
        /// Add the files from the specified tree to the project
        /// </summary>
        /// <param name="tree">Tree representing disk structure to add to the project</param>
        /// <param name="extensions">Extensions from the tree to add to the project (null adds all files)</param>
        /// <param name="folder">Folder that should contain the files after adding (null adds to root)</param>
        public void AddFilesFromTree(Tree<FileSystemInfo> tree, IEnumerable<string> extensions, SledProjectFilesFolderType folder)
        {
            if (tree == null)
                return;

            AddTree(m_projectService, folder, tree, extensions);
        }

        private static void AddTree(ISledProjectService projectService, SledProjectFilesFolderType folder, Tree<FileSystemInfo> tree, IEnumerable<string> extensions)
        {
            if ((projectService == null) || (tree == null))
                return;

            var parent = folder;
            if (tree.Value.IsDirectory())
                projectService.AddFolder(tree.Value.Name, folder, out parent);

            foreach (var item in tree.Children)
            {
                if (item.Value.IsDirectory())
                    AddTree(projectService, parent, item, extensions);
                else
                {
                    var theItem = item;
                    var shouldAdd =
                        extensions == null
                            ? true
                            : extensions.Any(ext => string.Compare(ext, theItem.Value.Extension, StringComparison.OrdinalIgnoreCase) == 0);

                    if (!shouldAdd)
                        continue;

                    SledProjectFilesFileType projFile;
                    projectService.AddFile(item.Value.FullName, parent, out projFile);
                }
            }
        }

        #endregion

        private readonly ISledProjectService m_projectService;

        private readonly Func<FileSystemInfo, bool> m_isHidden;
        private readonly Func<DirectoryInfo, bool> m_dirPredicate;
    }
}