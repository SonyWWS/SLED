/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    sealed class SledTtyListView : ListView
    {
        public SledTtyListView()
        {
            OwnerDraw = true;

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MouseMove += SledTtyListViewMouseMove;
            DrawColumnHeader += SledTtyListViewDrawColumnHeader;
            DrawItem += SledTtyListViewDrawItem;
            DrawSubItem += SledTtyListViewDrawSubItem;

            BorderStyle = BorderStyle.Fixed3D;
            m_textColor = SystemColors.ControlText;
            BackColor = SystemColors.ControlLightLight;
            m_highlightTextColor = SystemColors.HighlightText;
            m_highlightBackColor = ((SolidBrush)SystemBrushes.Highlight).Color;
            m_gridLinesColor = DefaultBackColor;
        }

        public void ResetWorkaroundList()
        {
            m_lstWorkaround.Clear();
        }

        public Color TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; Invalidate(); }
        }

        public Color HighlightTextColor
        {
            get { return m_highlightTextColor; }
            set { m_highlightTextColor = value; Invalidate(); }
        }

        public Color HighlightBackColor
        {
            get { return m_highlightBackColor; }
            set { m_highlightBackColor = value; Invalidate(); }
        }

        public Color GridLinesColor
        {
            get { return m_gridLinesColor; }
            set { m_gridLinesColor = value; Invalidate(); }
        }

        public new bool GridLines
        {
            get { return m_gridLines; }
            set { m_gridLines = value; Invalidate(); }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case User32.WM_PAINT:
                {
                    try
                    {
                        m_painting = true;
                        base.WndProc(ref m);

                        if (VirtualListSize == 0)
                        {
                            using (Graphics gfx = CreateGraphics())
                                DrawBackground(gfx, Bounds, BackColor);
                        }
                    }
                    finally
                    {
                        m_painting = false;
                    }
                }
                break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawColumnHeader -= SledTtyListViewDrawColumnHeader;
                DrawItem -= SledTtyListViewDrawItem;
                DrawSubItem -= SledTtyListViewDrawSubItem;
            }

            base.Dispose(disposing);
        }

        private void SledTtyListViewMouseMove(object sender, MouseEventArgs e)
        {
            // Taken from MSDN for an issue with the underlying control
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.listview.ownerdraw(v=VS.85).aspx

            var lstItem = GetItemAt(e.X, e.Y);
            if (lstItem == null)
                return;

            // Only invalidate once
            if (m_lstWorkaround.Contains(lstItem))
                return;

            m_lstWorkaround.Add(lstItem);
            Invalidate(lstItem.Bounds);
        }

        private void SledTtyListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void SledTtyListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // All drawing performed in SledTtyListViewDrawSubItem
        }

        private void SledTtyListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (!m_painting)
                return;

            // Grab message details
            var message = e.Item.Tag.As<SledTtyMessage>();
            if (message == null)
                return;

            var isLastColumn = (e.ColumnIndex == (Columns.Count - 1));
            var isLastVisibleItem = e.Item.Index == (VirtualListSize - 1);

            DrawBackground(e.Graphics, e.Bounds, message.BackColor);

            if (GridLines)
                DrawGridLines(e.Graphics, e.Bounds, GridLinesColor);

            // special stuff when drawing last column... we draw past the column so that:
            // 1) if the control gets disabled, then things don't look awful
            // 2) if gridlines are enabled, then fake them out to infinity
            if (isLastColumn)
            {
                var extraneousFauxNonClientRect =
                    new Rectangle(
                        e.Bounds.Right,
                        e.Bounds.Top,
                        Bounds.Right - e.Bounds.Right,
                        e.Bounds.Height);

                DrawBackground(e.Graphics, extraneousFauxNonClientRect, message.BackColor);

                if (isLastVisibleItem)
                {
                    var rect =
                        new Rectangle(
                            e.Bounds.Right,
                            e.Bounds.Y + e.Bounds.Height,
                            Bounds.Right - e.Bounds.Right,
                            Bounds.Bottom - e.Bounds.Bottom);

                    DrawBackground(e.Graphics, rect, BackColor);
                }

                // continue grid lines out to 'infinity' horizontally
                if (GridLines)
                {
                    var extraneousFauxNonClientGridLinesRect = extraneousFauxNonClientRect;
                    extraneousFauxNonClientGridLinesRect.Inflate(1, 0);

                    DrawGridLines(e.Graphics, extraneousFauxNonClientGridLinesRect, GridLinesColor);
                }
            }

            var flags = TextFormatFlags.VerticalCenter;

            // Add ellipsis if text is wider than bounds
            {
                var textSize = TextRenderer.MeasureText(e.Graphics, e.SubItem.Text, e.Item.Font);

                if (textSize.Width > e.Bounds.Width)
                    flags |= TextFormatFlags.EndEllipsis;
            }

            // Highlight cells if item is selected
            if (e.Item.Selected)
            {
                using (Brush brush = new SolidBrush(HighlightBackColor))
                    e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var textColor =
                e.Item.Selected
                    ? HighlightTextColor
                    : message.TextColor;

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, textColor, flags);
        }

        private static void DrawBackground(Graphics gfx, Rectangle bounds, Color color)
        {
            using (var brush = new SolidBrush(color))
                gfx.FillRectangle(brush, bounds);
        }

        private static void DrawGridLines(Graphics gfx, Rectangle bounds, Color color)
        {
            using (var p = new Pen(color))
                gfx.DrawRectangle(p, bounds);
        }

        private bool m_painting;
        private bool m_gridLines;

        private Color m_textColor;
        private Color m_highlightTextColor;
        private Color m_highlightBackColor;
        private Color m_gridLinesColor;

        private readonly List<ListViewItem> m_lstWorkaround =
            new List<ListViewItem>();
    }
}