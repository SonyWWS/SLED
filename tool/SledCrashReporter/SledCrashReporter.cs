/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.CrashReporter
{
    /// <summary>
    /// Wrapper around internal library, disabled in open source release
    /// </summary>
    public class SledCrashReporter : ISledCrashReporter
    {
        public void Initialize()
        {
            // crash reporter uses an internal library and therefore disabled for open source release
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "SLED crash reporting disabled " + 
                    "(unable to create CrashReporter)!");
            }

            //SledOutDevice.OutLine(SledMessageType.Info, "SLED crash reporting enabled!");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // disabled
        }

        #endregion

        #region Implementation of ICrashLogger

        public void LogException(Exception e)
        {
            // disabled
        }

        #endregion
    }
}