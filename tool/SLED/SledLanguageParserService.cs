/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

using Timerz = System.Windows.Forms.Timer;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLanguageParserService))]
    [Export(typeof(SledLanguageParserService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledLanguageParserService : IInitializable, ISledLanguageParserService
    {
        [ImportingConstructor]
        public SledLanguageParserService(ISettingsService settingsService)
        {
            Verbosity = SledLanguageParserVerbosity.Overall;

            var verboseProp =
                new BoundPropertyDescriptor(
                    this,
                    () => Verbosity,
                    "Verbosity",
                    null,
                    "Verbosity level");

            // Persist settings
            settingsService.RegisterSettings(this, verboseProp);

            // Add user settings
            settingsService.RegisterUserSettings("Language Parser", verboseProp);

            m_syncContext = SynchronizationContext.Current;

            m_batchTimer = new Timerz { Interval = TimerIntervalMsec };
            m_batchTimer.Tick += BatchTimerTick;
            m_batchTimer.Start();
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_documentService.Saved += DocumentServiceSaved;

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRemoved += ProjectServiceFileRemoved;
            m_projectService.Closed += ProjectServiceClosed;
        }

        #endregion

        #region ISledLanguageParserService Interface

        /// <summary>
        /// Returns whether a syntax check is currently running or not
        /// </summary>
        public bool Running
        {
            get { lock (m_lock) return m_queue.Count > 0; }
        }

        /// <summary>
        /// Gets/sets syntax checking verbosity level
        /// </summary>
        public SledLanguageParserVerbosity Verbosity { get; set; }

        /// <summary>
        /// Schedule a parsing of files
        /// </summary>
        /// <param name="files">Files to parse</param>
        /// <returns>True if files enqueued false if not</returns>
        public bool ParseFilesAsync(IEnumerable<SledProjectFilesFileType> files)
        {
            if (files == null)
                throw new ArgumentNullException("files");

            return Enqueue(files);
        }

        /// <summary>
        /// Register a file parsing function with a particular language plugin and allow optional user data
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="func"></param>
        /// <param name="userData"></param>
        public void RegisterFilesParserFunction(ISledLanguagePlugin plugin, SledLanguageParserFilesParseDelegate func, object userData)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");

            if (func == null)
                throw new ArgumentNullException("func");

            if (m_dictFileCheckFuncs.ContainsKey(plugin))
                return;

            m_dictFileCheckFuncs.Add(plugin, new Pair<SledLanguageParserFilesParseDelegate, object>(func, userData));
        }

        /// <summary>
        /// Event fired after files have been parsed
        /// </summary>
        public event EventHandler<SledLanguageParserEventArgs> FilesParserFinished;

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceSaved(object sender, SledDocumentServiceEventArgs e)
        {
            AddToBatchQueue(new[] { e.Document.SledProjectFile });
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            StartThread();

            AddToBatchQueue(e.Project.AllFiles);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            StartThread();

            AddToBatchQueue(e.Project.AllFiles);
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            AddToBatchQueue(new[] { e.File });
        }

        private void ProjectServiceFileRemoved(object sender, SledProjectServiceFileEventArgs e)
        {
            m_batchQueue.Remove(e.File.Guid);
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            StopThread();

            m_batchQueue.Clear();
            m_queue.Clear();
        }

        #endregion

        #region Member Methods

        private void AddToBatchQueue(IEnumerable<SledProjectFilesFileType> files)
        {
            if (files == null)
                return;

            var validFiles = files.Where(f => f != null);
            if (!validFiles.Any())
                return;

            foreach (var file in validFiles)
            {
                if (!m_batchQueue.ContainsKey(file.Guid))
                    m_batchQueue.Add(file.Guid, file);
            }

            m_lastBatchAdd = DateTime.Now;
        }

        private bool Enqueue(IEnumerable<SledProjectFilesFileType> files)
        {
            if (files == null)
                return false;

            if (files.All(f => f == null))
                return false;

            var retval = false;

            foreach (var kv in m_dictFileCheckFuncs)
            {
                var plugin = kv.Key;

                var pluginFiles = files.Where(f => plugin == f.LanguagePlugin);
                if (!pluginFiles.Any())
                    continue;

                retval = true;

                var func = kv.Value.First;
                var userData = kv.Value.Second;

                lock (m_lock)
                    m_queue.Enqueue(new Args(plugin, pluginFiles, userData, func));
            }

            return retval;
        }

        private void StartThread()
        {
            StopThread();

            m_cancel.Value = false;
            m_thread =
                new Thread(ThreadRun)
                {
                    Name = "SLED LanguageParser Thread",
                    IsBackground = true,
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        private void StopThread()
        {
            if (m_thread == null)
                return;

            // Abort causes exceptions in C++/CLI that apparently can't be caught?
            // ATF UnhandledExceptionService eventually catches the ThreadAbortException
            // but the app gets brought down anyway. So, manually stop the thread.
            //m_thread.Abort();
            m_cancel.Value = true;
            m_threadRunning = false;

            m_thread.Join();
            m_thread = null;
        }

        private void ThreadRun()
        {
            m_threadRunning = true;

            while (m_threadRunning)
            {
                try
                {
                    Thread.Sleep(100);

                    Args arg;
                    lock (m_lock)
                    {
                        if (m_queue.Count <= 0)
                            continue;

                        arg = m_queue.Dequeue();
                    }

                    if (arg == null)
                        continue;

                    var verbosity = Verbosity;
                    var results = arg.Function(arg.Files, verbosity, arg.UserData, m_cancel).ToList();

                    // Associate results to their project file
                    var filesAndResults = new Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>>();
                    foreach (var file in arg.Files)
                    {
                        var localFile = file;

                        var fileResults = new List<SledLanguageParserResult>(results.Where(r => r.File == localFile));
                        filesAndResults.Add(localFile, fileResults);
                    }

                    m_syncContext.Post(
                        obj =>
                        {
                            if (m_threadRunning)
                                AnnounceResults(arg.Plugin, filesAndResults, arg.UserData);
                        }, null);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Unhandled exception in SLED language parser thread: {1}",
                        this, ex.Message);
                }
            }
        }

        private void AnnounceResults(ISledLanguagePlugin plugin, Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>> filesAndResults, object userData)
        {
            if (!m_threadRunning)
                return;

            if ((filesAndResults == null) || (m_thread == null))
                return;

            if (m_projectService == null)
                return;

            if (!m_projectService.Active)
                return;

            FilesParserFinished.Raise(
                this,
                new SledLanguageParserEventArgs(plugin, filesAndResults, userData));
        }

        private void BatchTimerTick(object sender, EventArgs e)
        {
            if (m_batchQueue.Count <= 0)
                return;

            var span = DateTime.Now.Subtract(m_lastBatchAdd);
            if (span.Milliseconds <= TimerIntervalMsec)
                return;

            var amount = Math.Min(BatchThreshold, m_batchQueue.Count);

            var items = m_batchQueue
                .Take(amount)
                .Select(kv => kv.Value)
                .ToList();

            foreach (var item in items)
                m_batchQueue.Remove(item.Guid);

            Enqueue(items);
        }

        private static int BatchThreshold
        {
            get { return Environment.ProcessorCount * 4; }
        }

        #endregion

        private Thread m_thread;
        private DateTime m_lastBatchAdd;

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledDocumentService m_documentService;

        [Import]
        private ISledProjectService m_projectService;

#pragma warning restore 649

        private readonly Timerz m_batchTimer;
        private readonly SynchronizationContext m_syncContext;

        private volatile bool m_threadRunning;

        private volatile object m_lock =
            new object();

        private readonly Queue<Args> m_queue =
            new Queue<Args>();

        private readonly SledUtil.BoolWrapper m_cancel =
            new SledUtil.BoolWrapper();

        private readonly Dictionary<Guid, SledProjectFilesFileType> m_batchQueue =
            new Dictionary<Guid, SledProjectFilesFileType>();

        private readonly Dictionary<ISledLanguagePlugin, Pair<SledLanguageParserFilesParseDelegate, object>> m_dictFileCheckFuncs =
            new Dictionary<ISledLanguagePlugin, Pair<SledLanguageParserFilesParseDelegate, object>>();

        private const int TimerIntervalMsec = 100;

        #region Private Classes

        private class Args
        {
            public Args(ISledLanguagePlugin plugin, IEnumerable<SledProjectFilesFileType> files, object userData, SledLanguageParserFilesParseDelegate func)
            {
                Plugin = plugin;
                Files = new List<SledProjectFilesFileType>(files);
                UserData = userData;
                Function = func;
            }

            public ISledLanguagePlugin Plugin { get; private set; }

            public IEnumerable<SledProjectFilesFileType> Files { get; private set; }

            public object UserData { get; private set; }

            public SledLanguageParserFilesParseDelegate Function { get; private set; }
        }

        #endregion
    }
}