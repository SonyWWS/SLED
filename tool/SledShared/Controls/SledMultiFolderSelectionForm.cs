/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// SledMultiFolderSelectionForm Class
    /// </summary>
    public partial class SledMultiFolderSelectionForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledMultiFolderSelectionForm()
        {
            InitializeComponent();

            // Make it fill the list
            m_lstFolders.Columns[0].Width = -2;
        }

        /// <summary>
        /// Gets/sets folders to use
        /// </summary>
        public List<string> Folders
        {
            get
            {
                return
                    (from ListViewItem item in m_lstFolders.Items
                     select item.Text).ToList();
            }
            
            set
            {
                m_lstFolders.Items.Clear();
                foreach (var folder in value)
                    m_lstFolders.Items.Add(folder);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var dialog =
                new FolderBrowserDialog
                    {
                        Description = "Select a folder",
                        RootFolder = Environment.SpecialFolder.Desktop,
                        ShowNewFolderButton = false
                    };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var folder = dialog.SelectedPath;

                // Check for duplicates
                var bDuplicate = false;
                foreach (ListViewItem item in m_lstFolders.Items)
                {
                    if (string.Compare(folder, item.Text, StringComparison.OrdinalIgnoreCase) != 0)
                        continue;

                    bDuplicate = true;
                    break;
                }

                // Add item to ListView
                if (!bDuplicate)
                    m_lstFolders.Items.Add(folder);
            }
            dialog.Dispose();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (m_lstFolders.SelectedItems.Count <= 0)
                return;

            var iIndex = m_lstFolders.SelectedItems[0].Index;

            foreach (ListViewItem item in m_lstFolders.SelectedItems)
                m_lstFolders.Items.Remove(item);

            iIndex = SledUtil.Clamp(iIndex, 0, m_lstFolders.Items.Count - 1);

            if (m_lstFolders.Items.Count <= 0)
                return;

            m_lstFolders.Items[iIndex].Selected = true;
            m_lstFolders.Focus();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (m_lstFolders.Items.Count <= 0)
                DialogResult = DialogResult.None;
        }
    }
}
