/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Sled.Shared.Document;

namespace Sce.Sled
{
    public partial class SledReadOnlyDocResolutionForm : Form
    {
        public SledReadOnlyDocResolutionForm()
        {
            InitializeComponent();
            FormClosing += SledReadOnlyDocResolutionForm_FormClosing;
        }

        private void SledReadOnlyDocResolutionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bCanClose)
                e.Cancel = true;
        }

        public ISledDocument SledDocument
        {
            get { return m_sd; }
            set
            {
                if (m_sd != null)
                    return;

                m_sd = value;
                m_txtFile.Text = m_sd.Uri.LocalPath;
            }
        }

        private ISledDocument m_sd;
        private bool m_bCanClose;

        public event EventHandler<SledReadOnlyDocResolutionFormEventArgs> TryOptionEvent;

        private void BtnRemoveReadOnly_Click(object sender, EventArgs e)
        {
            ProcessButtonClick(sender, SledReadOnlyDocResolutionFormOptions.RemoveReadOnly);
        }

        private void BtnRemoveReadOnlyAndSave_Click(object sender, EventArgs e)
        {
            ProcessButtonClick(sender, SledReadOnlyDocResolutionFormOptions.RemoveReadOnlyAndSave);
        }

        private void BtnUndoChanges_Click(object sender, EventArgs e)
        {
            ProcessButtonClick(sender, SledReadOnlyDocResolutionFormOptions.UndoChanges);
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            ProcessButtonClick(sender, SledReadOnlyDocResolutionFormOptions.SaveAs);

            // Re-enable - always allow this option
            var btn = (Button)sender;
            btn.Enabled = true;
        }

        private void ProcessButtonClick(object sender, SledReadOnlyDocResolutionFormOptions option)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            var ea =
                new SledReadOnlyDocResolutionFormEventArgs(m_sd, option);

            TryOptionEvent.Raise(this, ea);

            if (!ea.Result)
                return;

            m_bCanClose = true;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class SledReadOnlyDocResolutionFormEventArgs : EventArgs
    {
        public SledReadOnlyDocResolutionFormEventArgs(ISledDocument sd, SledReadOnlyDocResolutionFormOptions option)
        {
            m_sd = sd;
            m_option = option;
            Result = false;
        }

        public ISledDocument SledDocument
        {
            get { return m_sd; }
        }

        public SledReadOnlyDocResolutionFormOptions Option
        {
            get { return m_option; }
        }

        public bool Result { get; set; }

        private readonly ISledDocument m_sd;
        private readonly SledReadOnlyDocResolutionFormOptions m_option;
    }

    public enum SledReadOnlyDocResolutionFormOptions
    {
        RemoveReadOnly,
        RemoveReadOnlyAndSave,
        UndoChanges,
        SaveAs,
    }
}
