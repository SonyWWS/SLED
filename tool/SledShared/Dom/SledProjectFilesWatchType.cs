/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type of a watched item
    /// </summary>
    public abstract class SledProjectFilesWatchType : SledProjectFilesBaseType
    {
        /// <summary>
        /// Gets or sets language plugin attribute
        /// </summary>
        public abstract string LanguagePluginString
        {
            get; // { return GetAttribute<string>(SledSchema.SledProjectFilesWatchType.language_pluginAttribute); }
            set; // { SetAttribute(SledSchema.SledProjectFilesWatchType.language_pluginAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the actual language plugin the watch belongs to
        /// </summary>
        public ISledLanguagePlugin LanguagePlugin
        {
            get { return m_languagePlugin ?? (m_languagePlugin = GetLanguagePlugin(LanguagePluginString)); }
            set
            {
                m_languagePlugin = value;
                LanguagePluginString = value.LanguageName;
            }
        }

        private static ISledLanguagePlugin GetLanguagePlugin(string languagePluginString)
        {
            if (string.IsNullOrEmpty(languagePluginString))
                return null;

            var languagePluginService =
                SledServiceInstance.Get<ISledLanguagePluginService>();

            foreach (var kv in languagePluginService.LanguagePlugins)
            {
                var plugin = kv.Value;
                if (plugin == null)
                    continue;

                if (string.Compare(plugin.LanguageName, languagePluginString, StringComparison.Ordinal) == 0)
                    return plugin;
            }

            return null;
        }

        private ISledLanguagePlugin m_languagePlugin;
    }
}
