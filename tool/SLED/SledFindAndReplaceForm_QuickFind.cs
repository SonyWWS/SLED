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
    partial class SledFindAndReplaceForm_QuickFind : UserControl, ISledFindAndReplaceSubForm
    {
        public SledFindAndReplaceForm_QuickFind()
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
            if (m_cmbLookIn.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.QuickFind.LookInIndex >= 0) &&
                    (SledFindAndReplaceSettings.QuickFind.LookInIndex < m_cmbLookIn.Items.Count))
                    m_cmbLookIn.SelectedIndex = SledFindAndReplaceSettings.QuickFind.LookInIndex;
                else
                    m_cmbLookIn.SelectedItem = m_cmbLookIn.Items[0];
            }
            m_chkMatchCase.Checked = SledFindAndReplaceSettings.QuickFind.MatchCaseChecked;
            m_chkMatchWholeWord.Checked = SledFindAndReplaceSettings.QuickFind.MatchWholeWordChecked;
            m_chkSearchUp.Checked = SledFindAndReplaceSettings.QuickFind.SearchUpChecked;
            m_chkUse.Checked = SledFindAndReplaceSettings.QuickFind.UseChecked;
            if (m_cmbUse.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.QuickFind.UseIndex >= 0) &&
                    (SledFindAndReplaceSettings.QuickFind.UseIndex < m_cmbUse.Items.Count))
                    m_cmbUse.SelectedIndex = SledFindAndReplaceSettings.QuickFind.UseIndex;
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

        private void SledFindAndReplaceFormQuickFindLoad(object sender, EventArgs e)
        {
            if (!SledFindAndReplaceSettings.QuickFind.FindOptionsExpanded)
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

            SledFindAndReplaceSettings.QuickFind.FindOptionsExpanded = (iPlusOrMinus > 0);
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

            if (FindAndReplaceEvent == null)
                return;

            var lookIn = SledFindAndReplaceLookIn.Invalid;
            if (m_cmbLookIn.SelectedItem != null)
                lookIn = (SledFindAndReplaceLookIn)((SledFindAndReplaceForm.TextAssociation)m_cmbLookIn.SelectedItem).Tag;

            var searchType = SledFindAndReplaceSearchType.Normal;
            if ((m_cmbUse.SelectedItem != null) && (m_chkUse.Checked))
                searchType = (SledFindAndReplaceSearchType)((SledFindAndReplaceForm.TextAssociation)m_cmbUse.SelectedItem).Tag;

            var ea = new SledFindAndReplaceEventArgs.QuickFind(
                m_cmbFindWhat.Text,
                lookIn,
                m_chkMatchCase.Checked,
                m_chkMatchWholeWord.Checked,
                m_chkSearchUp.Checked,
                searchType);

            FindAndReplaceEvent(this, ea);
        }

        private void CmbFindWhatTextChanged(object sender, EventArgs e)
        {
            m_btnFindNext.Enabled = !string.IsNullOrEmpty(m_cmbFindWhat.Text);
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

        private void CmbLookInSelectedIndexChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickFind.LookInIndex = ((ComboBox)sender).SelectedIndex;
        }

        private void ChkMatchCaseCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickFind.MatchCaseChecked = ((CheckBox)sender).Checked;
        }

        private void ChkMatchWholeWordCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickFind.MatchWholeWordChecked = ((CheckBox)sender).Checked;
        }

        private void ChkSearchUpCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickFind.SearchUpChecked = ((CheckBox)sender).Checked;
        }

        private void ChkUseCheckedChanged(object sender, EventArgs e)
        {
            m_cmbUse.Enabled = ((CheckBox)sender).Checked;
            SledFindAndReplaceSettings.QuickFind.UseChecked = ((CheckBox)sender).Checked;
        }

        private void CmbUseSelectedIndexChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.QuickFind.UseIndex = ((ComboBox)sender).SelectedIndex;
        }
    }
}
