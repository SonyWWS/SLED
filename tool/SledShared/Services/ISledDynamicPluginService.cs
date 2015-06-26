/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED dynamic plugin info class
    /// </summary>
    public class SledDynamicPluginInfo
    {
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="name">Name of plugin (with extension)</param>
        /// <param name="absPath">Absolute path to plugin</param>
        /// <param name="bLoaded">Whether the plugin was successfully loaded</param>
        /// <param name="exception">Exception reported when trying to load (or null if no exception occurred)</param>
        public SledDynamicPluginInfo(string name, string absPath, bool bLoaded, Exception exception)
        {
            Name = name;
            AbsolutePath = absPath;
            Loaded = bLoaded;
            Exception = exception;
        }

        /// <summary>
        /// Name of plugin (with extension)
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Absolute path to plugin
        /// </summary>
        public readonly string AbsolutePath;

        /// <summary>
        /// Whether the plugin was successfully loaded
        /// </summary>
        public readonly bool Loaded;

        /// <summary>
        /// Exception reported when trying to load (or null if no exception occurred)
        /// </summary>
        public readonly Exception Exception;
    }

    /// <summary>
    /// SLED dynamic plugin service interface
    /// </summary>
    public interface ISledDynamicPluginService
    {
        /// <summary>
        /// Get all dynamic plugin information
        /// </summary>
        IEnumerable<SledDynamicPluginInfo> Plugins { get; }

        /// <summary>
        /// Get failed-to-load plugin information
        /// </summary>
        IEnumerable<SledDynamicPluginInfo> FailedPlugins { get; }

        /// <summary>
        /// Get successfully loaded plugin information
        /// </summary>
        IEnumerable<SledDynamicPluginInfo> SuccessfulPlugins { get; }
    }
}