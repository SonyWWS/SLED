/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Shared.Document
{
    /// <summary>
    /// ISledDocument client interface
    /// </summary>
    public interface ISledDocumentClient : IDocumentClient
    {
        /// <summary>
        /// Get syntax highlighting stream (in a format the ActiproSoftware SyntaxEditor can use)</summary>
        SledDocumentSyntaxHighlighter SyntaxHighlighter { get; }

        /// <summary>
        /// Get embedded types to add to any document created with this IDocumentClient</summary>
        IEnumerable<SledDocumentEmbeddedTypeInfo> EmbeddedTypes { get; }
    }

    /// <summary>
    /// SledDocument client class</summary>
    public sealed class SledDocumentClient : ISledDocumentClient
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="fileType">File type (a string like &quot;Text&quot;, &quot;Lua&quot;, etc)</param>
        /// <param name="extension">Extension</param>
        /// <param name="imageName">Name of image file for editor's Open icon</param>
        /// <param name="bOpenEverything">Whether to open all documents</param>
        /// <param name="syntaxHighlighter">SledDocumentSyntaxHighlighter</param>
        /// <param name="embeddedTypes">SledDocumentEmbeddedTypeInfo array</param>
        public SledDocumentClient(string fileType, string extension, string imageName, bool bOpenEverything, SledDocumentSyntaxHighlighter syntaxHighlighter, params SledDocumentEmbeddedTypeInfo[] embeddedTypes)
        {
            Info =
                new DocumentClientInfo(fileType, extension, null, null)
                {
                    NewIconName = imageName,
                    OpenIconName = imageName
                };

            if (bOpenEverything)
            {
                if (s_catchAllClient != null)
                    throw new InvalidOperationException("catch-all ISledDocumentClient already set");

                s_catchAllClient = this;
            }

            SyntaxHighlighter = syntaxHighlighter;

            EmbeddedTypes =
                new List<SledDocumentEmbeddedTypeInfo>(
                    embeddedTypes);
        }

        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="fileType">File type (a string like &quot;Text&quot;, &quot;Lua&quot;, etc)</param>
        /// <param name="extension">Extension</param>
        /// <param name="imageName">Name of image file for editor's Open icon</param>
        /// <param name="syntaxHighlighter">SledDocumentSyntaxHighlighter</param>
        /// <param name="embeddedTypes">SledDocumentEmbeddedTypeInfo array</param>
        public SledDocumentClient(string fileType, string extension, string imageName, SledDocumentSyntaxHighlighter syntaxHighlighter, params SledDocumentEmbeddedTypeInfo[] embeddedTypes)
            : this(fileType, extension, imageName, false, syntaxHighlighter, embeddedTypes)
        {
        }

        #region IDocumentClient Interface

        /// <summary>
        /// Get DocumentClientInfo</summary>
        public DocumentClientInfo Info { get; private set; }

        /// <summary>
        /// Return true if document can be opened
        /// </summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff document can be opened</returns>
        public bool CanOpen(Uri uri)
        {
            if (s_catchAllClient == this)
                return !IsThereABetterDocumentClient(uri);

            return Info.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Open a document</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>IDocument of opened document</returns>
        public IDocument Open(Uri uri)
        {
            return m_documentClientService.Get.OpenDocument(uri, this);
        }

        /// <summary>
        /// Show a document</summary>
        /// <param name="document">IDocument of opened document</param>
        public void Show(IDocument document)
        {
            m_documentClientService.Get.ShowDocument(document);
        }

        /// <summary>
        /// Save a document</summary>
        /// <param name="document">IDocument of opened document</param>
        /// <param name="uri">URI to save document to</param>
        public void Save(IDocument document, Uri uri)
        {
            m_documentClientService.Get.SaveDocument(document, uri);
        }

        /// <summary>
        /// Close document</summary>
        /// <param name="document">IDocument of opened document</param>
        public void Close(IDocument document)
        {
            m_documentClientService.Get.CloseDocument(document);
        }

        #endregion

        #region ISledDocumentClient Interface

        /// <summary>
        /// Get syntax highlighting stream (in a format the ActiproSoftware SyntaxEditor can use)</summary>
        public SledDocumentSyntaxHighlighter SyntaxHighlighter { get; private set; }

        /// <summary>
        /// Get embedded types to add to any document created with this IDocumentClient</summary>
        public IEnumerable<SledDocumentEmbeddedTypeInfo> EmbeddedTypes { get; private set; }

        #endregion

        private static bool IsThereABetterDocumentClient(Uri uri)
        {
            var clients = SledServiceInstance.GetAll<ISledDocumentClient>();

            foreach (var client in clients)
            {
                if (client == s_catchAllClient)
                    continue;

                if (client.CanOpen(uri))
                    return true;
            }

            return false;
        }

        private static ISledDocumentClient s_catchAllClient;

        private readonly SledServiceReference<ISledDocumentClientService> m_documentClientService =
            new SledServiceReference<ISledDocumentClientService>();
    }
}