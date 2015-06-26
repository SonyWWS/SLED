/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Sled.Project.Resources;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Project
{
    /// <summary>
    /// Form for viewing the current files in the project and their relative path with respect to the asset directory
    /// </summary>
    public partial class SledViewProjectPathsForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledViewProjectPathsForm()
        {
            InitializeComponent();

            m_lstView.Columns.Add(Localization.SledFileNoAmpersand);
            m_lstView.Columns.Add(Localization.SledPath);
        }

        /// <summary>
        /// Gets/sets the project name
        /// </summary>
        public string ProjectName
        {
            get { return m_txtProjectName.Text; }
            set { m_txtProjectName.Text = value; }
        }

        /// <summary>
        /// Gets/sets the project Guid
        /// </summary>
        public string ProjectGuid
        {
            get { return m_txtProjectGuid.Text; }
            set { m_txtProjectGuid.Text = value; }
        }

        /// <summary>
        /// Gets/sets the project directory
        /// </summary>
        public string ProjectDirectory
        {
            get { return m_txtProjectDirectory.Text; }
            set { m_txtProjectDirectory.Text = value; }
        }

        /// <summary>
        /// Gets/sets the asset directory
        /// </summary>
        public string AssetDirectory
        {
            get { return m_txtAssetDirectory.Text; }
            set { m_txtAssetDirectory.Text = value; }
        }

        /// <summary>
        /// Gets/sets the project files
        /// </summary>
        public ICollection<SledProjectFilesFileType> ProjectFiles
        {
            get { return m_projectFiles; }
            set
            {
                m_lstView.Items.Clear();
                m_projectFiles.Clear();

                foreach (var file in value)
                {
                    var lstItem = new ListViewItem(new[] { file.Name, file.Path });
                    m_lstView.Items.Add(lstItem);

                    m_projectFiles.Add(file);
                }

                m_lstView.Columns[0].Width = -1;
                m_lstView.Columns[1].Width = -2;
            }
        }

        private readonly List<SledProjectFilesFileType> m_projectFiles = new List<SledProjectFilesFileType>();
    }
}
