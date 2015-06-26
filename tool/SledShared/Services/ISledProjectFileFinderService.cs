/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED project file finder service interface
    /// <remarks>Utility for finding files inside an active project</remarks>
    /// </summary>
    public interface ISledProjectFileFinderService
    {
        /// <summary>
        /// Try to find a SledDocument in the project
        /// </summary>
        /// <remarks>This version uses the currently active project (if any)</remarks>
        /// <param name="sd">SledDocument to look for</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(ISledDocument sd);

        /// <summary>
        /// Try to find a SledDocument in the specified project
        /// </summary>
        /// <param name="sd">SledDocument to look for</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(ISledDocument sd, SledProjectFilesType project);

        /// <summary>
        /// Try to find a file with the specified path in the project
        /// </summary>
        /// <remarks>This version uses the currently active project (if any)</remarks>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(string szAbsPath);

        /// <summary>
        /// Try to find a file with the specified path in the project
        /// </summary>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(string szAbsPath, SledProjectFilesType project);

        /// <summary>
        /// Try to find a file with the specified path in the project
        /// </summary>
        /// <remarks>This version uses the currently active project (if any)</remarks>
        /// <param name="uri">URI for file</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(Uri uri);

        /// <summary>
        /// Try to find a file with the specified path in the project
        /// </summary>
        /// <param name="uri">URI for file</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(Uri uri, SledProjectFilesType project);

        /// <summary>
        /// Try to find a matching ISledProjectFilesFileType in the project
        /// </summary>
        /// <remarks>This version uses the currently active project (if any)</remarks>
        /// <param name="file">File to look for</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(SledProjectFilesFileType file);

        /// <summary>
        /// Try to find a matching ISledProjectFilesFileType in the project
        /// </summary>
        /// <param name="file">File to look for</param>
        /// <param name="project">Project to look in</param>
        /// <returns>Reference to file in the project, otherwise null</returns>
        SledProjectFilesFileType Find(SledProjectFilesFileType file, SledProjectFilesType project);
    }
}
