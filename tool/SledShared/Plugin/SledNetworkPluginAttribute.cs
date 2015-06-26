/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// This attribute must be placed on SLED network plugins 
    /// if they are to be loaded by SLED.
    /// Add the attribute to the AssemblyInfo file as:
    /// <code>[assembly: Sce.Sled.Shared.Plugin.SledNetworkPlugin]</code>
    /// or
    /// <code>[assembly: Sce.Sled.Shared.Plugin.SledNetworkPlugin(user data string)]</code>
    /// <remarks>Copy/paste of ATF's ATFPluginAttribute.cs</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class SledNetworkPluginAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledNetworkPluginAttribute()
        {
        }

        /// <summary>
        /// Constructor with user data
        /// </summary>
        /// <param name="userData">Optional user data to pass in</param>
        public SledNetworkPluginAttribute(string userData)
        {
            Info = userData;
        }

        /// <summary>
        /// Get the user data string
        /// </summary>
        public string Info { get; private set; }
    }
}
