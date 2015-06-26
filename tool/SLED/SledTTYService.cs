/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Scmp;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledTtyService))]
    [Export(typeof(SledTtyService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledTtyService : IInitializable, ISledTtyService, IControlHostClient, ICommandClient, IInstancingContext
    {
        [ImportingConstructor]
        public SledTtyService(
            MainForm mainForm,
            ICommandService commandService,
            ISettingsService settingsService,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_commandService = commandService;
            m_lastFlush = DateTime.Now;

            // Create GUI
            m_control =
                new SledTtyGui
                    {
                        Name = "TTY",
                        ColumnNames = new[] {"Time", "Data"}
                    };
            m_control.SendClicked += ControlSendClicked;

            // Register menu
            commandService.RegisterMenu(
                Menu.Tty,
                Localization.SledTTY,
                Localization.SledTTYOptions);

            // Register command to bring up TTY filters
            commandService.RegisterCommand(
                Command.Filter,
                Menu.Tty,
                CommandGroup.Tty,
                Localization.SledTTYFilterTTYOutput,
                Localization.SledTTYFilterTTYOutputComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Register clear TTY window command
            commandService.RegisterCommand(
                Command.Clear,
                Menu.Tty,
                CommandGroup.Tty,
                Localization.SledTTYClearTTYWindow,
                Localization.SledTTYClearTTYWindowComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            // Save the TTY filter list
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => TtyFilters,
                    Resources.Resource.TTYFilterListTitle,
                    Resources.Resource.TTY,
                    Resources.Resource.TTYFilterListComment));

            // Save GUI settings
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => GuiSettings,
                    "Tty GUI Settings",
                    Resources.Resource.TTY,
                    "Tty GUI settings"));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_control.Initialize();
            
            var controlHostService =
                SledServiceInstance.Get<IControlHostService>();

            // Grab image
            var image =
                ResourceUtil.GetImage(Atf.Resources.UnsortedImage);

            // Rotate image
            image.RotateFlip(RotateFlipType.Rotate90FlipX);

            var controlInfo =
                new ControlInfo(
                    m_control.Name,
                    m_control.Name,
                    StandardControlGroup.Bottom,
                    image);

            // Show GUI
            controlHostService.RegisterControl(
                m_control,
                controlInfo,
                this);

            // Subscribe to events
            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.BreakpointContinue += DebugServiceBreakpointContinue;
            m_debugService.UpdateEnd += DebugServiceUpdateEnd;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_projectService.Get.Closed += ProjectServiceClosed;
        }

        #endregion

        #region Commands

        enum Command
        {
            Filter,
            Clear,
        }

        enum Menu
        {
            Tty,
        }

        enum CommandGroup
        {
            Tty,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.Clear:
                    case Command.Filter:
                        bEnabled = true;
                        break;
                }
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.Clear:
                    Clear();
                    break;

                case Command.Filter:
                    ShowTtyFilterForm();
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledTtyService Interface

        public void Write(SledTtyMessage message)
        {
            if (message == null)
                return;

            if (StringUtil.IsNullOrEmptyOrWhitespace(message.Message))
                return;

            m_lstMessages.Add(message);

            var now = DateTime.Now;
            if (now.Subtract(m_lastFlush).TotalSeconds >= 1)
            {
                Flush();
                return;
            }

            if (m_bShouldFlush)
                Flush();
        }

        public void Clear()
        {
            m_control.Clear();
        }

        public bool InputEnabled
        {
            get { return m_control.SendEnabled; }
            set { m_control.SendEnabled = value; }
        }

        public void RegisterLanguage(ISledLanguagePlugin languagePlugin)
        {
            if (languagePlugin == null)
                throw new ArgumentNullException("languagePlugin");

            m_control.RegisterLanguage(languagePlugin);
        }

        #endregion

        #region Persisted Settings

        public string TtyFilters
        {
            get
            {
                // Generate Xml string to contain the TTY filter list
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(
                    xmlDoc.CreateXmlDeclaration(
                        Resources.Resource.OnePointZero,
                        Resources.Resource.UtfDashEight,
                        Resources.Resource.YesLower));

                var root = xmlDoc.CreateElement(Resources.Resource.TTYFilters);
                xmlDoc.AppendChild(root);

                try
                {
                    foreach (var filter in m_lstFilters)
                    {
                        var elem = xmlDoc.CreateElement(Resources.Resource.TTYFilter);
                        elem.SetAttribute("filter", filter.Filter);
                        elem.SetAttribute("txtColorR", filter.TextColor.R.ToString());
                        elem.SetAttribute("txtColorG", filter.TextColor.G.ToString());
                        elem.SetAttribute("txtColorB", filter.TextColor.B.ToString());
                        elem.SetAttribute("bgColorR", filter.BackgroundColor.R.ToString());
                        elem.SetAttribute("bgColorG", filter.BackgroundColor.G.ToString());
                        elem.SetAttribute("bgColorB", filter.BackgroundColor.B.ToString());
                        elem.SetAttribute("result", (filter.Result == SledTtyFilterResult.Show) ? Resources.Resource.One : Resources.Resource.Zero);
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

                    var szSetting = Resources.Resource.TTYFilters;
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

                    var nodes = xmlDoc.DocumentElement.SelectNodes(Resources.Resource.TTYFilter);
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        var filter = new SledTtyFilter(
                            elem.GetAttribute("filter"),
                            ((int.Parse(elem.GetAttribute("result")) == 1) ? SledTtyFilterResult.Show : SledTtyFilterResult.Ignore),
                            Color.FromArgb(
                                int.Parse(elem.GetAttribute("txtColorR")),
                                int.Parse(elem.GetAttribute("txtColorG")),
                                int.Parse(elem.GetAttribute("txtColorB"))
                                ),
                            Color.FromArgb(
                                int.Parse(elem.GetAttribute("bgColorR")),
                                int.Parse(elem.GetAttribute("bgColorG")),
                                int.Parse(elem.GetAttribute("bgColorB"))
                                ));

                        m_lstFilters.Add(filter);
                    }
                }
                catch (Exception ex)
                {
                    var szSetting = Resources.Resource.TTYFilters;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        public string GuiSettings
        {
            get { return m_control.Settings; }
            set { m_control.Settings = value; }
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Activates the client control</summary>
        /// <param name="control">Client control to be activated</param>
        public void Activate(Control control)
        {
            if (control != m_control)
                return;

            // We need to do this so we can steal keystrokes
            m_commandService.SetActiveClient(this);

            if (m_contextRegistry.Get != null)
                m_contextRegistry.Get.ActiveContext = this;
        }

        /// <summary>
        /// Deactivates the client control</summary>
        /// <param name="control">Client control to be deactivated</param>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Closes the client control</summary>
        /// <param name="control">Client control to be closed</param>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            // Clear TTY when project closes
            Clear();
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (TypeCodes)e.Scmp.TypeCode;
            switch (typeCode)
            {
                case TypeCodes.TtyBegin:
                    HandleTtyBegin();
                    break;

                case TypeCodes.Tty:
                    HandleTty();
                    break;

                case TypeCodes.TtyEnd:
                    HandleTtyEnd();
                    break;
            }
        }

        private void DebugServiceBreakpointContinue(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            InputEnabled = false;
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            InputEnabled = true;
            Flush();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            InputEnabled = false;
        }

        #endregion

        #region IInstancingContext Interface

        public bool CanInsert(object data)
        {
            return false;
        }

        public void Insert(object data)
        {
        }

        public bool CanCopy()
        {
            return m_control.SelectionCount != 0;
        }

        public object Copy()
        {
            var sb = new StringBuilder();

            foreach (var message in m_control.Selection)
                sb.Append(message.Message);

            return new DataObject(sb.ToString());
        }

        public bool CanDelete()
        {
            return false;
        }

        public void Delete()
        {
        }

        #endregion

        #region Member Methods

        private void ShowTtyFilterForm()
        {
            using (var form = new SledTtyFilterForm())
            {
                form.TtyFilterList = m_lstFilters;
                form.ShowDialog(m_mainForm);
            }
        }

        private void HandleTtyBegin()
        {
            m_builder.Remove(0, m_builder.Length);
        }

        private void HandleTty()
        {
            var tty = m_debugService.GetScmpBlob<Tty>();
            m_builder.Append(tty.Message);
        }

        private void HandleTtyEnd()
        {
            ProcessTtyMessage(m_builder.ToString());
        }

        private void ProcessTtyMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Scan for embedded color tags
            if (CheckTtyEmbeddedColorCodes(message))
                return;

            // No color tags; check against filters
            if (m_lstFilters.Count <= 0)
            {
                // Not filters so display normally
                Write(new SledTtyMessage(SledMessageType.Info, message));
            }
            else
            {
                // Run message through TTY filters
                foreach (var filter in m_lstFilters)
                {
                    // If it passes the filter it means the message
                    // matched the filter pattern
                    if (PassesTtyFilter(message, filter))
                    {
                        // Check what to do with this message
                        if (filter.Result == SledTtyFilterResult.Show)
                            Write(new SledTtyMessage(message, filter.TextColor, filter.BackgroundColor));

                        return;
                    }
                }

                // Default display - didn't match any filters
                Write(new SledTtyMessage(SledMessageType.Info, message));
            }
        }

        private bool CheckTtyEmbeddedColorCodes(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            var colorCodes = message.Split(s_ttyColorCodes, StringSplitOptions.RemoveEmptyEntries);
            if (colorCodes.Length <= 1)
                return false;

            var bRetval = false;
            var colTxtColor = Color.Black;
            var colBgColor = Color.White;
            var strippedMsg = message;

            foreach (var colorCode in colorCodes)
            {
                var color = colorCode.Split(s_ttyColorCodesSep, StringSplitOptions.RemoveEmptyEntries);
                if (color.Length != 4)
                    continue;

                int r, g, b;

                if (!int.TryParse(color[1], out r))
                    continue;
                if (!int.TryParse(color[2], out g))
                    continue;
                if (!int.TryParse(color[3], out b))
                    continue;

                if (color[0] == "txt")
                    colTxtColor = Color.FromArgb(255, r, g, b);
                else if (color[0] == "bg")
                    colBgColor = Color.FromArgb(255, r, g, b);
                else
                    continue;

                // If we got here lets pull the color code completely out of the string
                var iPos = strippedMsg.IndexOf(Resources.Resource.LeftSquiggly + colorCode + Resources.Resource.RightSquiggly);
                if (iPos == -1)
                    continue;

                bRetval = true;
                strippedMsg = strippedMsg.Remove(iPos, colorCode.Length + 2);
            }

            if (bRetval)
                Write(new SledTtyMessage(strippedMsg, colTxtColor, colBgColor));

            return bRetval;
        }

        private static bool PassesTtyFilter(string message, SledTtyFilter filter)
        {
            if (filter.Asterisks.Length <= 0)
            {
                // Direct comparison with filter string
                if (message == filter.Filter)
                    return true;
            }
            else
            {
                // Filter pattern

                // If filter string - number of asterisks is greater than the messages length it can't match
                if ((filter.Filter.Length - filter.Asterisks.Length) > message.Length)
                    return false;

                var iPos = -1;

                // Go through checking each part of the filter
                for (var i = 0; i < filter.FilterList.Count; i++)
                {
                    iPos = message.IndexOf(filter.FilterList[i], iPos + 1);

                    // Pattern wasn't found
                    if (iPos == -1)
                        return false;

                    // On first iteration check first-asterisk condition
                    if ((i == 0) && !filter.FirstAsterisk && (iPos != 0))
                        return false;

                    // On last iteration check last-asterisk condition
                    if ((i == (filter.FilterList.Count - 1)) && !filter.LastAsterisk && (iPos != (message.Length - filter.FilterList[filter.FilterList.Count - 1].Length)))
                        return false;
                }

                return true;
            }

            return false;
        }

        private void ControlSendClicked(object sender, SledTtyGui.SendClickedEventArgs e)
        {
            if (e.Plugin == null)
            {
                // Don't clear the user entered text
                e.ClearText = false;

                // Show error message
                MessageBox.Show(
                    Localization.SledTTYErrorInvalidLanguage,
                    Localization.SledTTYErrorTitle,
                    MessageBoxButtons.OK);

                return;
            }

            Write(new SledTtyMessage(SledMessageType.Info, e.Text));
            m_bShouldFlush = true;
            m_debugService.SendScmp(new DevCmd(e.Plugin.LanguageId, e.Text));
        }

        private void Flush()
        {
            try
            {
                m_control.AppendMessages(m_lstMessages);
                m_lstMessages.Clear();
            }
            finally
            {
                m_lastFlush = DateTime.Now;
                m_bShouldFlush = false;
            }
        }

        #endregion

        private bool m_bShouldFlush;
        private DateTime m_lastFlush;

        private ISledDebugService m_debugService;

        private readonly MainForm m_mainForm;
        private readonly SledTtyGui m_control;
        private readonly ICommandService m_commandService;

        private readonly StringBuilder m_builder =
            new StringBuilder();

        private readonly List<SledTtyFilter> m_lstFilters = 
            new List<SledTtyFilter>();

        private readonly List<SledTtyMessage> m_lstMessages =
            new List<SledTtyMessage>();

        private readonly static char[] s_ttyColorCodes =
            new[] { '{', '}' };

        private readonly static char[] s_ttyColorCodesSep =
            new[] { '|' };

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();

        private readonly SledServiceReference<IContextRegistry> m_contextRegistry =
            new SledServiceReference<IContextRegistry>();
    }
}
