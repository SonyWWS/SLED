/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Used as a event args for EditorTextChanged event.
    /// </summary>
    public class EditorTextChangedEventArgs : EventArgs
    {
        /// <summary>
        /// create new instance
        /// </summary>
        /// <param name="text">the text that been changed</param>
        /// <param name="startOffset">the starting offset of the changed text</param>
        /// <param name="endOffset">the end offset of the changed text.</param>
        /// <param name="startLineNumber">document line index at which the modification occurred. </param>
        /// <param name="endLineNumber">document line index at which the modification occurred. </param>        
        internal EditorTextChangedEventArgs(string text, int startOffset, int endOffset, int startLineNumber, int endLineNumber)
        {
            Changes = text;
            StartOffset = startOffset;
            EndOffset = endOffset;
            StartLineNumber = startLineNumber;
            EndLineNumber = endLineNumber;
        }

        public string Changes { get; private set; }

        public int StartOffset { get; private set; }

        public int EndOffset { get; private set; }

        public int StartLineNumber { get; private set; }

        public int EndLineNumber { get; private set; }

        public override string ToString()
        {
            return
                string.Format(
                    "Change:{0}\r\nstartOffset:{1} EndOffset:{2} " + 
                    "StartLine:{3} EndLine:{4}",
                    Changes,
                    StartOffset,
                    EndOffset,
                    StartLineNumber,
                    EndLineNumber);
            
        }
    }

}
