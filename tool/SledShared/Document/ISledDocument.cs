/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled.Shared.Document
{
    /// <summary>
    /// Region in a SledDocument
    /// </summary>
    public enum SledDocumentRegions
    {
        /// <summary>
        /// The breakpoint region
        /// </summary>
        Breakpoint = 1,

        /// <summary>
        /// The line number region
        /// </summary>
        LineNumber = 2,

        /// <summary>
        /// The text area region
        /// </summary>
        TextArea = 3,

        /// <summary>
        /// All other regions
        /// </summary>
        EverythingElse = 18,
    }

    /// <summary>
    /// Control positioning enumeration</summary>
    public enum SledDocumentEmbeddedTypePosition
    {
        /// <summary>
        /// Embed control to the left of document
        /// </summary>
        Left,

        /// <summary>
        /// Embed control above the document
        /// </summary>
        Top,

        /// <summary>
        /// Embed control to the right of document
        /// </summary>
        Right,

        /// <summary>
        /// Embed control below the document
        /// </summary>
        Bottom,
    }

    /// <summary>
    /// Class for embedding types in document</summary>
    public class SledDocumentEmbeddedTypeInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type to create and embed in the document</param>
        /// <param name="position">Place to put the embedded type</param>
        public SledDocumentEmbeddedTypeInfo(Type type, SledDocumentEmbeddedTypePosition position)
        {
            Type = type;
            Position = position;
        }

        /// <summary>
        /// Gets type to create and embed in the document
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets place to put the embedded type
        /// </summary>
        public SledDocumentEmbeddedTypePosition Position { get; private set; }
    }

    /// <summary>
    /// Arguments for SLED document context</summary>
    public class SledDocumentContextMenuArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sd">Document</param>
        /// <param name="region">Region</param>
        /// <param name="lineNumber">Line number</param>
        public SledDocumentContextMenuArgs(ISledDocument sd, SledDocumentRegions region, int lineNumber)
        {
            m_sd = sd;
            m_region = region;
            m_lineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the document
        /// </summary>
        public ISledDocument Document
        {
            get { return m_sd; }
        }

        /// <summary>
        /// Gets the region
        /// </summary>
        public SledDocumentRegions Region
        {
            get { return m_region; }
        }

        /// <summary>
        /// Gets the line number
        /// </summary>
        public int LineNumber
        {
            get { return m_lineNumber; }
        }

        private readonly ISledDocument m_sd;
        private readonly SledDocumentRegions m_region;
        private readonly int m_lineNumber;
    }

    /// <summary>
    /// Arguments for hovering over SLED document events</summary>
    public class SledDocumentHoverOverTokenArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sd">SledDocument</param>
        /// <param name="e">Event arguments</param>
        public SledDocumentHoverOverTokenArgs(ISledDocument sd, MouseHoverOverTokenEventArgs e)
        {
            m_sd = sd;
            m_e = e;
        }

        /// <summary>
        /// Gets the document
        /// </summary>
        public ISledDocument Document
        {
            get { return m_sd; }
        }

        /// <summary>
        /// Gets the event arguments
        /// </summary>
        public MouseHoverOverTokenEventArgs Args
        {
            get { return m_e; }
        }

        private readonly ISledDocument m_sd;
        private readonly MouseHoverOverTokenEventArgs m_e;
    }

    /// <summary>
    /// SledDocument line count changing event arguments</summary>
    public class SledDocumentLineCountChangedArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iOldLineCount">Old number of lines</param>
        /// <param name="iNewLineCount">New number of lines</param>
        public SledDocumentLineCountChangedArgs(int iOldLineCount, int iNewLineCount)
        {
            m_iOldLineCount = iOldLineCount;
            m_iNewLineCount = iNewLineCount;
        }

        /// <summary>
        /// Gets the old line count
        /// </summary>
        public int OldLineCount
        {
            get { return m_iOldLineCount; }
        }

        /// <summary>
        /// Gets the new line count
        /// </summary>
        public int NewLineCount
        {
            get { return m_iNewLineCount; }
        }

        private readonly int m_iOldLineCount;
        private readonly int m_iNewLineCount;
    }

    /// <summary>
    /// Interface for SLED documents</summary>
    public interface ISledDocument : IDocument, IInstancingContext, IDisposable
    {
        /// <summary>
        /// Gets the SyntaxEditor for this SledDocument
        /// </summary>
        ISyntaxEditorControl Editor
        {
            get;
        }

        /// <summary>
        /// Gets or sets the project file that corresponds to SledDocument; may be null</summary>
        SledProjectFilesFileType SledProjectFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the language plugin that is responsible for this document</summary>
        ISledLanguagePlugin LanguagePlugin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the SyntaxEditorControl</summary>
        Control Control
        {
            get;
        }

        /// <summary>
        /// Gets ControlInfo</summary>
        ControlInfo ControlInfo
        {
            get;
        }

        /// <summary>
        /// Gets whether document has selected text</summary>
        bool HasSelection
        {
            get;
        }

        /// <summary>
        /// Gets selected text in document or string.Empty if no selection</summary>
        string Selection
        {
            get;
        }

        /// <summary>
        /// Read document contents from disk</summary>
        void Read();

        /// <summary>
        /// Write document contents to disk</summary>
        void Write();

        /// <summary>
        /// Returns true if the line number is a valid line number in the document</summary>
        /// <param name="lineNumber">Line number in the document</param>
        /// <returns>True iff the line number is a valid line number in the document</returns>
        bool IsValidLine(int lineNumber);

        /// <summary>
        /// Returns whether or not a breakpoint is set on the line</summary>
        /// <param name="lineNumber">Line number in the document</param>
        /// <returns>True iff a breakpoint is set on the line</returns>
        bool IsBreakpointSet(int lineNumber);

        /// <summary>
        /// Returns whether or not a breakpoint is enabled on the line</summary>
        /// <param name="lineNumber">Line number in the document</param>
        /// <returns>True iff a breakpoint is enabled on the line</returns>
        bool IsBreakpointEnabled(int lineNumber);

        /// <summary>
        /// Event for line count in a file changing</summary>
        event EventHandler<SledDocumentLineCountChangedArgs> DocumentLineCountChanged;
    }

    /// <summary>
    /// Interface for types embedded in SledDocument
    /// <remarks>Types getting embedded in a SledDocument 
    /// should use this interface so that they can know 
    /// which document they are embedded in.
    /// Embedded types should implement this
    /// interface and be castable to Control.</remarks>
    /// </summary>
    public interface ISledDocumentEmbeddedType
    {
        /// <summary>
        /// Called after creation but before being shown
        /// </summary>
        /// <param name="sd">SledDocument the embedded type is in</param>
        void Initialize(ISledDocument sd);

        /// <summary>
        /// Called when the SledDocument is shown
        /// </summary>
        void Shown();

        /// <summary>
        /// Called when the SledDocument is closing
        /// </summary>
        void Closing();
    }

    /// <summary>
    /// Interface for comparing documents</summary>
    // ReSharper disable InconsistentNaming
    public class ISledDocumentComparer : IEqualityComparer<ISledDocument>
    // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// Determine if two ISledDocuments are equal</summary>
        /// <param name="sd1">ISledDocument 1</param>
        /// <param name="sd2">ISledDocument 2</param>
        /// <returns>True iff the two ISledDocuments are equal</returns>
        public bool Equals(ISledDocument sd1, ISledDocument sd2)
        {
            return string.Compare(sd1.Uri.LocalPath, sd2.Uri.LocalPath, StringComparison.Ordinal) == 0;
        }

        /// <summary>
        /// Computes a hash code for an ISledDocument</summary>
        /// <param name="sd">ISledDocument</param>
        /// <returns>Hash code of the ISledDocument's PathName property</returns>
        public int GetHashCode(ISledDocument sd)
        {
            return sd.Uri.GetHashCode();
        }
    }
}
