/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledProjectFileGathererService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectFileGathererService))]
    [Export(typeof(SledProjectFileGathererService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledProjectFileGathererService : IInitializable, ISledProjectFileGathererService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledProjectFileGathererService Interface

        /// <summary>
        /// Get all project files
        /// </summary>
        /// <returns>List of project files</returns>
        public IEnumerable<SledProjectFilesFileType> GetFiles()
        {
            return
                !m_projectService.Get.Active
                    ? s_emptyList
                    : m_projectService.Get.AllFiles;
        }

        /// <summary>
        /// Get all project files that match certain extensions
        /// </summary>
        /// <param name="extensions">List of extensions</param>
        /// <returns>List of project files</returns>
        public IEnumerable<SledProjectFilesFileType> GetFilesMatchingExtensions(IEnumerable<string> extensions)
        {
            if (!m_projectService.Get.Active)
                return s_emptyList;

            if (extensions == null)
                return GetFiles();

            // Go through all files in the project checking extensions
            var lstFiles =
                from projFile in m_projectService.Get.AllFiles
                let projFileExt = Path.GetExtension(projFile.Path)
                where !string.IsNullOrEmpty(projFileExt)
                let bFound = extensions.Any(ext => string.Compare(projFileExt, ext, true) == 0)
                where bFound
                select projFile;

            return lstFiles;
        }

        /// <summary>
        /// Get all project files that are owned by the specific plugin
        /// </summary>
        /// <param name="languagePlugin">Language plugin</param>
        /// <returns>List of project files</returns>
        public IEnumerable<SledProjectFilesFileType> GetFilesOwnedByPlugin(ISledLanguagePlugin languagePlugin)
        {
            if (!m_projectService.Get.Active || (languagePlugin == null))
                return s_emptyList;

            // Go through all files in the project checking language plugin
            var lstFiles =
                from projFile in m_projectService.Get.AllFiles
                where projFile.LanguagePlugin == languagePlugin
                select projFile;

            return lstFiles;
        }

        #endregion

        private static readonly SledProjectFilesFileType[] s_emptyList =
            EmptyArray<SledProjectFilesFileType>.Instance;

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();
    }
}
