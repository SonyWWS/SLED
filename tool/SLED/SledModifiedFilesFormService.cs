/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledModifiedFilesFormService))]
    [Export(typeof(SledModifiedFilesFormService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledModifiedFilesFormService : IInitializable, ISledModifiedFilesFormService
    {
        [ImportingConstructor]
        public SledModifiedFilesFormService(MainForm mainForm, ISledFileWatcherService fileWatcherService)
        {
            m_mainForm = mainForm;
            m_fileWatcherService = fileWatcherService;

            m_mainForm.FormClosing += MainFormFormClosing;

            m_fileWatcherService.AttributeChangedEvent += FileWatcherServiceAttributeChangedEvent;
            m_fileWatcherService.FileChangedEvent += FileWatcherServiceFileChangedEvent;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledModifiedFilesFormService Interface

        /// <summary>
        /// Event fired when the user selects to reload a file that has been modified outside of the editor
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> FileReloading;

        /// <summary>
        /// Event fired after a modified file has been reloaded in the editor
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> FileReloaded;

        /// <summary>
        /// Event fired when the user selects to ignore a file modified outside of the editor
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> FileIgnoring;

        /// <summary>
        /// Event fired after a modified file has been ignored in the editor
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> FileIgnored;

        #endregion

        #region ISledFileWatcherService Events

        private void FileWatcherServiceAttributeChangedEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            var sd = e.Document as SledDocument;
            if (sd == null)
                return;

            // Don't mess with hidden documents
            if (sd.Hidden)
                return;

            // Toggle readonly status of document
            sd.IsReadOnly = !sd.IsReadOnly;

            // If document is dirty and its now readonly then we have a problem
            // and need to bring up the readonly resolution form.
            if (!sd.IsReadOnly || !sd.Dirty)
                return;

            using (var form = new SledReadOnlyDocResolutionForm())
            {
                form.SledDocument = sd;
                form.TryOptionEvent += FormTryOptionEvent;
                form.ShowDialog(m_mainForm);
            }
        }

        private void FormTryOptionEvent(object sender, SledReadOnlyDocResolutionFormEventArgs e)
        {
            var sd = e.SledDocument as SledDocument;
            if (sd == null)
                return;

            switch (e.Option)
            {
                case SledReadOnlyDocResolutionFormOptions.RemoveReadOnly:
                {
                    e.Result = m_fileWatcherService.TryRemoveReadOnlyAttribute(sd);
                    if (e.Result)
                        sd.IsReadOnly = false;
                }
                break;

                case SledReadOnlyDocResolutionFormOptions.RemoveReadOnlyAndSave:
                {
                    e.Result = m_fileWatcherService.TryRemoveReadOnlyAttribute(sd);
                    if (e.Result)
                    {
                        sd.IsReadOnly = false;
                        m_documentService.Get.Save(e.SledDocument);
                    }
                }
                break;

                case SledReadOnlyDocResolutionFormOptions.SaveAs:
                {
                    //bool bOrigValue = sd.IsNew;

                    //// Force so we can save-as
                    //if (!bOrigValue)
                    //    sd.IsNew = true;

                    //// Try and save-as
                    //e.Result = m_documentService.Get.SaveAs(sd);
                    //if (e.Result)
                    //    sd.ReadOnly = false;

                    //// Failed to save-as so reset 'new' setting if it was changed
                    //if (!e.Result)
                    //    sd.IsNew = bOrigValue;
                }
                break;

                case SledReadOnlyDocResolutionFormOptions.UndoChanges:
                {
                    // Reload the file and re-register control
                    try
                    {
                        sd.Read();
                        sd.IsReadOnly = true;
                        sd.Dirty = false;
                        e.Result = true;
                    }
                    catch (Exception)
                    {
                        e.Result = false;
                    }
                }
                break;

                default:
                {
                    e.Result = false;
                }
                break;
            }
        }

        private void FileWatcherServiceFileChangedEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            if (m_modifiedFilesForm == null)
            {
                m_modifiedFilesForm = new SledModifiedFilesForm();
                m_modifiedFilesForm.ReloadFileEvent += ModifiedFilesFormReloadFileEvent;
                m_modifiedFilesForm.IgnoreFileEvent += ModifiedFilesFormIgnoreFileEvent;
            }

            m_modifiedFilesForm.AddFile(e.Document);
            m_modifiedFilesForm.Show(m_mainForm);
        }

        #endregion

        #region SledModifiedFilesForm Events

        private void ModifiedFilesFormReloadFileEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            FileReloading.Raise(this, e);

            var absPath = e.Document.Uri.LocalPath;
            m_documentService.Get.Close(e.Document);

            ISledDocument sd;
            m_documentService.Get.Open(new Uri(absPath), out sd);

            FileReloaded.Raise(this, new SledFileWatcherServiceEventArgs(sd));
        }

        private void ModifiedFilesFormIgnoreFileEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            FileIgnoring.Raise(this, e);
            FileIgnored.Raise(this, e);
        }

        #endregion

        #region Member Methods

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_modifiedFilesForm == null)
                return;

            m_modifiedFilesForm.ReloadFileEvent -= ModifiedFilesFormReloadFileEvent;
            m_modifiedFilesForm.IgnoreFileEvent -= ModifiedFilesFormIgnoreFileEvent;
            m_modifiedFilesForm.Close();
            m_modifiedFilesForm.Dispose();
            m_modifiedFilesForm = null;
        }

        #endregion

        private readonly MainForm m_mainForm;
        private readonly ISledFileWatcherService m_fileWatcherService;

        private SledModifiedFilesForm m_modifiedFilesForm;

        private readonly SledServiceReference<ISledDocumentService> m_documentService =
            new SledServiceReference<ISledDocumentService>();
    }
}
