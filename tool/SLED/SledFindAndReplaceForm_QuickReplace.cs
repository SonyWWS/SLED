/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Controls;

namespace Sce.Sled
{
    partial class SledFindAndReplaceForm_QuickReplace : UserControl, ISledFindAndReplaceSubForm
    {
        public SledFindAndReplaceForm_QuickReplace()
        {
            InitializeComponent();

            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInCurrentDocument, SledFindAndReplaceLookIn.CurrentDocument));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInAllOpenDocuments, SledFindAndReplaceLookIn.AllOpenDocuments));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInCurrentProject, SledFindAndReplaceLookIn.CurrentProject));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInEntireSolution, SledFindAndReplaceLookIn.EntireSolution));

            m_cmbUse.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceUsePatternRegularExpressions, SledFindAndReplaceSearchType.RegularExpressions));
            m_cmbUse.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceUsePatternWildCards, SledFindAndReplaceSearchType.WildCards));

            m_grpFindOptions.ExpandedEvent += SledCollapsibleGroupBoxExpandedEvent;
            m_grpFindOptions.CollapsedEvent += SledCollapsibleGroupBoxCollapsedEvent;

            m_cmbFindWhat.Items.AddRange(SledFindAndReplaceSettings.GlobalFindWhat.Items);
            m_cmbReplaceWith.Items.AddRange(SledFindAndReplaceSettings.GlobalReplaceWith.Items);
            if (m_cmbLookIn.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.QuickReplace.LookInIndex >= 0) &&
                    (SledFindAndReplaceSettings.QuickReplace.LookInIndex < m_cmbLookIn.Items.Count))
                    m_cmbLookIn.SelectedIndex = SledFindAndReplaceSettings.QuickReplace.LookInIndex;
                else
                    m_cmbLookIn.SelectedItem = m_cmbLookIn.Items[0];
            }
            m_chkMatchCase.Checked = SledFindAndReplaceSettings.QuickReplace.MatchCaseChecked;
            m_chkMatchWholeWord.Checked = SledFindAndReplaceSettings.QuickReplace.MatchWholeWordChecked;
            m_chkSearchUp.Checked = SledFindAndReplaceSettings.QuickReplace.SearchUpChecked;
            m_chkUse.Checked = SledFindAndReplaceSettings.QuickReplace.UseChecked;
            if (m_cmbUse.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.QuickReplace.UseIndex >= 0) &&
                    (SledFindAndReplaceSettings.QuickReplace.UseIndex < m_cmbUse.Items.Count))
                    m_cmbUse.SelectedIndex = SledFindAndReplaceSettings.QuickReplace.UseIndex;
                else
                    m_cmbUse.SelectedItem = m_cmbUse.Items[0];
            }
        }

        #region ISledFindAndReplaceSubForm

        public Control Control
        {
            get { return this; }
        }

        public string InitialText
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                m_cmbFindWhat.Text = value;
            }
        }

        public event EventHandler<SledFindAndReplaceEventArgs> FindAndReplaceEvent;

        #endregion

        private void SledFindAndReplaceFormQuickReplaceLoad(object sender, EventArgs e)
        {
            if (!SledFindAndReplaceSettings.QuickReplace.FindOptionsExpanded)
                m_grpFindOptions.Collapse();
        }

        private void SledCollapsibleGroupBoxExpandedEvent(object sender, EventArgs e)
        {
            CollapsedOrExpanded(sender, 1);
        }

        private void SledCollapsibleGroupBoxCollapsedEvent(object sender, EventArgs e)
        {
            CollapsedOrExpanded(sender, -1);
        }

        private void CollapsedOrExpanded(object sender, int iPlusOrMinus)
        {
            var grpBox = (SledCollapsibleGroupBox)sender;

            // Resize self & parent
            Size = new Size(Size.Width, Size.Height + (grpBox.LastHeight * iPlusOrMinus));
            
            if (ParentForm != null)
                ParentForm.ClientSize = Size;

            SledFindAndReplaceSettings.QuickReplace.FindOptionsExpanded = (iPlusOrMinus > 0);
        }

        private void BtnFindNextClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_cmbFindWhat.Text))
                return;

            // Add item to shared list
            SledFindAndReplaceSettings.GlobalFindWhat.Add(m_cmbFindWhat.Text);

            // Re-add the history of items including the new item
            m_cmbFindWhat.Items.Clear();
            m_cmbFindWhat.Items.AddRange(SledFindAndReplaceSettings.GlobalFindWhat.Items);

            // Run find event
            if (FindAndReplaceEvent == null)
                return;

            var lookIn = SledFindAndReplaceLookIn.Invalid;
            if (m_cmbLookIn.SelectedItem != null)
                lookIn = (SledFindAndReplaceLookIn)((SledFindAndReplaceForm.TextAssociation)m_cmbLookIn.SelectedItem).Tag;

            var searchType = SledFindAndReplaceSearchType.Normal;
            if ((m_cmbUse.SelectedItem != null) && (m_chkUse.Checked))
                searchType = (SledFindAndReplaceSearchType)((SledFindAndReplaceForm.TextAssociation)m_cmbUse.SelectedItem).Tag;

            var ea = new SledFindAndReplaceEventArgs.QuickReplace(
                m_cmbFindWhat.Text,
                null,
                lookIn,
                m_chkMatchCase.Checked,
                m_chkMatchWholeWord.Checked,
                m_chkSearchUp.Checked,
                searchType);

            FindAndReplaceEvent(this, ea);
        }

        private void BtnReplaceClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_cmbReplaceWith.Text) || string.IsNullOrEmpty(m_cmbFindWhat.Text))
                return;

            // Add items to shared lists
            SledFindAndReplaceSettings.GlobalFindWhat.Add(m_cmbFindWhat.Text);
            SledFindAndReplaceSettings.GlobalReplaceWith.Add(m_cmbReplaceWith.Text);

            // Re-add the history of items including the new items
            m_cmbFindWhat.Items.Clear();
            m_cmbFindWhat.Items.AddRange(SledFindAndReplaceSettings.GlobalFindWhat.Items);
            m_cmbReplaceWith.Items.Clear();
            m_cmbReplaceWith.Items.AddRange(SledFindAndReplaceSettings.GlobalReplaceWith.Items);

            // Run replace event
            if (FindAndReplaceEvent == null)
                return;

            var lookIn = SledFindAndReplaceLookIn.Invalid;
            if (m_cmbLookIn.SelectedItem != null)
                lookIn = (SledFindAndReplaceLookIn)((SledFindAndReplaceForm.TextAssociation) m_cmbLookIn.SelectedItem).Tag;

            var searchType = SledFindAndReplaceSearchType.Normal;
            if ((m_cmbUse.SelectedItem != null) && (m_chkUse.Checked))
                searchType = (SledFindAndReplaceSearchType)((SledFindAndReplaceForm.TextAssociation) m_cmbUse.SelectedItem).Tag;

            var ea = new SledFindAndReplaceEventArgs.QuickReplace(
                m_cmbFindWhat.Text,
                m_cmbReplaceWith.Text,
                lookIn,
                m_chkMatchCase.Checked,
                m_chkMatchWholeWord.Checked,
                m_chkSearchUp.Checked,
                searchType);

            FindAndReplaceEvent(this, ea);
        }

        private void BtnReplaceAllClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_cmbReplaceWith.Text) || string.IsNullOrEmpty(m_cmbFindWhat.Text))
                return;

            // Add items to shared lists
            SledFindAndReplaceSettings.GlobalFindWhat.Add(m_cmbFindWhat.Text);
            SledFindAndReplaceSettings.GlobalReplaceWith.Add(m_cmbReplaceWith.Text);

            // Re-add the history of items including the new items
            m_cmbFindWhat.Items.Clear();
            m_cmbFindWhat.Items.AddRange(SledFindAndReplaceSettings.GlobalFindWhat.Items);
            m_cmbReplaceWith.Items.Clear();
            m_cmbReplaceWith.Items.AddRange(SledFindAndReplaceSettings.GlobalReplaceWith.Items);

            // Run replace all event
            if (FindAndReplaceEvent == null)
                return;

            var lookIn = SledFindAndReplaceLookIn.Invalid;
            if (m_cmbLookIn.SelectedItem != null)
                lookIn = (SledFindAndReplaceLookIn)((SledFindAndReplaceForm.TextAssociation)m_cmbLookIn.SelectedItem).Tag;

            var searchType = SledFindAndReplaceSearchType.Normal;
            if ((m_cmbUse.SelectedItem != null) && (m_chkUse.Checked))
                searchType = (SledFindAndReplaceSearchType)((SledFindAndReplaceForm.TextAssociation)m_cmbUse.SelectedItem).Tag;

            var ea = new SledFindAndReplaceEventArgs.ReplaceInFiles(
                m_cmbFindWhat.Text,
                m_cmbReplaceWith.Text,
                lookIn,
                null,
                false,
                null,
                m_chkMatchCase.Checked,
                m_chkMatchWholeWord.Checked,
                searchType,
                SledFindAndReplaceResultsWindow.None,
                true);

            FindAndReplaceEvent(this, ea);
        }

        private void CmbFindWhatKeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrEmpty(m_cmbFindWhat.Text))
                return;

            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                m_btnFindNext.PerformClick();
            }
        }

        private void CmbFindWhatTextChanged(object sender, EventArgs e)
        {
            m_btnFindNext.Enabled = !string.IsNullOrEmpty(m_cmbFindWhat.Text);
            m_btnReplace.Enabled = (m_btnFindNext.Enabled && !string.IsNullOrEmpty(m_cmbReplaceWith.Text));
            m_btnReplaceAll.Enabled = m_btnReplace.Enabled;
        }

        private void CmbReplaceWithTextChanged(object sender, EventArgs e)
        {
            m_btnReplace.Enabled = (m_btnFindNext.Enabled && !string.IsNullOrEmpty(m_cmbReplaceWith.Text));
            m_btnReplaceAll.Enabled = m_btnReplace.Enabled;
        }

        private void CmbLookInSelectedIndexChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickReplace.LookInIndex = ((ComboBox)sender).SelectedIndex;
        }

        private void ChkMatchCaseCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickReplace.MatchCaseChecked = ((CheckBox)sender).Checked;
        }

        private void ChkMatchWholeWordCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickReplace.MatchWholeWordChecked = ((CheckBox)sender).Checked;
        }

        private void ChkSearchUpCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickReplace.SearchUpChecked = ((CheckBox)sender).Checked;
        }

        private void ChkUseCheckedChanged(object sender, EventArgs e)
        {
            m_cmbUse.Enabled = ((CheckBox)sender).Checked;
            SledFindAndReplaceSettings.QuickReplace.UseChecked = ((CheckBox)sender).Checked;
        }

        private void CmbUseSelectedIndexChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickReplace.UseIndex = ((ComboBox)sender).SelectedIndex;
        }
    }
}
