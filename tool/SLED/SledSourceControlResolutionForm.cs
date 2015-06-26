/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

namespace Sce.Sled
{
    public partial class SledSourceControlResolutionForm : Form
    {
        public SledSourceControlResolutionForm()
        {
            InitializeComponent();
        }

        private void BtnGetLatestClick(object sender, EventArgs e)
        {
            Choice = UserChoice.GetLatest;
            DialogResult = DialogResult.OK;
        }

        private void BtnEditCurrentClick(object sender, EventArgs e)
        {
            Choice = UserChoice.EditCurrent;
            DialogResult = DialogResult.OK;
        }

        public Uri Uri
        {
            set { m_txtFile.Text = value.LocalPath; }
        }

        public UserChoice Choice { get; private set; }

        public enum UserChoice
        {
            GetLatest,
            EditCurrent,
        }
    }
}
