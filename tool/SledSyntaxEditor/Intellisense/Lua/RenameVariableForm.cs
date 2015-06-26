/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    public partial class RenameVariableForm : Form
    {
        public RenameVariableForm()
        {
            InitializeComponent();
        }

        public string InputText
        {
            set { m_txtInput.Text = value; }
            get { return m_txtInput.Text.Trim(); }
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(InputText))
                DialogResult = DialogResult.OK;
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void RenameVariableFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(InputText) && (DialogResult != DialogResult.Cancel))
                e.Cancel = true;
        }
    }
}