/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Sled.Shared.Document;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED file watcher service EventArgs class
    /// </summary>
    public class SledFileWatcherServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sd">ISledDocument</param>
        public SledFileWatcherServiceEventArgs(ISledDocument sd)
        {
            Document = sd;
        }

        /// <summary>
        /// ISledDocument
        /// </summary>
        public readonly ISledDocument Document;
    }

    /// <summary>
    /// SLED file watcher service interface
    /// </summary>
    public interface ISledFileWatcherService
    {
        /// <summary>
        /// Event triggered when a file is changed
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> FileChangedEvent;

        /// <summary>
        /// Event triggered when a file's attribute is changed
        /// </summary>
        event EventHandler<SledFileWatcherServiceEventArgs> AttributeChangedEvent;

        /// <summary>
        /// Try to remove the read only attribute from a file
        /// </summary>
        /// <param name="sd">Document to remove read only attribute from</param>
        /// <returns>True iff read only attribute was removed</returns>
        bool TryRemoveReadOnlyAttribute(ISledDocument sd);
    }
}
