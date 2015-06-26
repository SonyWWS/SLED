/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Document;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED document service EventArgs Class
    /// </summary>
    public class SledDocumentServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sd">ISledDocument</param>
        public SledDocumentServiceEventArgs(ISledDocument sd)
            : this(sd, null)
        {
        }

        /// <summary>
        /// Constructor for Save[ing/ed] as events
        /// </summary>
        /// <param name="sd">SledDocument</param>
        /// <param name="uri">URI to old file when Save[ing/ed] as</param>
        public SledDocumentServiceEventArgs(ISledDocument sd, Uri uri)
        {
            Document = sd;
            Uri = uri;
        }

        /// <summary>
        /// SledDocument
        /// </summary>
        public readonly ISledDocument Document;

        /// <summary>
        /// Absolute path to file (mainly used for Save as)
        /// </summary>
        public readonly Uri Uri;
    }

    /// <summary>
    /// SLED document client service interface
    /// </summary>
    public interface ISledDocumentClientService
    {
        /// <summary>
        /// Open document
        /// </summary>
        /// <param name="uri">URI for document location</param>
        /// <param name="client">ISledDocumentClient</param>
        /// <returns>IDocument of opened document</returns>
        IDocument OpenDocument(Uri uri, ISledDocumentClient client);

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        void ShowDocument(IDocument document);

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        void SaveDocument(IDocument document, Uri uri);

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        void CloseDocument(IDocument document);
    }

    /// <summary>
    /// SLED document service interface
    /// </summary>
    public interface ISledDocumentService : IControlHostClient, ICommandClient, IDocumentService, ISledDocumentClientService
    {
        /// <summary>
        /// Get collection of open documents
        /// </summary>
        ICollection<ISledDocument> OpenDocuments
        {
            get;
        }

        /// <summary>
        /// Get whether there is an active document
        /// </summary>
        bool Active
        {
            get;
        }

        /// <summary>
        /// Get the current active document
        /// </summary>
        ISledDocument ActiveDocument
        {
            get;
        }

        /// <summary>
        /// Try to open a file through the open file dialog
        /// </summary>
        void Open();

        /// <summary>
        /// Try to open a file
        /// </summary>
        /// <param name="uri">Path to file</param>
        /// <param name="sd">Handle to open file</param>
        /// <returns>True iff file could be opened</returns>
        bool Open(Uri uri, out ISledDocument sd);

        /// <summary>
        /// Check if a file is open
        /// </summary>
        /// <param name="uri">File to check if open</param>
        /// <param name="sd">Handle to open document or null if not open</param>
        /// <returns>True iff document open</returns>
        bool IsOpen(Uri uri, out ISledDocument sd);

        /// <summary>
        /// Obtain a SledDocumentClient that is used for files
        /// that have the same extension as the given URI
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>ISledDocumentClient</returns>
        ISledDocumentClient GetDocumentClient(Uri uri);

        /// <summary>
        /// Event triggered after opening an existing document or a new document
        /// </summary>
        event EventHandler<SledDocumentServiceEventArgs> Opened;

        /// <summary>
        /// Event triggered after the active document is changed
        /// </summary>
        event EventHandler ActiveDocumentChanged;

        /// <summary>
        /// Event triggered when saving a document (including Saving as)
        /// </summary>
        event EventHandler<SledDocumentServiceEventArgs> Saving;

        /// <summary>
        /// Event triggered after saving a document (includes Saved as)
        /// </summary>
        event EventHandler<SledDocumentServiceEventArgs> Saved;

        /// <summary>
        /// Event triggered when closing a document
        /// </summary>
        event EventHandler<SledDocumentServiceEventArgs> Closing;

        /// <summary>
        /// Event triggered after a document has been closed
        /// </summary>
        event EventHandler<SledDocumentServiceEventArgs> Closed;
    }
}