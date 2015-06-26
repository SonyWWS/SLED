/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Scmp;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugHeartbeatService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugHeartbeatService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugHeartbeatService : IInitializable
    {
        [ImportingConstructor]
        public SledDebugHeartbeatService(ISettingsService settingsService)
        {
            // Save heartbeat settings
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => HeartbeatSettings,
                    "Heartbeat Settings",
                    "DebugHeartbeat",
                    "Heartbeat Settings"));

            // Add some user settings to edit > preferences
            settingsService.RegisterUserSettings(
                "Heartbeat",
                new BoundPropertyDescriptor(
                    this,
                    () => Audible,
                    "Verbose",
                    null,
                    "Enable or disable heartbeat output"),
                new BoundPropertyDescriptor(
                    this,
                    () => ControlConnection,
                    "Control Connection",
                    null,
                    "True if heartbeat can disconnect the connection after 3 timeouts or false if it cannot"));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.Ready += DebugServiceReady;
            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.Disconnected += DebugServiceDisconnected;
        }

        #endregion

        #region Persisted Settings

        public string HeartbeatSettings
        {
            get
            {
                // Generate Xml string to contain the TTY filter list
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration(Resources.Resource.OnePointZero, Resources.Resource.UtfDashEight, Resources.Resource.YesLower));
                var root = xmlDoc.CreateElement("HeartbeatSettings");
                xmlDoc.AppendChild(root);

                try
                {
                    var elem = xmlDoc.CreateElement("HeartbeatSettings");
                    elem.SetAttribute("Audible", m_bAudible.ToString());
                    elem.SetAttribute("ControlConnection", m_bControlConnection.ToString());
                    root.AppendChild(elem);

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    const string szSetting = "HeartbeatSettings";
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionSavingSetting, szSetting, ex.Message));
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    var nodes = xmlDoc.DocumentElement.SelectNodes("HeartbeatSettings");
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        m_bAudible = (elem.GetAttribute("Audible") == true.ToString());
                        m_bControlConnection = (elem.GetAttribute("ControlConnection") == true.ToString());
                    }
                }
                catch (Exception ex)
                {
                    const string szSetting = "HeartbeatSettings";
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        public bool Audible
        {
            get { return m_bAudible; }
            set { m_bAudible = value; }
        }

        public bool ControlConnection
        {
            get { return m_bControlConnection; }
            set { m_bControlConnection = value; }
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceReady(object sender, SledDebugServiceEventArgs e)
        {
            StartHeartbeat();
        }

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            UpdateHeartbeat();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            StopHeartbeat();
        }

        #endregion

        #region Member Methods

        private void StartHeartbeat()
        {
            if (m_timer != null)
                StopHeartbeat();

            UpdateHeartbeat();

            m_timer = new Timer {Interval = PingTime};
            m_timer.Tick += HeartbeatTimerTick;
            m_timer.Start();
        }

        private void UpdateHeartbeat()
        {
            m_iLostCounter = 0;
            m_lastRecvTime = DateTime.Now;
        }

        private void StopHeartbeat()
        {
            if (m_timer == null)
                return;

            m_timer.Stop();
            m_timer.Tick -= HeartbeatTimerTick;
            m_timer.Dispose();
            m_timer = null;
        }

        private void HeartbeatTimerTick(object sender, EventArgs e)
        {
            if (!m_debugService.IsConnected)
                return;

            // Send heartbeat message
            m_debugService.SendScmp(new Heartbeat(m_debugService.SledPluginId));

            // Calculate time since last update
            var span = DateTime.Now.Subtract(m_lastRecvTime);
            if (span.Seconds <= TimeOutSec)
                return;

            // Start showing we're getting no response if audible
            if (m_bAudible)
                SledOutDevice.OutLine(SledMessageType.Error, Localization.SledTargetHeartBeatNoResponse);

            if (m_iLostCounter >= 2)
            {
                // Disconnect if heartbeat is allowed to control the connection
                if (m_bControlConnection)
                    ((SledDebugService)m_debugService).Disconnect();
            }

            m_iLostCounter++;
        }

        #endregion

        private ISledDebugService m_debugService;

        private Timer m_timer;
        private DateTime m_lastRecvTime;

        public const int PingTime = 1500;  // every 1.5 seconds
        public const int TimeOutSec = 5;      // 5 seconds

        private int m_iLostCounter;
        private bool m_bAudible;
        private bool m_bControlConnection;
    }
}
