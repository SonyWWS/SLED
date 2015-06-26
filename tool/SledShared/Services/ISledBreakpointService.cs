/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// Types of SLED breakpoint changes
    /// </summary>
    public enum SledBreakpointChangeType
    {
        /// <summary>
        /// Line number change
        /// </summary>
        LineNumber,

        /// <summary>
        /// Breakpoint is now enabled (previously disabled)
        /// </summary>
        Enabled,
        
        /// <summary>
        /// Breakpoint is now disabled (previously enabled)
        /// </summary>
        Disabled,

        /// <summary>
        /// Condition change
        /// </summary>
        Condition,

        /// <summary>
        /// Condition is now enabled (previously disabled)
        /// </summary>
        ConditionEnabled,
        
        /// <summary>
        /// Condition is now disabled (previously enabled)
        /// </summary>
        ConditionDisabled,

        /// <summary>
        /// Condition result will now be evaluated against the value "true" (previously "false")
        /// </summary>
        ConditionResultTrue,

        /// <summary>
        /// Condition result will now be evaluated against the value "false" (previously "true")
        /// </summary>
        ConditionResultFalse,

        /// <summary>
        /// Use the function's environment table as the function environment when checking conditional breakpoints
        /// </summary>
        UseFunctionEnvironmentTrue,

        /// <summary>
        /// Use the global table as the function environment when checking conditional breakpoints
        /// </summary>
        UseFunctionEnvironmentFalse,
    }

    /// <summary>
    /// SLED breakpoint service breakpoint EventArgs class
    /// </summary>
    public class SledBreakpointServiceBreakpointEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bp">Breakpoint type</param>
        public SledBreakpointServiceBreakpointEventArgs(SledProjectFilesBreakpointType bp)
        {
            Breakpoint = bp;
        }

        /// <summary>
        /// Get breakpoint type
        /// </summary>
        public SledProjectFilesBreakpointType Breakpoint { get; private set; }
    }

    /// <summary>
    /// SLED breakpoint changing event arguments class
    /// <remarks>This class has various constructors for different types of breakpoint changes</remarks>
    /// </summary>
    public class SledBreakpointServiceBreakpointChangingEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeType">Breakpoint change type</param>
        /// <param name="bp">Breakpoint type</param>
        public SledBreakpointServiceBreakpointChangingEventArgs(SledBreakpointChangeType changeType, SledProjectFilesBreakpointType bp)
            : this(changeType, bp, -1, -1, null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeType">Breakpoint change type</param>
        /// <param name="bp">Breakpoint type</param>
        /// <param name="iOldLine">Old breakpoint line number</param>
        /// <param name="iNewLine">New breakpoint line number</param>
        public SledBreakpointServiceBreakpointChangingEventArgs(SledBreakpointChangeType changeType, SledProjectFilesBreakpointType bp, int iOldLine, int iNewLine)
            : this(changeType, bp, iOldLine, iNewLine, null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeType">Breakpoint change type</param>
        /// <param name="bp">Breakpoint type</param>
        /// <param name="oldCondition">Old breakpoint condition</param>
        /// <param name="newCondition">New breakpoint condition</param>
        public SledBreakpointServiceBreakpointChangingEventArgs(SledBreakpointChangeType changeType, SledProjectFilesBreakpointType bp, string oldCondition, string newCondition)
            : this(changeType, bp, -1, -1, oldCondition, newCondition)
        {
        }

        private SledBreakpointServiceBreakpointChangingEventArgs(SledBreakpointChangeType changeType, SledProjectFilesBreakpointType bp, int iOldLine, int iNewLine, string oldCondition, string newCondition)
        {
            ChangeType = changeType;
            Breakpoint = bp;
            OldLine = iOldLine;
            NewLine = iNewLine;
            OldCondition = oldCondition;
            NewCondition = newCondition;
        }

        /// <summary>
        /// Get breakpoint change type
        /// </summary>
        public SledBreakpointChangeType ChangeType { get; private set; }

        /// <summary>
        /// Gets the breakpoint being changed
        /// </summary>
        public SledProjectFilesBreakpointType Breakpoint { get; private set; }

        /// <summary>
        /// Get old breakpoint line number
        /// </summary>
        public int OldLine { get; private set; }

        /// <summary>
        /// Gets new breakpoint line number
        /// </summary>
        public int NewLine { get; private set; }

        /// <summary>
        /// Get old breakpoint condition
        /// </summary>
        public string OldCondition { get; private set; }

        /// <summary>
        /// Get new breakpoint condition
        /// </summary>
        public string NewCondition { get; private set; }
    }

    /// <summary>
    /// SLED breakpoint service interface
    /// </summary>
    public interface ISledBreakpointService
    {
        /// <summary>
        /// Event triggered when a breakpoint has been added
        /// </summary>
        event EventHandler<SledBreakpointServiceBreakpointEventArgs> Added;

        /// <summary>
        /// Event triggered when a breakpoint has been silently added
        /// </summary>
        event EventHandler<SledBreakpointServiceBreakpointEventArgs> SilentAdded;

        /// <summary>
        /// Event triggered when a breakpoint is being removed
        /// </summary>
        event EventHandler<SledBreakpointServiceBreakpointEventArgs> Removing;

        /// <summary>
        /// Event triggered when a breakpoint is about to be changed
        /// </summary>
        event EventHandler<SledBreakpointServiceBreakpointChangingEventArgs> Changing;

        /// <summary>
        /// Event triggered after a breakpoint has changed
        /// </summary>
        event EventHandler<SledBreakpointServiceBreakpointChangingEventArgs> Changed;

        /// <summary>
        /// Add a breakpoint to a file
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        void AddBreakpoint(SledProjectFilesFileType file, int lineNumber);

        /// <summary>
        /// Add a breakpoint to a file, supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        /// <param name="condition">Breakpoint condition</param>
        /// <param name="bConditionResult">Whether condition evaluates to true or false</param>
        void AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool bConditionResult);

        /// <summary>
        /// Add a breakpoint to a file supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Breakpoint line number</param>
        /// <param name="condition">Breakpoint condition</param>
        /// <param name="bConditionResult">Whether breakpoint condition evaluates to true or false</param>
        /// <param name="bUseFunctionEnvironment">Whether to use the current function's environment or _G when checking the breakpoint condition (if any)</param>
        void AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool bConditionResult, bool bUseFunctionEnvironment);

        /// <summary>
        /// Add a breakpoint to a file supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        /// <param name="condition">Breakpoint condition</param>
        /// <param name="conditionResult">Whether breakpoint condition evaluates to true or false</param>
        /// <param name="conditionEnabled">Whether the breakpoint condition is enabled or not</param>
        /// <param name="useFunctionEnvironment">Whether to use the current function's environment or _G when checking the breakpoint condition (if any)</param>
        /// <param name="breakpoint">The breakpoint if it was added, otherwise null</param>
        /// <returns>Whether the breakpoint was added or not</returns> 
        bool AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool conditionResult, bool conditionEnabled, bool useFunctionEnvironment, out SledProjectFilesBreakpointType breakpoint);

        /// <summary>
        /// Remove a breakpoint from a file
        /// </summary>
        /// <param name="file">File to remove breakpoint from</param>
        /// <param name="lineNumber">Line number breakpoint is on</param>
        void RemoveBreakpoint(SledProjectFilesFileType file, int lineNumber);
    }
}
