/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// Find/Replace In Files Form
    /// </summary>
    partial class SledFindAndReplaceForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private SledFindAndReplaceForm()
        {
            s_instance = this;
            
            InitializeComponent();

            if (!StartLocation.IsEmpty)
            {
                StartPosition = FormStartPosition.Manual;
                Location = StartLocation;
            }

            // Associate SledFindAndReplaceModes enum to find/replace buttons
            m_strpFindDropDown_QuickFindItem.Tag = SledFindAndReplaceModes.QuickFind;
            m_strpFindDropDown_FindInFilesItem.Tag = SledFindAndReplaceModes.FindInFiles;
            m_strpReplaceDropDown_QuickReplaceItem.Tag = SledFindAndReplaceModes.QuickReplace;
            m_strpReplaceDropDown_ReplaceInFilesItem.Tag = SledFindAndReplaceModes.ReplaceInFiles;

            //
            // Add all individual find/replace controls to the form
            //

            // Add "Quick Find"
            m_frmQuickFind = new SledFindAndReplaceForm_QuickFind {Tag = SledFindAndReplaceModes.QuickFind};
            m_lstForms.Add(m_frmQuickFind);
            m_frmQuickFind.FindAndReplaceEvent += SubFormFindAndReplaceEvent;

            // Add "Find In Files"
            m_frmFindInFiles = new SledFindAndReplaceForm_FindInFiles {Tag = SledFindAndReplaceModes.FindInFiles};
            m_lstForms.Add(m_frmFindInFiles);
            m_frmFindInFiles.FindAndReplaceEvent += SubFormFindAndReplaceEvent;

            // Add "Quick Replace"
            m_frmQuickReplace = new SledFindAndReplaceForm_QuickReplace {Tag = SledFindAndReplaceModes.QuickReplace};
            m_lstForms.Add(m_frmQuickReplace);
            m_frmQuickReplace.FindAndReplaceEvent += SubFormFindAndReplaceEvent;

            // Add "Replace In Files"
            m_frmReplaceInFiles = new SledFindAndReplaceForm_ReplaceInFiles {Tag = SledFindAndReplaceModes.ReplaceInFiles};
            m_lstForms.Add(m_frmReplaceInFiles);
            m_frmReplaceInFiles.FindAndReplaceEvent += SubFormFindAndReplaceEvent;
            
            SkinService.SkinChangedOrApplied += SkinServiceSkinChangedOrApplied;
            ApplySkin();
        }

        /// <summary>
        /// Hide the base class Show() method
        /// </summary>
        public new void Show()
        {
            Show(null);
        }

        /// <summary>
        /// Hide the base class Show() method
        /// </summary>
        /// <param name="owner"></param>
        public new void Show(IWin32Window owner)
        {
            if (Visible)
                Activate();
            else
                base.Show(owner);
        }

        /// <summary>
        /// Gets/sets the find/replace mode to use
        /// </summary>
        public SledFindAndReplaceModes Mode
        {
            get { return m_mode; }
            set
            {
                var bChangeMode = (m_bLoaded && (m_mode != value));

                m_mode = value;

                if (bChangeMode)
                    ChangeMode(value);
            }
        }

        /// <summary>
        /// Initial text to input in the find or replace form
        /// </summary>
        public string InitialText { get; set; }

        /// <summary>
        /// Check if the Find In Files dialog already exists or not
        /// </summary>
        public static SledFindAndReplaceForm Instance
        {
            get { return s_instance ?? (s_instance = new SledFindAndReplaceForm()); }
        }

        private static SledFindAndReplaceForm s_instance;

        /// <summary>
        /// Gets/sets the start location of the form
        /// </summary>
        public static Point StartLocation { get; set; }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
                Close();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void StrpDropDownDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag == null)
                return;

            var mode = (SledFindAndReplaceModes)e.ClickedItem.Tag;

            if (Mode != mode)
                ChangeMode(mode);
        }

        private void SledFindAndReplaceFormLoad(object sender, EventArgs e)
        {
            if (m_mode == SledFindAndReplaceModes.None)
                m_mode = SledFindAndReplaceModes.QuickFind;

            m_bLoaded = true;
        }

        private void SledFindAndReplaceFormShown(object sender, EventArgs e)
        {
            if (!m_firstShown)
            {
                ChangeMode(Mode);
                m_firstShown = true;
            }
        }

        private void SledFindAndReplaceFormFormClosed(object sender, FormClosedEventArgs e)
        {
            SkinService.SkinChangedOrApplied -= SkinServiceSkinChangedOrApplied;
            s_instance = null;
        }

        private void SubFormFindAndReplaceEvent(object sender, SledFindAndReplaceEventArgs e)
        {
            // Run find or replace event
            ((SledFindAndReplaceService)m_findAndReplaceService.Get).Run(e);

            var bMakeSound = true;

            // Check result
            switch (e.Result)
            {
                    // Do nothing
                case SledFindAndReplaceResult.Success:
                    bMakeSound = false;
                    break;

                    // No results were found
                case SledFindAndReplaceResult.NothingFound:
                    SledOutDevice.OutLine(SledMessageType.Info, Localization.SledFindAndReplaceErrorNothingFound);
                    break;

                    // No documents to search in
                case SledFindAndReplaceResult.NothingToSearch:
                    SledOutDevice.OutLine(SledMessageType.Info, Localization.SledFindAndReplaceErrorNoDocsToSearch);
                    break;

                default: NotifyMissingResultProperty(this); break;
            }

            // Beep if not successful
            if (bMakeSound)
            {
                // Beep like VS does
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void NotifyMissingResultProperty(IWin32Window form)
        {
            SledOutDevice.OutLine(SledMessageType.Warning, "[Find & Replace] Your find or replace event needs to set the \"Result\" property!");
            MessageBox.Show(form, "Your find or replace event needs to set the \"Result\" property!");
        }

        private void ChangeMode(SledFindAndReplaceModes mode)
        {
            m_mode = mode;

            ISledFindAndReplaceSubForm theFrm = null;

            // Activate correct control and deactivate incorrect control
            foreach (ISledFindAndReplaceSubForm frm in m_lstForms)
            {
                if ((SledFindAndReplaceModes)frm.Control.Tag == Mode)
                    theFrm = frm;

                Controls.Remove(frm.Control);
            }

            if (theFrm == null)
                return;

            Controls.Add(theFrm.Control);
            theFrm.InitialText = InitialText;
            Invalidate(true);
        }

        private void SledFindAndReplaceFormLocationChanged(object sender, EventArgs e)
        {
            StartLocation = Location;
        }

        private void SkinServiceSkinChangedOrApplied(object sender, EventArgs e)
        {
            ApplySkin();
        }

        private void ApplySkin()
        {
            foreach (var frm in m_lstForms)
                SkinService.ApplyActiveSkin(frm.Control);

            var mode = Mode;
            ChangeMode(SledFindAndReplaceModes.None);
            ChangeMode(mode);
        }

        private readonly SledFindAndReplaceForm_QuickFind m_frmQuickFind;
        private readonly SledFindAndReplaceForm_FindInFiles m_frmFindInFiles;
        private readonly SledFindAndReplaceForm_QuickReplace m_frmQuickReplace;
        private readonly SledFindAndReplaceForm_ReplaceInFiles m_frmReplaceInFiles;
        private readonly IList<ISledFindAndReplaceSubForm> m_lstForms = new List<ISledFindAndReplaceSubForm>();

        private readonly SledServiceReference<ISledFindAndReplaceService> m_findAndReplaceService =
            new SledServiceReference<ISledFindAndReplaceService>();

        private bool m_bLoaded;
        private bool m_firstShown;
        private SledFindAndReplaceModes m_mode = SledFindAndReplaceModes.None;

        /// <summary>
        /// Utility class for a ComboBox item and to also aid with embedding some data with a string on the ComboBox
        /// </summary>
        public class TextAssociation
        {
            public TextAssociation(string text, object tag)
            {
                m_text = text;
                m_tag = tag;
            }

            public override string ToString()
            {
                if (m_text == null)
                    return string.Empty;

                return m_text;
            }

            public string Text
            {
                get { return m_text; }
            }

            public object Tag
            {
                get { return m_tag; }
            }

            private readonly string m_text;
            private readonly object m_tag;
        }
    }
}
