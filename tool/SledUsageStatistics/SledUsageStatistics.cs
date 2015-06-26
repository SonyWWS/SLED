/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.UsageStatistics
{
    /// <summary>
    /// Wrapper around internal library, disabled for open source release
    /// </summary>
    public class SledCrashReporter : ISledUsageStatistics
    {
        #region Initialize Interface

        public void Initialize()
        {
            m_canConnect = false;

            SledOutDevice.OutLine(
                SledMessageType.Info,
                m_canConnect
                    ? "SLED usage statistics enabled!"
                    : "SLED usage statistics disabled!");
        }

        #endregion

        #region ISledUsageStatistics Interface

        public void PhoneHome()
        {
            if (!m_canConnect)
                return;

            try
            {
                AtfUsageLogger.SendAtfUsageInfo();
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(SledMessageType.Error, "{0}: Exception in PhoneHome: {1}", this, ex.Message);
            }
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
        }

        #endregion

        private bool m_canConnect;
    }
}