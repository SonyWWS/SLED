/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED crash reporter interface
    /// </summary>
    public interface ISledCrashReporter : IInitializable, IDisposable, ICrashLogger
    {
    }
}