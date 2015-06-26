/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED project file gatherer service interface
    /// </summary>
    public interface ISledProjectFileGathererService
    {
        /// <summary>
        /// Get all project files
        /// </summary>
        /// <returns>List of project files</returns>
        IEnumerable<SledProjectFilesFileType> GetFiles();

        /// <summary>
        /// Get all project files that match given extensions
        /// </summary>
        /// <param name="extensions">List of extensions</param>
        /// <returns>List of project files</returns>
        IEnumerable<SledProjectFilesFileType> GetFilesMatchingExtensions(IEnumerable<string> extensions);

        /// <summary>
        /// Get all project files that are owned by the given language plugin
        /// </summary>
        /// <param name="languagePlugin">Language plugin</param>
        /// <returns>List of project files</returns>
        IEnumerable<SledProjectFilesFileType> GetFilesOwnedByPlugin(ISledLanguagePlugin languagePlugin);
    }
}
