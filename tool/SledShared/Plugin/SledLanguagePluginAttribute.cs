/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// This attribute must be placed on SLED language plugins 
    /// if they are to be loaded by SLED.
    /// Add the attribute to the AssemblyInfo file as:
    /// <code>[assembly: Sce.Sled.Shared.Plugin.SledLanguagePluginAttribute]</code>
    /// or
    /// <code>[assembly: Sce.Sled.Shared.Plugin.SledLanguagePluginAttribute(user data string)]</code>
    /// <remarks>Copy/paste of ATF's ATFPluginAttribute.cs</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class SledLanguagePluginAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledLanguagePluginAttribute()
        {
        }

        /// <summary>
        /// Constructor with user data
        /// </summary>
        /// <param name="userData">Optional user data to pass in</param>
        public SledLanguagePluginAttribute(string userData)
        {
            Info = userData;
        }

        /// <summary>
        /// Get the user data string
        /// </summary>
        public string Info { get; private set; }
    }
}