/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    partial class SledFindAndReplaceForm_FindInFiles : UserControl, ISledFindAndReplaceSubForm
    {
        public SledFindAndReplaceForm_FindInFiles()
        {
            InitializeComponent();

            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInCurrentDocument, SledFindAndReplaceLookIn.CurrentDocument));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInAllOpenDocuments, SledFindAndReplaceLookIn.AllOpenDocuments));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInCurrentProject, SledFindAndReplaceLookIn.CurrentProject));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInEntireSolution, SledFindAndReplaceLookIn.EntireSolution));
            m_cmbLookIn.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceLookInCustom, SledFindAndReplaceLookIn.Custom));
            m_iCmbLookInCustomIdx = m_cmbLookIn.Items.Count - 1; // Store for later

            m_cmbUse.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceUsePatternRegularExpressions, SledFindAndReplaceSearchType.RegularExpressions));
            m_cmbUse.Items.Add(new SledFindAndReplaceForm.TextAssociation(Localization.SledFindAndReplaceUsePatternWildCards, SledFindAndReplaceSearchType.WildCards));

            m_grpFindOptions.ExpandedEvent += SledCollapsibleGroupBoxExpandedEvent;
            m_grpFindOptions.CollapsedEvent += SledCollapsibleGroupBoxCollapsedEvent;
            m_grpResultOptions.ExpandedEvent += SledCollapsibleGroupBoxExpandedEvent;
            m_grpResultOptions.CollapsedEvent += SledCollapsibleGroupBoxCollapsedEvent;

            m_cmbFindWhat.Items.AddRange(SledFindAndReplaceSettings.GlobalFindWhat.Items);
            m_chkIncludeSubFolders.Checked = SledFindAndReplaceSettings.FindInFiles.IncludeSubFolders;
            if (m_cmbLookIn.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.FindInFiles.LookInIndex >= 0) &&
                    (SledFindAndReplaceSettings.FindInFiles.LookInIndex < m_cmbLookIn.Items.Count))
                    m_cmbLookIn.SelectedIndex = SledFindAndReplaceSettings.FindInFiles.LookInIndex;
                else
                    m_cmbLookIn.SelectedItem = m_cmbLookIn.Items[0];
            }
            m_chkMatchCase.Checked = SledFindAndReplaceSettings.FindInFiles.MatchCaseChecked;
            m_chkMatchWholeWord.Checked = SledFindAndReplaceSettings.FindInFiles.MatchWholeWordChecked;
            m_chkUse.Checked = SledFindAndReplaceSettings.FindInFiles.UseChecked;
            if (m_cmbUse.Items.Count > 0)
            {
                if ((SledFindAndReplaceSettings.FindInFiles.UseIndex >= 0) &&
                    (SledFindAndReplaceSettings.FindInFiles.UseIndex < m_cmbUse.Items.Count))
                    m_cmbUse.SelectedIndex = SledFindAndReplaceSettings.FindInFiles.UseIndex;
                else
                    m_cmbUse.SelectedItem = m_cmbUse.Items[0];
            }
            if (SledFindAndReplaceSettings.FindInFiles.Results1WindowChecked)
            {
                m_rdoFindResults1Window.Checked = true;
                m_rdoFindResults2Window.Checked = false;
            }
            else
            {
                m_rdoFindResults1Window.Checked = false;
                m_rdoFindResults2Window.Checked = true;
            }
            m_chkDisplayFileNamesOnly.Checked = SledFindAndReplaceSettings.FindInFiles.DisplayFileNamesOnlyChecked;

            // Set up extensions
            var sb = new StringBuilder();
            m_cmbLookAtTheseFileTypes.Items.Add(SledFindAndReplaceService.AllExtension);
            m_cmbLookAtTheseFileTypes.SelectedIndex = 0;
            m_cmbLookAtTheseFileTypes.Items.Add(SledFindAndReplaceService.TxtExtension);
            sb.Append(SledFindAndReplaceService.TxtExtension);
            sb.Append(";");
            //foreach (ISledLanguagePlugin plugin in SledShared.IDE.LanguagePlugins)
            foreach (KeyValuePair<UInt16, ISledLanguagePlugin> kv in m_languagePluginService.Get.LanguagePlugins)
            {
                ISledLanguagePlugin plugin = kv.Value;

                foreach (string ext in plugin.LanguageExtensions)
                {
                    m_cmbLookAtTheseFileTypes.Items.Add(ext);
                    sb.Append(ext);
                    sb.Append(";");
                }
            }
            if (sb.Length >= 0)
            {
                sb.Remove(sb.Length - 1, 1);
                m_cmbLookAtTheseFileTypes.Items.Add(sb.ToString());
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

        private void SledFindAndReplaceFormFindInFilesLoad(object sender, EventArgs e)
        {
            if (!SledFindAndReplaceSettings.FindInFiles.FindOptionsExpanded)
                m_grpFindOptions.Collapse();

            if (!SledFindAndReplaceSettings.FindInFiles.ResultOptionsExpanded)
                m_grpResultOptions.Collapse();
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

            if (grpBox == m_grpFindOptions)
            {
                m_grpResultOptions.Location = new Point(m_grpResultOptions.Location.X, m_grpResultOptions.Location.Y + (grpBox.LastHeight * iPlusOrMinus));
                SledFindAndReplaceSettings.FindInFiles.FindOptionsExpanded = (iPlusOrMinus > 0);
            }
            else if (grpBox == m_grpResultOptions)
            {
                SledFindAndReplaceSettings.FindInFiles.ResultOptionsExpanded = (iPlusOrMinus > 0);
            }

            // Resize self & parent
            Size = new Size(Size.Width, Size.Height + (grpBox.LastHeight * iPlusOrMinus));
            
            if (ParentForm != null)
                ParentForm.ClientSize = Size;
        }

        private void BtnFindAllClick(object sender, EventArgs e)
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

            var ea = new SledFindAndReplaceEventArgs.FindInFiles(
                m_cmbFindWhat.Text,
                lookIn,
                m_lstLookInFolders.ToArray(),
                m_chkIncludeSubFolders.Checked,
                lookIn != SledFindAndReplaceLookIn.Custom ? null : GetExtensions(),
                m_chkMatchCase.Checked,
                m_chkMatchWholeWord.Checked,
                searchType,
                m_rdoFindResults1Window.Checked);

            FindAndReplaceEvent(this, ea);
        }

        private void CmbFindWhatTextChanged(object sender, EventArgs e)
        {
            m_btnFindAll.Enabled = !string.IsNullOrEmpty(m_cmbFindWhat.Text);
        }

        private void CmbFindWhatKeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrEmpty(m_cmbFindWhat.Text))
                return;

            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                m_btnFindAll.PerformClick();
            }
        }
        
        private void CmbLookInSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bStopRecursion)
                return;

            int iSelectedIndex = ((ComboBox)sender).SelectedIndex;
            m_chkIncludeSubFolders.Enabled = false;
            m_cmbLookAtTheseFileTypes.Enabled = false;

            // Update persisted settings
            SledFindAndReplaceSettings.FindInFiles.LookInIndex = iSelectedIndex;

            // If they select "Custom" we need to pop up the multi folder select form. Then,
            // if they don't select any folders we need to reset the LookIn index.
            if (iSelectedIndex == m_iCmbLookInCustomIdx)
            {
                var form = new SledMultiFolderSelectionForm { Folders = m_lstLookInFolders };
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    m_lstLookInFolders.Clear();
                    m_lstLookInFolders.AddRange(form.Folders);

                    // Don't actually persist "Custom" search
                    SledFindAndReplaceSettings.FindInFiles.LookInIndex = 0;
                    m_chkIncludeSubFolders.Enabled = true;
                    m_cmbLookAtTheseFileTypes.Enabled = true;
                }
                else
                {
                    // Select something other than "Custom"
                    m_bStopRecursion = true;
                    ((ComboBox)sender).SelectedIndex = 0;
                    SledFindAndReplaceSettings.FindInFiles.LookInIndex = 0;
                    m_bStopRecursion = false;
                }

                form.Close();
            }
        }

        private void BtnLookInClick(object sender, EventArgs e)
        {
            var form = new SledMultiFolderSelectionForm { Folders = m_lstLookInFolders };
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                m_lstLookInFolders.Clear();
                m_lstLookInFolders.AddRange(form.Folders);
                
                // Update LookIn check box
                m_bStopRecursion = true;
                m_cmbLookIn.SelectedIndex = m_iCmbLookInCustomIdx;
                m_bStopRecursion = false;
                m_chkIncludeSubFolders.Enabled = true;
                m_cmbLookAtTheseFileTypes.Enabled = true;
            }

            form.Close();
        }

        private void ChkIncludeSubFoldersCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.IncludeSubFolders = ((CheckBox)sender).Checked;
        }

        private void ChkMatchCaseCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.MatchCaseChecked = ((CheckBox)sender).Checked;
        }

        private void ChkMatchWholeWordCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.MatchWholeWordChecked = ((CheckBox)sender).Checked;
        }

        private void ChkUseCheckedChanged(object sender, EventArgs e)
        {
            m_cmbUse.Enabled = ((CheckBox)sender).Checked;
            SledFindAndReplaceSettings.FindInFiles.UseChecked = ((CheckBox)sender).Checked;
        }

        private void CmbUseSelectedIndexChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.UseIndex = ((ComboBox)sender).SelectedIndex;
        }

        private void RdoFindResults1WindowCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.Results1WindowChecked = m_rdoFindResults1Window.Checked;
        }

        private void ChkDisplayFileNamesOnlyCheckedChanged(object sender, EventArgs e)
        {
            SledFindAndReplaceSettings.FindInFiles.DisplayFileNamesOnlyChecked = ((CheckBox)sender).Checked;
        }

        private string[] GetExtensions()
        {
            string extensions = m_cmbLookAtTheseFileTypes.Text;
            if (string.IsNullOrEmpty(extensions))
                return new[] { SledFindAndReplaceService.AllExtension };

            if (string.Compare(extensions, SledFindAndReplaceService.AllExtension, true) == 0)
                return new[] { SledFindAndReplaceService.AllExtension };

            // Remove any asterisks
            extensions = extensions.Replace(Resource.Asterisk, string.Empty);

            // Break up separating by semicolon
            string[] exts = extensions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return exts;
        }

        private readonly SledServiceReference<ISledLanguagePluginService> m_languagePluginService =
            new SledServiceReference<ISledLanguagePluginService>();

        private bool m_bStopRecursion;
        private readonly int m_iCmbLookInCustomIdx;
        private readonly List<string> m_lstLookInFolders = new List<string>();
    }
}
