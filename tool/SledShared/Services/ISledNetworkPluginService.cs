/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Sled.Shared.Plugin;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED network plugin service
    /// </summary>
    public interface ISledNetworkPluginService
    {
        /// <summary>
        /// Get network plugins
        /// </summary>
        IEnumerable<ISledNetworkPlugin> NetworkPlugins
        {
            get;
        }

        /// <summary>
        /// Get number of network plugins
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Get whether the service has run its initialization method yet
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Event triggered when the service runs its initialization method
        /// </summary>
        event EventHandler Initialized;
    }
}
