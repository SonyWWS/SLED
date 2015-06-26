/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public partial class SledProjectModifiedForm : Form
    {
        public SledProjectModifiedForm()
        {
            InitializeComponent();
            GridLines = true;

            m_lstChanges.BorderStyle = BorderStyle.Fixed3D;
            m_lstChanges.ForeColor = SystemColors.ControlText;
            m_lstChanges.BackColor = SystemColors.ControlLightLight;
            m_highlightTextColor = SystemColors.HighlightText;
            m_highlightBackColor = ((SolidBrush)SystemBrushes.Highlight).Color;
            m_categoryTextColor = SystemColors.ControlText;
            m_gridLinesColor = DefaultBackColor;

            m_lstChanges.ListViewItemSorter = new ChangeWrapperSorter();

            // Add categories
            AddCategoryHeader("Pending", UserCategory.Pending);
            AddCategoryHeader("Accepted", UserCategory.Accepted);
            AddCategoryHeader("Ignored", UserCategory.Ignored);

            UpdateButtonStates();
        }

        public new void Show()
        {
            if (!Visible)
                base.Show();
            else
                Activate();
        }

        public new void Show(IWin32Window owner)
        {
            if (!Visible)
                base.Show(owner);
            else
                Activate();
        }

        public void ReportChanges(ICollection<SledModifiedProjectChange> changes)
        {
            //
            // If there are no changes already reported we can accept them all wholesale
            //
            // If there are changes reported it means the user has made repetitive changes outside of SLED to
            // the project and we need to do some additional processing. Specifically, we need to see if
            // any changes that were previously made have now been reverted. For instance, the user could
            // have removed a file then exported (which would make this dialog pop up with a file removal notice),
            // then added the file back and exported (which would mean the file removal is no longer valid).
            //

            //
            // Example reversions:
            // 
            // If there was no guid change this update but there's a guid item on the list then we need to
            // remove it as the change was reverted.
            //
            // If there was no asset dir change this update but there's an asset dir item on the list then
            // we need to remove it as the change was reverted.
            //
            // etc.
            //

            lock (m_lock)
            {
                var existingChanges = new List<SledModifiedProjectChange>(Changes);
                var bAnyExistingChanges = existingChanges.Count > 0;

                try
                {
                    m_lstChanges.BeginUpdate();

                    if (!bAnyExistingChanges)
                    {
                        //
                        // If no existing items simply add everything
                        //

                        foreach (var change in changes)
                            ReportNewOrUpdateExistingChange(change, null);
                    }
                    else
                    {
                        //
                        // Previous changes have been made and therefore
                        // we require more processing of these latest changes
                        //

                        var bReportedName = false;
                        var existingName = FindWrapperByChangeType(SledModifiedProjectChangeType.Name);

                        var bReportedGuid = false;
                        var existingGuid = FindWrapperByChangeType(SledModifiedProjectChangeType.Guid);

                        var bReportedAssetDir = false;
                        var existingAssetDir = FindWrapperByChangeType(SledModifiedProjectChangeType.AssetDir);

                        var lstExistingFileAdditions = new List<ChangeWrapper>(FindWrappersByChangeType(SledModifiedProjectChangeType.FileAdded));
                        var lstFileAdditionsUseThisIteration = new List<ChangeWrapper>();

                        var lstExistingFileRemovals = new List<ChangeWrapper>(FindWrappersByChangeType(SledModifiedProjectChangeType.FileRemoved));
                        var lstFileRemovalsUsedThisIteration = new List<ChangeWrapper>();

                        foreach (var change in changes)
                        {
                            if (change.ChangeType == SledModifiedProjectChangeType.Name)
                            {
                                bReportedName = true;
                                ReportNewOrUpdateExistingChange(change, existingName);
                            }

                            if (change.ChangeType == SledModifiedProjectChangeType.Guid)
                            {
                                bReportedGuid = true;
                                ReportNewOrUpdateExistingChange(change, existingGuid);
                            }

                            if (change.ChangeType == SledModifiedProjectChangeType.AssetDir)
                            {
                                bReportedAssetDir = true;
                                ReportNewOrUpdateExistingChange(change, existingAssetDir);
                            }

                            if (change.ChangeType == SledModifiedProjectChangeType.FileAdded)
                            {
                                var fileThisAdded = ((SledModifiedProjectFileAddedChange)change).AbsolutePath;

                                var existingFileAdded =
                                    lstExistingFileAdditions.Find(
                                        delegate(ChangeWrapper existingFileTemp)
                                        {
                                            var changeExistingFileAdded = (SledModifiedProjectFileAddedChange)existingFileTemp.Change;

                                            var iResult = string.Compare(fileThisAdded, changeExistingFileAdded.AbsolutePath, true);
                                            return iResult == 0;
                                        });

                                if (existingFileAdded != null)
                                {
                                    if (!lstFileAdditionsUseThisIteration.Contains(existingFileAdded))
                                        lstFileAdditionsUseThisIteration.Add(existingFileAdded);
                                }

                                ReportNewOrUpdateExistingChange(change, existingFileAdded);
                            }

                            if (change.ChangeType == SledModifiedProjectChangeType.FileRemoved)
                            {
                                var fileThisRemoved = ((SledModifiedProjectFileRemovedChange)change).AbsolutePath;

                                var existingFileRemoved =
                                    lstExistingFileRemovals.Find(
                                        delegate(ChangeWrapper existingFileTemp)
                                        {
                                            var changeExistingFileRemoved = (SledModifiedProjectFileRemovedChange)existingFileTemp.Change;

                                            var iResult = string.Compare(fileThisRemoved, changeExistingFileRemoved.AbsolutePath, true);
                                            return iResult == 0;
                                        });

                                if (existingFileRemoved != null)
                                {
                                    if (!lstFileRemovalsUsedThisIteration.Contains(existingFileRemoved))
                                        lstFileRemovalsUsedThisIteration.Add(existingFileRemoved);
                                }

                                ReportNewOrUpdateExistingChange(change, existingFileRemoved);
                            }
                        }

                        // Check if name got reverted
                        if (!bReportedName && (existingName != null))
                            m_lstChanges.Items.Remove(existingName.Item);

                        // Check if guid got reverted
                        if (!bReportedGuid && (existingGuid != null))
                            m_lstChanges.Items.Remove(existingGuid.Item);

                        // Check if asset dir got reverted
                        if (!bReportedAssetDir && (existingAssetDir != null))
                            m_lstChanges.Items.Remove(existingAssetDir.Item);

                        // Check file addition reversions
                        {
                            foreach (var wrapperExistingFileAddition in lstExistingFileAdditions)
                            {
                                if (lstFileAdditionsUseThisIteration.Contains(wrapperExistingFileAddition))
                                    continue;

                                if (wrapperExistingFileAddition.Item != null)
                                    m_lstChanges.Items.Remove(wrapperExistingFileAddition.Item);
                            }
                        }

                        // Check file removal reversions
                        {
                            foreach (var wrapperExistingFileRemoval in lstExistingFileRemovals)
                            {
                                if (lstFileRemovalsUsedThisIteration.Contains(wrapperExistingFileRemoval))
                                    continue;

                                if (wrapperExistingFileRemoval.Item != null)
                                    m_lstChanges.Items.Remove(wrapperExistingFileRemoval.Item);
                            }
                        }
                    }
                }
                finally
                {
                    m_lstChanges.EndUpdate();

                    // Adjust column header width
                    if (m_lstChanges.Items.Count > m_iCategoryCount)
                        m_lstChanges.Columns[0].Width = -1;

                    UpdateButtonStates();
                }
            }
        }

        public IEnumerable<SledModifiedProjectChange> Changes
        {
            get
            {
                lock (m_lock)
                {
                    foreach (ListViewItem lstItem in m_lstChanges.Items)
                    {
                        if (lstItem.Tag == null)
                            continue;

                        if (!(lstItem.Tag is ChangeWrapper))
                            continue;

                        var wrapper = lstItem.Tag as ChangeWrapper;

                        yield return wrapper.Change;
                    }
                }
            }
        }

        public event EventHandler<SledModifiedProjectChangesEventArgs> ChangesSubmitted;

        /// <summary>
        /// Gets or sets the text color</summary>
        public Color TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; m_lstChanges.Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the highlight text color</summary>
        public Color HighlightTextColor
        {
            get { return m_highlightTextColor; }
            set { m_highlightTextColor = value; m_lstChanges.Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the highlight background color</summary>
        public Color HighlightBackColor
        {
            get { return m_highlightBackColor; }
            set { m_highlightBackColor = value; m_lstChanges.Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the category text color</summary>
        public Color CategoryTextColor
        {
            get { return m_categoryTextColor; }
            set { m_categoryTextColor = value; m_lstChanges.Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the grid lines color</summary>
        public Color GridLinesColor
        {
            get { return m_gridLinesColor; }
            set { m_gridLinesColor = value; m_lstChanges.Invalidate(); }
        }

        /// <summary>
        /// Gets or sets whether grid lines are visible</summary>
        public bool GridLines
        {
            get { return m_gridLines; }
            set { m_gridLines = value; m_lstChanges.Invalidate(); }
        }

        #region Form Events

        private void SledProjectModifiedFormFormClosing(object sender, FormClosingEventArgs e)
        {
            lock (m_lock)
            {
                e.Cancel = true;

                if ((m_lstChanges.Items.Count - m_iCategoryCount) <= 0)
                {
                    Hide();
                }
                else
                {
                    MessageBox.Show(
                        this,
                        @"There are still pending changes! You must accept or " +
                        @"or ignore them and then click submit before continuing.",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                }
            }
        }

        private void LstChangesSelectedIndexChanged(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                UpdateButtonStates();
            }
        }

        private void LstChangesDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void LstChangesDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;

            if (e.Item.Tag is UserCategory)
                DrawCategoryHeader(e, CategoryTextColor);
        }

        private void LstChangesDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = false;

            var wrapper = e.Item.Tag as ChangeWrapper;
            if (wrapper == null)
                return;

            var isLastColumn = (e.ColumnIndex == (m_lstChanges.Columns.Count - 1));
            var isLastVisibleItem = e.ItemIndex == m_lstChanges.Items.Count - 1;

            DrawBackground(e.Graphics, e.Bounds, BackColor);

            if (GridLines)
                DrawGridLines(e.Graphics, e.Bounds, GridLinesColor);

            if (isLastColumn)
            {
                var extraneousFauxNonClientRect =
                    new Rectangle(
                        e.Bounds.Right,
                        e.Bounds.Top,
                        Bounds.Right - e.Bounds.Right,
                        e.Bounds.Height);

                DrawBackground(e.Graphics, extraneousFauxNonClientRect, BackColor);

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

            // Start formatting flags
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

            var textColor = e.Item.Selected ? HighlightTextColor : TextColor;

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, textColor, flags);
        }

        private void LstChangesKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.A) && (e.Modifiers == Keys.Control))
            {
                foreach (ListViewItem lstItem in m_lstChanges.Items)
                {
                    if (lstItem.Tag == null)
                        continue;

                    if (lstItem.Tag is UserCategory)
                        continue;

                    if (!(lstItem.Tag is ChangeWrapper))
                        continue;

                    if (!lstItem.Selected)
                        lstItem.Selected = true;
                }
            }

            if ((e.KeyCode == Keys.Delete) ||
                (e.KeyCode == Keys.Space))
            {
                var bAnyChanges = false;

                var category =
                    e.KeyCode == Keys.Delete
                        ? UserCategory.Ignored
                        : UserCategory.Accepted;

                try
                {
                    foreach (ListViewItem lstItem in m_lstChanges.SelectedItems)
                    {
                        if (lstItem.Tag == null)
                            continue;

                        if (lstItem.Tag is UserCategory)
                            continue;

                        if (!(lstItem.Tag is ChangeWrapper))
                            continue;

                        var wrapper =
                            lstItem.Tag as ChangeWrapper;

                        if (wrapper.Category == category)
                            continue;

                        wrapper.Category = category;

                        if (!bAnyChanges)
                            bAnyChanges = true;
                    }
                }
                finally
                {
                    if (bAnyChanges)
                        m_lstChanges.Sort();
                }
            }
        }

        private void BtnAcceptAllClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                ChangeAllListViewItemsToCategory(UserCategory.Accepted);
                UpdateButtonStates();
            }
        }

        private void BtnAcceptSelectedClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                ChangeSelectedListViewItemsToCategory(UserCategory.Accepted);
                UpdateButtonStates();
            }
        }

        private void BtnIgnoreSelectedClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                ChangeSelectedListViewItemsToCategory(UserCategory.Ignored);
                UpdateButtonStates();
            }
        }

        private void BtnIgnoreAllClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                ChangeAllListViewItemsToCategory(UserCategory.Ignored);
                UpdateButtonStates();
            }
        }

        private void BtnSubmitClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                var lstAccepted = new List<SledModifiedProjectChange>();
                var lstIgnored = new List<SledModifiedProjectChange>();
                var lstRemoveItems = new List<ListViewItem>();

                foreach (ListViewItem lstItem in m_lstChanges.Items)
                {
                    if (lstItem.Tag == null)
                        continue;

                    if (lstItem.Tag is UserCategory)
                        continue;

                    if (!(lstItem.Tag is ChangeWrapper))
                        continue;

                    var wrapper = lstItem.Tag as ChangeWrapper;

                    if ((wrapper.Category != UserCategory.Accepted) &&
                        (wrapper.Category != UserCategory.Ignored))
                        continue;

                    // Mark for deletion
                    lstRemoveItems.Add(lstItem);

                    // Add to proper list
                    if (wrapper.Category == UserCategory.Accepted)
                        lstAccepted.Add(wrapper.Change);
                    else
                        lstIgnored.Add(wrapper.Change);
                }

                // Fire event
                ChangesSubmitted.Raise(this, new SledModifiedProjectChangesEventArgs(lstAccepted, lstIgnored));

                // Remove all submitted changes
                m_lstChanges.BeginUpdate();
                foreach (var lstItem in lstRemoveItems)
                    m_lstChanges.Items.Remove(lstItem);
                m_lstChanges.EndUpdate();

                // Update
                UpdateButtonStates();

                // Stop showing now that no more items exist
                if ((m_lstChanges.Items.Count - m_iCategoryCount) <= 0)
                    Hide();
            }
        }

        #endregion

        #region Member Methods

        private void AddCategoryHeader(string header, UserCategory category)
        {
            var lstItem = new ListViewItem(header) {Tag = category};

            m_iCategoryCount++;

            m_lstChanges.Items.Add(lstItem);
        }

        private void ReportNewOrUpdateExistingChange(SledModifiedProjectChange change, ChangeWrapper existing)
        {
            var wrapper = new ChangeWrapper(change) { Category = UserCategory.Pending };

            // Default category

            if (existing == null)
            {
                // Create node representing change
                var lstItem = new ListViewItem(wrapper.Change.ToString()) { Tag = wrapper };

                // Set references
                wrapper.Item = lstItem;

                // Add to list
                m_lstChanges.Items.Add(lstItem);
            }
            else
            {
                // Update existing item
                existing.Update(change);
            }
        }

        private ChangeWrapper FindWrapperByChangeType(SledModifiedProjectChangeType changeType)
        {
            lock (m_lock)
            {
                return
                    (from ListViewItem lstItem in m_lstChanges.Items
                     where lstItem.Tag != null
                     where (lstItem.Tag is ChangeWrapper)
                     select lstItem.Tag as ChangeWrapper).FirstOrDefault(itemWrapper => itemWrapper.Change.ChangeType == changeType);
            }
        }

        private IEnumerable<ChangeWrapper> FindWrappersByChangeType(SledModifiedProjectChangeType changeType)
        {
            lock (m_lock)
            {
                foreach (ListViewItem lstItem in m_lstChanges.Items)
                {
                    if (lstItem.Tag == null)
                        continue;

                    if (!(lstItem.Tag is ChangeWrapper))
                        continue;

                    var itemWrapper = lstItem.Tag as ChangeWrapper;

                    if (itemWrapper.Change.ChangeType != changeType)
                        continue;

                    yield return itemWrapper;
                }
            }
        }

        private void ChangeAllListViewItemsToCategory(UserCategory category)
        {
            lock (m_lock)
            {
                var bChangedAnyItems = false;

                foreach (ListViewItem lstItem in m_lstChanges.Items)
                {
                    if (lstItem.Tag == null)
                        continue;

                    if (lstItem.Tag is UserCategory)
                        continue;

                    if (!(lstItem.Tag is ChangeWrapper))
                        continue;

                    var wrapper = lstItem.Tag as ChangeWrapper;

                    if (wrapper.Category == category)
                        continue;

                    wrapper.Category = category;

                    if (!bChangedAnyItems)
                        bChangedAnyItems = true;
                }

                if (bChangedAnyItems)
                    m_lstChanges.Sort();
            }
        }

        private void ChangeSelectedListViewItemsToCategory(UserCategory category)
        {
            lock (m_lock)
            {
                var bChangedAnyItems = false;

                foreach (ListViewItem lstItem in m_lstChanges.SelectedItems)
                {
                    if (lstItem.Tag == null)
                        continue;

                    if (lstItem.Tag is UserCategory)
                        continue;

                    if (!(lstItem.Tag is ChangeWrapper))
                        continue;

                    var wrapper = lstItem.Tag as ChangeWrapper;

                    if (wrapper.Category == category)
                        continue;

                    wrapper.Category = category;

                    if (!bChangedAnyItems)
                        bChangedAnyItems = true;
                }

                if (bChangedAnyItems)
                    m_lstChanges.Sort();
            }
        }

        private void UpdateButtonStates()
        {
            var iPendingCount = 0;
            var iAcceptedCount = 0;
            var iIgnoredCount = 0;
            var iCountSelected = 0;

            foreach (ListViewItem lstItem in m_lstChanges.Items)
            {
                if (lstItem.Tag == null)
                    continue;

                if (lstItem.Tag is UserCategory)
                    continue;

                if (!(lstItem.Tag is ChangeWrapper))
                    continue;

                var wrapper = lstItem.Tag as ChangeWrapper;

                if (wrapper.Category == UserCategory.Pending)
                    iPendingCount++;

                if (wrapper.Category == UserCategory.Accepted)
                    iAcceptedCount++;

                if (wrapper.Category == UserCategory.Ignored)
                    iIgnoredCount++;

                if (lstItem.Selected)
                    iCountSelected++;
            }

            m_btnAcceptAll.Enabled = (m_lstChanges.Items.Count - m_iCategoryCount) > 0;
            m_btnAcceptSelected.Enabled = ((iIgnoredCount > 0) || (iPendingCount > 0)) && (iCountSelected > 0);
            m_btnIgnoreSelected.Enabled = ((iAcceptedCount > 0) || (iPendingCount > 0)) && (iCountSelected > 0);
            m_btnIgnoreAll.Enabled = (m_lstChanges.Items.Count - m_iCategoryCount) > 0;
            m_btnSubmit.Enabled = (iAcceptedCount > 0) || (iIgnoredCount > 0);
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

        private static void DrawCategoryHeader(DrawListViewItemEventArgs e, Color color)
        {
            using (var font = new Font(e.Item.Font, FontStyle.Bold))
            {
                using (var sf = new StringFormat())
                {
                    sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

                    using (var brush = new SolidBrush(color))
                    {
                        e.Graphics.DrawString(e.Item.Text, font, brush, e.Item.Bounds.X, e.Item.Bounds.Y, sf);
                    }
                }
            }
        }

        #endregion

        #region Private Classes

        private enum UserCategory
        {
            Pending = 1,
            Accepted = 2,
            Ignored = 3,
        }

        private class ChangeWrapper
        {
            public ChangeWrapper(SledModifiedProjectChange change)
            {
                Change = change;
            }

            public void Update(SledModifiedProjectChange change)
            {
                Change = change;

                if (Item != null)
                    Item.Text = change.ToString();
            }

            public ListViewItem Item;
            public UserCategory Category;
            public SledModifiedProjectChange Change;
        }

        private class ChangeWrapperSorter : IComparer
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

                // Compare categories
                if ((lstX.Tag is UserCategory) && (lstY.Tag is UserCategory))
                {
                    return CompareCategories((UserCategory)lstX.Tag, (UserCategory)lstY.Tag);
                }

                // Compare category to wrapper
                if ((lstX.Tag is UserCategory) && (lstY.Tag is ChangeWrapper))
                {
                    return CompareCategoryToChangeWrapper((UserCategory)lstX.Tag, (ChangeWrapper)lstY.Tag);
                }

                // Compare wrapper to category
                if ((lstX.Tag is ChangeWrapper) && (lstY.Tag is UserCategory))
                {
                    return CompareCategoryToChangeWrapper((UserCategory)lstY.Tag, (ChangeWrapper)lstX.Tag) * -1;
                }

                // Compare wrapper to wrapper
                if ((lstX.Tag is ChangeWrapper) && (lstY.Tag is ChangeWrapper))
                {
                    return CompareChangeWrapperToChangeWrapper((ChangeWrapper)lstX.Tag, (ChangeWrapper)lstY.Tag);
                }

                return 0;
            }

            #endregion

            private static int CompareCategories(UserCategory cat1, UserCategory cat2)
            {
                if (cat1 == cat2)
                    return 0;

                return cat1 < cat2 ? -1 : 1;
            }

            private static int CompareCategoryToChangeWrapper(UserCategory cat, ChangeWrapper wrapper)
            {
                if (cat == wrapper.Category)
                    return -1;

                return CompareCategories(cat, wrapper.Category);
            }

            private static int CompareChangeWrapperToChangeWrapper(ChangeWrapper wrapper1, ChangeWrapper wrapper2)
            {
                // Check categories
                if (wrapper1.Category != wrapper2.Category)
                    return CompareCategories(wrapper1.Category, wrapper2.Category);

                // Compare actual changes
                if (wrapper1.Change.ChangeType == wrapper2.Change.ChangeType)
                {
                    var retval = 0;

                    switch (wrapper1.Change.ChangeType)
                    {
                        case SledModifiedProjectChangeType.FileAdded:
                            retval =
                                CompareFileAddedChanges(
                                    wrapper1.Change as SledModifiedProjectFileAddedChange,
                                    wrapper2.Change as SledModifiedProjectFileAddedChange);
                            break;

                        case SledModifiedProjectChangeType.FileRemoved:
                            retval =
                                CompareFileRemovedChanges(
                                    wrapper1.Change as SledModifiedProjectFileRemovedChange,
                                    wrapper2.Change as SledModifiedProjectFileRemovedChange);
                            break;
                    }

                    return retval;
                }
                
                return wrapper1.Change.ChangeType < wrapper2.Change.ChangeType ? -1 : 1;
            }

            private static int CompareFileAddedChanges(SledModifiedProjectFileAddedChange change1, SledModifiedProjectFileAddedChange change2)
            {
                return string.Compare(change1.AbsolutePath, change2.AbsolutePath);
            }

            private static int CompareFileRemovedChanges(SledModifiedProjectFileRemovedChange change1, SledModifiedProjectFileRemovedChange change2)
            {
                return string.Compare(change1.AbsolutePath, change2.AbsolutePath);
            }
        }

        #endregion

        private int m_iCategoryCount;
        private bool m_gridLines;

        private Color m_textColor;
        private Color m_highlightTextColor;
        private Color m_highlightBackColor;
        private Color m_categoryTextColor;
        private Color m_gridLinesColor;

        private volatile object m_lock = new object();
    }
}
