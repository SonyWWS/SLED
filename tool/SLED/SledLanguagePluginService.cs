/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledLanguagePluginService Class
    /// </summary>
    [Export(typeof(ISledLanguagePluginService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLanguagePluginService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledLanguagePluginService : IInitializable, ISledLanguagePluginService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            var languagePlugins =
                SledServiceInstance.GetAll<ISledLanguagePlugin>();

            m_dictPlugins.Clear();

            using (new SledOutDevice.BreakBlock())
            {
                foreach (var langPlugin in languagePlugins)
                {
                    // Add plugin to list
                    m_dictPlugins.Add(langPlugin.LanguageId, langPlugin);

                    // Report the plugin was found
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledLanguagePluginLoaded, langPlugin.LanguageName, langPlugin.LanguageDescription));
                }
            }
        }

        #endregion

        #region ISledLanguagePluginService Interface

        /// <summary>
        /// Gets plugins
        /// </summary>
        public IEnumerable<KeyValuePair<UInt16, ISledLanguagePlugin>> LanguagePlugins
        {
            get { return m_dictPlugins; }
        }

        /// <summary>
        /// Returns number of language plugins
        /// </summary>
        public int Count
        {
            get { return m_dictPlugins.Count; }
        }

        /// <summary>
        /// Try and find a plugin with a certain Id in the dictionary
        /// </summary>
        /// <param name="pluginId">Id to look for</param>
        /// <param name="plugin">Handle to plugin if found</param>
        /// <returns>True if found false if not</returns>
        public bool TryGetPlugin(UInt16 pluginId, out ISledLanguagePlugin plugin)
        {
            return m_dictPlugins.TryGetValue(pluginId, out plugin);
        }

        /// <summary>
        /// Return a reference to a plugin that should manage the file of a given extension
        /// </summary>
        /// <param name="ext">file extension (including the period)</param>
        /// <returns>Language plugin reference or null</returns>
        public ISledLanguagePlugin GetPluginForExtension(string ext)
        {
            return
                (from kv in m_dictPlugins
                 from szExt in kv.Value.LanguageExtensions
                 where string.Compare(ext, szExt, true) == 0
                 select kv.Value).FirstOrDefault();
        }

        #endregion

        private readonly Dictionary<UInt16, ISledLanguagePlugin> m_dictPlugins =
            new Dictionary<UInt16, ISledLanguagePlugin>();
    }
}
