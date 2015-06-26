/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public partial class SledModifiedFilesForm : Form
    {
        public SledModifiedFilesForm()
        {
            InitializeComponent();
        }

        public new void Show()
        {
            Show(null);
        }

        public new void Show(IWin32Window owner)
        {
            // TODO: Enable this code once we figure out
            // TODO: why SLED gets stuck minimized...
            // TODO: StateMachine Editor uses the same code
            // TODO: but doesn't have the stuck-minimized issue.
            //if (owner != null)
            //{
            //    var ownerForm = FromHandle(owner.Handle) as Form;
            //    if ((ownerForm != null) && (ownerForm.WindowState == FormWindowState.Minimized))
            //        WindowState = FormWindowState.Minimized;
            //}

            if (!Visible)
                base.Show(owner);
            else
                Activate();
        }

        public void AddFile(ISledDocument sd)
        {
            lock (m_lock)
            {
                if (m_lstFiles.Contains(sd))
                    return;

                // Add to collection
                m_lstFiles.Add(sd);
                // Add to list box
                m_lstBoxFiles.Items.Add(new SledModifiedFilesFormItem(sd));

                // Select first item added
                if (m_lstBoxFiles.Items.Count == 1)
                    m_lstBoxFiles.SelectedIndex = 0;
            }
        }

        public int Count
        {
            get
            {
                lock (m_lock)
                {
                    return m_lstFiles.Count;
                }
            }
        }

        public event EventHandler<SledFileWatcherServiceEventArgs> ReloadFileEvent;

        public event EventHandler<SledFileWatcherServiceEventArgs> IgnoreFileEvent;

        #region Form Events

        private void SledModifiedFilesFormFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            BtnIgnoreAllClick(sender, EventArgs.Empty);
        }

        private void BtnReloadClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                if (m_lstBoxFiles.SelectedItem == null)
                    return;

                var iIndex = m_lstBoxFiles.SelectedIndex;
                var item = (SledModifiedFilesFormItem)m_lstBoxFiles.SelectedItem;

                ReloadFile(item);
                RemoveFile(item);

                SelectNext(iIndex);

                if (m_lstBoxFiles.Items.Count <= 0)
                    Hide();
            }
        }

        private void BtnIgnoreClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                if (m_lstBoxFiles.SelectedItem == null)
                    return;

                var iIndex = m_lstBoxFiles.SelectedIndex;
                var item = (SledModifiedFilesFormItem)m_lstBoxFiles.SelectedItem;

                IgnoreFile(item);
                RemoveFile(item);

                SelectNext(iIndex);

                if (m_lstBoxFiles.Items.Count <= 0)
                    Hide();
            }
        }

        private void BtnReloadAllClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                while (m_lstBoxFiles.Items.Count > 0)
                {
                    var item = (SledModifiedFilesFormItem)m_lstBoxFiles.Items[0];

                    ReloadFile(item);
                    RemoveFile(item);
                }

                Hide();
            }
        }

        private void BtnIgnoreAllClick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                while (m_lstBoxFiles.Items.Count > 0)
                {
                    var item = (SledModifiedFilesFormItem)m_lstBoxFiles.Items[0];

                    IgnoreFile(item);
                    RemoveFile(item);
                }

                Hide();
            }
        }

        #endregion

        private void ReloadFile(SledModifiedFilesFormItem item)
        {
            lock (m_lock)
            {
                ReloadFileEvent.Raise(this, new SledFileWatcherServiceEventArgs(item.SledDocument));
            }
        }

        private void IgnoreFile(SledModifiedFilesFormItem item)
        {
            lock (m_lock)
            {
                IgnoreFileEvent.Raise(this, new SledFileWatcherServiceEventArgs(item.SledDocument));
            }
        }

        private void RemoveFile(SledModifiedFilesFormItem item)
        {
            lock (m_lock)
            {
                m_lstFiles.Remove(item.SledDocument);
                m_lstBoxFiles.Items.Remove(item);
            }
        }

        private void SelectNext(int iIndex)
        {
            if (m_lstBoxFiles.Items.Count <= 0)
                return;

            if (m_lstBoxFiles.Items.Count <= iIndex)
                iIndex--;

            m_lstBoxFiles.SelectedIndex = iIndex;
        }

        #region Private Class SledModifiedFilesFormItem

        private class SledModifiedFilesFormItem
        {
            public SledModifiedFilesFormItem(ISledDocument sd)
            {
                m_sledDocument = sd;
            }

            public ISledDocument SledDocument
            {
                get { return m_sledDocument; }
            }

            public override string ToString()
            {
                return m_sledDocument.Uri.LocalPath;
            }

            private readonly ISledDocument m_sledDocument;
        }

        #endregion

        private volatile object m_lock = 
            new object();

        private readonly IList<ISledDocument> m_lstFiles = 
            new List<ISledDocument>();
        
    }
}
