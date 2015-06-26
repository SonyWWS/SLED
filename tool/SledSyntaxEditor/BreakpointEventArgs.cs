/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Break point event argument for Syntax editor control
    /// </summary>
    public class BreakpointEventArgs : EventArgs
    {
        /// <summary>
        /// Construct new BreakpointEventArgs with specified values.
        /// </summary>
        /// <param name="isSet">true if the breakpoint set, false otherwise</param>
        /// <param name="lineNumber">the line number of the breakpoint</param>
        /// <param name="lineText">the line text of the breakpoint</param>
        public BreakpointEventArgs(bool isSet, int lineNumber, string lineText)
        {
            IsSet = isSet;
            LineNumber = lineNumber;
            LineText = lineText;
        }

        /// <summary>
        /// get is break point is set
        /// </summary>
        public bool IsSet { get; private set; }

        /// <summary>
        /// get the line number for the breakpoint
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// get the line text for the breakpoint
        /// </summary>
        public string LineText { get; private set; }

        /// <summary>
        /// Cancel break point operation
        /// </summary>
        public bool Cancel { get; set; }
    }
}