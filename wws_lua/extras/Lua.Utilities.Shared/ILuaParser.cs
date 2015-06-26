/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Lua.Utilities
{
    /// <summary>
    /// Lua variable entry
    /// </summary>
    public class LuaVariableEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="line">Line the variable is on</param>
        public LuaVariableEntry(string name, int line)
        {
            Name = name;
            Line = line;
            Occurrence = 1;
        }

        /// <summary>
        /// Variable name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Line the variable is on
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Occurrence of the variable on the line
        /// </summary>
        public int Occurrence { get; set; }
    }

    /// <summary>
    /// Lua function entry
    /// </summary>
    public class LuaFunctionEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="lineDefined">Line the function starts on</param>
        /// <param name="lastLineDefined">Last line the function ends on</param>
        public LuaFunctionEntry(string name, int lineDefined, int lastLineDefined)
        {
            Name = name;
            LineDefined = lineDefined;
            LastLineDefined = lastLineDefined;
        }

        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Line the function starts on
        /// </summary>
        public int LineDefined { get; private set; }

        /// <summary>
        /// Last line the function ends on
        /// </summary>
        public int LastLineDefined { get; private set; }
    }

    /// <summary>
    /// Delegate for variable parsing
    /// </summary>
    /// <param name="name">Name of the Lua variable</param>
    /// <param name="line">Line number the variable is on</param>
    public delegate void ParserVariableDelegate(string name, int line);

    /// <summary>
    /// Delegate for function parsing
    /// </summary>
    /// <param name="function">Function name</param>
    /// <param name="lineDefined">Line number the function is defined on</param>
    /// <param name="lastLineDefined">Line number of the last line of the function</param>
    public delegate void ParserFunctionDelegate(string function, int lineDefined, int lastLineDefined);

    /// <summary>
    /// Delegate for breakpoint parsing
    /// </summary>
    /// <param name="line">Valid line for a breakpoint</param>
    public delegate void ParserBreakpointDelegate(int line);

    /// <summary>
    /// Delegate for logging stuff during parsing
    /// </summary>
    /// <param name="message">Log message</param>
    public delegate void ParserLogDelegate(string message);

    /// <summary>
    /// Lua parser interface
    /// </summary>
    public interface ILuaParser : IDisposable
    {
        /// <summary>
        /// Parse a script file
        /// </summary>
        /// <param name="scriptFile">Path to script file</param>
        /// <returns>True if successful otherwise false if error encountered</returns>
        bool Parse(Uri scriptFile);

        /// <summary>
        /// Lua error if parsing failed
        /// </summary>
        string Error { get; }

        /// <summary>
        /// Gets/sets a log handler
        /// </summary>
        ParserLogDelegate LogHandler { get; set; }

        /// <summary>
        /// All of the parsed globals from the script
        /// </summary>
        IEnumerable<LuaVariableEntry> Globals { get; }

        /// <summary>
        /// All of the parsed locals from the script
        /// </summary>
        IEnumerable<LuaVariableEntry> Locals { get; }

        /// <summary>
        /// All of the parsed upvalues from the script
        /// </summary>
        IEnumerable<LuaVariableEntry> Upvalues { get; }

        /// <summary>
        /// All of the parsed functions from the script
        /// </summary>
        IEnumerable<LuaFunctionEntry> Functions { get; }

        /// <summary>
        /// All of the valid line numbers for breakpoints from the script
        /// </summary>
        IEnumerable<int> ValidBreakpointLines { get; }
    }
}