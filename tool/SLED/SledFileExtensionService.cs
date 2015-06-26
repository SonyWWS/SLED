/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using Sce.Atf;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledDocumentService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledFileExtensionService))]
    [Export(typeof(SledFileExtensionService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledFileExtensionService : IInitializable, ISledFileExtensionService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            var documentClients = SledServiceInstance.GetAll<ISledDocumentClient>();

            foreach (var client in documentClients)
            {
                m_dictExtensions.Add(client.Info.FileType, client.Info.Extensions.ToList());
            }

            m_bModifiedSinceLastBuild = true;
        }

        #endregion

        #region ISledFileExtensionService Interface

        /// <summary>
        /// Add a file extension
        /// </summary>
        /// <param name="fileType">Document file type (ie. Text, Lua, Python, etc.)</param>
        /// <param name="ext">File extension (should start with a period)</param>
        public void AddExtension(string fileType, string ext)
        {
            if (string.IsNullOrEmpty(ext))
                return;

            List<string> extensions;
            if (!m_dictUserExtensions.TryGetValue(fileType, out extensions))
            {
                extensions = new List<string>();
                m_dictUserExtensions.Add(fileType, extensions);
            }

            if (extensions.Contains(ext))
                return;

            extensions.Add(ext);
            m_bModifiedSinceLastBuild = true;
        }

        /// <summary>
        /// Return list of pairs of file type and extension
        /// </summary>
        public Dictionary<string, List<string>> FileTypesAndExtensions
        {
            get
            {
                var combined = new Dictionary<string, List<string>>();

                var dictionaries = new[] { m_dictExtensions, m_dictUserExtensions };
                foreach (var dictionary in dictionaries)
                {
                    foreach (var kv in dictionary)
                    {
                        List<string> extensions;
                        if (!combined.TryGetValue(kv.Key, out extensions))
                            extensions = new List<string>();

                        extensions.AddRange(kv.Value);

                        combined.Remove(kv.Key);
                        combined.Add(kv.Key, extensions.Distinct().ToList());
                    }
                }

                return combined;
            }
        }

        /// <summary>
        /// Return list of extensions
        /// </summary>
        public IEnumerable<string> AllExtensions
        {
            get
            {
                var extensions = new List<string>();

                foreach (var kv in m_dictExtensions)
                    extensions.AddRange(kv.Value);

                foreach (var kv in m_dictUserExtensions)
                    extensions.AddRange(kv.Value);

                return extensions.Distinct();
            }
        }

        /// <summary>
        /// Return a string that includes all the extensions in a format that is useful for open file or other file related dialogs
        /// </summary>
        public string FilterString
        {
            get
            {
                if (m_bModifiedSinceLastBuild)
                    BuildFilterString();

                return m_builder.ToString();
            }
        }

        #endregion

        private void BuildFilterString()
        {
            // Clear
            m_builder.Remove(0, m_builder.Length);
            var sbAll = new StringBuilder();

            // Build up filter string
            foreach (var kv in FileTypesAndExtensions)
            {
                foreach (var ext in kv.Value)
                {
                    sbAll.Append("*" + ext + "*;");
                    m_builder.Append(kv.Key + " (*" + ext + ")|*" + ext + "|");
                }
            }

            // Build all supported files option
            sbAll.Append("*.txt");
            m_builder.Insert(0, "|");
            m_builder.Insert(0, sbAll.ToString());
            m_builder.Insert(0, ")|");
            m_builder.Insert(0, sbAll.Replace("*;", ", "));
            m_builder.Insert(0, "All Supported Files (");
            m_builder.Append("Text File (*.txt)|*.txt|All Files (*.*)|*.*");
        }

        private bool m_bModifiedSinceLastBuild = true;

        private readonly StringBuilder m_builder =
            new StringBuilder();

        private readonly Dictionary<string, List<string>> m_dictExtensions =
            new Dictionary<string, List<String>>();

        private readonly Dictionary<string, List<string>> m_dictUserExtensions =
            new Dictionary<string, List<string>>();
    }
}
