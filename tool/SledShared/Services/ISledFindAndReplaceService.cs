/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// Enumeration for find and replace modes
    /// </summary>
    public enum SledFindAndReplaceModes
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Quick find
        /// </summary>
        QuickFind,

        /// <summary>
        /// Find in files
        /// </summary>
        FindInFiles,

        /// <summary>
        /// Quick replace
        /// </summary>
        QuickReplace,

        /// <summary>
        /// Replace in files
        /// </summary>
        ReplaceInFiles,
    }

    /// <summary>
    /// SLED find and replace service interface
    /// </summary>
    public interface ISledFindAndReplaceService
    {
        /// <summary>
        /// Show a particular find and replace dialog
        /// </summary>
        /// <param name="dlg">Dialog to show</param>
        void ShowDialog(SledFindAndReplaceModes dlg);

        /// <summary>
        /// Show a particular find and replace dialog
        /// </summary>
        /// <param name="dlg">Dialog to show</param>
        /// <param name="initialText">Initial text to add to the find or replace box</param>
        void ShowDialog(SledFindAndReplaceModes dlg, string initialText);
    }
}
