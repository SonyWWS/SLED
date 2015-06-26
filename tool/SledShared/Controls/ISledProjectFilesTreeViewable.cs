/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// Interface for viewing project files in a tree</summary>
    public interface ISledProjectFilesTreeViewable : IAdaptable
    {
        /// <summary>
        /// Gets the name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets whether the item is expanded</summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets the parent</summary>
        ISledProjectFilesTreeViewable Parent { get; }

        /// <summary>
        /// Gets any children</summary>
        IEnumerable<ISledProjectFilesTreeViewable> Children { get; }
    }
}
