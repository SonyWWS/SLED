/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED directory info service interface
    /// </summary>
    public interface ISledDirectoryInfoService
    {
        /// <summary>
        /// Get path to Sled.exe
        /// </summary>
        string ExePath { get; }

        /// <summary>
        /// Get directory containing Sled.exe
        /// </summary>
        string ExeDirectory { get; }

        /// <summary>
        /// Get directory containing SLED plugins
        /// </summary>
        string PluginDirectory { get; }
    }
}