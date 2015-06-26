/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledCheckEventsFiredService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledCheckEventsFiredService : IInitializable, ICommandClient
    {
        public SledCheckEventsFiredService()
            : this(Events.None)
        {
        }

        public SledCheckEventsFiredService(Events eventsToWatch)
        {
            m_eventsToWatch = eventsToWatch;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            if (m_eventsToWatch == Events.None)
                return;

            m_commandService.RegisterMenu(Menu.EventsFiredService, "Mark", "Mark");
            m_commandService.RegisterCommand(Command.Mark, Menu.EventsFiredService, Group.EventsFiredService, "Mark", "Mark", Keys.None, null, this);
            m_commandService.RegisterCommand(Command.Clear, Menu.EventsFiredService, Group.EventsFiredService, "Clear", "Clear", Keys.None, null, this);

            if (IsSet(Events.Document))
            {
                m_documentService = SledServiceInstance.Get<ISledDocumentService>();
                m_documentService.Opened += DocumentService_Opened;
                m_documentService.ActiveDocumentChanged += DocumentService_ActiveDocumentChanged;
                m_documentService.Saving += DocumentService_Saving;
                m_documentService.Saved += DocumentService_Saved;
                m_documentService.Closing += DocumentService_Closing;
                m_documentService.Closed += DocumentService_Closed;
            }

            if (IsSet(Events.Project))
            {
                m_projectService = SledServiceInstance.Get<ISledProjectService>();
                m_projectService.Created += ProjectService_Created;
                m_projectService.Opened += ProjectService_Opened;
                m_projectService.AssetDirChanging += ProjectService_AssetDirChanging;
                m_projectService.AssetDirChanged += ProjectService_AssetDirChanged;
                m_projectService.GuidChanging += ProjectService_GuidChanging;
                m_projectService.GuidChanged += ProjectService_GuidChanged;
                m_projectService.NameChanging += ProjectService_NameChanging;
                m_projectService.NameChanged += ProjectService_NameChanged;
                m_projectService.FileAdded += ProjectService_FileAdded;
                m_projectService.FileOpened += ProjectService_FileOpened;
                m_projectService.FileClosing += ProjectService_FileClosing;
                m_projectService.FileClosed += ProjectService_FileClosed;
                m_projectService.FileRenaming += ProjectService_FileRenaming;
                m_projectService.FileRenamed += ProjectService_FileRenamed;
                m_projectService.FolderRenaming += ProjectService_FolderRenaming;
                m_projectService.FolderRenamed += ProjectService_FolderRenamed;
                m_projectService.FolderRemoving += ProjectService_FolderRemoving;
                m_projectService.FolderRemoved += ProjectService_FolderRemoved;
                m_projectService.FileRemoving += ProjectService_FileRemoving;
                m_projectService.FileRemoved += ProjectService_FileRemoved;
                m_projectService.Saved += ProjectService_Saved;
                m_projectService.SavedAsing += m_projectService_SavedAsing;
                m_projectService.SavedAsed += m_projectService_SavedAsed;
                m_projectService.SavingSettings += ProjectService_SavingSettings;
                m_projectService.SavedSettings += ProjectService_SavedSettings;
                m_projectService.Closing += ProjectService_Closing;
                m_projectService.Closed += ProjectService_Closed;
            }

            if (IsSet(Events.FileWatcher))
            {
                m_fileWatcherService = SledServiceInstance.Get<ISledFileWatcherService>();
                m_fileWatcherService.AttributeChangedEvent += FileWatcherService_AttributeChangedEvent;
                m_fileWatcherService.FileChangedEvent += FileWatcherService_FileChangedEvent;
            }

            if (IsSet(Events.Debug))
            {
                m_debugService = SledServiceInstance.Get<ISledDebugService>();
                m_debugService.Connecting += DebugServiceConnecting;
                m_debugService.Connected += DebugServiceConnected;
                m_debugService.PluginsReady += DebugServicePluginsReady;
                m_debugService.Ready += DebugServiceReady;
                m_debugService.Disconnected += DebugServiceDisconnected;
                m_debugService.Error += DebugServiceError;
                m_debugService.DataReady += DebugServiceDataReady;
                m_debugService.BreakpointHitting += DebugServiceBreakpointHitting;
                m_debugService.BreakpointHit += DebugServiceBreakpointHit;
                m_debugService.BreakpointContinue += DebugServiceBreakpointContinue;
                m_debugService.UpdateBegin += DebugServiceUpdateBegin;
                m_debugService.UpdateSync += DebugServiceUpdateSync;
                m_debugService.UpdateEnd += DebugServiceUpdateEnd;
                m_debugService.DebugConnect += DebugServiceDebugConnect;
                m_debugService.DebugDisconnect += DebugServiceDebugDisconnect;
                m_debugService.DebugStart += DebugServiceDebugStart;
                m_debugService.DebugCurrentStatement += DebugServiceDebugCurrentStatement;
                m_debugService.DebugStepInto += DebugServiceDebugStepInto;
                m_debugService.DebugStepOver += DebugServiceDebugStepOver;
                m_debugService.DebugStepOut += DebugServiceDebugStepOut;
                m_debugService.DebugStop += DebugServiceDebugStop;
            }

            if (IsSet(Events.Breakpoint))
            {
                m_breakpointService = SledServiceInstance.Get<ISledBreakpointService>();
                m_breakpointService.Added += BreakpointService_Added;
                m_breakpointService.SilentAdded += BreakpointService_SilentAdded;
                m_breakpointService.Changing += BreakpointService_Changing;
                m_breakpointService.Changed += BreakpointService_Changed;
                m_breakpointService.Removing += BreakpointService_Removing;
            }

            if (IsSet(Events.AtfSettings))
            {
                m_settingsService = SledServiceInstance.Get<ISettingsService>();
                m_settingsService.Loading += SettingsServiceLoading;
                m_settingsService.Reloaded += SettingsServiceReloaded;
                m_settingsService.Saving += SettingsServiceSaving;
            }

            if (IsSet(Events.AtfMainForm))
            {
                m_mainForm = SledServiceInstance.Get<MainForm>();
                m_mainForm.Loading += MainFormLoading;
                m_mainForm.Loaded += MainFormLoaded;
                m_mainForm.Shown += MainFormShown;
            }
        }

        #endregion

        #region ICommandClient Interface

        enum Command
        {
            Mark,
            Clear,
        }

        enum Menu
        {
            EventsFiredService,
        }

        enum Group
        {
            EventsFiredService,
        }

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            bool bEnabled = false;

            switch ((Command)commandTag)
            {
                case Command.Mark:
                case Command.Clear:
                    bEnabled = true;
                    break;
            }

            return bEnabled;
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.Mark:
                    SledOutDevice.OutLine(SledMessageType.Info, "------ Mark ------");
                    break;

                case Command.Clear:
                    SledOutDevice.Clear();
                    break;
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentService_Opened(object sender, SledDocumentServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.Opened");
        }

        private void DocumentService_ActiveDocumentChanged(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.ActiveDocumentChanged");
        }

        private void DocumentService_Saving(object sender, SledDocumentServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.Saving");
        }

        private void DocumentService_Saved(object sender, SledDocumentServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.Saved");
        }

        private void DocumentService_Closing(object sender, SledDocumentServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.Closing");
        }

        private void DocumentService_Closed(object sender, SledDocumentServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledDocumentService.Closed");
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectService_Created(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.Created");
        }

        private void ProjectService_Opened(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.Opened");
        }

        private void ProjectService_AssetDirChanging(object sender, SledProjectServiceAssetDirEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.AssetDirChanging");
        }

        private void ProjectService_AssetDirChanged(object sender, SledProjectServiceAssetDirEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.AssetDirChanged");
        }

        private void ProjectService_GuidChanging(object sender, SledProjectServiceProjectGuidEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.GuidChanging");
        }

        private void ProjectService_GuidChanged(object sender, SledProjectServiceProjectGuidEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.GuidChanged");
        }

        private void ProjectService_NameChanging(object sender, SledProjectServiceProjectNameEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.NameChanging");
        }

        private void ProjectService_NameChanged(object sender, SledProjectServiceProjectNameEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.NameChanged");
        }

        private void ProjectService_FileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileAdded");
        }

        private void ProjectService_FileOpened(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileOpened");
        }

        private void ProjectService_FileClosing(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileClosing");
        }

        private void ProjectService_FileClosed(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileClosed");
        }

        private void ProjectService_FileRenaming(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileRenaming");
        }

        private void ProjectService_FileRenamed(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileRenamed");
        }

        private void ProjectService_FolderRenaming(object sender, SledProjectServiceFolderEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FolderRenaming");
        }

        private void ProjectService_FolderRenamed(object sender, SledProjectServiceFolderEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FolderRenamed");
        }

        private void ProjectService_FolderRemoving(object sender, SledProjectServiceFolderEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FolderRemoving");
        }

        private void ProjectService_FolderRemoved(object sender, SledProjectServiceFolderEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FolderRemoved");
        }

        private void ProjectService_FileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileRemoving");
        }

        private void ProjectService_FileRemoved(object sender, SledProjectServiceFileEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.FileRemoved");
        }

        private void ProjectService_Saved(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.Saved");
        }

        private void m_projectService_SavedAsing(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.SavedAsing");
        }

        private void m_projectService_SavedAsed(object sender, SledProjectServiceProjectSaveAsEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.SavedAsed");
        }

        private void ProjectService_SavingSettings(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.SavingSettings");
        }

        private void ProjectService_SavedSettings(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.SavedSettings");
        }

        private void ProjectService_Closing(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.Closing");
        }

        private void ProjectService_Closed(object sender, SledProjectServiceProjectEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledProjectService.Closed");
        }

        #endregion

        #region ISledFileWatcherService Events

        private void FileWatcherService_AttributeChangedEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledFileWatcherService.AttributeChangedEvent");
        }

        private void FileWatcherService_FileChangedEvent(object sender, SledFileWatcherServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledFileWatcherService.FileChangedEvent");
        }

        #endregion

        #region IDebugService Events

        private void DebugServiceConnecting(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.Connecting");
        }

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.Connected");
        }

        private void DebugServicePluginsReady(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.PluginsReady");
        }

        private void DebugServiceReady(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.Ready");
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.Disconnected");
        }

        private void DebugServiceError(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.Error");
        }

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            // Gets too spammy with this one
            //SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DataReady");
        }

        private void DebugServiceBreakpointHitting(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.BreakpointHitting");
        }

        private void DebugServiceBreakpointHit(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.BreakpointHit");
        }

        private void DebugServiceBreakpointContinue(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.BreakpointContinue");
        }

        private void DebugServiceUpdateBegin(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.UpdateBegin");
        }

        private void DebugServiceUpdateSync(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.UpdateSync");
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.UpdateEnd");
        }

        private void DebugServiceDebugConnect(object sender, SledDebugServiceEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugConnect");
        }

        private void DebugServiceDebugDisconnect(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugDisconnect");
        }

        private void DebugServiceDebugStart(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugStart");
        }

        private void DebugServiceDebugCurrentStatement(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugCurrentStatement");
        }

        private void DebugServiceDebugStepInto(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugStepInto");
        }

        private void DebugServiceDebugStepOver(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugStepOver");
        }

        private void DebugServiceDebugStepOut(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugStepOut");
        }

        private void DebugServiceDebugStop(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "DebugService.DebugStop");
        }

        #endregion

        #region ISledBreakpointService Events

        private void BreakpointService_Added(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledBreakpointService.Added");
        }

        private void BreakpointService_SilentAdded(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledBreakpointService.SilentAdded");
        }

        private void BreakpointService_Changing(object sender, SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledBreakpointService.Changing");
        }

        private void BreakpointService_Changed(object sender, SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledBreakpointService.Changed");
        }

        private void BreakpointService_Removing(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISledBreakpointService.Removing");
        }

        #endregion

        #region ISettingsService Events

        private void SettingsServiceLoading(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISettingsService.Loading");
        }

        private void SettingsServiceReloaded(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISettingsService.Reloaded");
        }

        private void SettingsServiceSaving(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "ISettingsService.Saving");
        }

        #endregion

        #region MainForm Events

        private void MainFormLoading(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "MainForm.Loading");
        }

        private void MainFormLoaded(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "MainForm.Loaded");
        }

        private void MainFormShown(object sender, EventArgs e)
        {
            SledOutDevice.OutLine(SledMessageType.Info, "MainForm.Shown");
        }

        #endregion

        [Flags]
        public enum Events
        {
            None        = 0,
            Document    = 1,
            Project     = 2,
            FileWatcher = 4,
            Debug       = 8,
            Breakpoint  = 16,
            AtfSettings = 32,
            AtfMainForm = 64,
            All         = Document | Project | FileWatcher | Debug | Breakpoint | AtfSettings | AtfMainForm,
        }

        private bool IsSet(Events eventToCheck)
        {
            return (m_eventsToWatch & eventToCheck) == eventToCheck;
        }

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ICommandService m_commandService;

#pragma warning restore 649

        private ISettingsService m_settingsService;
        private MainForm m_mainForm;

        private ISledDocumentService m_documentService;
        private ISledProjectService m_projectService;
        private ISledFileWatcherService m_fileWatcherService;
        private ISledDebugService m_debugService;
        private ISledBreakpointService m_breakpointService;

        private readonly Events m_eventsToWatch;
    }
}