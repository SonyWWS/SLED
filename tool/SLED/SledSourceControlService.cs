/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IPartImportsSatisfiedNotification))]
    [Export(typeof(ISledSourceControlService))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(SledSourceControlService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledSourceControlService : IInitializable, IPartImportsSatisfiedNotification, ISledSourceControlService, IControlHostClient, ICommandClient, IContextMenuCommandProvider, ISledDocumentPlugin
    {
        [ImportingConstructor]
        public SledSourceControlService(
            MainForm mainForm,
            ICommandService commandService,
            ISettingsService settingsService,
            IControlHostService controlHostService,
            ISledProjectService projectService,
            ISledDocumentService documentService,
            IContextRegistry contextRegistry)
        {
            m_mainForm = mainForm;
            m_projectService = projectService;
            m_documentService = documentService;
            m_contextRegistry = contextRegistry;

            // Associate values for commands the 'public' can use
            m_dictPublicCommands.Add(SledSourceControlCommand.Add, Command.Add);
            m_dictPublicCommands.Add(SledSourceControlCommand.CheckIn, Command.CheckIn);
            m_dictPublicCommands.Add(SledSourceControlCommand.CheckOut, Command.CheckOut);
            m_dictPublicCommands.Add(SledSourceControlCommand.Refresh, Command.Refresh);
            m_dictPublicCommands.Add(SledSourceControlCommand.Revert, Command.Revert);
            m_dictPublicCommands.Add(SledSourceControlCommand.History, Command.History);

            commandService.RegisterMenu(
                Menu.SourceControl,
                SourceControlText,
                "Source Control Menu");

            try
            {
                m_go = ResourceUtil.GetImageList16().Images[SledIcon.Go];
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception acquiring \"{0}\" image: {1}",
                    SledIcon.Go, ex.Message);

                m_go = null;
            }

            try
            {
                m_stop = ResourceUtil.GetImageList16().Images[SledIcon.Stop];
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception acquiring \"{0}\" image: {1}",
                    SledIcon.Stop, ex.Message);

                m_stop = null;
            }

            m_status =
                commandService.RegisterCommand(
                    Command.Enabled,
                    Menu.SourceControl,
                    Group.SourceControl,
                    "No source control plugin found!",
                    "Enable or disable using the source control plugin",
                    Keys.None,
                    SledIcon.Stop,
                    CommandVisibility.All,
                    this);

            commandService.RegisterCommand(
                Command.Add,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Add",
                "Add to source control",
                Keys.None,
                Atf.Resources.DocumentAddImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.Revert,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Revert",
                "Revert add or check out from source control",
                Keys.None,
                Atf.Resources.DocumentRevertImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.CheckIn,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Check In",
                "Check in to source control",
                Keys.None,
                Atf.Resources.DocumentLockImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.CheckOut,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Check Out",
                "Check out from source control",
                Keys.None,
                Atf.Resources.DocumentCheckOutImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.Sync,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Get Latest Version",
                "Get latest version from source control",
                Keys.None,
                Atf.Resources.DocumentGetLatestImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.Refresh,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Refresh Status",
                "Refresh status in source control",
                Keys.None,
                Atf.Resources.DocumentRefreshImage,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.History,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "History",
                "Obtain history of items in source control",
                Keys.None,
                Atf.Resources.ResourceFolderImage,
                CommandVisibility.All,
                this);

            //if ((RegisterCommands & CommandRegister.Reconcile) == CommandRegister.Reconcile)
            //{
            //    CommandServices.RegisterCommand(m_commandService,
            //        InternalCommand.Reconcile,
            //        StandardMenu.File,
            //        StandardCommandGroup.FileOther,
            //        Localizer.Localize("Source Control/Reconcile Offline Work..."),
            //        Localizer.Localize("Reconcile Offline Work"),
            //        Keys.None,
            //        Resources.DocumentRefreshImage,
            //        CommandVisibility.Menu,
            //        this);
            //}

            commandService.RegisterCommand(
                Command.Connection,
                Menu.SourceControl,
                Group.SourceControl,
                SourceControlCommandPrefix + "Open Connection...",
                "Source control connection",
                Keys.None,
                Atf.Resources.ShowAllImage,
                CommandVisibility.All,
                this);

            // Persist settings
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => PersistedSettings,
                    SettingsDisplayName,
                    SettingsCategory,
                    SettingsDescription));

            documentService.Opened += DocumentServiceOpened;
            documentService.Closing += DocumentServiceClosing;

            projectService.FileAdded += ProjectServiceFileAdded;
            projectService.FileRemoving += ProjectServiceFileRemoving;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
        }

        #endregion

        #region IPartImportsSatisfiedNotification Interface

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (SourceControlService == null)
                return;

            SourceControlService.StatusChanged += SourceControlServiceStatusChanged;

            // Don't lock files
            SourceControlService.AllowMultipleCheckout = true;
        }

        #endregion

        #region ISledSourceControlService Interface

        public bool CanUseSourceControl
        {
            get
            {
                return
                    (SourceControlService != null) &&
                    (SourceControlService.Enabled) &&
                    SourceControlService.IsConnected;
            }
        }

        public SourceControlStatus GetStatus(IResource sc)
        {
            if (sc == null)
                return SourceControlStatus.FileDoesNotExist;

            return
                !CanUseSourceControl
                    ? SourceControlStatus.FileDoesNotExist
                    : SourceControlService.GetStatus(sc.Uri);
        }

        public bool CanDoCommand(SledSourceControlCommand command, ISourceControlContext context)
        {
            return CanDoCommandHelper(m_dictPublicCommands[command], context);
        }

        public bool DoCommand(SledSourceControlCommand command, ISourceControlContext context)
        {
            return DoCommandHelper(m_dictPublicCommands[command], context);
        }

        public event EventHandler<SourceControlEventArgs> StatusChanged;

        #endregion

        #region IControlHostClient Interface

        public void Activate(Control control)
        {
        }

        public void Deactivate(Control control)
        {
        }

        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        #region ICommandClient Interface

        enum Command
        {
            Add,
            Revert,
            CheckIn,
            CheckOut,
            Refresh,
            Sync,
            History,

            Enabled,
            Connection,
        }

        enum Menu
        {
            SourceControl,
        }

        enum Group
        {
            SourceControl,
        }

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            return
                CanDoCommandHelper(
                    (Command)commandTag,
                    m_contextRegistry.GetMostRecentContext<ISourceControlContext>());
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            DoCommandHelper(
                (Command)commandTag,
                m_contextRegistry.GetMostRecentContext<ISourceControlContext>());
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is Command))
                return;

            if (SourceControlService == null)
                return;

            switch ((Command)commandTag)
            {
                case Command.Enabled:
                {
                    state.Text =
                        SourceControlService.Enabled
                            ? SourceControlEnabledText
                            : SourceControlDisabledText;

                    state.Check = SourceControlService.Enabled;

                    var image =
                        (SourceControlService.Enabled && SourceControlService.IsConnected)
                            ? m_go
                            : m_stop;

                    if (!ReferenceEquals(m_status.GetButton().Image, image))
                        m_status.GetButton().Image = image;
                }
                break;
            }
        }

        #endregion

        #region IContextMenuCommandProvider Interface

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (SourceControlService == null)
                return s_emptyEnumerable;

            if (!SourceControlService.Enabled)
                return s_emptyEnumerable;

            if (!SourceControlService.IsConnected)
                return new object[] { Command.Connection };

            var sourceControlContext =
                context.As<ISourceControlContext>();

            if (sourceControlContext == null)
                return s_emptyEnumerable;

            var commands = new List<object> { Command.Refresh };

            foreach (var status in sourceControlContext.Resources.Select(resource => GetStatus(resource)))
            {
                switch (status)
                {
                    case SourceControlStatus.Added:
                        commands.Add(Command.Revert);
                        commands.Add(Command.CheckIn);
                        break;

                    case SourceControlStatus.CheckedIn:
                        commands.Add(Command.CheckOut);
                        commands.Add(Command.Sync);
                        commands.Add(Command.History);
                        break;

                    case SourceControlStatus.CheckedOut:
                        commands.Add(Command.Revert);
                        commands.Add(Command.CheckIn);
                        commands.Add(Command.History);
                        break;

                    case SourceControlStatus.NotControlled:
                        commands.Add(Command.Add);
                        break;
                }
            }

            return commands.Distinct();
        }

        #endregion

        #region ISledDocumentPlugin Interface

        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            return GetCommands(m_documentService, args.Document).ToList();
        }

        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return null;
        }

        #endregion

        #region Persisted Settings

        public string PersistedSettings
        {
            get
            {
                // Generate Xml string to contain the Mru project list
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                var root = xmlDoc.CreateElement(PersistedSettingsElement);
                xmlDoc.AppendChild(root);

                try
                {
                    var sourceControlEnabled =
                        (SourceControlService != null) && SourceControlService.Enabled;

                    var elem = xmlDoc.CreateElement(SettingsCategory);
                    elem.SetAttribute(SettingsEnabledAttribute, sourceControlEnabled.ToString());
                    root.AppendChild(elem);

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    const string szSetting =
                        PersistedSettingsElement;

                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "Exception saving {0} settings: {1}",
                        szSetting, ex.Message);
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    var nodes = xmlDoc.DocumentElement.SelectNodes(SettingsCategory);
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        if (!elem.HasAttribute(SettingsEnabledAttribute))
                            continue;

                        var enabledAttribute =
                            elem.GetAttribute(SettingsEnabledAttribute);

                        bool enabled;
                        if (bool.TryParse(enabledAttribute, out enabled))
                        {
                            if (SourceControlService != null)
                                SourceControlService.Enabled = enabled;
                        }
                    }
                }
                catch (Exception ex)
                {
                    const string szSetting =
                        PersistedSettingsElement;

                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "Exception loading {0} settings: {1}",
                        szSetting, ex.Message);
                }
            }
        }

        #endregion

        #region ISledDocument Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            e.Document.DirtyChanged += SledDocumentDirtyChanged;
        }

        private void DocumentServiceClosing(object sender, SledDocumentServiceEventArgs e)
        {
            e.Document.DirtyChanged -= SledDocumentDirtyChanged;
        }

        private void SledDocumentDirtyChanged(object sender, EventArgs e)
        {
            var sd = sender.As<ISledDocument>();
            if (sd == null)
                return;

            if (sd.SledProjectFile == null)
                return;

            EnsureCheckedOutOnEdit(sd);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            EnsureCheckedOutOnEdit(e.File.Project);
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            EnsureCheckedOutOnEdit(e.File.Project);
        }

        #endregion

        #region Member Methods

        private void SourceControlServiceStatusChanged(object sender, SourceControlEventArgs e)
        {
            StatusChanged.Raise(this, e);
        }

        private bool DoCheckOut(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            var lstNeedResolves = new List<IResource>();

            using (new ProgressBarWrapper("Checking out..."))
            {
                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    "{0}: Refreshing {1} item(s)...",
                    SourceControlText, resources.Count());

                SourceControlService.RefreshStatus(resources.Select(r => r.Uri));

                foreach (var resource in resources)
                {
                    if (!SourceControlService.IsSynched(resource.Uri))
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Info,
                            "{0}: Need user resolution: {1}",
                            SourceControlText, GetUriRelPath(resource));

                        lstNeedResolves.Add(resource);
                    }
                    else
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Info,
                            "{0}: Checking out: {1}",
                            SourceControlText, GetUriRelPath(resource));

                        SourceControlService.CheckOut(resource.Uri);
                    }
                }
            }

            if (lstNeedResolves.Count <= 0)
                return true;

            foreach (var resource in lstNeedResolves)
            {
                using (var dialog = new SledSourceControlResolutionForm())
                {
                    // Default action
                    var choice = SledSourceControlResolutionForm.UserChoice.EditCurrent;

                    dialog.Uri = resource.Uri;
                    if (dialog.ShowDialog(m_mainForm) == DialogResult.OK)
                        choice = dialog.Choice;

                    // Do something based off of choice
                    switch (choice)
                    {
                        case SledSourceControlResolutionForm.UserChoice.EditCurrent:
                        {
                            SledOutDevice.OutLine(
                                SledMessageType.Info,
                                "{0}: Checking out: {1}",
                                SourceControlText, GetUriRelPath(resource));

                            SourceControlService.CheckOut(resource.Uri);
                        }
                        break;

                        case SledSourceControlResolutionForm.UserChoice.GetLatest:
                            DoSync(new[] { resource });
                            break;
                    }
                }
            }

            return true;
        }

        private bool DoCheckIn(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            using (var dialog = new CheckInForm(SourceControlService, resources))
            {
                dialog.Icon = m_mainForm.Icon;
                dialog.ShowDialog(m_mainForm);
            }

            return true;
        }

        private bool DoRevert(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            using (new ProgressBarWrapper("Reverting..."))
            {
                foreach (var resource in resources)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "{0}: Reverting: {1}",
                        SourceControlText, GetUriRelPath(resource));

                    SourceControlService.Revert(resource.Uri);
                }
            }

            return true;
        }

        private bool DoRefresh(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            using (new ProgressBarWrapper("Refreshing..."))
            {
                foreach (var resource in resources)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "{0}: Refreshing: {1}",
                        SourceControlText, GetUriRelPath(resource));

                    SourceControlService.RefreshStatus(resource.Uri);
                }
            }

            return true;
        }

        private bool DoSync(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            using (new ProgressBarWrapper("Syncing..."))
            {
                foreach (var resource in resources)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "{0}: Syncing: {1}",
                        SourceControlText, GetUriRelPath(resource));

                    if (!SourceControlService.IsSynched(resource.Uri))
                        SourceControlService.GetLatestVersion(resource.Uri);
                }
            }

            return true;
        }

        private bool DoHistory(IEnumerable<IResource> resources)
        {
            if (!CanUseSourceControl)
                return false;

            var tables = new DataTable[resources.Count()];
            for (var i = 0; i < tables.Length; i++)
                tables[i] = SourceControlService.GetRevisionLog(resources.ElementAt(i).Uri);

            for (var i = 0; i < tables.Length; i++)
            {
                using (var dialog = new SledSourceControlHistoryForm())
                {
                    dialog.Uri = resources.ElementAt(i).Uri;

                    foreach (DataRow row in tables[i].Rows)
                    {
                        // How row is created in P4 plugin
                        /*
                         * DataRow row = p4DataTable.NewRow();
                         * row["revision"] = Int32.Parse(change);
                         * row["user"] = user;
                         * row["description"] = desc;
                         * row["status"] = status;
                         * row["client"] = client;
                         * row["time"] = date;
                         */

                        // NOTE: These might not match what SVN uses for its row keys (!)

                        dialog.AddEntry(
                            new SledSourceControlHistoryForm.Entry
                                {
                                    Revision = (int)row["revision"],
                                    User = (string)row["user"],
                                    Description = (string)row["description"],
                                    Status = (string)row["status"],
                                    Client = (string)row["client"],
                                    Date = (DateTime)row["time"]
                                });
                    }

                    dialog.ShowDialog(m_mainForm);
                }
            }

            return true;
        }

        private void EnsureCheckedOutOnEdit(IResource resource)
        {
            if (m_ensuringCheckedOut)
                return;

            try
            {
                m_ensuringCheckedOut = true;

                if (!CanUseSourceControl)
                    return;

                var status = GetStatus(resource);

                if ((status == SourceControlStatus.NotControlled) ||
                    (status == SourceControlStatus.FileDoesNotExist) ||
                    (status == SourceControlStatus.Deleted) ||
                    (status == SourceControlStatus.CheckedOut) ||
                    (status == SourceControlStatus.Added))
                    return;

                DoCheckOut(new[] { resource });
            }
            finally
            {
                m_ensuringCheckedOut = false;
            }
        }

        private string GetUriRelPath(IResource resource)
        {
            if (resource == null)
                return string.Empty;

            try
            {
                return
                    SledUtil.GetRelativePath(
                        resource.Uri.LocalPath,
                        m_projectService.AssetDirectory);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception getting relative " + 
                    "path for Uri \"{0}\": {1}",
                    resource.Uri, ex.Message);

                return resource.Uri.LocalPath;
            }
        }

        private bool CanDoCommandHelper(Command cmd, ISourceControlContext context)
        {
            if (SourceControlService == null)
                return false;

            if (cmd == Command.Enabled)
                return true;

            if (cmd == Command.Connection)
                return SourceControlService.Enabled;

            if (!CanUseSourceControl)
                return false;

            if (context == null)
                return false;

            foreach (var status in context.Resources.Select(resource => GetStatus(resource)))
            {
                var bRetval = false;

                switch (cmd)
                {
                    case Command.Add:
                        bRetval = status == SourceControlStatus.NotControlled;
                        break;

                    case Command.Revert:
                        bRetval =
                            (status == SourceControlStatus.Added) ||
                            (status == SourceControlStatus.CheckedOut);
                        break;

                    case Command.CheckIn:
                        bRetval =
                            (status == SourceControlStatus.Added) ||
                            (status == SourceControlStatus.CheckedOut);
                        break;

                    case Command.CheckOut:
                        bRetval = status == SourceControlStatus.CheckedIn;
                        break;

                    case Command.Refresh:
                        bRetval = true;
                        break;

                    case Command.Sync:
                        bRetval = status == SourceControlStatus.CheckedIn;
                        break;

                    case Command.History:
                        bRetval =
                            (status == SourceControlStatus.CheckedIn) ||
                            (status == SourceControlStatus.CheckedOut);
                        break;
                }

                if (bRetval)
                    return true;
            }

            return false;
        }

        private bool DoCommandHelper(Command cmd, ISourceControlContext context)
        {
            if (SourceControlService == null)
                return false;

            if (cmd == Command.Enabled)
            {
                SourceControlService.Enabled = !SourceControlService.Enabled;
                return true;
            }

            if (cmd == Command.Connection)
            {
                SourceControlService.Connect();
                return true;
            }

            if (!CanUseSourceControl)
                return false;

            if (context == null)
                return false;

            switch (cmd)
            {
                case Command.Add:
                    return false;

                case Command.CheckIn:
                    return DoCheckIn(context.Resources);

                case Command.CheckOut:
                    return DoCheckOut(context.Resources);

                case Command.Revert:
                    return DoRevert(context.Resources);

                case Command.Refresh:
                    return DoRefresh(context.Resources);

                case Command.Sync:
                    return DoSync(context.Resources);

                case Command.History:
                    return DoHistory(context.Resources);
            }

            return false;
        }

        #endregion

        #region Private Class

        private class ProgressBarWrapper : IDisposable
        {
            public ProgressBarWrapper(string description)
            {
                m_dialog =
                    new ThreadSafeProgressDialog(
                        true,
                        true,
                        false) { Description = description };
            }

            public void Dispose()
            {
                if (m_dialog != null)
                    m_dialog.Close();
            }

            private readonly ThreadSafeProgressDialog m_dialog;
        }

        #endregion

        private bool m_ensuringCheckedOut;

        private readonly Image m_go;
        private readonly Image m_stop;
        
        private readonly MainForm m_mainForm;
        private readonly CommandInfo m_status;
        private readonly IContextRegistry m_contextRegistry;
        private readonly ISledProjectService m_projectService;
        private readonly ISledDocumentService m_documentService;

        private readonly Dictionary<SledSourceControlCommand, Command> m_dictPublicCommands =
            new Dictionary<SledSourceControlCommand, Command>();

#pragma warning disable 649 // Field is never assigned

        [Import(AllowDefault = true)]
        protected SourceControlService SourceControlService;

#pragma warning restore 649

        private static readonly IEnumerable<object> s_emptyEnumerable =
            EmptyEnumerable<object>.Instance;

        private const string PersistedSettingsElement = "SourceControlSettings";
        private const string SettingsCategory = "SourceControl";
        private const string SettingsDisplayName = "Source Control Settings";
        private const string SettingsDescription = "Source control settings";
        private const string SettingsEnabledAttribute = "Enabled";

        private const string SourceControlText = "Source Control";
        private const string SourceControlCommandPrefix = "Source Control/";

        private const string SourceControlEnabledText = "Source Control Enabled - Click To Disable";
        private const string SourceControlDisabledText = "Source Control Disabled - Click To Enable";
    }
}