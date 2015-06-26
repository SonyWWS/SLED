/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Project
{
    public partial class SledProjectOpenFile : Form
    {
        public SledProjectOpenFile()
        {
            InitializeComponent();

            m_comparer.TheComparer = CompareFiles;

            m_lstFiles.Sorting = SortOrder.Ascending;
            m_lstFiles.ListViewItemSorter = new ProjectFilesComparer();
        }

        public IEnumerable<SledProjectFilesFileType> Files
        {
            set
            {
                m_listViewItems.Clear();

                var newValues = value.ToList();
                newValues.Sort(m_comparer);

                foreach (var file in newValues)
                {
                    var time = GetDateTime(file);
                    var lstItem =
                        new ListViewItem(
                            new[]
                            {
                                file.Name,
                                file.AbsolutePath,
                                time.ToString()
                            }) { Tag = file };

                    m_listViewItems.Add(lstItem);
                }

                UpdateFileList();
            }
        }

        public IEnumerable<SledProjectFilesFileType> SelectedFiles
        {
            get
            {
                return
                    from ListViewItem item in m_lstFiles.SelectedItems
                    select (SledProjectFilesFileType)item.Tag;
            }
        }

        private void UpdateFileList()
        {
            try
            {
                m_lstFiles.BeginUpdate();
                m_lstFiles.Items.Clear();

                var text = m_txtFile.Text;
                var nullOrEmpty = string.IsNullOrEmpty(text);

                if (nullOrEmpty)
                    UpdateFileListAddEverything(m_lstFiles, m_listViewItems);
                else
                    UpdateFileListFiltered(m_lstFiles, m_listViewItems, text);
            }
            finally
            {
                m_lstFiles.EndUpdate();
            }
        }

        private void SledProjectOpenFileLoad(object sender, EventArgs e)
        {
            foreach (ColumnHeader header in m_lstFiles.Columns)
                header.Width = -1;

            if (m_lstFiles.Items.Count > 0)
                m_lstFiles.Columns[m_lstFiles.Columns.Count - 1].Width = -2;
        }

        private void TxtFileTextChanged(object sender, EventArgs e)
        {
            UpdateFileList();
        }

        private void TxtFileKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                    UserChangeSelection(m_lstFiles, e.KeyCode == Keys.Up);
                    break;
            }
        }

        private static void UpdateFileListAddEverything(ListView listView, IEnumerable<ListViewItem> items)
        {
            var first = true;
            foreach (var item in items)
            {
                item.Selected = false;

                listView.Items.Add(item);

                if (!first)
                    continue;

                item.Selected = true;
                first = false;
            }
        }

        private static void UpdateFileListFiltered(ListView listView, IEnumerable<ListViewItem> items, string filter)
        {
            var selection = new Pair<ListViewItem, int>();

            foreach (var item in items)
            {
                item.Selected = false;

                var filterPos =
                    item.Text.IndexOf(
                        filter,
                        StringComparison.CurrentCultureIgnoreCase);

                if (filterPos < 0)
                    continue;
                
                if (selection.First == null)
                    selection = new Pair<ListViewItem, int>(item, filterPos);
                else if ((filterPos == 0) && (selection.Second != 0))
                    selection = new Pair<ListViewItem, int>(item, filterPos);

                listView.Items.Add(item);
            }

            if (selection.First == null)
                return;

            selection.First.Selected = true;
            listView.EnsureVisible(selection.First.Index);
        }

        private static void UserChangeSelection(ListView listView, bool moveUp)
        {
            if (listView.Items.Count <= 1)
                return;

            int indexToSelect;

            {
                var indices = listView.SelectedIndices.Cast<int>();
                indexToSelect = moveUp ? indices.Min() - 1 : indices.Max() + 1;
            }

            if ((indexToSelect < 0) || (indexToSelect >= listView.Items.Count))
                return;

            try
            {
                listView.BeginUpdate();

                foreach (ListViewItem item in listView.SelectedItems)
                    item.Selected = false;

                listView.Items[indexToSelect].Selected = true;
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private static DateTime GetDateTime(SledProjectFilesFileType file)
        {
            try
            {
                var dateTime = File.GetLastWriteTime(file.AbsolutePath);
                return dateTime;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[SledProjectOpenFile] Exception " + 
                    "obtaining file info for \"{0}\": {1}",
                    file.AbsolutePath, ex.Message);

                return DateTime.Now;
            }
        }

        private static int CompareFiles(SledProjectFilesFileType file1, SledProjectFilesFileType file2)
        {
            return string.Compare(file1.Name, file2.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        private class ProjectFilesComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if ((x == null) && (y == null))
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                if (ReferenceEquals(x, y))
                    return 0;

                var lstItemX = (ListViewItem)x;
                var lstItemY = (ListViewItem)y;

                var fileX = (SledProjectFilesFileType)lstItemX.Tag;
                var fileY = (SledProjectFilesFileType)lstItemY.Tag;

                return CompareFiles(fileX, fileY);
            }
        }

        private readonly List<ListViewItem> m_listViewItems =
            new List<ListViewItem>();

        private readonly SledProjectFilesFileType.Comparer m_comparer =
            new SledProjectFilesFileType.Comparer();
    }
}
