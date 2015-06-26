/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Net.Tcp
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledNetworkPlugin))]
    [Export(typeof(SledNetPluginTcp))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledNetPluginTcp : IInitializable, ISledNetworkPlugin
    {
        [ImportingConstructor]
        public SledNetPluginTcp(ISettingsService settingsService)
        {
            // From AssemblyInfo.cs in this .dll
            PluginGuid = new Guid("7cdb0b45-9c29-4412-8bcd-1413b058611c");

            // Persist settings
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => PersistedSettings,
                    "TCP Settings",
                    "Network",
                    "TCP Settings"));

            // Add some user settings to edit > preferences
            settingsService.RegisterUserSettings(
                "Network",
                new BoundPropertyDescriptor(
                    this,
                    () => DefaultPortUserSetting,
                    "TCP Default Port",
                    null,
                    "Default TCP port"));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledNetworkPlugin Interface

        public void Connect(ISledTarget target)
        {
            var tcpTarget = target as SledTcpTarget;
            if (tcpTarget == null)
                throw new InvalidOperationException("target is not a TcpTarget");

            m_socket = new TargetTcpSocket(1000);
            m_socket.Connected += SocketConnected;
            m_socket.Disconnected += SocketDisconnected;
            m_socket.DataReady += SocketDataReady;
            m_socket.UnHandledException += SocketUnHandledException;

            try
            {
                m_socket.Connect(tcpTarget);
            }
            catch (Exception ex)
            {
                if (UnHandledExceptionEvent != null)
                    UnHandledExceptionEvent(this, ex);
            }
        }        

        public bool IsConnected
        {
            get
            {
                return m_socket != null && m_socket.IsConnected;
            }
        }

        public void Disconnect()
        {
            try
            {
                m_socket.Disconnect();
            }
            catch (Exception ex)
            {
                if (UnHandledExceptionEvent != null)
                    UnHandledExceptionEvent(this, ex);
            }
            finally
            {
                if (m_socket != null)
                {
                    m_socket.Connected -= SocketConnected;
                    m_socket.Disconnected -= SocketDisconnected;
                    m_socket.DataReady -= SocketDataReady;
                    m_socket.UnHandledException -= SocketUnHandledException;
                    m_socket = null;
                }
            }
        }

        public int Send(byte[] buffer)
        {
            return Send(buffer, buffer.Length);
        }

        public int Send(byte[] buffer, int length)
        {
            var iLen = length;

            try
            {
                m_socket.Send(buffer, length);
            }
            catch (Exception ex)
            {
                if (UnHandledExceptionEvent != null)
                    UnHandledExceptionEvent(this, ex);

                iLen = -1;
            }

            return iLen;
        }

        public void Dispose()
        {
            if (IsConnected)
                Disconnect();
        }

        public event ConnectionHandler ConnectedEvent;
        public event ConnectionHandler DisconnectedEvent;
        public event DataReadyHandler DataReadyEvent;
        public event UnHandledExceptionHandler UnHandledExceptionEvent;

        public string Name
        {
            get { return PluginName; }
        }

        public string Protocol
        {
            get { return PluginProtocol; }
        }

        public SledNetworkPluginTargetFormSettings CreateSettingsControl(ISledTarget target)
        {
            return new SledTcpSettingsControl(target);
        }

        public ISledTarget CreateAndSetup(string name, IPEndPoint endPoint, params object[] settings)
        {
            var target = new SledTcpTarget(name, endPoint, this, false);
            return target;
        }

        public ISledTarget[] ImportedTargets
        {
            get
            {
                var lstTargets = 
                    new List<ISledTarget>
                        {
                            new SledTcpTarget(
                                "localhost",
                                new IPEndPoint(IPAddress.Loopback, DefaultPort),
                                this,
                                true)
                        };

                return lstTargets.ToArray();
            }
        }

        public Guid PluginGuid { get; private set; }

        #endregion

        #region ISledNetworkPluginPersistedSettings

        public bool Save(ISledTarget target, XmlElement elem)
        {
            if (elem == null)
                return false;

            var tcpTarget = target as SledTcpTarget;
            if (tcpTarget == null)
                return false;

            elem.SetAttribute("name", tcpTarget.Name);
            elem.SetAttribute("ipaddress", tcpTarget.EndPoint.Address.ToString());
            elem.SetAttribute("port", tcpTarget.EndPoint.Port.ToString());

            return true;
        }

        public bool Load(out ISledTarget target, XmlElement elem)
        {
            target = null;

            if (elem == null)
                return false;

            var bSuccessful = false;

            try
            {
                var name = elem.GetAttribute("name");
                if (string.IsNullOrEmpty(name))
                    return false;

                var ipaddress = elem.GetAttribute("ipaddress");
                if (string.IsNullOrEmpty(ipaddress))
                    return false;

                IPAddress ipAddr;
                if (!IPAddress.TryParse(ipaddress, out ipAddr))
                    return false;

                int port;
                if (!int.TryParse(elem.GetAttribute("port"), out port))
                    return false;

                target = CreateAndSetup(name, new IPEndPoint(ipAddr, port), null);
                bSuccessful = target != null;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(SledMessageType.Error, "{0}: Exception loading settings: {1}", this, ex.Message);
                target = null;
            }
            
            return bSuccessful;
        }

        #endregion

        #region Persisted Settings

        public string PersistedSettings
        {
            get
            {
                // Generate Xml string to contain the Mru project list
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                var root = xmlDoc.CreateElement(PersistedSettingsElement);
                xmlDoc.AppendChild(root);

                try
                {
                    var elem = xmlDoc.CreateElement(PersistedSettingsTcpElement);
                    elem.SetAttribute(PersistedSettingsTcpAttribute, DefaultPort.ToString());
                    root.AppendChild(elem);

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    const string szSetting = PersistedSettingsElement;
                    SledOutDevice.OutLine(SledMessageType.Info, "Exception saving {0} settings: {1}", szSetting, ex.Message);
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

                    var nodes = xmlDoc.DocumentElement.SelectNodes(PersistedSettingsTcpElement);
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        if (!elem.HasAttribute(PersistedSettingsTcpAttribute))
                            continue;

                        var szPort = elem.GetAttribute(PersistedSettingsTcpAttribute);

                        int iPort;
                        if (int.TryParse(szPort, out iPort))
                            DefaultPort = iPort;
                    }
                }
                catch (Exception ex)
                {
                    const string szSetting = PersistedSettingsElement;
                    SledOutDevice.OutLine(SledMessageType.Info, "Exception loading {0} settings: {1}", szSetting, ex.Message);
                }
            }
        }

        public int DefaultPortUserSetting
        {
            get { return DefaultPort; }
            set { DefaultPort = value; }
        }

        public static int DefaultPort
        {
            get { return s_defaultPort; }
            set { s_defaultPort = value; }
        }

        private const string PersistedSettingsElement = "NetworkSettings";
        private const string PersistedSettingsTcpElement = "tcp";
        private const string PersistedSettingsTcpAttribute = "default_port";

        private static int s_defaultPort = 11111;

        #endregion

        #region Socket Events

        private void SocketConnected(object sender, ISledTarget target)
        {
            if (ConnectedEvent != null)
                ConnectedEvent(this, target);
        }

        private void SocketDisconnected(object sender, ISledTarget target)
        {
            if (DisconnectedEvent != null)
                DisconnectedEvent(this, target);
        }

        private void SocketDataReady(object sender, byte[] buffer)
        {
            if (DataReadyEvent != null)
                DataReadyEvent(this, buffer);
        }

        private void SocketUnHandledException(object sender, Exception ex)
        {
            if (UnHandledExceptionEvent != null)
                UnHandledExceptionEvent(this, ex);
        }

        #endregion

        private TargetTcpSocket m_socket;

        private const string PluginName = "SLED Tcp Network Plugin";
        private const string PluginProtocol = "TCP";
    }
}
