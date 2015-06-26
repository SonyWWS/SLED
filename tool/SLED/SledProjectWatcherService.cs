/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectWatcherService))]
    [Export(typeof(SledProjectWatcherService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledProjectWatcherService : IInitializable, ISledProjectWatcherService
    {
        [ImportingConstructor]
        public SledProjectWatcherService(MainForm mainForm)
        {
            m_mainForm = mainForm;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            var projectService = SledServiceInstance.Get<ISledProjectService>();
            projectService.Created += ProjectServiceCreated;
            projectService.Opened += ProjectServiceOpened;
            projectService.SavedAsing += ProjectServiceSavedAsing;
            projectService.SavedAsed += ProjectServiceSavedAsed;
            projectService.SavingSettings += ProjectServiceSavingSettings;
            projectService.SavedSettings += ProjectServiceSavedSettings;
            projectService.Closing += ProjectServiceClosing;
        }

        #endregion

        #region ISledProjectWatcherService Interface

        /// <summary>
        /// Event fired when the project file is changed
        /// </summary>
        public event EventHandler<SledProjectWatcherServiceEventArgs> FileChangedEvent;

        /// <summary>
        /// Event fired when the project file's attribute is changed
        /// </summary>
        public event EventHandler<SledProjectWatcherServiceEventArgs> AttributeChangedEvent;

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            Create(e.Project);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            Create(e.Project);
        }

        private void ProjectServiceSavedAsing(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            Cleanup();
        }

        private void ProjectServiceSavedAsed(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            Create(e.Project);
        }

        private void ProjectServiceSavingSettings(object sender, SledProjectServiceProjectEventArgs e)
        {
            Disable();
        }

        private void ProjectServiceSavedSettings(object sender, SledProjectServiceProjectEventArgs e)
        {
            Enable(e.Project);
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            Cleanup();
        }

        #endregion

        #region Member Methods

        private void Create(SledProjectFilesType project)
        {
            Cleanup();

            var absPath = project.AbsolutePath;

            // Create new file watcher for the project
            m_watcher =
                new SledFileSystemWatcher
                    {
                        Tag = project,
                        Path = Path.GetDirectoryName(absPath),
                        Filter = Path.GetFileName(absPath),
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes,
                        SynchronizingObject = m_mainForm
                    };
            m_watcher.Changed += WatcherChanged;
            m_watcher.EnableRaisingEvents = true;

            // Get initial stats
            m_fileStats = SledFileSystemFileStats.GetStats(absPath);
        }

        private void Cleanup()
        {
            if (m_watcher == null)
                return;

            m_watcher.Tag = null;
            m_watcher.EnableRaisingEvents = false;
            m_watcher.Changed -= WatcherChanged;
            m_watcher.Dispose();
            m_watcher = null;

            m_fileStats = null;
        }

        private void Enable(SledProjectFilesType project)
        {
            if (m_watcher == null)
                return;

            // Enable watching
            m_watcher.EnableRaisingEvents = true;

            // Refresh stats
            m_fileStats = SledFileSystemFileStats.GetStats(project.AbsolutePath);
        }

        private void Disable()
        {
            if (m_watcher == null)
                return;

            // Disable watching
            m_watcher.EnableRaisingEvents = false;
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

                var project = watcher.Tag as SledProjectFilesType;
                if (project == null)
                    return;

                // Any existing stats on this file?
                var curFileStats = SledFileSystemFileStats.GetStats(project.AbsolutePath);
                if (!curFileStats.Valid)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Exception obtaining file " +
                        "stats on file \"{0}\": {1}",
                        project.AbsolutePath, curFileStats.Exception);

                    return;
                }

                // Figure out which event to hit
                var change = SledFileSystemFileStats.Compare(m_fileStats, curFileStats);

                // Update
                m_fileStats = curFileStats;

                // No change made; don't fire any events
                if (change == SledFileSystemFileStatsChange.Nothing)
                    return;

                // Fire appropriate event
                (change == SledFileSystemFileStatsChange.LastWrite
                     ? FileChangedEvent
                     : AttributeChangedEvent).Raise(this, new SledProjectWatcherServiceEventArgs(project.AbsolutePath));
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[Project Watcher] Exception in WatchChanged event: {0}",
                    ex.Message);
            }
            finally
            {
                m_bChanging = false;
            }
        }

        #endregion

        private bool m_bChanging;

        private readonly MainForm m_mainForm;

        private SledFileSystemWatcher m_watcher;
        private SledFileSystemFileStats m_fileStats;
    }
}
