/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Lua.Utilities
{
    /// <summary>
    /// Lua compiler configuration
    /// </summary>
    public class LuaCompilerConfig
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endianness">Big or little endian</param>
        /// <param name="sizeofInt">Size of int in bytes</param>
        /// <param name="sizeofSizeT">Size of size_t in bytes</param>
        /// <param name="sizeofLuaNumber">Size of lua_Number in bytes</param>
        /// <param name="stripDebugInfo">Whether to strip debug info or not</param>
	    public LuaCompilerConfig(Endian endianness, int sizeofInt, int sizeofSizeT, int sizeofLuaNumber, bool stripDebugInfo)
	    {
            Endianness = endianness;
            SizeOfInt = sizeofInt;
            SizeOfSizeT = sizeofSizeT;
            SizeOfLuaNumber = sizeofLuaNumber;
            StripDebugInfo = stripDebugInfo;
	    }

        /// <summary>
        /// Big or little endian
        /// </summary>
        public Endian Endianness { get; private set; }

        /// <summary>
        /// Size of int in bytes
        /// </summary>
        public int SizeOfInt { get; private set; }

        /// <summary>
        /// Size of size_t in bytes
        /// </summary>
        public int SizeOfSizeT { get; private set; }

        /// <summary>
        /// Size of lua_Number in bytes
        /// </summary>
        public int SizeOfLuaNumber { get; private set; }

        /// <summary>
        /// Whether to strip debug info or not
        /// </summary>
        public bool StripDebugInfo { get; private set; }

        /// <summary>
        /// Endian enumeration
        /// </summary>
        public enum Endian
        {
            /// <summary>
            /// Big Endian
            /// </summary>
            Big = 0,

            /// <summary>
            /// Little Endian
            /// </summary>
            Little = 1,
        }
    }

    /// <summary>
    /// Lua compiler interface
    /// </summary>
    public interface ILuaCompiler : IDisposable
    {
        /// <summary>
        /// Compile a Lua script
        /// </summary>
        /// <param name="inputScriptFile">Script to compile</param>
        /// <param name="compiledScriptFile">Output file</param>
        /// <param name="config">Configuration parameters</param>
        /// <returns>True if compilation successful otherwise false</returns>
        bool Compile(Uri inputScriptFile, Uri compiledScriptFile, LuaCompilerConfig config);

        /// <summary>
        /// Lua error if compilation failed
        /// </summary>
        string Error { get; }
    }
}