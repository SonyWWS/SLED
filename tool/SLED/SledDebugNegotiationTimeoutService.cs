/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugNegotiationTimeoutService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugNegotiationTimeoutService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugNegotiationTimeoutService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.Connecting += DebugServiceConnecting;
            m_debugService.Connected += DebugServiceConnected;
            m_debugService.Disconnected += DebugServiceDisconnected;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceConnecting(object sender, SledDebugServiceEventArgs e)
        {
            StartTimer();
        }

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            StopTimer();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            StopTimer();
        }

        #endregion

        private void StartTimer()
        {
            StopTimer();

            m_timer = new Timer {Interval = TimeOutInSeconds*1000};
            m_timer.Tick += TimerTick;
            m_timer.Enabled = true;
            m_timer.Start();
        }

        private void StopTimer()
        {
            if (m_timer == null)
                return;

            m_timer.Stop();
            m_timer.Tick -= TimerTick;
            m_timer.Dispose();
            m_timer = null;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, 
                Localization.SledTargetConnectionErrorNegotiating);

            // Timed out so disconnect
            ((SledDebugService)m_debugService).Disconnect();
        }

        private Timer m_timer;

        private ISledDebugService m_debugService;

        private const int TimeOutInSeconds = 5;
        
    }
}
