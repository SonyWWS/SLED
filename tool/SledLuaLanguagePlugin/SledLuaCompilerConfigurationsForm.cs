/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    public partial class SledLuaCompilerConfigurationsForm : Form
    {
        public SledLuaCompilerConfigurationsForm()
        {
            InitializeComponent();

            // Create edit TextBox
            m_txtBox =
                new TextBox {Visible = false};
            m_txtBox.KeyPress += TxtBoxKeyPress;
            m_txtBox.Leave += TxtBoxLeave;
            m_lstConfigurations.Controls.Add(m_txtBox);

            // Create edit ListBox for big/little options
            m_lstBigLittleBox = new ListBox();
            m_lstBigLittleBox.Items.Add(Big);
            m_lstBigLittleBox.Items.Add(Little);
            m_lstBigLittleBox.Visible = false;
            m_lstBigLittleBox.SelectedIndexChanged += LstBigLittleBoxSelectedIndexChanged;
            m_lstBigLittleBox.Leave += LstBigLittleBoxLeave;
            m_lstConfigurations.Controls.Add(m_lstBigLittleBox);

            // Create edit ListBox for yes/no options
            m_lstYesNoBox = new ListBox();
            m_lstYesNoBox.Items.Add(Yes);
            m_lstYesNoBox.Items.Add(No);
            m_lstYesNoBox.Visible = false;
            m_lstYesNoBox.SelectedIndexChanged += LstYesNoBoxSelectedIndexChanged;
            m_lstYesNoBox.Leave += LstYesNoBoxLeave;
            m_lstConfigurations.Controls.Add(m_lstYesNoBox);

            // Create folder browser dialog
            m_folderBrowserDlg =
                new FolderBrowserDialog
                    {
                        RootFolder = Environment.SpecialFolder.Desktop,
                        ShowNewFolderButton = true,
                        Description = "Select folder"
                    };
        }

        public void AddConfigurations(IList<SledLuaCompileConfigurationType> configurations)
        {
            m_configurations = configurations;

            foreach (var configType in m_configurations)
                m_lstConfigurations.Items.Add(CreateItem(configType));
        }

        private static ListViewItem CreateItem(SledLuaCompileConfigurationType configType)
        {
            var lstItem =
                new ListViewItem(
                    new[]
                        {
                            configType.Name,
                            configType.LittleEndian ? Little : Big,
                            configType.SizeOfInt.ToString(),
                            configType.SizeOfSizeT.ToString(),
                            configType.SizeOfLuaNumber.ToString(),
                            configType.StripDebugInfo ? Yes : No,
                            configType.OutputPath,
                            configType.OutputExtension = Bin,
                            configType.PreserveRelativePathInfo ? Yes : No
                        }) {Tag = configType, Checked = configType.Selected};

            return lstItem;
        }

        private bool VerifyAndCheckIfWeCanCloseForm()
        {
            var bChecked = m_lstConfigurations.CheckedItems.Count > 0;
            if (bChecked)
                return true;

            if (m_configurations.Count <= 0)
                return true;

            // Nothing is checked & configurations exist that can be checked
            DialogResult = DialogResult.None;
            MessageBox.Show(this, "You have to select/check a configuration before closing!",
                            Localization.SledLuaCompiler, MessageBoxButtons.OK, MessageBoxIcon.Stop);

            return false;
        }

        #region Events

        private void SledLuaCompilerConfigurationsFormLoad(object sender, EventArgs e)
        {
            m_bLoaded = true;
        }

        private void BtnAddClick(object sender, EventArgs e)
        {
            var node =
                new DomNode(SledLuaSchema.SledLuaCompileConfigurationType.Type);

            // Create new configuration with default values
            var configType =
                node.As<SledLuaCompileConfigurationType>();

            configType.Name = NewConfiguration;
            configType.LittleEndian = true;
            configType.SizeOfInt = 4;
            configType.SizeOfSizeT = 4;
            configType.SizeOfLuaNumber = 8;
            configType.StripDebugInfo = false;
            configType.OutputPath = m_projectService.Get.AssetDirectory;
            configType.OutputExtension = Bin;
            configType.PreserveRelativePathInfo = true;

            configType.Selected = false;

            // If first configuration then check it or
            // if no other items are checked then check it
            if (m_lstConfigurations.Items.Count <= 0)
                configType.Selected = true;
            else if (m_lstConfigurations.CheckedItems.Count <= 0)
                configType.Selected = true;

            // Add to list of configurations
            m_configurations.Add(configType);

            // Add to ListView
            m_bCheckChanging = true;
            m_lstConfigurations.Items.Add(CreateItem(configType));
            m_bCheckChanging = false;
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            if (m_lstConfigurations.SelectedItems.Count <= 0)
                return;

            var lstItem = m_lstConfigurations.SelectedItems[0];
            if (lstItem == null)
                return;

            var iPos = lstItem.Index;

            var configType = 
                lstItem.Tag as SledLuaCompileConfigurationType;
            
            if (configType == null)
                return;

            // Remove from list of configurations
            m_configurations.Remove(configType);

            // Remove from ListView
            m_lstConfigurations.Items.Remove(lstItem);
            
            if (m_lstConfigurations.Items.Count <= 0)
                return;

            // Try and select an item if there are still items on the list
            if (iPos >= m_lstConfigurations.Items.Count)
                iPos = m_lstConfigurations.Items.Count - 1;

            m_lstConfigurations.Items[iPos].Selected = true;

            // Check last remaining item
            if (m_lstConfigurations.Items.Count == 1)
            {
                m_bCheckChanging = true;
                m_lstConfigurations.Items[0].Checked = true;
                m_bCheckChanging = false;
            }
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void LstConfigurationsSelectedIndexChanged(object sender, EventArgs e)
        {
            var bItemSelected = (m_lstConfigurations.SelectedItems.Count != 0);

            m_btnDelete.Enabled = bItemSelected;
        }

        private void LstConfigurationsBeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            var lstView = sender as ListView;
            if (lstView == null)
                return;

            e.CancelEdit = true;

            // Get mouse position in client coordinates
            var clientPoint = lstView.PointToClient(MousePosition);

            // Find out which item was actually clicked
            var lstItem = lstView.GetItemAt(clientPoint.X, clientPoint.Y);
            if (lstItem == null)
                return;

            // Find out which sub-item was actually clicked
            var lstSubItem = 
                lstItem.GetSubItemAt(clientPoint.X, clientPoint.Y);

            // Find column number
            var iColumn = 0;
            if (lstItem.Bounds != lstSubItem.Bounds)
                iColumn = lstItem.SubItems.IndexOf(lstSubItem);

            // Set editing control
            Control ctrl = null;
            switch (iColumn)
            {
                case 1: ctrl = m_lstBigLittleBox; break;
                case 5:
                case 8: ctrl = m_lstYesNoBox; break;
                case 6:
                {
                    var configType =
                        lstItem.Tag as SledLuaCompileConfigurationType;

                    if (configType == null)
                        return;

                    m_folderBrowserDlg.SelectedPath = configType.OutputPath;
                    if (m_folderBrowserDlg.ShowDialog(this) == DialogResult.OK)
                    {
                        var path = m_folderBrowserDlg.SelectedPath;
                        configType.OutputPath = path;
                        lstSubItem.Text = path;
                    }
                }
                break;
                default: ctrl = m_txtBox; break;
            }

            if (ctrl == null)
                return;

            // Set up editing control
            ctrl.Tag = lstSubItem;
            ctrl.Location = lstSubItem.Bounds.Location;
            ctrl.Width = lstSubItem.Bounds.Width;
            if (ctrl is TextBox)
                ctrl.Height = lstSubItem.Bounds.Height;
            ctrl.Text = lstSubItem.Text;
            ctrl.Visible = true;
            ctrl.BringToFront();
            ctrl.Focus();
        }

        private void LstConfigurationsItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!m_bLoaded)
                return;

            if (m_bCheckChanging)
                return;

            try
            {
                m_bCheckChanging = true;

                // Uncheck all
                foreach (ListViewItem lstItem in m_lstConfigurations.CheckedItems)
                    lstItem.Checked = false;

                // Un-select all
                foreach (var ct in m_configurations)
                    ct.Selected = false;

                // Check item being changed
                e.Item.Checked = true;

                var configType = 
                    e.Item.Tag as SledLuaCompileConfigurationType;

                if (configType == null)
                    return;

                // Select item being checked
                configType.Selected = true;
            }
            finally
            {
                m_bCheckChanging = false;
            }
        }

        private void TxtBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            var lstSubItem =
                m_txtBox.Tag as ListViewItem.ListViewSubItem;

            if (lstSubItem == null)
                return;

            if (e.KeyChar == (char)Keys.Return)
            {
                try
                {
                    var value = m_txtBox.Text.Trim();
                    var origValue = lstSubItem.Text.Trim();

                    // Find the ListViewItem the sub-item belongs to
                    var lstItem =
                        m_lstConfigurations.GetItemAt(lstSubItem.Bounds.X, lstSubItem.Bounds.Y);

                    if (lstItem == null)
                        return;

                    // Find column number
                    var iColumn = 0;
                    if (lstItem.Bounds != lstSubItem.Bounds)
                        iColumn = lstItem.SubItems.IndexOf(lstSubItem);

                    // Sync to DOM object
                    var configType =
                        lstItem.Tag as SledLuaCompileConfigurationType;

                    if (configType == null)
                        return;

                    var bSuccessfulValueChange = false;

                    switch (iColumn)
                    {
                        // Name
                        case 0:
                            configType.Name = value;
                            bSuccessfulValueChange = true;
                            break;

                        // Endian
                        case 1:
                            //  Big/Little not handled here
                            break;

                        // sizeof(int)
                        case 2: 
                        {
                            int iValue;
                            if (int.TryParse(value, out iValue))
                            {
                                // Make sure iValue is one of the valid values
                                if (m_lstValidSizeofIntValues.Contains(iValue))
                                {
                                    configType.SizeOfInt = iValue;
                                    bSuccessfulValueChange = true;
                                }
                            }

                            // Show tooltip if invalid values
                            if (!bSuccessfulValueChange)
                                ShowListViewSubItemToolTip(
                                    ListToString(m_lstValidSizeofIntValues), lstSubItem);
                        }
                        break;

                        // sizeof(size_t)
                        case 3: 
                        {
                            int iValue;
                            if (int.TryParse(value, out iValue))
                            {
                                // Make sure iValue is one of the valid values
                                if (m_lstValidSizeofSizeTValues.Contains(iValue))
                                {
                                    configType.SizeOfSizeT = iValue;
                                    bSuccessfulValueChange = true;
                                }
                            }

                            // Show tooltip if invalid values
                            if (!bSuccessfulValueChange)
                                ShowListViewSubItemToolTip(
                                    ListToString(m_lstValidSizeofSizeTValues), lstSubItem);
                        }
                        break;

                        // sizeof(lua_Number)
                        case 4:
                        {
                            int iValue;
                            if (int.TryParse(value, out iValue))
                            {
                                // Make sure iValue is one of the valid values
                                if (m_lstValidSizeofLuaNumberValues.Contains(iValue))
                                {
                                    configType.SizeOfLuaNumber = iValue;
                                    bSuccessfulValueChange = true;
                                }
                            }

                            // Show tooltip if invalid values
                            if (!bSuccessfulValueChange)
                                ShowListViewSubItemToolTip(
                                    ListToString(m_lstValidSizeofLuaNumberValues), lstSubItem);
                        }
                        break;

                        // Strip Debug Info
                        case 5:
                            // Yes/No not handled here
                            break;

                        // Output Path
                        case 6:
                            // Not handled here
                            break;

                        // Output Extension
                        case 7:
                        {
                            if (value.StartsWith("."))
                                value = value.Remove(0, 1);

                            configType.OutputExtension = value;
                            bSuccessfulValueChange = true;
                        }
                        break;
                    }

                    // Update item text
                    lstSubItem.Text = bSuccessfulValueChange ? value : origValue;
                }
                finally
                {
                    // Hide box
                    m_txtBox.Visible = false;
                    m_txtBox.Tag = null;
                    e.Handled = true;
                }
            }
        }

        private void TxtBoxLeave(object sender, EventArgs e)
        {
            // Hide TextBox
            m_txtBox.Visible = false;
            m_txtBox.Tag = null;
        }

        private void LstBigLittleBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_lstBigLittleBox.Visible)
                return;

            var lstSubItem =
                m_lstBigLittleBox.Tag as ListViewItem.ListViewSubItem;

            if (lstSubItem == null)
                return;

            try
            {
                var value = m_lstBigLittleBox.Text;

                // Find the ListViewItem the sub-item belongs to
                var lstItem =
                    m_lstConfigurations.GetItemAt(lstSubItem.Bounds.X, lstSubItem.Bounds.Y);

                if (lstItem == null)
                    return;

                var configType =
                    lstItem.Tag as SledLuaCompileConfigurationType;

                if (configType == null)
                    return;

                // Sync to DOM object
                configType.LittleEndian = string.Compare(value, Little, StringComparison.Ordinal) == 0;
                
                // Update ListView
                lstSubItem.Text = value;
            }
            finally
            {
                m_lstBigLittleBox.Visible = false;
                m_lstBigLittleBox.Tag = null;
            }
        }

        private void LstBigLittleBoxLeave(object sender, EventArgs e)
        {
            // Hide ListBox
            m_lstBigLittleBox.Visible = false;
            m_lstBigLittleBox.Tag = null;
        }

        private void LstYesNoBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_lstYesNoBox.Visible)
                return;

            var lstSubItem =
                m_lstYesNoBox.Tag as ListViewItem.ListViewSubItem;

            if (lstSubItem == null)
                return;

            try
            {
                var value = m_lstYesNoBox.Text;

                // Find the ListViewItem the sub-item belongs to
                var lstItem =
                    m_lstConfigurations.GetItemAt(lstSubItem.Bounds.X, lstSubItem.Bounds.Y);

                if (lstItem == null)
                    return;

                // Find column number
                var iColumn = 0;
                if (lstItem.Bounds != lstSubItem.Bounds)
                    iColumn = lstItem.SubItems.IndexOf(lstSubItem);

                var configType =
                    lstItem.Tag as SledLuaCompileConfigurationType;

                if (configType == null)
                    return;

                // Sync to DOM object
                switch (iColumn)
                {
                    case 5:
                        configType.StripDebugInfo = string.Compare(value, Yes, StringComparison.Ordinal) == 0;
                        break;

                    case 8:
                        configType.PreserveRelativePathInfo = string.Compare(value, Yes, StringComparison.Ordinal) == 0;
                        break;
                }

                // Update ListView
                lstSubItem.Text = value;
            }
            finally
            {
                m_lstYesNoBox.Visible = false;
                m_lstYesNoBox.Tag = null;
            }
        }

        private void LstYesNoBoxLeave(object sender, EventArgs e)
        {
            // Hide ListBox
            m_lstYesNoBox.Visible = false;
            m_lstYesNoBox.Tag = null;
        }

        private void SledLuaCompilerConfigurationsFormFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !VerifyAndCheckIfWeCanCloseForm();
        }

        #endregion

        private static string ListToString(IEnumerable<int> lstValues)
        {
            var sb = new StringBuilder();

            var bFirst = true;
            foreach (var value in lstValues)
            {
                if (!bFirst)
                    sb.Append(", ");

                sb.Append(value);

                if (bFirst)
                    bFirst = false;
            }

            return sb.ToString();
        }

        private void ShowListViewSubItemToolTip(string validValues, ListViewItem.ListViewSubItem lstSubItem)
        {
            var point =
                new Point(lstSubItem.Bounds.Right - 16, lstSubItem.Bounds.Top - 40);

            var toolTip =
                new ToolTip {IsBalloon = true};
            toolTip.Show("Valid values: " + validValues, m_lstConfigurations, point, 5000);
        }

        private IList<SledLuaCompileConfigurationType> m_configurations;

        private bool m_bLoaded;
        private bool m_bCheckChanging;

        private TextBox m_txtBox;
        private ListBox m_lstBigLittleBox;
        private ListBox m_lstYesNoBox;
        private FolderBrowserDialog m_folderBrowserDlg;

        private readonly List<int> m_lstValidSizeofIntValues =
            new List<int>(new[] { 2, 4, 8 }); // short/int/__int64

        private readonly List<int> m_lstValidSizeofSizeTValues =
            new List<int>(new[] { 2, 4, 8 }); // short/int/__int64

        private readonly List<int> m_lstValidSizeofLuaNumberValues =
            new List<int>(new[] { 4, 8 }); // float/double

        private const string NewConfiguration = "New configuration";
        private const string Bin = "bin";
        private const string Little = "Little";
        private const string Big = "Big";
        private const string Yes = "Yes";
        private const string No = "No";

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();
    }
}
