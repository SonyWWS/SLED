/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED goto service interface
    /// </summary>
    public interface ISledGotoService
    {
        /// <summary>
        /// Go to a specific line in a file
        /// </summary>
        /// <param name="sd">Document</param>
        /// <param name="iLine">Line in document</param>
        /// <param name="bUseCsi">Whether to use a "current statement indicator" or not</param>
        void GotoLine(ISledDocument sd, int iLine, bool bUseCsi);

        /// <summary>
        /// Go to a specific word on a line in a file
        /// </summary>
        /// <param name="sd">Document</param>
        /// <param name="szWord">word to find</param>
        /// <param name="iLine">Line in document</param>
        /// <param name="iOccurence">If the word occurs multiple times on a line, this represents which occurrence</param>
        /// <param name="bUseCsi">Whether to use a "current statement indicator" or not</param>
        void GotoLineWord(ISledDocument sd, string szWord, int iLine, int iOccurence, bool bUseCsi);

        /// <summary>
        /// Go to a specific line in a file
        /// </summary>
        /// <param name="szFile">Absolute path to file</param>
        /// <param name="iLine">Line in file</param>
        /// <param name="bUseCsi">Whether to use a "current statement indicator" or not</param>
        void GotoLine(string szFile, int iLine, bool bUseCsi);

        /// <summary>
        /// Go to a specific word on a line in a file
        /// </summary>
        /// <param name="szFile">Absolute path to file</param>
        /// <param name="szWord">Word to find</param>
        /// <param name="iLine">Line in file</param>
        /// <param name="iOccurence">If the word occurs multiple times on a line, this represents which occurrence</param>
        /// <param name="bUseCsi">Whether to use a "current statement indicator" or not</param>
        void GotoLineWord(string szFile, string szWord, int iLine, int iOccurence, bool bUseCsi);

        /// <summary>
        /// Go to a specific text range in a file and highlight the range
        /// <remarks>Used mainly for Find and Replace</remarks>
        /// </summary>
        /// <param name="szFile">File to goto</param>
        /// <param name="iStartOffset">Start offset of text range</param>
        /// <param name="iEndOffset">End offset of text range</param>
        void GotoFileAndHighlightRange(string szFile, int iStartOffset, int iEndOffset);

        /// <summary>
        /// Go to a specific variable instance in a file
        /// </summary>
        /// <param name="var">Variable to go to</param>
        void GotoVariable(ISledVarBaseType var);
    }
}
