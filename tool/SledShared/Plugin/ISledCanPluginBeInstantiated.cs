/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// Interface to determine if plugin can be instantiated 
    /// </summary>
    public interface ISledCanPluginBeInstantiated
    {
        /// <summary>
        /// Gets whether plugin can be loaded and used later
        /// </summary>
        bool CanPluginBeInstantiated { get; }
    }
}