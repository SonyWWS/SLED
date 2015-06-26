/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

namespace Sce.Sled
{
    public partial class SledSourceControlHistoryForm : Form
    {
        public SledSourceControlHistoryForm()
        {
            InitializeComponent();
        }

        public Uri Uri
        {
            set { m_txtHistory.Text = value.LocalPath; }
        }

        public void AddEntry(Entry entry)
        {
            var lstItem =
                new ListViewItem(
                    new[]
                    {
                        entry.Revision.ToString(),
                        entry.Date.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        entry.User,
                        entry.Description
                    }) {Tag = entry};

            m_lstHistory.Items.Add(lstItem);
        }

        public class Entry
        {
            public int Revision { get; set; }
            public string User { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string Client { get; set; }
            public DateTime Date { get; set; }
        }

        private void SledSourceControlHistoryFormLoad(object sender, EventArgs e)
        {
            foreach (ColumnHeader column in m_lstHistory.Columns)
                column.Width = -1;

            m_lstHistory.Columns[m_lstHistory.Columns.Count - 1].Width = -2;
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
