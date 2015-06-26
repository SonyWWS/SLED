/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    public partial class SledLuaVariableFilterVarNameForm : Form
    {
        public SledLuaVariableFilterVarNameForm()
        {
            InitializeComponent();
        }

        public string VarName
        {
            get { return m_txtName.Text; }
            set { m_txtName.Text = value; }
        }

        /// <summary>
        /// Validate text box when clicking the "OK" button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnOkClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_txtName.Text))
                DialogResult = DialogResult.None;
            else
            {
                // Copy text over
                var szText = m_txtName.Text;

                // Find all occurences of *
                var asterisks = SledUtil.IndicesOf(szText, '*');

                // Verify no back to back asterisks
                if (asterisks.Length > 0)
                {
                    var iLast = -5;
                    var iOffset = 0;

                    for (var i = 0; i < asterisks.Length; i++)
                    {
                        if (asterisks[i] == (iLast + 1))
                            szText = szText.Remove(asterisks[i] - iOffset++, 1);

                        iLast = asterisks[i];
                    }
                }

                // Update text if it differs
                if (string.Compare(szText, m_txtName.Text, StringComparison.Ordinal) != 0)
                    m_txtName.Text = szText;
            }            
        }
    }
}
