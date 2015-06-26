/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    public partial class SledChangeAssetDirectoryForm : Form
    {
        public SledChangeAssetDirectoryForm()
        {
            InitializeComponent();
        }

        public string OldAssetDirectory
        {
            get { return m_assetDirOld; }
            set { m_assetDirOld = value; }
        }

        public string NewAssetDirectory
        {
            get { return m_assetDirNew; }
            set { m_assetDirNew = value; }
        }

        public ICollection<SledProjectFilesFileType> Files
        {
            set { m_files = value; }
        }

        public bool Option1
        {
            get { return m_rdoOption1.Checked; }
        }

        private void SledChangeAssetDirectoryForm_Load(object sender, EventArgs e)
        {
            UpdatePaths(true);
        }

        private void RdoOption1_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePaths(m_rdoOption1.Checked);
        }

        private void UpdatePaths(bool option1)
        {
            m_lstFiles.Items.Clear();
            m_lstFiles.BeginUpdate();

            foreach (var file in m_files)
            {
                string pathNew;

                if (option1)
                {
                    var absPath = 
                        SledUtil.GetAbsolutePath(file.Path, m_assetDirOld);

                    pathNew = 
                        SledUtil.GetRelativePath(absPath, m_assetDirNew);
                }
                else
                {
                    var absPath =
                        m_assetDirNew + file.Path;

                    pathNew =
                        SledUtil.GetRelativePath(absPath, m_assetDirNew);
                }

                var lstItem = 
                    new ListViewItem(
                        new[] { file.Name, file.Path, pathNew });

                m_lstFiles.Items.Add(lstItem);
            }

            m_lstFiles.EndUpdate();

            foreach (ColumnHeader hdr in m_lstFiles.Columns)
                hdr.Width = -1;
        }

        private string m_assetDirOld;
        private string m_assetDirNew;

        private ICollection<SledProjectFilesFileType> m_files;
    }
}
