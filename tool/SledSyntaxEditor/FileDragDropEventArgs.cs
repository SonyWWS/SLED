/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Used as a event args for FileDragDropped event</summary>
    public class FileDragDropEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="paths">Files getting drag and dropped</param>
        public FileDragDropEventArgs(IEnumerable<string> paths)
        {
            Paths = new List<string>(paths);
        }

        /// <summary>
        /// Gets the absolute paths to the files or folders being drag and dropped</summary>
        public IEnumerable<string> Paths { get; private set; }

        /// <summary>
        /// Gets or sets whether the event should be cancelled</summary>
        public bool Cancel { get; set; }
    }
}