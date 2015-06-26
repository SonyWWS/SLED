/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED target service interface
    /// </summary>
    public interface ISledTargetService
    {
        /// <summary>
        /// Get number of targets
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Get enumeration of all targets
        /// </summary>
        IEnumerable<ISledTarget> Targets
        {
            get;
        }

        /// <summary>
        /// Get currently selected target or null
        /// </summary>
        /// <returns>Selected target or null</returns>
        ISledTarget SelectedTarget
        {
            get;
        }

        /// <summary>
        /// Show the target dialog
        /// </summary>
        void ShowTargetDlg();
    }
}
