/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Event args for MouseHoverOverToken event</summary>
    public class MouseHoverOverTokenEventArgs : EventArgs
    {
        /// <summary>
        /// Construct a new MouseHoverOverTokenEventArgs with the specified arguments
        /// </summary>
        /// <param name="language">language</param>
        /// <param name="token">Token type</param>        
        /// <param name="lineNumber">the line number of the token</param>
        public MouseHoverOverTokenEventArgs(Languages language,Token token, int lineNumber)
        {
            Language = language;
            Token = token;            
            LineNumber = lineNumber;            
        }

        /// <summary>
        /// Get the name of language
        /// </summary>
        public Languages Language { get; private set; }

        /// <summary>
        /// Get the token
        /// </summary>
        public Token Token { get; private set; }

        /// <summary>
        /// get line number of the token
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Get/Set tooltip text
        /// </summary>
        public string TooltipText { get; set; }
    }
}
