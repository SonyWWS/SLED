/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED modified files form service interface
    /// </summary>
    public interface ISledModifiedFilesFormService
    {
        /// <summary>
        /// Event triggered when the user selects to reload a file that has been modified outside of the editor
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> FileReloading;

        /// <summary>
        /// Event triggered after a modified file has been reloaded in the editor
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> FileReloaded;

        /// <summary>
        /// Event triggered when the user selects to ignore a file modified outside of the editor
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> FileIgnoring;

        /// <summary>
        /// Event triggered after a modified file has been ignored in the editor
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> FileIgnored;
    }
}
