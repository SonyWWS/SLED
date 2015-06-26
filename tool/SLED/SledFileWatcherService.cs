/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledFileWatcherService))]
    [Export(typeof(SledFileWatcherService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledFileWatcherService : IInitializable, ISledFileWatcherService
    {
        [ImportingConstructor]
        public SledFileWatcherService(MainForm mainForm, ISledDocumentService documentService)
        {
            m_mainForm = mainForm;
            m_documentService = documentService;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_documentService.Opened += DocumentServiceOpened;
            m_documentService.Saving += DocumentServiceSaving;
            m_documentService.Saved += DocumentServiceSaved;
            m_documentService.Closing += DocumentServiceClosing;
        }

        #endregion

        #region ISledFileWatcherService Interface

        /// <summary>
        /// Event fired when a file is changed
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> FileChangedEvent;

        /// <summary>
        /// Event fired when a file's attribute is changed
        /// </summary>
        public event EventHandler<SledFileWatcherServiceEventArgs> AttributeChangedEvent;

        /// <summary>
        /// Try and remove the read only attribute from a file
        /// </summary>
        /// <param name="sd">Document to remove read only attribute from</param>
        /// <returns>True if read only attribute was removed false if it wasn't</returns>
        public bool TryRemoveReadOnlyAttribute(ISledDocument sd)
        {
            var bResult = false;
            var bValidDoc = IsValidDocument(sd);

            try
            {
                if (bValidDoc)
                    Disable(sd);

                File.SetAttributes(sd.Uri.LocalPath, FileAttributes.Normal);

                // Successful
                bResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    m_mainForm,
                    string.Format(
                        "{0}{1}{1}{2}",
                        Localization.SledFailChangePermissions,
                        Environment.NewLine,
                        SledUtil.TransSub(Localization.SledExceptionWas, ex.Message)),
                    Localization.SledFileReadOnlyError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (bValidDoc)
                    Enable(sd);
            }

            return bResult;
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            Add(e.Document);
        }

        private void DocumentServiceSaving(object sender, SledDocumentServiceEventArgs e)
        {
            Disable(e.Document);
        }

        private void DocumentServiceSaved(object sender, SledDocumentServiceEventArgs e)
        {
            if (!Enable(e.Document))
                Add(e.Document);
        }

        private void DocumentServiceClosing(object sender, SledDocumentServiceEventArgs e)
        {
            Remove(e.Document);
        }

        #endregion

        #region ISledProjectService Events

        //private void ProjectService_FileRenaming(object sender, SledProjectServiceFileEventArgs e)
        //{
        //    ISledDocument sd = e.File.SledDocument;
        //    if (sd == null)
        //        return;

        //    if (!IsValidDocument(sd))
        //        return;

        //    Remove(sd);
        //}

        //private void ProjectService_FileRenamed(object sender, SledProjectServiceFileEventArgs e)
        //{
        //    ISledDocument sd = e.File.SledDocument;
        //    if (sd == null)
        //        return;

        //    if (!IsValidDocument(sd))
        //        return;

        //    Add(sd);
        //}

        #endregion

        #region Member Methods

        private void Add(ISledDocument sd)
        {
            if (!IsValidDocument(sd))
                return;

            sd.UriChanged += SledDocumentUriChanged;

            if (m_dict.ContainsKey(sd))
                return;

            var watcher =
                new SledFileSystemWatcher
                    {
                        Tag = sd,
                        Path = Path.GetDirectoryName(sd.Uri.LocalPath),
                        Filter = Path.GetFileName(sd.Uri.LocalPath),
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes,
                        SynchronizingObject = m_mainForm
                    };
            watcher.Changed += WatcherChanged;
            watcher.EnableRaisingEvents = true;

            // Add to dictionary
            m_dict[sd] = watcher;
            m_dictFileStats[sd] = SledFileSystemFileStats.GetStats(sd.Uri.LocalPath);
        }

        private void Remove(ISledDocument sd)
        {
            if (!IsValidDocument(sd))
                return;

            sd.UriChanged -= SledDocumentUriChanged;

            SledFileSystemWatcher watcher;
            if (!m_dict.TryGetValue(sd, out watcher))
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= WatcherChanged;
            watcher.Dispose();

            // Remove from dictionary
            m_dict.Remove(sd);
            m_dictFileStats.Remove(sd);
        }

        private bool Enable(ISledDocument sd)
        {
            // Check if item in the dictionary
            SledFileSystemWatcher watcher;
            if (!m_dict.TryGetValue(sd, out watcher))
                return false;
                
            watcher.EnableRaisingEvents = true;
            m_dictFileStats[sd] = SledFileSystemFileStats.GetStats(sd.Uri.LocalPath);

            // Successfully enabled
            return true;
        }

        private void Disable(ISledDocument sd)
        {
            SledFileSystemWatcher watcher;
            if (!m_dict.TryGetValue(sd, out watcher))
                return;
                
            watcher.EnableRaisingEvents = false;
        }

        private void SledDocumentUriChanged(object sender, UriChangedEventArgs e)
        {
            // Uri changed in a SledDocument so if we
            // were watching it we need to remove it.
            // This happens when documents are "save-as"'d
            var sd = sender as ISledDocument;

            KeyValuePair<ISledDocument, SledFileSystemWatcher>? sentinelWatcher = null;
            KeyValuePair<ISledDocument, SledFileSystemFileStats>? sentinelStats = null;

            // We do this ReferenceEqual checking because the Uri has changed
            // in the SledDocument and that makes the dictionary built-in
            // checks invalid (since they're using a comparer that uses the Uri
            // property for checking equality and obtaining a hash code).

            // Search for watcher
            foreach (var kv in m_dict)
            {
                if (!ReferenceEquals(kv.Key, sd))
                    continue;

                sentinelWatcher = kv;
                break;
            }

            // Search for stats
            foreach (var kv in m_dictFileStats)
            {
                if (!ReferenceEquals(kv.Key, sd))
                    continue;

                sentinelStats = kv;
                break;
            }

            // Remove file system watcher
            if (sentinelWatcher.HasValue)
            {
                sentinelWatcher.Value.Value.EnableRaisingEvents = false;
                sentinelWatcher.Value.Value.Changed -= WatcherChanged;
                sentinelWatcher.Value.Value.Dispose();
                m_dict.Remove(sentinelWatcher.Value);
            }

            // Remove file system stats
            if (sentinelStats.HasValue)
                m_dictFileStats.Remove(sentinelStats.Value);
        }

        private static bool IsValidDocument(ISledDocument sd)
        {
            return sd != null && File.Exists(sd.Uri.LocalPath);
        }

        private void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (m_bChanging)
                return;

            try
            {
                m_bChanging = true;

                var watcher = sender as SledFileSystemWatcher;
                if (watcher == null)
                    return;

                if (watcher.Tag == null)
                    return;

                var sd = watcher.Tag as ISledDocument;
                if (sd == null)
                    return;

                // Any existing stats on this file?
                SledFileSystemFileStats fileStats;
                if (!m_dictFileStats.TryGetValue(sd, out fileStats))
                    return;

                var curFileStats = SledFileSystemFileStats.GetStats(sd.Uri.LocalPath);
                if (!curFileStats.Valid)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Exception obtaining file " + 
                        "stats on file \"{0}\": {1}",
                        sd.Uri.LocalPath, curFileStats.Exception);

                    return;
                }

                // Figure out which event to hit
                var change = SledFileSystemFileStats.Compare(fileStats, curFileStats);

                // Update
                m_dictFileStats[sd] = curFileStats;

                // No change made; don't fire any events
                if (change == SledFileSystemFileStatsChange.Nothing)
                    return;
                
                // Fire appropriate event
                (change == SledFileSystemFileStatsChange.LastWrite
                     ? FileChangedEvent
                     : AttributeChangedEvent).Raise(this, new SledFileWatcherServiceEventArgs(sd));
            }
            finally
            {
                m_bChanging = false;
            }
        }

        #endregion

        private readonly MainForm m_mainForm;

        private readonly IDictionary<ISledDocument, SledFileSystemWatcher> m_dict = 
            new Dictionary<ISledDocument, SledFileSystemWatcher>(new ISledDocumentComparer());

        private readonly IDictionary<ISledDocument, SledFileSystemFileStats> m_dictFileStats = 
            new Dictionary<ISledDocument, SledFileSystemFileStats>(new ISledDocumentComparer());

        private bool m_bChanging;
        private readonly ISledDocumentService m_documentService;
    }
}
