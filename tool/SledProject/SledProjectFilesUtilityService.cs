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
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledProjectFilesUtilityService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledProjectFilesUtilityService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        /// <summary>
        /// Create a new project file or return a reference to one in the project if it already exists
        /// </summary>
        /// <param name="sd">Open document</param>
        /// <param name="bAlreadyInProject">Whether the project file already exists in the project</param>
        /// <param name="project">Project to aid in file creation</param>
        /// <returns>Project file</returns>
        public SledProjectFilesFileType CreateFrom(ISledDocument sd, SledProjectFilesType project, out bool bAlreadyInProject)
        {
            // Check for existing
            var file = m_projectFilesFinderService.Get.Find(sd);
            if (file != null)
            {
                bAlreadyInProject = true;
                return file;
            }

            // Create new
            file = SledProjectFilesFileType.Create(sd.Uri.LocalPath, project);
            SetupFile(file);

            // Set references
            SetReferences(file, sd);

            bAlreadyInProject = false;
            return file;
        }

        /// <summary>
        /// Create a new project file or return a reference to one in the project if it already exists
        /// </summary>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <param name="bAlreadyInProject">Whether the project file already exists in the project</param>
        /// <param name="project">Project to aid in file creation</param>
        /// <returns>Project file</returns>
        public SledProjectFilesFileType CreateFrom(string szAbsPath, SledProjectFilesType project, out bool bAlreadyInProject)
        {
            // Check for existing
            var file = m_projectFilesFinderService.Get.Find(szAbsPath);
            if (file != null)
            {
                bAlreadyInProject = true;
                return file;
            }

            // Create new
            file = SledProjectFilesFileType.Create(szAbsPath, project);
            SetupFile(file);

            // Check if matching open document
            ISledDocument sd;
            if (m_documentService.Get.IsOpen(new Uri(szAbsPath), out sd))
            {
                // Set references
                SetReferences(file, sd);
            }

            bAlreadyInProject = false;
            return file;
        }

        /// <summary>
        /// Setup files (assign language plugins, generate MD5 hash, sync breakpoints between project files and open documents)
        /// </summary>
        /// <param name="files">Files</param>
        public void SetupFiles(IEnumerable<SledProjectFilesFileType> files)
        {
            var filesEnumerated = new List<SledProjectFilesFileType>(files);

            // sets up Uri so that we can...
            foreach (var file in filesEnumerated)
            {
                SetupFile(file);
            }

            // ... preload the dictionary with files so that references can be set correctly
            ((SledProjectFileFinderService)m_projectFilesFinderService.Get).ClearAndReload(filesEnumerated);

            // Setup references between any open documents that may now
            // be project documents and also sync up breakpoints if files match
            foreach (var sd in m_documentService.Get.OpenDocuments)
            {
                if (sd.SledProjectFile != null)
                    continue;

                // Try and find a file in the project that matches this open document
                var projFile = m_projectFilesFinderService.Get.Find(sd);
                if (projFile != null)
                    SetReferences(projFile, sd);
            }
        }

        private void SetupFile(SledProjectFilesFileType file)
        {
            // Assign language plugin (if any)
            file.LanguagePlugin = m_languagePluginService.Get.GetPluginForExtension(Path.GetExtension(file.Path));

            var project = file.Project ?? m_projectService.Get.ActiveProject;

            // Set Uri
            var absPath = SledUtil.GetAbsolutePath(file.Path, project.AssetDirectory);

            file.Uri = new Uri(absPath);
        }

        private static void SetReferences(SledProjectFilesFileType file, ISledDocument sd)
        {
            file.SledDocument = sd;
            sd.SledProjectFile = file;
        }

        private readonly SledServiceReference<ISledDocumentService> m_documentService = 
            new SledServiceReference<ISledDocumentService>();

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();

        private readonly SledServiceReference<ISledLanguagePluginService> m_languagePluginService = 
            new SledServiceReference<ISledLanguagePluginService>();

        private readonly SledServiceReference<ISledProjectFileFinderService> m_projectFilesFinderService = 
            new SledServiceReference<ISledProjectFileFinderService>();
    }
}
