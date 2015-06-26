/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// SledEvent class
    /// <remarks>Copy/paste of ATF3 Event.cs for the most part</remarks>
    /// </summary>
    public static class SledEvent
    {
        /// <summary>
        /// Raises an event
        /// </summary>
        /// <param name="handler">Handler, or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
                handler(sender, e);
        }

        /// <summary>
        /// Rasies an event
        /// </summary>
        /// <typeparam name="T">Event type, derived from EventArgs</typeparam>
        /// <param name="handler">Handler, or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e)
            where T : EventArgs            
        {
            if (handler != null)
                handler(sender, e);
        }
    }
}
