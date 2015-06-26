/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// Base interface for adding a language to SLED</summary>
    public interface ISledLanguagePlugin
    {
        /// <summary>
        /// Get name of the language
        /// </summary>
        string LanguageName
        {
            get;
        }

        /// <summary>
        /// Get file extensions this language supports
        /// </summary>
        string[] LanguageExtensions
        {
            get;
        }

        /// <summary>
        /// Get description of the language
        /// </summary>
        string LanguageDescription
        {
            get;
        }

        /// <summary>
        /// Get language ID for network messages
        /// </summary>
        UInt16 LanguageId
        {
            get;
        }
    }
}
