/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Project
{
    /// <summary>
    /// SledProjectFileFinderService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectFileFinderService))]
    [Export(typeof(SledProjectFileFinderService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledProjectFileFinderService : IInitializable, ISledProjectFileFinderService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectService">Project service</param>
        [ImportingConstructor]
        public SledProjectFileFinderService(ISledProjectService projectService)
        {
            m_projectService = projectService;
        }

        #region IInitializable Interface

        /// <summary>
        /// Initialize
        /// </summary>
        void IInitializable.Initialize()
        {
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.SavedAsing += (s, e) => m_projectSaveAs = true;
            m_projectService.SavedAsed += (s, e) => m_projectSaveAs = false;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRenamed += ProjectServiceFileRenamed;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;
            m_projectService.Closing += ProjectServiceClosing;
            
            m_projectService.AssetDirChanging += ProjectServiceAssetDirChanging;
        }

        #endregion

        #region ISledProjectFileFinderService Interface

        /// <summary>
        /// Try and find a SledDocument in the project
        /// </summary>
        /// <param name="sd">SledDocument to look for</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(ISledDocument sd)
        {
            return Find(sd, m_projectService.ActiveProject);
        }

        /// <summary>
        /// Try and find a SledDocument in the specified project
        /// </summary>
        /// <param name="sd">SledDocument to look for</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(ISledDocument sd, SledProjectFilesType project)
        {
            return sd == null ? null : Find(sd.Uri, project);
        }

        /// <summary>
        /// Try and find a file with the specified path in the project
        /// </summary>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(string szAbsPath)
        {
            return Find(szAbsPath, m_projectService.ActiveProject);
        }

        /// <summary>
        /// Try and find a file with the specified path in the project
        /// </summary>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(string szAbsPath, SledProjectFilesType project)
        {
            if (string.IsNullOrEmpty(szAbsPath))
                return null;

            if (!File.Exists(szAbsPath))
                return null;

            if (project == null)
                return null;

            SledProjectFilesFileType projFile;
            m_dictProjFiles.TryGetValue(szAbsPath, out projFile);

            return projFile;
        }

        /// <summary>
        /// Try and find a file with the specified path in the project
        /// </summary>
        /// <param name="uri">Path to file</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(Uri uri)
        {
            return Find(uri, m_projectService.ActiveProject);
        }

        /// <summary>
        /// Try and find a file with the specified path in the project
        /// </summary>
        /// <param name="uri">Path to file</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(Uri uri, SledProjectFilesType project)
        {
            return Find(uri.LocalPath, project);
        }

        /// <summary>
        /// Try and find a matching ISledProjectFilesFileType in the project
        /// </summary>
        /// <param name="file">File to look for</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(SledProjectFilesFileType file)
        {
            return Find(file, m_projectService.ActiveProject);
        }

        /// <summary>
        /// Try and find a matching ISledProjectFilesFileType in the project
        /// </summary>
        /// <param name="file">File to look for</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project otherwise null</returns>
        public SledProjectFilesFileType Find(SledProjectFilesFileType file, SledProjectFilesType project)
        {
            return file == null ? null : Find(file.AbsolutePath, project);
        }

        #endregion

        #region SledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            ClearAndReload(e.Project.AllFiles);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            ClearAndReload(e.Project.AllFiles);
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            m_dictProjFiles.Add(e.File.AbsolutePath, e.File);
        }

        private void ProjectServiceFileRenamed(object sender, SledProjectServiceFileEventArgs e)
        {
            m_dictProjFiles.Remove(e.OldPath);
            m_dictProjFiles.Add(e.File.AbsolutePath, e.File);
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            m_dictProjFiles.Remove(e.File.AbsolutePath);
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_dictProjFiles.Clear();
        }

        private void ProjectServiceAssetDirChanging(object sender, SledProjectServiceAssetDirEventArgs e)
        {
            if (m_projectSaveAs)
                return;

            m_dictProjFiles.Clear();
        }

        #endregion

        internal void ClearAndReload(IEnumerable<SledProjectFilesFileType> files)
        {
            m_dictProjFiles.Clear();

            foreach (var file in files)
                m_dictProjFiles.Add(file.AbsolutePath, file);
        }

        private bool m_projectSaveAs;

        private readonly ISledProjectService m_projectService;

        private readonly Dictionary<string, SledProjectFilesFileType> m_dictProjFiles =
            new Dictionary<string, SledProjectFilesFileType>(StringComparer.CurrentCultureIgnoreCase);
    }
}
