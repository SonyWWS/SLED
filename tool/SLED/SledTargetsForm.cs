/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using Sce.Atf;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public partial class SledTargetsForm : Form
    {
        public SledTargetsForm(List<ISledTarget> lstTargets, ISledTarget selectedTarget)
        {
            InitializeComponent();

            m_lstView.ListViewItemSorter = new TargetSorter();

            // Add each plugin as a category
            foreach (var plugin in m_networkPluginService.Get.NetworkPlugins)
            {
                var lstItem = new ListViewItem(plugin.Protocol) { Tag = plugin };
                m_lstView.Items.Add(lstItem);
            }

            // Store list reference
            m_lstTargets = lstTargets;

            // Add list contents to ListView
            foreach (var target in m_lstTargets)
            {
                var lstItem = CreateItemFromTarget(target);

                // Check mark selected item
                if (target == selectedTarget)
                    lstItem.Checked = true;

                m_lstView.Items.Add(lstItem);
            }

            if (m_lstView.Items.Count > 0)
                AdjustColumnHeaders();

            UpdateButtonStates();
        }

        public bool TryGetSelectedTarget(out ISledTarget target)
        {
            target = null;

            foreach (ListViewItem lstItem in m_lstView.CheckedItems)
                target = lstItem.Tag as ISledTarget;

            return target != null;
        }

        private void AddNewTarget(ISledTarget target)
        {
            if (target == null)
                return;

            m_lstTargets.Add(target);

            var lstItem = CreateItemFromTarget(target);
            m_lstView.Items.Add(lstItem);

            AdjustColumnHeaders();
        }

        private void UpdateButtonStates()
        {
            var iImportedSelected = 0;
            var iProtocolsSelected = 0;

            foreach (ListViewItem lstItem in m_lstView.SelectedItems)
            {
                if (lstItem.Tag is ISledNetworkPlugin)
                    iProtocolsSelected++;

                if (!(lstItem.Tag is ISledTarget))
                    continue;

                var target = (ISledTarget)lstItem.Tag;
                if (target.Imported)
                    iImportedSelected++;
            }

            // Can only edit 1 item at a time
            m_btnEdit.Enabled =
                (iImportedSelected == 0) &&
                (iProtocolsSelected == 0) &&
                (m_lstView.SelectedItems.Count == 1);

            // Can only delete actual targets
            m_btnDelete.Enabled = (m_lstView.SelectedItems.Count - iProtocolsSelected - iImportedSelected) > 0;
        }

        private void AdjustColumnHeaders()
        {
            bool bShouldAdjust = false;

            foreach (ListViewItem lstItem in m_lstView.Items)
            {
                if (lstItem.Tag is ISledNetworkPlugin)
                    continue;

                bShouldAdjust = true;
                break;
            }

            if (!bShouldAdjust)
                return;

            // Adjust column header sizes
            foreach (ColumnHeader hdr in m_lstView.Columns)
                hdr.Width = -1;
        }

        private static ListViewItem CreateItemFromTarget(ISledTarget target)
        {
            var lstItem =
                new ListViewItem(
                    new[]
                    {
                        target.Name,
                        target.EndPoint.Address.ToString(),
                        target.EndPoint.Port.ToString()
                    }) { Tag = target };

            return lstItem;
        }

        private bool VerifyAndCheckIfWeCanCloseForm()
        {
            var bChecked = m_lstView.CheckedItems.Count > 0;
            if (bChecked)
                return true;

            if (m_lstTargets.Count <= 0)
                return true;

            // Nothing is checked & targets exist that can be checked
            DialogResult = DialogResult.None;
            MessageBox.Show(this,
                            @"You have to check a target before closing!",
                            @"Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);

            return false;
        }

        private void SledTargetsFormLoad(object sender, EventArgs e)
        {
            m_bLoaded = true;
        }

        private void BtnAddClick(object sender, EventArgs e)
        {
            using (var form = new SledTargetForm(null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    AddNewTarget(form.Target);
            }

            UpdateButtonStates();
        }

        private void BtnEditClick(object sender, EventArgs e)
        {
            var lstSelected = new List<ListViewItem>();
            foreach (ListViewItem lstItem in m_lstView.SelectedItems)
                lstSelected.Add(lstItem);

            while (lstSelected.Count > 0)
            {
                var target = lstSelected[0].Tag as ISledTarget;
                if (target != null)
                {
                    using (var form = new SledTargetForm(target))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            // If editing we're returned a completely new target
                            // so we should remove the old stuff and add the new

                            // Remove from target list
                            m_lstTargets.Remove(target);
                            // Remove from ListView
                            m_lstView.Items.Remove(lstSelected[0]);

                            // Add new
                            AddNewTarget(form.Target);
                        }
                    }
                }

                lstSelected.Remove(lstSelected[0]);
            }

            UpdateButtonStates();
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            var lstSelected = new List<ListViewItem>();
            foreach (ListViewItem lstItem in m_lstView.SelectedItems)
            {
                // Don't delete protocols
                if (lstItem.Tag is ISledNetworkPlugin)
                    continue;

                // Don't delete imported targets
                if (lstItem.Tag is ISledTarget)
                {
                    var target = (ISledTarget)lstItem.Tag;
                    if (target.Imported)
                        continue;
                }

                lstSelected.Add(lstItem);
            }

            // Go through removing items from target service's list of items
            foreach (var lstItem in lstSelected)
                m_lstTargets.Remove(lstItem.Tag as ISledTarget);

            // Go through and remove from ListView
            foreach (var lstItem in lstSelected)
                m_lstView.Items.Remove(lstItem);
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            VerifyAndCheckIfWeCanCloseForm();
        }

        private void SledTargetsFormFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !VerifyAndCheckIfWeCanCloseForm();
        }

        private void LstViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void LstViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var listView = (TargetListView)sender;
            if (!listView.Painting)
                return;

            e.DrawDefault = false;

            if (!(e.Item.Tag is ISledNetworkPlugin))
                return;

            using (var font = new Font(e.Item.Font, FontStyle.Bold))
            {
                using (var sf = new StringFormat())
                {
                    sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

                    using (var brush = new SolidBrush(listView.TextColor))
                    {
                        e.Graphics.DrawString(e.Item.Text, font, brush, e.Item.Bounds.X, e.Item.Bounds.Y, sf);
                    }
                }
            }
        }

        private void LstViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var listView = (TargetListView)sender;
            if (!listView.Painting)
                return;

            var target = e.Item.Tag as ISledTarget;
            if (target == null)
                return;

            var isLastColumn = e.ColumnIndex == (listView.Columns.Count - 1);

            // Used to compare against default drawing
            //if (!target.Imported)
            //{
            //    e.DrawDefault = true;
            //    return;
            //}

            DrawBackground(e.Graphics, e.Bounds, listView.BackColor);
            DrawGridLines(e.Graphics, e.Bounds, listView.GridLinesColor);

            if (isLastColumn)
            {
                var extraneousFauxNonClientRect =
                    new Rectangle(
                        e.Bounds.Right,
                        e.Bounds.Top,
                        Bounds.Right - e.Bounds.Right,
                        e.Bounds.Height);

                DrawBackground(e.Graphics, extraneousFauxNonClientRect, listView.BackColor);

                var extraneousFauxNonClientGridLinesRect = extraneousFauxNonClientRect;
                extraneousFauxNonClientGridLinesRect.Inflate(1, 0);

                DrawGridLines(e.Graphics, extraneousFauxNonClientGridLinesRect, listView.GridLinesColor);
            }

            // Start forming formatting flags
            var flags = TextFormatFlags.VerticalCenter;

            // Offset based on whether checkbox is drawn or not
            var iOffset = 0;

            // Draw a checkbox if first column
            if (e.ColumnIndex == 0)
            {
                var checkState =
                    e.Item.Checked
                        ? CheckBoxState.CheckedNormal
                        : CheckBoxState.UncheckedNormal;

                var glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, checkState);

                // Close to default ListView offset but not quite
                // exact. Not sure what their spacing values are.
                var iWidthOffset = (glyphSize.Width / 2) / 2;

                // Horizontal offset for creating new bounding
                // rectangle that excludes the checkbox
                iOffset = glyphSize.Width + (iWidthOffset * 2);

                // Try and center vertically within cell bounds
                var y = (e.Bounds.Height / 2) - (glyphSize.Height / 2);
                
                var pos = new Point(e.Bounds.X + iWidthOffset, e.Bounds.Y + y);

                // Draw checkbox finally
                CheckBoxRenderer.DrawCheckBox(e.Graphics, pos, checkState);
            }
            else
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }

            // Calculate new bounds based on any offset
            var newBounds =
                new Rectangle(
                    e.Bounds.X + iOffset,
                    e.Bounds.Y,
                    e.Bounds.Width - iOffset,
                    e.Bounds.Height);

            // Check if the renderer wants to draw the items
            if (target.Plugin is ISledNetworkTargetsFormRenderer)
            {
                var args =
                    new SledNetworkTargetsFormRenderArgs(
                        e.Graphics,
                        newBounds,
                        e.Item.Font,
                        e.Item.Selected,
                        target,
                        listView.TextColor,
                        listView.HighlightTextColor,
                        listView.HighlightBackColor);

                var renderer = (ISledNetworkTargetsFormRenderer)target.Plugin;
                switch (e.ColumnIndex)
                {
                    case 0: renderer.DrawName(args); break;
                    case 1: renderer.DrawHost(args); break;
                    case 2: renderer.DrawPort(args); break;
                }

                // Don't draw any more if default drawing disabled
                if (!args.DrawDefault)
                    return;
            }

            // Highlight cells if item is selected
            if (e.Item.Selected)
            {
                using (var brush = new SolidBrush(listView.HighlightBackColor))
                    e.Graphics.FillRectangle(brush, newBounds);
            }

            // Figure out text color
            var textColor =
                e.Item.Selected
                    ? listView.HighlightTextColor
                    : listView.TextColor;

            // Default font
            var font = e.Item.Font;
            
            // If imported target then italicize target name
            var bItalicized = target.Imported && (e.ColumnIndex == 0);

            // Italicize imported target name
            if (bItalicized)
                font = new Font(e.Item.Font, FontStyle.Italic);

            // Add ellipsis if text is wider than bounds
            {
                var textSize = TextRenderer.MeasureText(e.Graphics, e.SubItem.Text, font);

                if (textSize.Width > newBounds.Width)
                    flags |= TextFormatFlags.EndEllipsis;
            }

            // Draw the item text
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font, newBounds, textColor, flags);

            // Cleanup
            if (bItalicized)
                font.Dispose();
        }

        private void LstViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!m_bLoaded)
                return;

            if (m_bCheckChanging)
                return;

            var lstView = sender as ListView;
            if (lstView == null)
                return;

            // Make sure only one item is checked at a time
            // and don't allow protocols to get checked
            try
            {
                m_bCheckChanging = true;

                // Don't allow protocols to get checked
                if (e.Item.Tag is ISledNetworkPlugin)
                    return;

                // Uncheck all
                foreach (ListViewItem lstItem in lstView.CheckedItems)
                    lstItem.Checked = false;

                // Check item being changed
                e.Item.Checked = true;
            }
            finally
            {
                m_bCheckChanging = false;
            }
        }

        private void LstViewSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
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

        #region Private Classes

        private class TargetSorter : IComparer
        {
            #region IComparer Interface

            public int Compare(object x, object y)
            {
                var lstX = x as ListViewItem;
                var lstY = y as ListViewItem;

                if ((lstX == null) && (lstY == null))
                    return 0;

                if (lstX == null)
                    return -1;

                if (lstY == null)
                    return 1;

                if (ReferenceEquals(lstX, lstY))
                    return 0;

                // Compare plugins
                if ((lstX.Tag is ISledNetworkPlugin) && (lstY.Tag is ISledNetworkPlugin))
                {
                    return
                        ComparePluginToPlugin(
                            lstX.Tag as ISledNetworkPlugin,
                            lstY.Tag as ISledNetworkPlugin);
                }


                // Compare plugin to target
                if ((lstX.Tag is ISledNetworkPlugin) && (lstY.Tag is ISledTarget))
                {
                    return
                        ComparePluginToTarget(
                            lstX.Tag as ISledNetworkPlugin,
                            lstY.Tag as ISledTarget);
                }

                // Compare plugin to target
                if ((lstX.Tag is ISledTarget) && (lstY.Tag is ISledNetworkPlugin))
                {
                    return
                        -ComparePluginToTarget(
                            lstY.Tag as ISledNetworkPlugin,
                            lstX.Tag as ISledTarget);
                }

                // Compare two targets
                if ((lstX.Tag is ISledTarget) && (lstY.Tag is ISledTarget))
                {
                    return
                        CompareTargetToTarget(
                            lstX.Tag as ISledTarget,
                            lstY.Tag as ISledTarget);
                }

                return 0;
            }

            #endregion

            private static int ComparePluginToPlugin(ISledNetworkPlugin plugin1, ISledNetworkPlugin plugin2)
            {
                return
                    string.Compare(
                        plugin1.Protocol,
                        plugin2.Protocol);
            }

            private static int ComparePluginToTarget(ISledNetworkPlugin plugin, ISledTarget target)
            {
                return
                    string.Compare(
                        plugin.Protocol,
                        target.Plugin.Protocol);
            }

            private static int CompareTargetToTarget(ISledTarget target1, ISledTarget target2)
            {
                return
                    target1.Plugin == target2.Plugin
                        ? string.Compare(target1.Name, target2.Name)
                        : string.Compare(target1.Plugin.Protocol, target2.Plugin.Protocol);
            }
        }

        private class TargetListView : ListView
        {
            public TargetListView()
            {
                base.DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                // default colors & such
                BorderStyle = BorderStyle.Fixed3D;
                m_textColor = SystemColors.ControlText;
                m_highlightTextColor = SystemColors.HighlightText;
                m_highlightBackColor = ((SolidBrush)SystemBrushes.Highlight).Color;
                m_gridLinesColor = SystemColors.ControlLightLight;
            }

            public bool Painting { get; private set; }

            /// <summary>
            /// SkinService gets/sets this
            /// </summary>
            public Color TextColor
            {
                get { return m_textColor; }
                set { m_textColor = value; Invalidate(); }
            }

            /// <summary>
            /// SkinService gets/sets this
            /// </summary>
            public Color HighlightTextColor
            {
                get { return m_highlightTextColor; }
                set { m_highlightTextColor = value; Invalidate(); }
            }
            
            /// <summary>
            /// SkinService gets/sets this
            /// </summary>
            public Color HighlightBackColor
            {
                get { return m_highlightBackColor; }
                set { m_highlightBackColor = value; Invalidate(); }
            }
            
            /// <summary>
            /// SkinService gets/sets this
            /// </summary>
            public Color GridLinesColor
            {
                get { return m_gridLinesColor; }
                set { m_gridLinesColor = value; Invalidate(); }
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case User32.WM_PAINT:
                        Painting = true;
                        base.WndProc(ref m);
                        Painting = false;
                        break;

                    default: base.WndProc(ref m);  break;
                }
            }

            private Color m_textColor;
            private Color m_highlightTextColor;
            private Color m_highlightBackColor;
            private Color m_gridLinesColor;
        }

        #endregion

        private bool m_bLoaded;
        private bool m_bCheckChanging;
        private readonly List<ISledTarget> m_lstTargets;

        private readonly SledServiceReference<ISledNetworkPluginService> m_networkPluginService =
            new SledServiceReference<ISledNetworkPluginService>();
    }
}
