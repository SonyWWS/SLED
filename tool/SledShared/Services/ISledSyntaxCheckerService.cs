/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// Verbosity levels for syntax checking enumeration
    /// </summary>
    public enum SledSyntaxCheckerVerbosity
    {
        /// <summary>
        /// No information
        /// </summary>
        None = 0,

        /// <summary>
        /// Overall information
        /// </summary>
        Overall = 5,

        /// <summary>
        /// Finely detailed information
        /// </summary>
        Detailed = 10,
    }

    /// <summary>
    /// Syntax checker entry class
    /// </summary>
    public class SledSyntaxCheckerEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="language">Language error is in</param>
        /// <param name="file">File syntax error is in</param>
        /// <param name="line">Line number in file that syntax error is on</param>
        /// <param name="error">Syntax error description</param>
        public SledSyntaxCheckerEntry(ISledLanguagePlugin language, SledProjectFilesFileType file, int line, string error)
        {
            Language = language;
            File = file;
            Line = line;
            Error = error;
        }

        /// <summary>
        /// Get language error is in
        /// </summary>
        public ISledLanguagePlugin Language { get; private set; }

        /// <summary>
        /// Get file syntax error is in
        /// </summary>
        public SledProjectFilesFileType File { get; private set; }

        /// <summary>
        /// Get line number in file that syntax error is on
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Get syntax error description
        /// </summary>
        public string Error { get; private set; }
    }

    /// <summary>
    /// SLED syntax check files EventArgs class
    /// </summary>
    public class SledSyntaxCheckFilesEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin">Language plugin</param>
        /// <param name="filesAndErrors">Dictionary mapping all project files scanned to any errors they have</param>
        /// <param name="userData">Optional user data</param>
        public SledSyntaxCheckFilesEventArgs(ISledLanguagePlugin plugin, Dictionary<SledProjectFilesFileType, List<SledSyntaxCheckerEntry>> filesAndErrors, object userData)
        {
            Plugin = plugin;
            FilesAndErrors = filesAndErrors;
            UserData = userData;
        }

        /// <summary>
        /// Get language plugin
        /// </summary>
        public ISledLanguagePlugin Plugin { get; private set; }

        /// <summary>
        /// Get dictionary mapping all project files scanned to any errors they have
        /// </summary>
        /// <remarks>Some files might not have any errors</remarks>
        public Dictionary<SledProjectFilesFileType, List<SledSyntaxCheckerEntry>> FilesAndErrors { get; private set; }

        /// <summary>
        /// Get optional user data
        /// </summary>
        public object UserData { get; private set; }
    }

    /// <summary>
    /// Callback function signature to use for syntax checking a string
    /// </summary>
    /// <param name="value">String ot syntax check</param> 
    /// <param name="verbosity">Verbosity</param>
    /// <param name="userData">Option useradata</param>
    /// <returns>Syntax errors found in string</returns>
    public delegate IEnumerable<SledSyntaxCheckerEntry> SledSyntaxCheckerStringCheckDelegate(string value, SledSyntaxCheckerVerbosity verbosity, object userData);

    /// <summary>
    /// Multi-thread safe callback function signature to use for syntax checking files
    /// </summary>
    /// <param name="files">Files to syntax check</param>
    /// <param name="verbosity">Verbosity</param>
    /// <param name="userData">Optional userdata</param>
    /// <param name="shouldCancel">To check if, in long running functions, the asynchronous activity should stop</param>
    /// <returns>Syntax errors found in files</returns>
    public delegate IEnumerable<SledSyntaxCheckerEntry> SledSyntaxCheckerFilesCheckDelegate(IEnumerable<SledProjectFilesFileType> files, SledSyntaxCheckerVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel);

    /// <summary>
    /// SLED syntax checker service interface
    /// </summary>
    public interface ISledSyntaxCheckerService
    {
        /// <summary>
        /// Get whether a syntax check is currently running
        /// </summary>
        bool Running { get; }

        /// <summary>
        /// Get whether the syntax checker is enabled or disabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Get or set syntax checking verbosity level
        /// </summary>
        SledSyntaxCheckerVerbosity Verbosity { get; set; }

        /// <summary>
        /// Syntax check a string
        /// </summary>
        /// <param name="plugin">Language plugin to obtain user data and syntax checking from</param>
        /// <param name="value">String to check</param>
        /// <returns>Syntax errors found in string</returns>
        IEnumerable<SledSyntaxCheckerEntry> CheckString(ISledLanguagePlugin plugin, string value);

        /// <summary>
        /// Schedule syntax checking of files
        /// </summary>
        /// <param name="files">Files to syntax check</param>
        /// <returns>True iff files enqueued</returns>
        bool CheckFilesAsync(IEnumerable<SledProjectFilesFileType> files);

        /// <summary>
        /// Register a file syntax checking function with a particular language plugin with optional user data
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="func">Multi-thread safe syntax checking function</param>
        /// <param name="userData">Optional user data</param>
        void RegisterFilesCheckFunction(ISledLanguagePlugin plugin, SledSyntaxCheckerFilesCheckDelegate func, object userData);

        /// <summary>
        /// Register a string syntax checking function with a particular language plugin and optional user data
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="func">Syntax checking function</param>
        /// <param name="userData">Optional user data</param>
        void RegisterStringCheckFunction(ISledLanguagePlugin plugin, SledSyntaxCheckerStringCheckDelegate func, object userData);

        /// <summary>
        /// Event fired when a file syntax check has completed
        /// </summary>
        event EventHandler<SledSyntaxCheckFilesEventArgs> FilesCheckFinished;
    }
}
