/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Shared
{
    /// <summary>
    /// This attribute should only be applied to SLED.exe 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class SledAssemblyAttribute : Attribute
    {
    }
}
