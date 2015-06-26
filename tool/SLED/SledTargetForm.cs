/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public partial class SledTargetForm : Form
    {
        public SledTargetForm(ISledTarget target)
        {
            InitializeComponent();

            // Add protocols to combo box
            foreach (var netPlugin in m_networkPluginService.Get.NetworkPlugins)
            {
                if (netPlugin is ISledNetworkPluginTargetFormCustomizer)
                {
                    var customizer = (ISledNetworkPluginTargetFormCustomizer)netPlugin;
                    if (!customizer.AllowProtocolOption)
                        continue;
                }

                m_cmbProtocol.Items.Add(new PluginTextAssociation(netPlugin));
                
                //
                // Add settings control from each plugin
                //

                // Create plugin defined user control
                var userControl = netPlugin.CreateSettingsControl(target);

                // Move inside the group box a bit
                userControl.Location = new System.Drawing.Point(10, 15);
                
                // Set up references
                userControl.Tag = netPlugin;
                m_dictControls.Add(netPlugin, userControl);

                // Add control to group box
                m_grpProtocolSettings.Controls.Add(userControl);

                // Hide control for now
                userControl.Hide();
            }

            // Select a protocol
            if (target == null)
            {
                // Select TCP by default
                SelectProtocol("TCP");
            }
            else
            {
                m_targetDefault = target;

                // Set up from existing details
                m_txtName.Text = target.Name;
                m_txtHost.Text = target.EndPoint.Address.ToString();
                m_txtPort.Text = target.EndPoint.Port.ToString();
                SelectProtocol(target.Plugin);
            }
        }

        public ISledTarget Target { get; private set; }

        private void SelectProtocol(ISledNetworkPlugin networkPlugin)
        {
            if (m_bSelectingItem)
                return;

            if (networkPlugin == null)
                return;

            try
            {
                m_bSelectingItem = true;

                foreach (PluginTextAssociation assoc in m_cmbProtocol.Items)
                {
                    if (assoc.NetworkPlugin != networkPlugin)
                        continue;

                    m_cmbProtocol.SelectedItem = assoc;
                    ShowSettings(assoc.NetworkPlugin);
                    break;
                }
            }
            finally
            {
                m_bSelectingItem = false;
            }
        }

        private void SelectProtocol(string protocol)
        {
            if (m_bSelectingItem)
                return;

            try
            {
                m_bSelectingItem = true;

                if (string.IsNullOrEmpty(protocol))
                    return;

                foreach (PluginTextAssociation assoc in m_cmbProtocol.Items)
                {
                    if (string.Compare(assoc.NetworkPlugin.Protocol, protocol, true) == 0)
                    {
                        m_cmbProtocol.SelectedItem = assoc;
                        ShowSettings(assoc.NetworkPlugin);
                        break;
                    }
                }
            }
            finally
            {
                m_bSelectingItem = false;
            }
        }

        private void ShowSettings(ISledNetworkPlugin netPlugin)
        {
            foreach (var kv in m_dictControls)
            {
                if (kv.Key == netPlugin)
                {
                    kv.Value.Show();

                    var bSetPort = false;

                    if (m_targetDefault != null)
                    {
                        // Restore port if switching back to protocol
                        // that the target originally was
                        if (m_targetDefault.Plugin == netPlugin)
                        {
                            m_txtPort.Text = m_targetDefault.EndPoint.Port.ToString();
                            bSetPort = true;
                        }
                    }

                    // Set port to default protocol port
                    if (!bSetPort)
                        m_txtPort.Text = kv.Value.DefaultPort.ToString();
                }
                else
                {
                    kv.Value.Hide();
                }
            }
        }

        private bool TryGetSettings(ISledNetworkPlugin netPlugin, out SledNetworkPluginTargetFormSettings settings)
        {
            settings =
                (from kv in m_dictControls
                 where kv.Key == netPlugin
                 select kv.Value).FirstOrDefault();

            return settings != null;
        }

        #region Private Classes

        private class PluginTextAssociation
        {
            public PluginTextAssociation(ISledNetworkPlugin netPlugin)
            {
                m_networkPlugin = netPlugin;
            }

            public ISledNetworkPlugin NetworkPlugin
            {
                get { return m_networkPlugin; }
            }

            public override string ToString()
            {
                return m_networkPlugin.Protocol;
            }

            private readonly ISledNetworkPlugin m_networkPlugin;
        }

        #endregion

        private void ShowFailureAndDontClose(string message, Control errorControl)
        {
            // Show error message
            MessageBox.Show(
                this,
                message,
                @"Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            // Don't allow closing yet
            DialogResult = DialogResult.None;
            
            // Focus on the control that needs fixing
            if (errorControl != null)
                errorControl.Focus();
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            // Verify name not empty
            if (string.IsNullOrEmpty(m_txtName.Text.Trim()))
            {
                ShowFailureAndDontClose(
                    "Name can't be empty!",
                    m_txtName);

                return;
            }

            // Verify host not empty
            if (string.IsNullOrEmpty(m_txtHost.Text))
            {
                ShowFailureAndDontClose(
                    "Host can't be empty!",
                    m_txtHost);

                return;
            }

            // Verify host actual host
            IPAddress ipAddr;
            if (!IPAddress.TryParse(m_txtHost.Text, out ipAddr))
            {
                try
                {
                    var hostEntry = Dns.GetHostEntry(m_txtHost.Text);
                    ipAddr = hostEntry.AddressList.FirstOrDefault();
                }
                catch (ArgumentNullException) { }
                catch (ArgumentException) { }
                catch (System.Net.Sockets.SocketException) { }
            }

            if (ipAddr == null)
            {
                ShowFailureAndDontClose(
                    "Invalid host!",
                    m_txtHost);

                return;
            }

            // Verify port not empty
            if (string.IsNullOrEmpty(m_txtPort.Text))
            {
                ShowFailureAndDontClose(
                    "Port can't be empty!",
                    m_txtPort);

                return;
            }

            // Verify port is a valid number
            int port;
            if (!int.TryParse(m_txtPort.Text, out port))
            {
                ShowFailureAndDontClose(
                    "Port isn't a number!",
                    m_txtPort);

                return;
            }

            // Make sure port within valid range
            if ((port < IPEndPoint.MinPort) || (port > IPEndPoint.MaxPort))
            {
                ShowFailureAndDontClose(
                    string.Format(
                        "Port must be within the range {0} to {1}!",
                        IPEndPoint.MinPort,
                        IPEndPoint.MaxPort),
                    m_txtPort);

                return;
            }

            // Make sure valid protocol selected
            var assoc = m_cmbProtocol.SelectedItem as PluginTextAssociation;
            if (assoc == null)
            {
                ShowFailureAndDontClose(
                    "A valid protocol must be selected!",
                    m_cmbProtocol);

                return;
            }

            // Verify network plugin still available
            var bFoundPlugin =
                m_networkPluginService.Get.NetworkPlugins.Any(
                    netPlugin => assoc.NetworkPlugin == netPlugin);

            if (!bFoundPlugin)
            {
                ShowFailureAndDontClose(
                    "The selected protocol is no longer valid!",
                    m_cmbProtocol);

                return;
            }

            SledNetworkPluginTargetFormSettings settings;
            if (!TryGetSettings(assoc.NetworkPlugin, out settings))
            {
                ShowFailureAndDontClose(
                    "The selected protocol is no longer valid!",
                    m_cmbProtocol);

                return;
            }

            string settingsMsg;
            Control settingsControl;
            if (settings.ContainsErrors(out settingsMsg, out settingsControl))
            {
                ShowFailureAndDontClose(
                    settingsMsg,
                    settingsControl);

                return;
            }

            // Finally create the target
            Target =
                assoc.NetworkPlugin.CreateAndSetup(
                    m_txtName.Text.Trim(),
                    new IPEndPoint(ipAddr, port),
                    settings.GetDataBlob());
        }

        private void CmbProtocolSelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null)
                return;

            var assoc = comboBox.SelectedItem as PluginTextAssociation;
            if (assoc == null)
                return;

            SelectProtocol(assoc.NetworkPlugin);
        }

        private bool m_bSelectingItem;

        private readonly ISledTarget m_targetDefault;

        private readonly SledServiceReference<ISledNetworkPluginService> m_networkPluginService =
            new SledServiceReference<ISledNetworkPluginService>();

        private readonly Dictionary<ISledNetworkPlugin, SledNetworkPluginTargetFormSettings> m_dictControls =
            new Dictionary<ISledNetworkPlugin, SledNetworkPluginTargetFormSettings>();
    }
}
