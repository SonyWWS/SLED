/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED debug freeze service interface
    /// </summary>
    public interface ISledDebugFreezeService
    {
        /// <summary>
        /// Get whether GUIs should be frozen or not
        /// </summary>
        bool Frozen { get; }

        /// <summary>
        /// Event triggered when GUIs should freeze
        /// </summary>
        event EventHandler Freezing;

        /// <summary>
        /// Event triggered when GUIs should unfreeze
        /// </summary>
        event EventHandler Thawing;
    }
}