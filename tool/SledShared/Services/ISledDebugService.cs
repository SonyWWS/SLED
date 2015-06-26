/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Net;

using Sce.Atf.Applications;

using Sce.Sled.Shared.Plugin;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// Interface for remote machine to connect to
    /// </summary>
    public interface ISledTarget : ICloneable
    {
        /// <summary>
        /// Get or set target name
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set target details
        /// </summary>
        IPEndPoint EndPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set network plugin that this target uses
        /// </summary>
        ISledNetworkPlugin Plugin
        {
            get;
            set;
        }

        /// <summary>
        /// Get whether this target is imported from a network plugin or is user created
        /// </summary>
        bool Imported
        {
            get;
        }

        /// <summary>
        /// Convert ISledTarget to string representation
        /// </summary>
        /// <returns>String representation of ISledTarget</returns>
        string ToString();
    }

    /// <summary>
    /// Convenience class using ISledTarget for clients to derive from
    /// </summary>
    public abstract class SledTargetBase : ISledTarget
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="endPoint">IPEndPoint</param>
        /// <param name="plugin">Network plugin</param>
        protected SledTargetBase(string name, IPEndPoint endPoint, ISledNetworkPlugin plugin)
        {
            Name = name;
            EndPoint = endPoint;
            Plugin = plugin;
        }

        #region ISledTarget Interface

        /// <summary>
        /// Get or set target name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set target details
        /// </summary>
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// Get or set network plugin that this target uses
        /// </summary>
        public ISledNetworkPlugin Plugin { get; set; }

        /// <summary>
        /// Get or set whether this target is imported from a network plugin or is user created
        /// </summary>
        public bool Imported { get; set; }

        /// <summary>
        /// Convert ISledTarget to string representation
        /// </summary>
        /// <returns>String representation of ISledTarget</returns>
        public override string ToString()
        {
            var protocol = 
                Plugin == null
                    ? "Unknown"
                    : Plugin.Protocol;

            return 
                string.Format("{0} ({1}:{2} - {3})",
                    Name,
                    EndPoint.Address,
                    EndPoint.Port,
                    protocol);
        }

        #endregion

        #region ICloneable Interface

        /// <summary>
        /// Clone the ISledTarget
        /// </summary>
        /// <returns>New ISledTarget</returns>
        public abstract object Clone();

        #endregion
    }

    /// <summary>
    /// Sled debug service event arguments
    /// </summary>
    public class SledDebugServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">Target</param>
        public SledDebugServiceEventArgs(ISledTarget target)
            : this(target, null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="msg">Message</param>
        public SledDebugServiceEventArgs(ISledTarget target, string[] msg)
            : this(target, msg, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="scmp">SLED Control Message Protocol (SCMP) payload</param>
        public SledDebugServiceEventArgs(ISledTarget target, Scmp.IScmp scmp)
            : this(target, null, scmp)
        {
        }

        private SledDebugServiceEventArgs(ISledTarget target, string[] msg, Scmp.IScmp scmp)
        {
            m_target = target;
            m_msg = msg;
            m_scmp = scmp;
        }

        /// <summary>
        /// Get the target 
        /// </summary>
        public ISledTarget Target
        {
            get { return m_target; }
        }

        /// <summary>
        /// Get message (error messages or exception text)
        /// </summary>
        public string[] Message
        {
            get { return m_msg; }
        }

        /// <summary>
        /// Get data (payload) as SLED Control Message Protocol (SCMP) structure
        /// </summary>
        public Scmp.IScmp Scmp
        {
            get { return m_scmp; }
        }

        private readonly ISledTarget m_target;
        private readonly string[] m_msg;
        private readonly Scmp.IScmp m_scmp;
    }

    /// <summary>
    /// SLED network breakpoint class
    /// </summary>
    public class SledNetworkBreakpoint : ICloneable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="languageId">ID of language plugin breakpoint is in</param>
        /// <param name="file">File</param>
        /// <param name="line">Line number</param>
        public SledNetworkBreakpoint(UInt16 languageId, string file, int line)
        {
            LanguageId = languageId;
            File = file.Clone() as string;
            Line = line;
        }

        /// <summary>
        /// Clone object
        /// </summary>
        /// <returns>Clone of original SledNetworkBreakpoint</returns>
        public object Clone()
        {
            var bp = new SledNetworkBreakpoint(LanguageId, File.Clone() as string, Line);

            return bp;
        }

        /// <summary>
        /// Language plugin ID
        /// </summary>
        public readonly UInt16 LanguageId;

        /// <summary>
        /// File breakpoint is in
        /// </summary>
        public readonly string File;

        /// <summary>
        /// Line number of breakpoint
        /// </summary>
        public readonly int Line;
    }

    /// <summary>
    /// Extension methods for SledNetworkBreakpoint
    /// </summary>
    public static class SledNetworkBreakpointExtension
    {
        /// <summary>
        /// Returns true if the breakpoint does not correspond to a file on disk
        /// </summary>
        /// <param name="netBp">Breakpoint</param>
        /// <returns>True if hte breakpoint does not correspond to a file on disk otherwise false</returns>
        public static bool IsUnknownFile(this SledNetworkBreakpoint netBp)
        {
            if (netBp == null)
                throw new ArgumentNullException("netBp");

            return string.IsNullOrEmpty(netBp.File);
        }
    }

    /// <summary>
    /// SLED debug service breakpoint EventArgs class
    /// </summary>
    public class SledDebugServiceBreakpointEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="breakpoint">Breakpoint</param>
        public SledDebugServiceBreakpointEventArgs(SledNetworkBreakpoint breakpoint)
        {
            Breakpoint = breakpoint;
        }

        /// <summary>
        /// Breakpoint
        /// </summary>
        public readonly SledNetworkBreakpoint Breakpoint;
    }

    /// <summary>
    /// Endian enumeration
    /// </summary>
    public enum Endian
    {
        /// <summary>
        /// Big endian
        /// </summary>
        Big,

        /// <summary>
        /// Little endian
        /// </summary>
        Little,

        /// <summary>
        /// Unknown endianness (i.e. haven't determined whether big or little yet, due to not being connected or something else)
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// DebugCommand enumeration
    /// </summary>
    public enum DebugCommand
    {
        /// <summary>
        /// Default debug mode
        /// </summary>
        Default,

        /// <summary>
        /// Single step mode
        /// </summary>
        StepInto,

        /// <summary>
        /// Step over mode
        /// </summary>
        StepOver,

        /// <summary>
        /// Step out mode
        /// </summary>
        StepOut,

        /// <summary>
        /// Stop mode
        /// </summary>
        Stop,
    }

    /// <summary>
    /// SLED debug service interface
    /// </summary>
    public interface ISledDebugService : ICommandClient
    {
        /// <summary>
        /// Unique ID of SLED: should always be "0" (zero)
        /// </summary>
        UInt16 SledPluginId
        {
            get;
        }

        /// <summary>
        /// Get whether connecting to a target
        /// </summary>
        bool IsConnecting
        {
            get;
        }

        /// <summary>
        /// Get whether connected to a target
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Get whether disconnected. True if not connecting and not connected.
        /// </summary>
        bool IsDisconnected
        {
            get;
        }

        /// <summary>
        /// Get whether debugging
        /// </summary>
        bool IsDebugging
        {
            get;
        }

        /// <summary>
        /// Obtain data from the network buffer as a specific SLED Control Message Protocol (SCMP) structure
        /// </summary>
        /// <typeparam name="T">Type of SCMP structure</typeparam>
        /// <returns>SCMP structure of given type</returns>
        T GetScmpBlob<T>() where T : Scmp.IScmp, new();

        /// <summary>
        /// Send a SLED Control Message Protocol (SCMP) structure
        /// </summary>
        /// <param name="scmp">Data to send</param>
        /// <returns>Length of data sent</returns>
        int SendScmp(Scmp.IScmp scmp);

        /// <summary>
        /// Get whether the UpdateBegin packet has been received
        /// </summary>
        bool IsUpdateInProgress
        {
            get;
        }

        /// <summary>
        /// Get the last debug command that was issued
        /// </summary>
        DebugCommand LastDebugCmd  { get; }

        /// <summary>
        /// Get the currently hit breakpoint
        /// </summary>
        SledNetworkBreakpoint CurrentlyHitBp { get; }

        /// <summary>
        /// Get whether the current stopped on line of source contains an actual breakpoint or not
        /// <remarks>Step into, step over, step out, and stop debug commands can make execution halt
        /// on arbitrary lines of source, which may or may not have actual breakpoints set on them.</remarks>
        /// </summary>
        bool IsCurrentlyHitBpActualBp { get; }

        /// <summary>
        /// Event triggered when the socket has connected but before any authentication has taken place
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> Connecting;

        /// <summary>
        /// Event triggered when connected to a target and all authentication has been completed
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> Connected;

        /// <summary>
        /// Event triggered after "Connected" but before "Ready" to signify plugins on the target are ready
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> PluginsReady;

        /// <summary>
        /// Event triggered after "Connected" to signify debugging is ready to start
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> Ready;

        /// <summary>
        /// Event triggered when disconnected from a target
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> Disconnected;

        /// <summary>
        /// Event triggered when an error is encountered
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> Error;

        /// <summary>
        /// Event triggered when data has arrived
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> DataReady;

        /// <summary>
        /// Event triggered when a breakpoint is being hit and before the UpdateBegin/UpdateSync/UpdateEnd process
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointHitting;

        /// <summary>
        /// Event triggered when a breakpoint is hit and after the UpdateBegin/UpdateSync/UpdateEnd process
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointHit;

        /// <summary>
        /// Event triggered when continuing from a breakpoint
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> BreakpointContinue;

        /// <summary>
        /// Event triggered when remote target update has begun
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateBegin;

        /// <summary>
        /// Event triggered when remote target update sync point has been reached
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateSync;

        /// <summary>
        /// Event triggered when remote target update has ended
        /// </summary>
        event EventHandler<SledDebugServiceBreakpointEventArgs> UpdateEnd;

        /// <summary>
        /// Event triggered when "Connect" is selected in the Debug menu
        /// </summary>
        event EventHandler<SledDebugServiceEventArgs> DebugConnect;

        /// <summary>
        /// Event triggered when "Disconnect" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugDisconnect;

        /// <summary>
        /// Event triggered when "Start" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugStart;

        /// <summary>
        /// Event triggered when "Current Statement" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugCurrentStatement;

        /// <summary>
        /// Event triggered when "Step Into" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugStepInto;

        /// <summary>
        /// Event triggered when "Step Over" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugStepOver;

        /// <summary>
        /// Event triggered when "Step Out" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugStepOut;

        /// <summary>
        /// Event triggered when "Stop" is selected in the Debug menu
        /// </summary>
        event EventHandler DebugStop;
    }
}
