/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    public partial class SledTtyFilterNameForm : Form
    {
        /// <summary>
        /// Constructo
        /// </summary>
        public SledTtyFilterNameForm()
        {
            TextColor = Color.Black;
            BackgroundColor = Color.White;

            InitializeComponent();

            m_txtRect = SetupRectangle(m_btnTxtColor.ClientRectangle);
            m_bgRect = SetupRectangle(m_btnBgColor.ClientRectangle);

            m_btnTxtColor.Paint += BtnTxtColor_Paint;
            m_btnBgColor.Paint += BtnBgColor_Paint;
        }

        /// <summary>
        /// Create a smaller rectangle based off an input rectangle
        /// </summary>
        /// <param name="clientRect"></param>
        /// <returns>rectangle</returns>
        private static Rectangle SetupRectangle(Rectangle clientRect)
        {
            var rect =
                new Rectangle(
                    clientRect.Location,
                    clientRect.Size);

            // Decrease width & height
            rect.Inflate(-40, -5);
            
            // Move colored part to the right
            rect.Offset(30, 0);

            return rect;
        }

        /// <summary>
        /// Gets/sets the filter text
        /// </summary>
        public string FilterName
        {
            get { return m_txtName.Text; }
            set { m_txtName.Text = value; }
        }

        /// <summary>
        /// Gets/sets the text color
        /// </summary>
        public Color TextColor
        {
            get { return m_txtColor; }
            set
            {
                m_txtColor = value;

                if (m_txtBrush != null)
                    m_txtBrush.Dispose();

                m_txtBrush = new SolidBrush(m_txtColor);
            }
        }

        /// <summary>
        /// Gets/sets the bg color
        /// </summary>
        public Color BackgroundColor
        {
            get { return m_bgColor; }
            set
            {
                m_bgColor = value;

                if (m_bgBrush != null)
                    m_bgBrush.Dispose();

                m_bgBrush = new SolidBrush(m_bgColor);
            }
        }

        /// <summary>
        /// Gets whether the filter will show or ignore text
        /// </summary>
        public SledTtyFilterResult FilterResult
        {
            get
            {
                return
                    m_rdoColorText.Checked
                        ? SledTtyFilterResult.Show
                        : SledTtyFilterResult.Ignore;
            }

            set
            {
                if (value == SledTtyFilterResult.Show)
                    m_rdoColorText.Checked = true;
                else
                {
                    m_rdoColorText.Checked = false;
                    m_rdoIgnoreText.Checked = true;
                }
            }
        }

        /// <summary>
        /// Validate text box when clicking the "OK" button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnOk_Click(object sender, EventArgs e)
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

                    foreach (var t in asterisks)
                    {
                        if (t == (iLast + 1))
                            szText = szText.Remove(t - iOffset++, 1);

                        iLast = t;
                    }
                }

                // Update text if it differs
                if (string.Compare(szText, m_txtName.Text) != 0)
                    m_txtName.Text = szText;
            }
        }

        /// <summary>
        /// Custom paint the text color button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnTxtColor_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, m_txtRect.X - 1, m_txtRect.Y - 1, m_txtRect.Width + 1, m_txtRect.Height + 1);
            e.Graphics.FillRectangle(m_txtBrush, m_txtRect);
        }

        /// <summary>
        /// Custom paint the bg color button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnBgColor_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, m_bgRect.X - 1, m_bgRect.Y - 1, m_bgRect.Width + 1, m_bgRect.Height + 1);
            e.Graphics.FillRectangle(m_bgBrush, m_bgRect);
        }

        /// <summary>
        /// Event for clicking the text color button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnTxtColor_Click(object sender, EventArgs e)
        {
            var dlg =
                new ColorDialog {Color = m_txtColor};
            if (dlg.ShowDialog(this) == DialogResult.OK)
                TextColor = dlg.Color;
        }

        /// <summary>
        /// Event for clicking the bg color buton
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnBgColor_Click(object sender, EventArgs e)
        {
            var dlg =
                new ColorDialog {Color = m_bgColor};
            if (dlg.ShowDialog(this) == DialogResult.OK)
                BackgroundColor = dlg.Color;
        }

        private void RdoColorText_CheckedChanged(object sender, EventArgs e)
        {
            if (m_rdoColorText.Checked)
            {
                m_btnTxtColor.Enabled = true;
                m_btnBgColor.Enabled = true;
            }
            else
            {
                m_btnTxtColor.Enabled = false;
                m_btnBgColor.Enabled = false;
            }
        }

        private Color m_txtColor;
        private Color m_bgColor;
        private SolidBrush m_txtBrush;
        private SolidBrush m_bgBrush;
        private Rectangle m_txtRect;
        private Rectangle m_bgRect;
    }
}
