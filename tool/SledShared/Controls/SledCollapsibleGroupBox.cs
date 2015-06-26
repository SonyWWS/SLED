/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// Collapsible GroupBox Control
    /// </summary>
    public class SledCollapsibleGroupBox : GroupBox
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledCollapsibleGroupBox()
        {
            m_btn =
                new Button
                {
                    Location = new Point(0, 0),
                    Size = new Size(BtnWidth, BtnHeight)
                };
            m_btn.Paint += BtnPaint;
            m_btn.Click += BtnClick;

            m_btnFormat = new StringFormat {Alignment = StringAlignment.Center};

            Controls.Add(m_btn);
        }

        /// <summary>
        /// Get or set Text property
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                // In DesignMode don't add the extra padding. It messes up things
                // when the app is run live (ie. it will have extra padding).
                base.Text =
                    DesignMode
                        ? value
                        : value.Insert(0, PaddingString);
            }
        }

        /// <summary>
        /// Expand the GroupBox
        /// </summary>
        public void Expand()
        {
            if (IsExpanded)
                return;

            // Resize GroupBox
            Height = m_iSavedHeight;

            // Show all controls in the GroupBox
            foreach (Control ctrl in Controls)
            {
                if (ctrl == m_btn)
                    continue;

                ctrl.Visible = true;
            }

            // Fire expanded event
            var handler = ExpandedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);

            // Toggle state & force redraw
            m_bExpanded = !m_bExpanded;
            m_btn.Invalidate();
        }

        /// <summary>
        /// Collapse the GroupBox
        /// </summary>
        public void Collapse()
        {
            if (IsCollapsed)
                return;

            // Hide all controls in the GroupBox
            foreach (Control ctrl in Controls)
            {
                if (ctrl == m_btn)
                    continue;

                ctrl.Visible = false;
            }

            // Resize GroupBox
            m_iSavedHeight = Height;
            m_iLastHeight = m_iSavedHeight - CollapseHeight;
            Height = CollapseHeight;

            // Fire collapsed event
            var handler = CollapsedEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);

            // Toggle state & force redraw
            m_bExpanded = !m_bExpanded;
            m_btn.Invalidate();
        }

        /// <summary>
        /// Gets whether the control is collapsed or not
        /// </summary>
        public bool IsCollapsed
        {
            get { return !m_bExpanded; }
        }

        /// <summary>
        /// Get whether the control is expanded or not
        /// </summary>
        public bool IsExpanded
        {
            get { return m_bExpanded; }
        }

        /// <summary>
        /// Get the last height of the form
        /// </summary>
        public int LastHeight
        {
            get { return m_iLastHeight; }
        }

        /// <summary>
        /// Event triggered when control is collapsed
        /// </summary>
        public event EventHandler CollapsedEvent;

        /// <summary>
        /// Event triggered when control is expanded
        /// </summary>
        public event EventHandler ExpandedEvent;

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_btn != null)
                {
                    m_btn.Dispose();
                    m_btn = null;
                }

                if (m_btnFormat != null)
                {
                    m_btnFormat.Dispose();
                    m_btnFormat = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draw plus or minus on button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Paint event arguments</param>
        private void BtnPaint(object sender, PaintEventArgs e)
        {
            using (var brush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(
                    m_bExpanded ? "-" : "+",
                    Font,
                    brush,
                    new RectangleF(0.0f, 0.0f, m_btn.Width, m_btn.Height),
                    m_btnFormat);
            }
        }

        /// <summary>
        /// Collapse or expand the GroupBox
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnClick(object sender, EventArgs e)
        {
            if (IsExpanded)
                Collapse();
            else
                Expand();
        }

        private Button m_btn;
        private StringFormat m_btnFormat;

        private bool m_bExpanded = true;
        private int m_iSavedHeight;
        private int m_iLastHeight;

        private const string PaddingString = "   ";
        private const int CollapseHeight = 24;

        private const int BtnWidth = 16;
        private const int BtnHeight = 16;
    }
}
