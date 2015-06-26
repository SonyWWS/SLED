/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Used by the ISyntaxEditorControl ShowContextMenu event
    /// </summary>
    public class ShowContextMenuEventArg : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="syntaxEditorControl"></param>
        /// <param name="mouseLocation"></param>
        /// <param name="menuLocation"></param>
        /// <param name="lineNumber"></param>
        public ShowContextMenuEventArg(ISyntaxEditorControl syntaxEditorControl, Point mouseLocation, Point menuLocation, int lineNumber)
        {
            SyntaxEditorControl = syntaxEditorControl;
            LineNumber = lineNumber;
            MenuLocation = menuLocation;
            MouseLocation = mouseLocation;
        }

        /// <summary>
        /// Gets the syntax editor control
        /// </summary>
        public ISyntaxEditorControl SyntaxEditorControl { get; private set; }

        /// <summary>
        /// Gets mouse location
        /// </summary>
        public Point MouseLocation { get; private set; }

        /// <summary>
        /// Gets the location where the menu should be displayed.
        /// </summary>
        public Point MenuLocation { get; private set; }

        /// <summary>
        /// Gets the line number when applicable.
        /// The value -1 means that this property is not applicable 
        /// for the current region.
        /// </summary>
        public int LineNumber { get; private set; }
    }
}
