/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public partial class SledTtyGui : UserControl, IInitializable
    {
        public SledTtyGui()
        {
            InitializeComponent();

            var none = new ComboBoxItem(null);

            m_cmbLanguages.Items.Add(none);
            m_cmbLanguages.SelectedItem = none;
        }

        public void Initialize()
        {
            BuildControl();

            var mainForm = SledServiceInstance.TryGet<MainForm>();
            mainForm.Shown += MainFormShown;
            
            SkinService.ApplyActiveSkin(SledTtyMessageColorer.Instance);
            SkinService.SkinChangedOrApplied += SkinServiceSkinChangedOrApplied;
        }

        private void MainFormShown(object sender, EventArgs e)
        {
            if (m_splitterDistance == 0)
                return;

            try
            {
                m_split.SplitterDistance = m_splitterDistance;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "SledTtyGui: Exception restoring splitter distance: {0}",
                    ex.Message);
            }
        }

        public string[] ColumnNames
        {
            get { return m_columnNames; }
            set { m_columnNames = value; }
        }

        public IEnumerable<SledTtyMessage> Selection
        {
            get
            {
                if (SelectionCount == 0)
                    yield break;

                foreach (int index in m_lstOutput.SelectedIndices)
                {
                    yield return m_messages[index];
                }
            }
        }

        public int SelectionCount
        {
            get { return m_lstOutput.SelectedIndices.Count; }
        }

        public string Settings
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                var root = xmlDoc.CreateElement("Columns");
                xmlDoc.AppendChild(root);

                // Save column widths
                foreach (var kv in m_columnWidths)
                {
                    var columnElement = xmlDoc.CreateElement("Column");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", kv.Key);
                    columnElement.SetAttribute("Width", kv.Value.ToString());
                }

                // Save splitter position
                var splitterElement = xmlDoc.CreateElement("Splitter");
                splitterElement.SetAttribute("Distance", m_split.SplitterDistance.ToString());
                root.AppendChild(splitterElement);

                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                var root = xmlDoc.DocumentElement;
                if ((root == null) || (root.Name != "Columns"))
                    throw new Exception("Invalid SledTtyGui settings");

                var columns = root.SelectNodes("Column");
                if (columns != null)
                {
                    foreach (XmlElement columnElement in columns)
                    {
                        var name = columnElement.GetAttribute("Name");
                        var widthString = columnElement.GetAttribute("Width");
                        int width;
                        if (!string.IsNullOrEmpty(widthString) && int.TryParse(widthString, out width))
                            m_columnWidths[name] = width;
                    }
                }

                var splitter = root.SelectNodes("Splitter");
                if (splitter != null)
                {
                    foreach (XmlElement splitterElement in splitter)
                    {
                        var distanceString = splitterElement.GetAttribute("Distance");
                        int distance;
                        if (!string.IsNullOrEmpty(distanceString) && int.TryParse(distanceString, out distance))
                            m_splitterDistance = distance;

                        break;
                    }
                }

                if (columns == null)
                    return;

                m_lstOutput.SuspendLayout();

                foreach (ColumnHeader column in m_lstOutput.Columns)
                    SetColumnWidth(column);

                m_lstOutput.ResumeLayout();
            }
        }

        public void Clear()
        {
            try
            {
                m_lstOutput.VirtualListSize = 0;
                m_lstOutput.ResetWorkaroundList();
            }
            finally
            {
                m_messages.Clear();
            }
        }

        public bool SendEnabled
        {
            get { return m_btnSend.Enabled; }
            set { m_btnSend.Enabled = value; }
        }

        public void RegisterLanguage(ISledLanguagePlugin language)
        {
            var item = new ComboBoxItem(language);
            m_cmbLanguages.Items.Add(item);
            m_cmbLanguages.SelectedItem = item;
        }

        public void AppendMessages(List<SledTtyMessage> messages)
        {
            if (messages.Count <= 0)
                return;

            m_messages.AddRange(messages);
            m_lstOutput.VirtualListSize = m_messages.Count;
            m_lstOutput.EnsureVisible(m_messages.Count - 1);
        }

        public event EventHandler<SendClickedEventArgs> SendClicked;

        private void BuildControl()
        {
            if (m_bBuiltControl)
                return;

            try
            {
                m_lstOutput.Columns.Clear();
                foreach (var name in m_columnNames)
                {
                    var column = new ColumnHeader {Text = name};
                    SetColumnWidth(column);
                    m_lstOutput.Columns.Add(column);
                }
            }
            finally
            {
                m_bBuiltControl = false;
            }
        }

        private void SetColumnWidth(ColumnHeader column)
        {
            int width;
            if (m_columnWidths.TryGetValue(column.Text, out width))
                column.Width = width;
            else
                m_columnWidths[column.Text] = column.Width;
        }

        private void LstOutputColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            var column = m_lstOutput.Columns[e.ColumnIndex];
            m_columnWidths[column.Text] = column.Width;
        }

        private void LstOutputRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var message = m_messages[e.ItemIndex];

            // Show time with milliseconds included
            var time = message.Time.ToString("hh:mm:ss.fff");
            var lstItem = new ListViewItem(time) {Tag = message};

            lstItem.SubItems.Add(message.Message);
            e.Item = lstItem;
        }

        private void TxtInputKeyUp(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = false;
            e.Handled = false;

            if (!e.Control || (e.KeyValue != 13))
                return;

            if (!SendEnabled)
                return;

            if (StringUtil.IsNullOrEmptyOrWhitespace(m_txtInput.Text))
                return;

            m_btnSend.PerformClick();
            e.Handled = true;
        }

        private void BtnSendClick(object sender, EventArgs e)
        {
            var text = m_txtInput.Text;

            if (string.IsNullOrEmpty(text))
                return;

            var item = m_cmbLanguages.SelectedItem as ComboBoxItem;
            if (item == null)
                return;

            var ea = new SendClickedEventArgs(item.Plugin, text);
            SendClicked.Raise(this, ea);

            if (ea.ClearText)
                m_txtInput.Text = string.Empty;
        }

        private void SkinServiceSkinChangedOrApplied(object sender, EventArgs e)
        {
            SkinService.ApplyActiveSkin(SledTtyMessageColorer.Instance);
            m_lstOutput.Invalidate(true);
        }

        private bool m_bBuiltControl;
        private string[] m_columnNames;
        private int m_splitterDistance;

        private readonly List<SledTtyMessage> m_messages =
            new List<SledTtyMessage>();

        private readonly Dictionary<string, int> m_columnWidths =
            new Dictionary<string, int>();

        #region Private Classes

        private class ComboBoxItem
        {
            public ComboBoxItem(ISledLanguagePlugin plugin)
            {
                Plugin = plugin;
            }

            public override string ToString()
            {
                return Plugin == null ? Localization.SledNone : Plugin.LanguageName;
            }

            public readonly ISledLanguagePlugin Plugin;
        }

        #endregion

        #region Public Classes

        public class SendClickedEventArgs : EventArgs
        {
            public SendClickedEventArgs(ISledLanguagePlugin plugin, string text)
            {
                Plugin = plugin;
                Text = text;
                ClearText = true;
            }

            public readonly ISledLanguagePlugin Plugin;
            public readonly string Text;
            public bool ClearText;
        }
        #endregion
    }
}
