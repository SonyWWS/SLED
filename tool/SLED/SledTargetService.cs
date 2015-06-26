/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SLED target service
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledTargetService))]
    [Export(typeof(SledTargetService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledTargetService : ICommandClient, IInitializable, ISledTargetService
    {
        [ImportingConstructor]
        public SledTargetService(
            MainForm mainForm,
            ICommandService commandService,
            ISettingsService settingsService)
        {
            m_mainForm = mainForm;

            // Create a new menu for target options
            var targetMenuInfo =
                commandService.RegisterMenu(
                    Menu.Target,
                    Localization.SledTargetMenuTitle,
                    Localization.SledTargetMenuTitleComment);

            commandService.RegisterCommand(
                Command.ShowDialog,
                Menu.Target,
                CommandGroup.Target,
                Localization.SledTargetMenuManageTargets,
                Localization.SledTargetMenuManageTargetsComment,
                Keys.Control | Keys.T,
                SledIcon.DebugManageTargets,
                CommandVisibility.All,
                this);

            m_remoteTargetComboBox = new ToolStripComboBox(Resources.Resource.RemoteTargets);
            m_remoteTargetComboBox.SelectedIndexChanged += RemoteTargetComboBoxSelectedIndexChanged;
            m_remoteTargetComboBox.IntegralHeight = true;
            m_remoteTargetComboBox.DropDownWidth += 100;
            if (m_remoteTargetComboBox.ComboBox != null)
            {
                var iWidth = m_remoteTargetComboBox.ComboBox.Width;
                m_remoteTargetComboBox.ComboBox.Width = iWidth + 100;
            }

            if (targetMenuInfo != null)
                targetMenuInfo.GetToolStrip().Items.Add(m_remoteTargetComboBox);

            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => SledTargets,
                    "Sled Targets",
                    null,
                    null));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService = SledServiceInstance.Get<ISledDebugService>();

            m_networkPluginService = SledServiceInstance.Get<ISledNetworkPluginService>();

            LoadImportedTargets();
            UpdateRemoteTargetComboBox();
        }

        #endregion

        #region Commands

        enum Command
        {
            ShowDialog,
        }

        enum Menu
        {
            Target,
        }

        enum CommandGroup
        {
            Target,
        }

        #endregion

        #region ICommandClient interface

        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            // Only one command to worry about

            // If DebugService is null we can still
            // let the user interact with the target
            // GUI but if DebugService is available
            // we can only allow changes while disconnected

            return
                m_debugService == null
                    ? true
                    : m_debugService.IsDisconnected;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.ShowDialog:
                    ShowTargetDlg();
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledTargetService Interface

        /// <summary>
        /// Return number of targets
        /// </summary>
        public int Count
        {
            get { return m_lstTargets.Count; }
        }

        /// <summary>
        /// Return list of all targets
        /// </summary>
        public IEnumerable<ISledTarget> Targets
        {
            get { return m_lstTargets.AsReadOnly(); }
        }

        /// <summary>
        /// Return currently selected target
        /// </summary>
        /// <returns>Selected target</returns>
        public ISledTarget SelectedTarget
        {
            get { return m_selectedTarget; }
        }

        /// <summary>
        /// Show the target dialog
        /// </summary>
        public void ShowTargetDlg()
        {
            using (var form =
                new SledTargetsForm(m_lstTargets, m_selectedTarget))
            {
                form.ShowDialog(m_mainForm);
                form.TryGetSelectedTarget(out m_selectedTarget);
                UpdateRemoteTargetComboBox();
            }
        }

        #endregion

        #region Persisted Settings Interface

        public string SledTargets
        {
            get
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.AppendChild(
                    xmlDoc.CreateXmlDeclaration(
                        Resources.Resource.OnePointZero,
                        Resources.Resource.UtfDashEight,
                        Resources.Resource.YesLower));

                var root =
                    xmlDoc.CreateElement(
                        Resources.Resource.SledTargets);

                xmlDoc.AppendChild(root);

                try
                {
                    // Save selected target
                    {
                        var target = SelectedTarget;

                        var elem = xmlDoc.CreateElement(Resources.Resource.Selected);
                        elem.SetAttribute("ip", target == null ? "null" : target.EndPoint.Address.ToString());
                        elem.SetAttribute("port", target == null ? "null" : target.EndPoint.Port.ToString());
                        elem.SetAttribute("protocol", target == null ? "null" : target.Plugin.Protocol);
                        elem.SetAttribute("imported", target == null ? "False" : target.Imported.ToString());
                        root.AppendChild(elem);
                    }

                    // Save targets
                    foreach (var target in m_lstTargets)
                    {
                        // Don't save imported targets
                        if (target.Imported)
                            continue;

                        var elem = xmlDoc.CreateElement(Resources.Resource.SledTarget);

                        // Mark which plugin this target belongs to for when we load data
                        elem.SetAttribute(Resources.Resource.NetworkPlugin, target.Plugin.Protocol);

                        // Let the plugin fill in the rest of the details
                        if (target.Plugin.Save(target, elem))
                            root.AppendChild(elem);
                    }

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    var szSetting =
                        Resources.Resource.SledTargets;

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

                    string selectedIp = null;
                    string selectedPort = null;
                    string selectedProtocol = null;
                    var selectedImported = false;

                    // Grab selected target data
                    var nodes =
                        xmlDoc.DocumentElement.SelectNodes(Resources.Resource.Selected);

                    if ((nodes != null) && (nodes.Count == 1))
                    {
                        var elem = nodes[0] as XmlElement;
                        if (elem != null)
                        {
                            selectedIp = elem.GetAttribute("ip");
                            selectedPort = elem.GetAttribute("port");
                            selectedProtocol = elem.GetAttribute("protocol");
                            selectedImported = bool.Parse(elem.GetAttribute("imported"));
                        }
                    }

                    // Stop "edit" > "Preferences" from adding duplicates
                    m_lstTargets.Clear();

                    // Add 'imported' targets
                    LoadImportedTargets();

                    nodes = xmlDoc.DocumentElement.SelectNodes(Resources.Resource.SledTarget);
                    if ((nodes != null) && (nodes.Count > 0))
                    {
                        // Add targets saved from Xml
                        foreach (XmlElement elem in nodes)
                        {
                            // Try and figure out which network plugin owns this target
                            var pluginProtocol =
                                elem.GetAttribute(Resources.Resource.NetworkPlugin);
                            
                            if (m_networkPluginService == null)
                                continue;

                            // Search through loaded network plugins for proper plugin
                            foreach (var networkPlugin in m_networkPluginService.NetworkPlugins)
                            {
                                if (string.Compare(networkPlugin.Protocol, pluginProtocol, true) != 0)
                                    continue;

                                // Try and load settings from the XmlElement into the target then add to list
                                ISledTarget target;
                                if (networkPlugin.Load(out target, elem))
                                    m_lstTargets.Add(target);

                                break;
                            }
                        }
                    }

                    // Search for and try to set a selected target
                    foreach (var target in m_lstTargets)
                    {
                        if (selectedImported != target.Imported)
                            continue;

                        if (string.Compare(selectedProtocol, target.Plugin.Protocol) != 0)
                            continue;

                        if (string.Compare(selectedIp, target.EndPoint.Address.ToString()) != 0)
                            continue;

                        if (string.Compare(selectedPort, target.EndPoint.Port.ToString()) != 0)
                            continue;

                        m_selectedTarget = target;
                        UpdateRemoteTargetComboBox();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var szSetting =
                        Resources.Resource.SledTargets;

                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        #endregion

        #region Member Methods

        private void UpdateRemoteTargetComboBox()
        {
            try
            {
                // Add all targets to combo box
                m_remoteTargetComboBox.Items.Clear();
                foreach (var target in m_lstTargets)
                    m_remoteTargetComboBox.Items.Add(target);

                // Select the proper target
                var selTarget = SelectedTarget;
                if (selTarget != null)
                    m_remoteTargetComboBox.SelectedItem = selTarget;
                else if (m_remoteTargetComboBox.Items.Count > 0)
                    m_remoteTargetComboBox.SelectedItem = m_remoteTargetComboBox.Items[0];
                else
                    m_remoteTargetComboBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledRemoteTargetErrorUpdatingComboBox, ex.Message));
            }
        }

        private void RemoteTargetComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            var selTarget = m_remoteTargetComboBox.SelectedItem as ISledTarget;

            if (selTarget != null)
                m_selectedTarget = selTarget;
        }

        private void LoadImportedTargets()
        {
            if (m_lstTargets.Count > 0)
                return;

            // Ask plugins for any 'imported' targets -> targets that
            // the plugin(s) can automatically create based on outside 
            // software/sources
            foreach (var networkPlugin in m_networkPluginService.NetworkPlugins)
            {
                ISledTarget[] importedTargets = null;

                try
                {
                    importedTargets = networkPlugin.ImportedTargets;
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception gathering imported targets from \"{1}\": {2}",
                        this, networkPlugin.Name, ex.Message);
                }
                
                if (importedTargets == null)
                    continue;

                if (importedTargets.Length <= 0)
                    continue;

                foreach (var importedTarget in importedTargets)
                    m_lstTargets.Add(importedTarget);
            }
        }

        #endregion

        private ISledTarget m_selectedTarget;
        private ISledDebugService m_debugService;
        private ISledNetworkPluginService m_networkPluginService;

        private readonly MainForm m_mainForm;

        private readonly List<ISledTarget> m_lstTargets =
            new List<ISledTarget>();

        private readonly ToolStripComboBox m_remoteTargetComboBox;
    }
}
