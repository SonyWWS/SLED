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
    public enum SledLanguageParserVerbosity
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
    /// SLED language parser result
    /// </summary>
    public abstract class SledLanguageParserResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin">Language plugin</param>
        /// <param name="file">Project file</param>
        protected SledLanguageParserResult(ISledLanguagePlugin plugin, SledProjectFilesFileType file)
        {
            Plugin = plugin;
            File = file;
        }

        /// <summary>
        /// Get language plugin
        /// </summary>
        public ISledLanguagePlugin Plugin { get; private set; }

        /// <summary>
        /// Get project file
        /// </summary>
        public SledProjectFilesFileType File { get; private set; }
    }

    /// <summary>
    /// Language parser event arguments
    /// </summary>
    public class SledLanguageParserEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="filesAndResults">Mapping of all project files parsed and any results they have</param>
        /// <param name="userData">Optional user data</param>
        public SledLanguageParserEventArgs(ISledLanguagePlugin plugin, Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>> filesAndResults, object userData)
        {
            Plugin = plugin;
            FilesAndResults = filesAndResults;
            UserData = userData;
        }

        /// <summary>
        /// Get language plugin
        /// </summary>
        public ISledLanguagePlugin Plugin { get; private set; }

        /// <summary>
        /// Get mapping of all project files parsed and any results they have
        /// </summary>
        public Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>> FilesAndResults { get; private set; }

        /// <summary>
        /// Get optional user data
        /// </summary>
        public object UserData { get; private set; }
    }

    /// <summary>
    /// Multi-thread safe callback function signature to use for parsing files
    /// </summary>
    /// <param name="files">Files to parse</param>
    /// <param name="verbosity">Verbosity</param>
    /// <param name="userData">Optional user data</param>
    /// <param name="shouldCancel">To check if, in long running functions, the asynchronous activity should stop</param>
    /// <returns>Items found during parsing</returns>
    public delegate IEnumerable<SledLanguageParserResult> SledLanguageParserFilesParseDelegate(IEnumerable<SledProjectFilesFileType> files, SledLanguageParserVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel);

    /// <summary>
    /// SLED language parser service interface
    /// </summary>
    public interface ISledLanguageParserService
    {
        /// <summary>
        /// Get whether a parser is currently running
        /// </summary>
        bool Running { get; }

        /// <summary>
        /// Get or set syntax checking verbosity level
        /// </summary>
        SledLanguageParserVerbosity Verbosity { get; set; }

        /// <summary>
        /// Schedule a parsing of files
        /// </summary>
        /// <param name="files">Files to parse</param>
        /// <returns>True iff files enqueued to parse</returns>
        bool ParseFilesAsync(IEnumerable<SledProjectFilesFileType> files);

        /// <summary>
        /// Register a file parsing function with a particular language plugin and allow optional user data
        /// </summary>
        /// <param name="plugin">Language plugin</param>
        /// <param name="func">Parsing function</param>
        /// <param name="userData">Optional user data</param>
        void RegisterFilesParserFunction(ISledLanguagePlugin plugin, SledLanguageParserFilesParseDelegate func, object userData);

        /// <summary>
        /// Event triggered after files have been parsed
        /// </summary>
        event EventHandler<SledLanguageParserEventArgs> FilesParserFinished;
    }
}