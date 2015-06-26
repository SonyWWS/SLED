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
    /// SLED language plugin service
    /// </summary>
    public interface ISledLanguagePluginService
    {
        /// <summary>
        /// Get plugins
        /// </summary>
        IEnumerable<KeyValuePair<UInt16, ISledLanguagePlugin>> LanguagePlugins
        {
            get;
        }

        /// <summary>
        /// Get number of language plugins
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Try to find a plugin with a certain ID in the dictionary
        /// </summary>
        /// <param name="pluginId">ID to look for</param>
        /// <param name="plugin">Handle to plugin if found</param>
        /// <returns>True iff found</returns>
        bool TryGetPlugin(UInt16 pluginId, out ISledLanguagePlugin plugin);

        /// <summary>
        /// Return a reference to a plugin that should manage a file of a given extension
        /// </summary>
        /// <param name="ext">File extension (including the period)</param>
        /// <returns>Language plugin reference or null</returns>
        ISledLanguagePlugin GetPluginForExtension(string ext);
    }
}
