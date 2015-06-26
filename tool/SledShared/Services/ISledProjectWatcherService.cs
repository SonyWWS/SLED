/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED project watcher service EventArgs class
    /// </summary>
    public class SledProjectWatcherServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="absolutePath">Absolute path to project</param>
        public SledProjectWatcherServiceEventArgs(string absolutePath)
        {
            AbsolutePath = absolutePath;
        }

        /// <summary>
        /// Absolute path to project that changed
        /// </summary>
        public readonly string AbsolutePath;
    }

    /// <summary>
    /// SLED project watcher service interface
    /// </summary>
    public interface ISledProjectWatcherService
    {
        /// <summary>
        /// Event triggered when the project file is changed
        /// </summary>
        event EventHandler<SledProjectWatcherServiceEventArgs> FileChangedEvent;

        /// <summary>
        /// Event triggered when the project file's attribute is changed
        /// </summary>
        event EventHandler<SledProjectWatcherServiceEventArgs> AttributeChangedEvent;
    }
}
