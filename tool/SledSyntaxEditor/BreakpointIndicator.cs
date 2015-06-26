/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor
{
    internal class BreakpointIndicator : BreakpointSpanIndicator, IBreakpoint
    {
        internal BreakpointIndicator() {}

        internal BreakpointIndicator(string name, Color foreColor, Color backColor)
            : base(name, foreColor, backColor)
        {
        }

        #region IBreakpoint Members

        /// <summary>
        /// Get starting offset of the text range for this instance.
        /// </summary>
        public int StartOffset
        {
            get { return TextRange.StartOffset; }
        }

        /// <summary>
        /// Get end offset of the text range for this instace.
        /// </summary>
        public int EndOffset
        {
            get { return TextRange.EndOffset; }
        }

        /// <summary>
        /// Gets the line number on which this instance is set.
        /// if this instance has been removed. this method will return
        /// the last known line number.
        /// </summary>
        public int LineNumber
        {
            get
            {
                if (Layer != null)
                {                   
                    foreach (DocumentLine line in Layer.Document.Lines)
                    {
                        if (line.TextRange != TextRange)
                            continue;

                        m_lineNumber = line.Index + 1;
                        break;
                    }
                }
                return m_lineNumber;
            }
        }

        /// <summary>
        /// Get the line text for this breakpoint.</summary>
        /// <remarks>Will be string.Empty if the file isn't open in the editor.</remarks>
        public string LineText
        {
            get
            {
                try
                {
                    if ((Layer == null) || (Layer.Document == null) || (Layer.Document.IsDisposed))
                        return string.Empty;

                    return Layer.Document.GetSubstring(TextRange);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// get/set whether to draw a marker on this breakpoint,
        /// this marker is used to identify conditional breakpoint from regular breakpoint.
        /// </summary>
        public bool Marker
        {
            get { return m_marker; }
            set { m_marker = value; }
        }

        #endregion

        /// <summary>
        /// Draws the glyph associated with the indicator.
        /// </summary>
        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            base.DrawGlyph(e, bounds);
            if (!m_marker)
                return;

            var lineWidth = 0.2f * bounds.Width;
            var marginW = (int)Math.Round(0.3f * bounds.Width);
            var marginH = (int)Math.Round(0.3f * bounds.Height);
            bounds.Inflate(-marginW, -marginH);
            var p = new Pen(ForeColor, lineWidth);
            e.Graphics.DrawLine(p, bounds.Left, (bounds.Top + bounds.Bottom) / 2, bounds.Right, (bounds.Top + bounds.Bottom) / 2);
            e.Graphics.DrawLine(p, (bounds.Left + bounds.Right) / 2, bounds.Top, (bounds.Left + bounds.Right) / 2, bounds.Bottom);
            p.Dispose();
        }

        /// <summary>
        /// Gets if this instance has been removed.
        /// </summary>
        public bool Removed
        {
            get { return Layer == null; }
        }

        private bool m_marker;
        private int m_lineNumber = -1;
    }
}
