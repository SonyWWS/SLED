/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED usage statistics interface
    /// </summary>
    public interface ISledUsageStatistics : IInitializable, IDisposable
    {
        /// <summary>
        /// Send usage data to the TNT recap server
        /// <remarks>Usage data includes SLED and ATF versions, as well
        /// as the version numbers of any DLLs loaded in the current process.</remarks>
        /// </summary>
        void PhoneHome();
    }
}