/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// SLED project settings plugin interface
    /// </summary>
    public interface ISledProjectSettingsPlugin
    {
        /// <summary>
        /// Determines whether project settings need to be saved or not
        /// </summary>
        /// <returns>True iff project settings need to be saved</returns>
        bool NeedsSaving();
    }
}
