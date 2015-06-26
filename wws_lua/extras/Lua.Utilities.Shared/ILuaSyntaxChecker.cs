/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Lua.Utilities
{
    /// <summary>
    /// Lua syntax checker
    /// </summary>
    public interface ILuaSyntaxChecker : IDisposable
    {
        /// <summary>
        /// Check a script for syntax errors
        /// </summary>
        /// <param name="scriptFile">Path to script</param>
        /// <returns>True if no errors otherwise false</returns>
        bool CheckFile(Uri scriptFile);

        /// <summary>
        /// Check a buffer for syntax errors
        /// </summary>
        /// <param name="bufferContents">Contents of buffer</param>
        /// <returns>True if no errors otherwise false</returns>
        bool CheckBuffer(string bufferContents);

        /// <summary>
        /// Lua error if the syntax check failed
        /// </summary>
        string Error { get; }
    }
}