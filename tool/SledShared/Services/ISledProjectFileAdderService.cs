/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED project file adder service interface
    /// </summary>
    public interface ISledProjectFileAdderService
    {
        /// <summary>
        /// Add files to the project
        /// </summary>
        /// <param name="absPaths">List of absolute paths of files to add to the project</param>
        void AddFiles(IEnumerable<string> absPaths);

        /// <summary>
        /// Add files to the project
        /// </summary>
        /// <param name="uris">List of URIs of files to add to the project</param>
        void AddFiles(IEnumerable<Uri> uris);

        /// <summary>
        /// Add files to the project
        /// </summary>
        /// <param name="absPaths">List of absolute paths to files to add to the project</param>
        /// <param name="folder">Folder that should contain the files in the project</param>
        void AddFiles(IEnumerable<string> absPaths, SledProjectFilesFolderType folder);

        /// <summary>
        /// Add files to the project
        /// </summary>
        /// <param name="uris">List of URIs for files to add to the project</param>
        /// <param name="folder">Folder that should contain the files in the project</param>
        void AddFiles(IEnumerable<Uri> uris, SledProjectFilesFolderType folder);

        /// <summary>
        /// Add files from specified folders to the project
        /// </summary>
        /// <param name="absFolderPaths">Folders to search in</param>
        /// <param name="searchOption">Whether to recursively search</param>
        /// <param name="extensions">File extensions to add</param>
        void AddFilesInFolders(IEnumerable<string> absFolderPaths, SearchOption searchOption, IEnumerable<string> extensions);

        /// <summary>
        /// Add files from specified folders to the project
        /// </summary>
        /// <param name="absFolderPaths">Folders to search in</param>
        /// <param name="searchOption">Whether to recursively search</param>
        /// <param name="extensions">File extensions to add</param>
        /// <param name="folder">Folder that should contain the files</param>
        void AddFilesInFolders(IEnumerable<string> absFolderPaths, SearchOption searchOption, IEnumerable<string> extensions, SledProjectFilesFolderType folder);

        /// <summary>
        /// Add files from specified tree to the project
        /// </summary>
        /// <param name="tree">Tree representing disk structure to add to the project</param>
        /// <param name="extensions">Extensions from tree to add to the project (null adds all files)</param>
        /// <param name="folder">Folder that should contain the files after adding (null adds to root)</param>
        void AddFilesFromTree(Tree<FileSystemInfo> tree, IEnumerable<string> extensions, SledProjectFilesFolderType folder);
    }
}