/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.IO;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// Wrapper around FileSystemWatcher class
    /// </summary>
    public class SledFileSystemWatcher : FileSystemWatcher
    {
        /// <summary>
        /// Get or set user data
        /// </summary>
        public object Tag { get; set; }
    }
}
