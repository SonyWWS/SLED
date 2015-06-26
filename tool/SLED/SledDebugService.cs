/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledDebugService))]
    [Export(typeof(SledDebugService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugService : IInitializable, ISledDebugService
    {
        [ImportingConstructor]
        public SledDebugService(
            MainForm mainForm,
            IStatusService statusService,
            ICommandService commandService)
        {
            m_mainForm = mainForm;

            m_connectStatus = statusService.AddText(350);
            m_connectStatus.Text = Localization.SledDisconnected;

            m_recvBuf.Reset();
            m_curTarget = null;
            m_netPlugin = null;

            SetEndianness(Endian.Unknown);

            // Create a new menu for debugging options
            commandService.RegisterMenu(
                Menu.Debug,
                Localization.SledDebugMenuTitle,
                Localization.SledDebugMenuTitleComment);

            // Command to start debugging
            commandService.RegisterCommand(
                Command.Start,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuStart,
                Localization.SledDebugMenuStartComment,
                Keys.F5,
                SledIcon.DebugStart,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.CurrentStatement,
                Menu.Debug,
                CommandGroup.Debug,
                "&Current Statement",
                "Jump to current statement",
                Keys.Alt | Keys.Multiply,
                SledIcon.BreakpointCsi,
                CommandVisibility.All,
                this);

            // Command to step through a line while debugging
            commandService.RegisterCommand(
                Command.StepInto,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuStep,
                Localization.SledDebugMenuStepComment,
                Keys.F11,
                SledIcon.DebugStepInto,
                CommandVisibility.All,
                this);

            // Command to step over while debugging
            commandService.RegisterCommand(
                Command.StepOver,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuStepOver,
                Localization.SledDebugMenuStepOverComment,
                Keys.F10,
                SledIcon.DebugStepOver,
                CommandVisibility.All,
                this);

            // Command to step out while debugging
            commandService.RegisterCommand(
                Command.StepOut,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuStepOut,
                Localization.SledDebugMenuStepOutComment,
                Keys.Shift | Keys.F11,
                SledIcon.DebugStepOut,
                CommandVisibility.All,
                this);

            // Command to stop debugging
            commandService.RegisterCommand(
                Command.Stop,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuStop,
                Localization.SledDebugMenuStopComment,
                Keys.Shift | Keys.F5,
                SledIcon.DebugStop,
                CommandVisibility.All,
                this);

            // Command to connect to a remote target
            commandService.RegisterCommand(
                Command.Connect,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuConnect,
                Localization.SledDebugMenuConnectComment,
                Keys.F7,
                SledIcon.DebugConnect,
                CommandVisibility.All,
                this);

            // Command to disconnect from a remote target
            commandService.RegisterCommand(
                Command.Disconnect,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuDisconnect,
                Localization.SledDebugMenuDisconnectComment,
                Keys.F8,
                SledIcon.DebugDisconnect,
                CommandVisibility.All,
                this);

            // Toggle a single breakpoint on/off
            commandService.RegisterCommand(
                Command.ToggleBreakpoint,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledProjectMenuToggleBreakpoint,
                Localization.SledProjectMenuToggleBreakpointComment,
                Keys.F9,
                SledIcon.ProjectToggleBreakpoint,
                CommandVisibility.Menu,
                this);

            RegisterDebugCommands(commandService, this);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugFreezeService = SledServiceInstance.Get<SledDebugFreezeService>();

            m_projectService.Get.Closing += ProjectServiceClosing;
        }

        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        private static void RegisterDebugCommands(ICommandService commandService, ICommandClient client)
        {
            // Enable these two commands if you want to log the SCMP, and libscriptdebugger.cpp also has some logging you can
            // enable.
            commandService.RegisterCommand(
                Command.ScmpLogging,
                Menu.Debug,
                CommandGroup.Debug,
                Localization.SledDebugMenuScmpProtocolLogging,
                Localization.SledDebugMenuScmpProtocolLoggingComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                client);

            //m_commandService.RegisterCommand(
            //    Command.ScmpSendMark,
            //    Menu.Debug,
            //    CommandGroup.Debug,
            //    Localization.SledDebugMenuScmpProtocolMark,
            //    Localization.SledDebugMenuScmpProtocolMarkComment,
            //    Keys.None,
            //    null,
            //    CommandVisibility.Menu,
            //    this);
        }

        #region Command, Menu, CommandGroup

        enum Menu
        {
            Debug,
        }

        enum Command
        {
            Connect,        // Command to try and connect to remote target
            Disconnect,     // Command to disconnect from remote target (will stop debugging too)
            Start,          // Command to start debugging/executing code on remote target (will connect if not connected also)
            CurrentStatement,
            Stop,           // Command to stop debugging
            StepInto,       // Command to step into a line while debugging
            StepOver,       // Command to step over a line
            StepOut,        // Command to step out of a function
            ScmpLogging,    // Command to turn on protocol debugging
            ScmpSendMark,   // Command to help debug protocol messages when viewing with SledProtocolReader

            // Random command... putting it here for now.
            ToggleBreakpoint,   // Command to toggle a breakpoint on/off on a line in the editor
        }

        enum CommandGroup
        {
            Debug,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.Connect:
                        bEnabled = !IsConnected && !IsConnecting && m_projectService.Get.Active;
                        //m_netToolbarConnect.Value = bEnabled;
                        break;

                    case Command.Disconnect:
                        bEnabled = IsConnected;
                        //m_netToolbarDisconnect.Value = bEnabled;
                        break;

                    case Command.Start:
                        bEnabled = IsConnected && !IsDebugging;
                        //m_netToolbarDebugStart.Value = bEnabled;
                        break;

                    case Command.CurrentStatement:
                        bEnabled = IsConnected && !IsDebugging;
                        //m_netToolbarCurrentStatementIndicator.Value = bEnabled;
                        break;

                    case Command.StepInto:
                        bEnabled = IsConnected && !IsDebugging;
                        //m_netToolbarDebugStepInto.Value = bEnabled;
                        break;

                    case Command.StepOver:
                        bEnabled = IsConnected && !IsDebugging;
                        //m_netToolbarDebugStepOver.Value = bEnabled;
                        break;

                    case Command.StepOut:
                        bEnabled = IsConnected && !IsDebugging;
                        //m_netToolbarDebugStepOut.Value = bEnabled;
                        break;

                    case Command.Stop:
                        bEnabled = IsConnected && IsDebugging;
                        //m_netToolbarDebugStop.Value = bEnabled;
                        break;

                    case Command.ScmpLogging:
                        bEnabled = !IsConnected;
                        break;

                    case Command.ScmpSendMark:
                        bEnabled = IsConnected;
                        break;

                    case Command.ToggleBreakpoint:
                    {
                        var sd = m_documentService.Get.ActiveDocument;
                        if (sd != null)
                            bEnabled = sd.IsValidLine(sd.Editor.CurrentLineNumber);
                    }
                    break;
                }
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command) commandTag)
            {
                case Command.Start:
                    Start();
                    break;

                case Command.CurrentStatement:
                    CurrentStatement();
                    break;

                case Command.Stop:
                    Stop();
                    break;

                case Command.StepInto:
                    StepInto();
                    break;

                case Command.StepOver:
                    StepOver();
                    break;

                case Command.StepOut:
                    StepOut();
                    break;

                case Command.Connect:
                    Connect();
                    break;

                case Command.Disconnect:
                    Disconnect();
                    break;

                case Command.ScmpLogging:
                    s_bScmpLoggingEnabled = !s_bScmpLoggingEnabled;
                    break;

                case Command.ScmpSendMark:
                    break;

                case Command.ToggleBreakpoint:
                {
                    var sd = m_documentService.Get.ActiveDocument;
                    if (sd != null)
                    {
                        if (sd.IsValidLine(sd.Editor.CurrentLineNumber))
                        {
                            var bSetOrUnSet = sd.IsBreakpointSet(sd.Editor.CurrentLineNumber);
                            sd.Editor.Breakpoint(sd.Editor.CurrentLineNumber, !bSetOrUnSet);
                        }
                    }
                }
                break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.ScmpLogging:
                    state.Check = s_bScmpLoggingEnabled;
                    break;
            }
        }

        #endregion

        #region ISledDebugService Interface

        /// <summary>
        /// Unique Id of SLED - should always be "0" (zero)
        /// </summary>
        public UInt16 SledPluginId
        {
            get { return 0; }
        }

        /// <summary>
        /// Returns true if connecting to a target
        /// </summary>
        public bool IsConnecting { get; private set; }

        /// <summary>
        /// Returns true if connected to a target
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Returns true if not connecting and not connected
        /// </summary>
        public bool IsDisconnected
        {
            get { return (!IsConnecting && !IsConnected); }
        }

        /// <summary>
        /// Returns true if debugging
        /// </summary>
        public bool IsDebugging { get; private set; }

        /// <summary>
        /// Grab data from the network buffer as a specific Scmp structure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetScmpBlob<T>() where T : Shared.Scmp.IScmp, new()
        {
            var scmp = new T();
            scmp.Unpack(m_recvBuf.Buffer);

            // This check significantly slows things down so leave it
            // off unless actively adding/testing new protocol messages
            //VerifyScmpAttribute(scmp, "Sce.Sled.Shared.Scmp.ScmpReceive");

            return scmp;
        }

        /// <summary>
        /// Send a Scmp structure
        /// </summary>
        /// <param name="scmp">Data to send</param>
        /// <returns>Length of data sent</returns>
        public int SendScmp(Shared.Scmp.IScmp scmp)
        {
            // This check significantly slows things down so leave it
            // off unless actively adding/testing new protocol messages
            //VerifyScmpAttribute(scmp, "Sce.Sled.Shared.Scmp.ScmpSend");

            if ((m_netPlugin != null) && m_netPlugin.IsConnected)
            {
                var buffer = scmp.Pack();

                LogScmp(ScmpLogType.Send, buffer);

                return m_netPlugin.Send(buffer, buffer.Length);
            }

            return -1;
        }

        /// <summary>
        /// Returns true after the UpdateBegin packet has been received
        /// and false once the UpdateEnd packet has been received.
        /// </summary>
        public bool IsUpdateInProgress { get; private set; }

        /// <summary>
        /// Gets the last debug command that was issued
        /// </summary>
        public DebugCommand LastDebugCmd
        {
            get { return m_lastDebugCmd; }
        }

        /// <summary>
        /// Gets the currently hit breakpoint
        /// </summary>
        public SledNetworkBreakpoint CurrentlyHitBp { get; private set; }

        /// <summary>
        /// Gets whether the current stopped on line of source contains an actual breakpoint or not
        /// <remarks>Step into, step over, step out, and stop debug commands can make execution halt
        /// on arbitrary lines of source which may or may not have actual breakpoints set on them.</remarks>
        /// </summary>
        public bool IsCurrentlyHitBpActualBp { get; private set; }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void VerifyScmpAttribute(Shared.Scmp.IScmp scmp, string attributeAsString)
        {
            var attributes =
                scmp.GetType().GetCustomAttributes(true);

            var bHasAttr =
                attributes.Cast<Attribute>().Any(
                    attr => string.Compare(
                        attr.ToString(),
                        attributeAsString,
                        true) == 0);

            if (bHasAttr)
                return;

            SledOutDevice.OutLine(
                SledMessageType.Warning,
                "[SledDebugService] I just packed or unpacked something " + 
                "[{0}] that didn't have the right attribute [{1}]!",
                scmp.GetType().Name, attributeAsString);

            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Event fired when the socket has connected but before any authentication has taken place
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> Connecting;

        /// <summary>
        /// Event fired when connected to a target and all authentication has been completed
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> Connected;

        /// <summary>
        /// Event fired after "Connected" but before "Ready" to signify plugins on the target are ready
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> PluginsReady;

        /// <summary>
        /// Event fired after "Connected" to signify debugging is ready to start
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> Ready;

        /// <summary>
        /// Event fired when disconnected from a target
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> Disconnected;

        /// <summary>
        /// Event fired when an error is encountered
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> Error;

        /// <summary>
        /// Event fired when data has arrived
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> DataReady;

        /// <summary>
        /// Event fired when a breakpoint is being hit and before the UpdateBegin/UpdateSync/UpdateEnd process
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointHitting;

        /// <summary>
        /// Event fired when a breakpoint is hit and after the UpdateBegin/UpdateSync/UpdateEnd process
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointHit;

        /// <summary>
        /// Event fired when continuing from a breakpoint
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointContinue;

        /// <summary>
        /// Event fired when remote target update has begun
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateBegin;

        /// <summary>
        /// Event fired when remote target update sync point has been reached
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateSync;

        /// <summary>
        /// Event fired when remote target update has ended
        /// </summary>
        public event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateEnd;

        /// <summary>
        /// Event fired when "Connect" is clicked in the debug menu
        /// </summary>
        public event EventHandler<SledDebugServiceEventArgs> DebugConnect;

        /// <summary>
        /// Event fired when "Disconnect" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugDisconnect;

        /// <summary>
        /// Event fired when "Start" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugStart;

        /// <summary>
        /// Event fired when "Current Statement" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugCurrentStatement;

        /// <summary>
        /// Event fired when "Step Into" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugStepInto;

        /// <summary>
        /// Event fired when "Step Over" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugStepOver;

        /// <summary>
        /// Event fired when "Step Out" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugStepOut;

        /// <summary>
        /// Event fired when "Stop" is clicked in the debug menu
        /// </summary>
        public event EventHandler DebugStop;

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            // Disconnect if connected
            Disconnect();
        }

        #endregion

        #region Scmp Receive Buffer Class

        /// <summary>
        /// Scmp Receive Buffer
        /// </summary>
        private class ScmpReceiveBuffer
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ScmpReceiveBuffer()
            {
                m_buffer = new byte[BufMaxSize];
                Size = 0;
            }

            /// <summary>
            /// Get the size of the buffer
            /// </summary>
            public int Size { get; private set; }

            /// <summary>
            /// Get the buffer
            /// </summary>
            public byte[] Buffer
            {
                get { return m_buffer; }
            }

            /// <summary>
            /// Reset the buffer
            /// </summary>
            public void Reset()
            {
                Size = 0;
            }

            /// <summary>
            /// Add data to the buffer
            /// </summary>
            /// <param name="buffer">data to add</param>
            public void Append(byte[] buffer)
            {
                if ((Size + buffer.Length) > BufMaxSize)
                    throw new ArgumentOutOfRangeException("buffer", "Total buffer length after adding buffer.Length to current size is greater than the max capacity");

                // Copy over
                for (var i = 0; i < buffer.Length; i++)
                    m_buffer[i + Size] = buffer[i];
                Size += buffer.Length;
            }

            /// <summary>
            /// Shuffle buffer contents "left"
            /// </summary>
            /// <param name="iHowMuch">amount to shuffle "left"</param>
            public void Shuffle(int iHowMuch)
            {
                if (iHowMuch > Size)
                    throw new ArgumentOutOfRangeException("iHowMuch", "Attempted to shuffle buffer \"left\" by more than its internal size");

                var pos = Size - iHowMuch;
                for (var i = 0; i < pos; i++)
                    m_buffer[i] = m_buffer[iHowMuch + i];
                Size -= iHowMuch;
            }

            private const int BufMaxSize = 4096;

            private readonly byte[] m_buffer;
        }

        #endregion

        #region Network Plugin Events

        private void NetPluginConnectingEvent(ISledTarget target)
        {
            // Fake event

            m_curTarget = target;

            SledOutDevice.OutLine(
                SledMessageType.Info,
                SledUtil.TransSub(Localization.SledTargetConnectionNegotiating, target));

            // Fire event
            Connecting.Raise(this, new SledDebugServiceEventArgs(target));
        }

        private void NetPluginConnectedEvent(object sender, ISledTarget target)
        {
            if (!m_bAuthenticated)
            {
                NetPluginConnectingEvent(target);
            }
            else
            {
                IsConnecting = false;
                IsConnected = true;
                IsDebugging = true;

                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    SledUtil.TransSub(Localization.SledTargetConnectionEstablishedTo, target));

                // Update status text
                m_connectStatus.Text = Localization.SledConnected + ": " + target;

                // Fire event
                Connected.Raise(this, new SledDebugServiceEventArgs(target));
            }
        }

        private void NetPluginPluginsReadyEvent(ISledTarget target)
        {
            // Fake event

            // Fire event
            PluginsReady.Raise(this, new SledDebugServiceEventArgs(target));

            // Respond saying we're ready to go
            SendScmp(new Shared.Scmp.Ready(SledPluginId));
        }

        private void NetPluginReadyEvent(ISledTarget target)
        {
            // Fake event

            SledOutDevice.OutLine(
                SledMessageType.Info,
                SledUtil.TransSub(Localization.SledTargetReady, target));

            // Fire event
            Ready.Raise(this, new SledDebugServiceEventArgs(target));
        }

        private void NetPluginDisconnectedEvent(object sender, ISledTarget target)
        {
            try
            {
                m_recvBuf.Reset();

                SetEndianness(Endian.Unknown);

                IsConnected = false;
                IsDebugging = false;
                IsConnecting = false;
                m_bAuthenticated = false;
                IsUpdateInProgress = false;
                CurrentlyHitBp = null;
                IsCurrentlyHitBpActualBp = false;

                Disconnected.Raise(this, new SledDebugServiceEventArgs(target));

                // Update status text
                m_connectStatus.Text = Localization.SledDisconnected;

                if (target != null)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledTargetDisconnectedFrom, target));
                }

                m_curTarget = null;

                // Unsubscribe from events & release resources
                m_netPlugin.ConnectedEvent -= NetPluginConnectedEvent;
                m_netPlugin.DisconnectedEvent -= NetPluginDisconnectedEvent;
                m_netPlugin.DataReadyEvent -= NetPluginDataReadyEvent;
                m_netPlugin.UnHandledExceptionEvent -= NetPluginUnHandledExceptionEvent;
                m_netPlugin.Dispose();
                m_netPlugin = null;
            }
            finally
            {
                Thaw();
            }
        }

        private void NetPluginDataReadyEvent(object sender, byte[] buffer)
        {
            // Process any items lingering in the buffer
            ProcessScmpRecvBuffer(sender);

            LogScmp(ScmpLogType.Receive, buffer);

            try
            {
                // This shouldn't throw now that we catch the problem in Dtlib but lets at least
                // stop annoying people with the stupid crashes from not handling the exception.
                m_recvBuf.Append(buffer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SledOutDevice.OutLine(SledMessageType.Error, ex.Message);

                // Can't continue so force disconnect
                Disconnect();

                return;
            }

            // Process any new items in the buffer
            ProcessScmpRecvBuffer(sender);
        }

        private void NetPluginUnHandledExceptionEvent(object sender, Exception ex)
        {
            SledOutDevice.OutLine(SledMessageType.Error, ex.Message);

            Error.Raise(this, new SledDebugServiceEventArgs(m_curTarget, new[] { ex.Message }));

            NetPluginDisconnectedEvent(sender, m_curTarget);
        }

        #endregion

        #region Scmp Logging

        enum ScmpLogType
        {
            Send,
            Receive,
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CreateScmpLoggingFile()
        {
            if (!s_bScmpLoggingEnabled)
                return;

            lock (s_hScmpFileLock)
            {
                try
                {
                    // Find a file that doesn't exist
                    s_hScmpFile = "ScmpLog" + s_iScmpFileLogNum++ + ".log";
                    while (File.Exists(s_hScmpFile))
                        s_hScmpFile = "ScmpLog" + s_iScmpFileLogNum++ + ".log";

                    using (var stream = File.Create(s_hScmpFile))
                    {
                        stream.Flush();
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Failed to create SCMP logging file: {0}",
                        ex.Message);
                }
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void LogScmp(ScmpLogType type, byte[] buffer)
        {
            if (!s_bScmpLoggingEnabled)
                return;

            lock (s_hScmpFileLock)
            {
                try
                {
                    if (string.IsNullOrEmpty(s_hScmpFile))
                        throw new NullReferenceException("SCMP log file is null or empty!");

                    using (var stream = File.Open(s_hScmpFile, FileMode.Append, FileAccess.Write))
                    {
                        stream.WriteByte(type == ScmpLogType.Receive ? (byte) 1 : (byte) 0);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "SCMP logging error: {0}",
                        ex.Message);
                }
            }
        }

        private static readonly object s_hScmpFileLock = new object();
        private static string s_hScmpFile;
        private static int s_iScmpFileLogNum;
        private static bool s_bScmpLoggingEnabled;

        #endregion

        #region Scmp Message Handling (Version, ProjectDetails, Breakpoint*)

        private void AuthenticateVersion()
        {
            var ver = GetScmpBlob<Shared.Scmp.Version>();
            var verNumber = Shared.Scmp.ScmpExtension.ToInt32(ver);

            var appVer = new Version(Application.ProductVersion);
            var appVerNumber = appVer.ToInt32();

            var minVerNumber = m_minVersion.ToInt32();

            // Allow a range of versions
            if ((verNumber >= minVerNumber) && (verNumber <= appVerNumber))
            {
                // Send success message
                SendScmp(new Shared.Scmp.Success(SledPluginId));
            }
            else
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(
                        Localization.SledRemoteTargetErrorVersionMismatch,
                        appVer.Major, appVer.Minor,
                        m_minVersion.Major, m_minVersion.Minor,
                        ver.Major, ver.Minor));

                // Send failure message
                SendScmp(new Shared.Scmp.Failure(SledPluginId));
            }
        }

        private void HandleBreakpointBegin()
        {
            var bp = GetScmpBlob<Shared.Scmp.BreakpointBegin>();

            NotifyBreakpointHitting(bp);

            // Package up breakpoint
            CurrentlyHitBp = new SledNetworkBreakpoint(bp.BreakPluginId, bp.RelFilePath, bp.Line);

            HandleCurrentlyHitBpActualBp(CurrentlyHitBp);
            HandleBreakpointHitting(CurrentlyHitBp);

            IsUpdateInProgress = true;

            // Fire event
            UpdateBegin.Raise(this, new SledDebugServiceBreakpointEventArgs(CurrentlyHitBp));

            SendScmp(bp);
        }

        private void HandleBreakpointSync()
        {
            var bp = GetScmpBlob<Shared.Scmp.BreakpointSync>();

            // Package up breakpoint
            var netBp = new SledNetworkBreakpoint(bp.BreakPluginId, bp.RelFilePath, bp.Line);

            // Fire event
            UpdateSync.Raise(this, new SledDebugServiceBreakpointEventArgs(netBp));

            SendScmp(bp);
        }

        private void HandleBreakpointEnd()
        {
            try
            {
                var bp = GetScmpBlob<Shared.Scmp.BreakpointEnd>();

                // Fire event
                UpdateEnd.Raise(this, new SledDebugServiceBreakpointEventArgs(CurrentlyHitBp));

                IsUpdateInProgress = false;

                HandleBreakpointHit(CurrentlyHitBp);

                SendScmp(bp);
            }
            finally
            {
                Thaw();
            }
        }

        private void HandleBreakpointHitting(SledNetworkBreakpoint netBp)
        {
            // Fire event
            BreakpointHitting.Raise(this, new SledDebugServiceBreakpointEventArgs(netBp));
        }

        private void HandleBreakpointHit(SledNetworkBreakpoint netBp)
        {
            IsDebugging = false;

            // Fire event
            BreakpointHit.Raise(this, new SledDebugServiceBreakpointEventArgs(netBp));
        }

        private void HandleBreakpointContinue()
        {
            var bp =
                GetScmpBlob<Shared.Scmp.BreakpointContinue>();

            // Fire event
            BreakpointContinue.Raise(this, new SledDebugServiceBreakpointEventArgs(CurrentlyHitBp));

            // Reset
            CurrentlyHitBp = null;
            IsCurrentlyHitBpActualBp = false;

            NotifyBreakpointContinue(bp);
        }

        private void HandleCurrentlyHitBpActualBp(SledNetworkBreakpoint netBp)
        {
            IsCurrentlyHitBpActualBp = false;

            if (netBp.IsUnknownFile())
                return;

            if (!(from file in m_projectService.Get.AllFiles
                  where file.LanguagePlugin != null
                  where file.LanguagePlugin.LanguageId == netBp.LanguageId
                  where string.Compare(file.Path, netBp.File, true) == 0
                  from ibp in file.Breakpoints
                  select ibp).Any(ibp => ibp.Line == netBp.Line))
                return;

            IsCurrentlyHitBpActualBp = true;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void NotifyBreakpointHitting(Shared.Scmp.BreakpointBegin bp)
        {
            SledOutDevice.OutLine(
                SledMessageType.Info,
                SledUtil.TransSub(Resources.Resource.BreakpointStopped, bp.RelFilePath, bp.Line));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void NotifyBreakpointContinue(Shared.Scmp.BreakpointContinue bp)
        {
            SledOutDevice.OutLine(
                SledMessageType.Info,
                SledUtil.TransSub(Resources.Resource.BreakpointContinue, bp.RelFilePath, bp.Line));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void NotifyFreeze()
        {
            SledOutDevice.OutLine(SledMessageType.Info, "SledDebugService: Freeze()");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void NotifyThaw()
        {
            SledOutDevice.OutLine(SledMessageType.Info, "SledDebugService: Thaw()");
        }

        #endregion

        #region Member Methods

        private void Connect()
        {
            var target = m_targetService.Get.SelectedTarget;
            if (target != null)
            {
                Connect(target);
            }
            else
            {
                var result =
                    MessageBox.Show(
                        m_mainForm,
                        Localization.SledRemoteTargetErrorString,
                        Localization.SledRemoteTargetErrorTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                    m_targetService.Get.ShowTargetDlg();
            }
        }

        private void Connect(ISledTarget target)
        {
            CurrentlyHitBp = null;
            m_lastDebugCmd = DebugCommand.Default;

            try
            {
                Freeze();

                if (target == null)
                    throw new NullReferenceException("Target is null!");

                if (target.Plugin == null)
                    throw new NullReferenceException("No network plugin found or specified!");

                // Grab network plugin from target
                m_netPlugin = target.Plugin;

                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    SledUtil.TransSub(Localization.SledTargetConnectingTo, target));

                SetEndianness(Endian.Unknown);
                IsConnecting = true;
                m_recvBuf.Reset();

                CreateScmpLoggingFile();

                // Subscribe to events
                m_netPlugin.ConnectedEvent += NetPluginConnectedEvent;
                m_netPlugin.DisconnectedEvent += NetPluginDisconnectedEvent;
                m_netPlugin.DataReadyEvent += NetPluginDataReadyEvent;
                m_netPlugin.UnHandledExceptionEvent += NetPluginUnHandledExceptionEvent;

                // Fire event
                DebugConnect.Raise(this, new SledDebugServiceEventArgs(target));

                // Try and connect
                m_netPlugin.Connect(target);
            }
            catch (Exception ex)
            {
                NetPluginUnHandledExceptionEvent(m_netPlugin, ex);
            }
        }

        internal void Disconnect()
        {
            if ((m_netPlugin != null) && m_netPlugin.IsConnected)
            {
                // Fire event
                DebugDisconnect.Raise(this, EventArgs.Empty);

                m_netPlugin.Disconnect();
            }
        }

        private void Start()
        {
            if (IsConnected && !IsDebugging)
            {
                IsDebugging = true;
                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledTargetStartCmd);

                Freeze();

                // Fire event
                DebugStart.Raise(this, EventArgs.Empty);

                SendScmp(new Shared.Scmp.DebugStart(SledPluginId));
            }

            m_lastDebugCmd = DebugCommand.Default;
        }

        private void CurrentStatement()
        {
            if (IsConnected && !IsDebugging)
            {
                // Fire event
                DebugCurrentStatement.Raise(this, EventArgs.Empty);
            }
        }

        private void StepInto()
        {
            if (IsConnected && !IsDebugging)
            {
                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledTargetStepIntoCmd);

                Freeze();

                // Fire event
                DebugStepInto.Raise(this, EventArgs.Empty);

                SendScmp(new Shared.Scmp.DebugStepInto(SledPluginId));
            }

            m_lastDebugCmd = DebugCommand.StepInto;
        }

        private void StepOver()
        {
            if (IsConnected && !IsDebugging)
            {
                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledTargetStepOverCmd);

                Freeze();

                // Fire event
                DebugStepOver.Raise(this, EventArgs.Empty);

                SendScmp(new Shared.Scmp.DebugStepOver(SledPluginId));
            }

            m_lastDebugCmd = DebugCommand.StepOver;
        }

        private void StepOut()
        {
            if (IsConnected && !IsDebugging)
            {
                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledTargetStepOutcmd);

                Freeze();

                // Fire event
                DebugStepOut.Raise(this, EventArgs.Empty);

                SendScmp(new Shared.Scmp.DebugStepOut(SledPluginId));
            }

            m_lastDebugCmd = DebugCommand.StepOut;
        }

        private void Stop()
        {
            if (IsConnected && IsDebugging)
            {
                SledOutDevice.OutLine(SledMessageType.Info, Localization.SledTargetStopCmd);

                Freeze();

                // Fire event
                DebugStop.Raise(this, EventArgs.Empty);

                SendScmp(new Shared.Scmp.DebugStop(SledPluginId));
            }

            m_lastDebugCmd = DebugCommand.Stop;
        }

        private void ProcessScmpRecvBuffer(object sender)
        {
            var bCanProcess = true;

            while ((m_recvBuf.Size >= Shared.Scmp.Base.SizeOf) && bCanProcess)
            {
                // Have received at least a Scmp.Base message
                var scmp = new Shared.Scmp.Base();
                scmp.Unpack(m_recvBuf.Buffer);

                // Determine endianness first thing. The first message 
                // that makes it here should be the endianness message.
                // If it is not (or endianness cannot be determined)
                // then we abort.
                if (m_endian == Endian.Unknown)
                {
                    var endianness = GetScmpBlob<Shared.Scmp.Endianness>();
                    SetEndianness(endianness.Order);                    

                    // If endianness is still undetermined then we cannot continue
                    if (m_endian == Endian.Unknown)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            Localization.SledTargetErrorUnknownEndianness);

                        m_recvBuf.Reset();
                        m_netPlugin.Disconnect();
                        break;
                    }

                    // Remove endianness message from buffer
                    m_recvBuf.Shuffle(endianness.Length);

                    // Restart loop to advance "scmp" to next chunk (if a chunk has been received) so we're not 
                    // referencing stale memory and processing the Endianness message more than once.
                    continue;
                }

                if (m_recvBuf.Size >= scmp.Length)
                {
                    var bFlag = false;

                    //Sce.Sled.Shared.SledOutDevice.OutLine(Sce.Sled.Shared.SledMessageType.Info, string.Format("[DebugService] Receiving {0}:{1}", scmp.GetType(), scmp.TypeCode));

                    // Have receive at least one full Scmp message; process it
                    switch (scmp.TypeCode)
                    {
                        case (UInt16)Shared.Scmp.TypeCodes.Disconnect:
                        {
                            m_netPlugin.Disconnect();
                            bFlag = true;
                        }
                        break;

                        case (UInt16)Shared.Scmp.TypeCodes.Authenticated:
                        {
                            m_bAuthenticated = true;
                            NetPluginConnectedEvent(sender, m_curTarget);
                        }
                        break;

                        case (UInt16)Shared.Scmp.TypeCodes.PluginsReady:
                        {
                            NetPluginPluginsReadyEvent(m_curTarget);
                        }
                        break;

                        case (UInt16)Shared.Scmp.TypeCodes.Ready:
                        {
                            NetPluginReadyEvent(m_curTarget);
                        }
                        break;

                        case (UInt16)Shared.Scmp.TypeCodes.Version:
                            AuthenticateVersion();
                            break;

                        case (UInt16)Shared.Scmp.TypeCodes.BreakpointBegin:
                            HandleBreakpointBegin();
                            break;

                        case (UInt16)Shared.Scmp.TypeCodes.BreakpointSync:
                            HandleBreakpointSync();
                            break;

                        case (UInt16)Shared.Scmp.TypeCodes.BreakpointEnd:
                            HandleBreakpointEnd();
                            break;

                        case (UInt16)Shared.Scmp.TypeCodes.BreakpointContinue:
                            HandleBreakpointContinue();
                            break;

                        default:
                        {
                            // Dispatch message
                            DataReady.Raise(this, new SledDebugServiceEventArgs(m_curTarget, scmp));
                        }
                        break;
                    }

                    // Remove this message
                    if (!bFlag)
                    {
                        try
                        {
                            // This shouldn't throw now that we catch the problem in Dtlib but lets at least
                            // stop annoying people with the stupid crashes from not handling the exception.
                            m_recvBuf.Shuffle(scmp.Length);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            SledOutDevice.OutLine(SledMessageType.Error, ex.Message);

                            // Can't continue so force disconnect
                            m_netPlugin.Disconnect();
                            bFlag = true;
                        }
                    }
                }
                else
                {
                    // Not a full message, need to wait until we've received more data
                    bCanProcess = false;
                }
            }
        }

        private void SetEndianness(Endian endian)
        {
            m_endian = endian;
            Shared.Scmp.SledNetworkBufferReader.Endian = endian;
            Shared.Scmp.SledNetworkBufferPacker.Endian = endian;
        }

        private void Freeze()
        {
            m_debugFreezeService.Freeze();
            NotifyFreeze();
        }

        private void Thaw()
        {
            NotifyThaw();
            m_debugFreezeService.Thaw();
        }

        #endregion

        private Endian m_endian;
        private ISledTarget m_curTarget;
        private ISledNetworkPlugin m_netPlugin;
        private readonly IStatusText m_connectStatus;

        private bool m_bAuthenticated;
        private DebugCommand m_lastDebugCmd = DebugCommand.Default;

        private SledDebugFreezeService m_debugFreezeService;

        private readonly MainForm m_mainForm;

        private readonly Version m_minVersion =
            new Version(5, 0);

        private readonly ScmpReceiveBuffer m_recvBuf =
            new ScmpReceiveBuffer();

        private readonly SledServiceReference<ISledTargetService> m_targetService =
            new SledServiceReference<ISledTargetService>();

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();

        private readonly SledServiceReference<ISledDocumentService> m_documentService =
            new SledServiceReference<ISledDocumentService>();

        //private readonly BoolVariable m_netToolbarConnect =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.Connect");

        //private readonly BoolVariable m_netToolbarDisconnect =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.Disconnect");

        //private readonly BoolVariable m_netToolbarCurrentStatementIndicator =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.CurrentStatementIndicator");

        //private readonly BoolVariable m_netToolbarDebugStart =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.DebugStart");

        //private readonly BoolVariable m_netToolbarDebugStepInto =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.DebugStepInto");

        //private readonly BoolVariable m_netToolbarDebugStepOver =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.DebugStepOver");

        //private readonly BoolVariable m_netToolbarDebugStepOut =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.DebugStepOut");

        //private readonly BoolVariable m_netToolbarDebugStop =
        //    new BoolVariable(typeof(SledDebugService) + ".Toolbar.DebugStop");
    }
}