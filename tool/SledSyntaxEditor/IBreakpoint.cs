/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;

namespace Sce.Sled.SyntaxEditor
{
    public interface IBreakpoint
    {
        /// <summary>
        /// get/set whether to draw a marker on this breakpoint,
        /// this marker is used to identify conditional breakpoint from regular breakpoint.
        /// </summary>
        bool Marker
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables breakpoint </summary>
        bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set forground color </summary>
        Color ForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set background color.</summary>
        Color BackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Get starting offset of the text range for this instance.</summary>
        int StartOffset
        {
            get;
        }

        /// <summary>
        /// Get end offset of the text range for this instace.</summary>
        int EndOffset
        {
            get;
        }

        /// <summary>
        /// Get the line number for this breakpoint.</summary>
        int LineNumber
        {
            get;
        }

        /// <summary>
        /// Get the line text for this breakpoint.</summary>
        /// <remarks>Will be string.Empty if the file isn't open in the editor.</remarks>
        string LineText
        {
            get;
        }
    }

    /// <summary>
    /// IBreakpointEventArgs Class
    /// </summary>
    public class IBreakpointEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bp"></param>
        public IBreakpointEventArgs(IBreakpoint bp)
        {
            Breakpoint = bp;
        }

        /// <summary>
        /// Breakpoint
        /// </summary>
        public IBreakpoint Breakpoint { get; private set; }
    }
}
