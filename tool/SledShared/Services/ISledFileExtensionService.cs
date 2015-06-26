/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED file extension service interface
    /// </summary>
    public interface ISledFileExtensionService
    {
        /// <summary>
        /// Add a file extension
        /// </summary>
        /// <param name="fileType">Document file type (i.e., Text, Lua, Python, etc.)</param>
        /// <param name="ext">File extension (should start with a period)</param>
        void AddExtension(string fileType, string ext);

        /// <summary>
        /// Get list of pairs of file type and extension
        /// </summary>
        Dictionary<string, List<string>> FileTypesAndExtensions
        {
            get;
        }

        /// <summary>
        /// Get list of all extensions
        /// </summary>
        IEnumerable<string> AllExtensions
        {
            get;
        }

        /// <summary>
        /// Get a string that includes all the extensions, in a format that is useful for open file or other file related dialogs
        /// </summary>
        string FilterString
        {
            get;
        }
    }
}
