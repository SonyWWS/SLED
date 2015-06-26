/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Text;

using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Scmp
{
    /// <summary>
    /// Enumeration to identify network message types
    /// </summary>
    public enum TypeCodes
    {
        /// <summary>
        /// Base ID
        /// </summary>
        Base = 0,
        
        /// <summary>
        /// Breakpoint details ID
        /// </summary>
        BreakpointDetails = 1,
        /// <summary>
        /// Breakpoint begin ID
        /// </summary>
        BreakpointBegin = 2,
        /// <summary>
        /// Breakpoint sync point ID
        /// </summary>
        BreakpointSync = 3,
        /// <summary>
        /// Breakpoint end ID
        /// </summary>
        BreakpointEnd = 4,
        /// <summary>
        /// Breakpoint continue ID
        /// </summary>
        BreakpointContinue = 5,

        /// <summary>
        /// Disconnect ID
        /// </summary>
        Disconnect = 6,

        /// <summary>
        /// Heartbeat ID
        /// </summary>
        Heartbeat = 8,
        /// <summary>
        /// Success ID
        /// </summary>
        Success = 9,
        /// <summary>
        /// Failure ID
        /// </summary>
        Failure = 10,
        /// <summary>
        /// Version ID
        /// </summary>
        Version = 11,

        /// <summary>
        /// Debug start ID
        /// </summary>
        DebugStart = 12,
        /// <summary>
        /// Debug step into ID
        /// </summary>
        DebugStepInto = 13,
        /// <summary>
        /// Debug step over ID
        /// </summary>
        DebugStepOver = 14,
        /// <summary>
        /// Debug step out ID
        /// </summary>
        DebugStepOut = 15,
        /// <summary>
        /// Debug stop ID
        /// </summary>
        DebugStop = 16,

        /// <summary>
        /// Script cache ID
        /// </summary>
        ScriptCache = 17,

        /// <summary>
        /// Authenticated ID
        /// </summary>
        Authenticated = 18,

        /// <summary>
        /// Ready ID
        /// </summary>
        Ready = 20,

        /// <summary>
        /// Plugins ready ID
        /// </summary>
        PluginsReady = 21,

        /// <summary>
        /// Function info ID
        /// </summary>
        FunctionInfo = 22,

        /// <summary>
        /// TTY begin ID
        /// </summary>
        TtyBegin = 23,
        /// <summary>
        /// TTY ID
        /// </summary>
        Tty = 24,
        /// <summary>
        /// TTY end ID
        /// </summary>
        TtyEnd = 25,

        /// <summary>
        /// Developer entered command ID
        /// </summary>
        DevCmd = 26,

        /// <summary>
        /// Edit and continue command ID
        /// </summary>
        EditAndContinue = 27,

        /// <summary>
        /// Endianness command ID
        /// </summary>
        Endianness = 28,

        /// <summary>
        /// Protocol debug mark command ID
        /// </summary>
        ProtocolDebugMark = 29,
    }

    #region IScmp Interface & Scmp Base message

    /// <summary>
    /// Interface for base SLED Control Message Protocol (SCMP) message
    /// </summary>
    public interface IScmp
    {
        /// <summary>
        /// Get or set the length/size of the structure in bytes
        /// </summary>
        Int32 Length
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the type code of the item
        /// </summary>
        UInt16 TypeCode
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the plugin ID of the item
        /// </summary>
        UInt16 PluginId
        {
            get;
            set;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        byte[] Pack();

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        void Unpack(byte[] buffer);

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SLED Control Message Protocol (SCMP) item is equal to the SCMP item compared to</returns>
        bool IsEqualTo(IScmp scmpItem);
    }

    /// <summary>
    /// Base SLED Control Message Protocol (SCMP) class for all network messages to derive from
    /// </summary>
    public class Base : IScmp
    {
        /// <summary>
        /// Get the length/size of the structure in bytes
        /// </summary>
        public Int32 Length
        {
            get { return m_length; }
            set { m_length = value; }
        }

        /// <summary>
        /// Get the type code of the item
        /// </summary>
        public UInt16 TypeCode
        {
            get { return m_typeCode; }
            set { m_typeCode = value; }
        }

        /// <summary>
        /// Get the plugin ID of the item
        /// </summary>
        public UInt16 PluginId
        {
            get { return m_pluginId; }
            set { m_pluginId = value; }
        }

        /// <summary>
        /// Pack an already existing buffer with the base SLED Control Message Protocol (SCMP) information
        /// </summary>
        /// <param name="buffer">Buffer to pack data into</param>
        /// <param name="iRealLength">Real length of the class (includes derived members)</param>
        protected virtual void Pack(ref byte[] buffer, Int32 iRealLength)
        {
            var packer = new SledNetworkBufferPacker(ref buffer);

            m_length = iRealLength; // This is the real length wrt all the members of the derived class
            packer.PackInt32(m_length);
            packer.PackUInt16(m_typeCode);
            packer.PackUInt16(m_pluginId);
        }

        /// <summary>
        /// Wrap all base SLED Control Message Protocol (SCMP) contents into a new byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public virtual byte[] Pack()
        {
            var buffer = new byte[4 + 2 + 2];
            var packer = new SledNetworkBufferPacker(ref buffer);

            packer.PackInt32(m_length);
            packer.PackUInt16(m_typeCode);
            packer.PackUInt16(m_pluginId);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the base SLED Control Message Protocol (SCMP) class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public virtual void Unpack(byte[] buffer)
        {
            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer);

            m_length = reader.ReadInt32();
            m_typeCode = reader.ReadUInt16();
            m_pluginId = reader.ReadUInt16();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is compared to</returns>
        public virtual bool IsEqualTo(IScmp scmpItem)
        {
            if (scmpItem == null)
                return false;

            if (GetType() != scmpItem.GetType())
                return false;

            return ((Length == scmpItem.Length) &&
                (PluginId == scmpItem.PluginId) &&
                (TypeCode == scmpItem.TypeCode));
        }

        /// <summary>
        /// Default length of strings
        /// </summary>
        public const int StringLen = 256;

        /// <summary>
        /// Size of this structure in bytes
        /// </summary>
        public const int SizeOf = 8;

        private Int32 m_length;
        private UInt16 m_typeCode;
        private UInt16 m_pluginId;
    }

    #endregion

    #region Scmp attributes

    /// <summary>
    /// Attribute to tag SLED Control Message Protocol (SCMP) structures that are to be sent over the network
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScmpSend : Attribute
    {
    }

    /// <summary>
    /// Attribute to tag SLED Control Message Protocol (SCMP) structures that are to be received over the network
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScmpReceive : Attribute
    {
    }

    #endregion

    /// <summary>
    /// Breakpoint details message
    /// </summary>
    [ScmpSend]
    public class BreakpointDetails : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BreakpointDetails()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.BreakpointDetails;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="szRelFilePath">Relative path to script file the breakpoint is in</param>
        /// <param name="iLine">Line number the breakpoint is on</param>
        /// <param name="szCondition">Breakpoint's condition</param>
        /// <param name="bResult">Expected result of the condition</param>
        /// <param name="bUseFunctionEnvironment">Whether to use function's environment or default environment ("_G")</param>
        public BreakpointDetails(UInt16 iPluginId, string szRelFilePath, Int32 iLine, string szCondition, bool bResult, bool bUseFunctionEnvironment)
            : this()
        {
            PluginId = iPluginId;

            RelFilePath = SledUtil.NetSlashes(szRelFilePath);
            Line = iLine;
            Condition = szCondition;
            Result = (Byte)(bResult ? 1 : 0);
            UseFunctionEnvironment = (Byte)(bUseFunctionEnvironment ? 1 : 0);

            if (Condition == null)
                Condition = string.Empty;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(RelFilePath).Length) + 4 + (2 + Encoding.UTF8.GetBytes(Condition).Length) + 1 + 1];
            base.Pack(ref buffer, buffer.Length);
            
            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(RelFilePath);
            packer.PackInt32(Line);
            packer.PackString(Condition);
            packer.PackByte(Result);
            packer.PackByte(UseFunctionEnvironment);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            RelFilePath = SledUtil.FixSlashes(reader.ReadString());
            Line = reader.ReadInt32();
            Condition = reader.ReadString();
            Result = reader.ReadByte();
            UseFunctionEnvironment = reader.ReadByte();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as BreakpointDetails;
            if (scmpCastItem == null)
                return false;

            return ((string.Compare(SledUtil.FixSlashes(RelFilePath), scmpCastItem.RelFilePath, StringComparison.Ordinal) == 0) &&
                (Line == scmpCastItem.Line) &&
                (string.Compare(Condition, scmpCastItem.Condition, StringComparison.Ordinal) == 0) &&
                (Result == scmpCastItem.Result) &&
                (UseFunctionEnvironment == scmpCastItem.UseFunctionEnvironment));
        }

        /// <summary>
        /// Relative path to script file the breakpoint is in
        /// </summary>
        public string RelFilePath;
        /// <summary>
        /// Line number breakpoint is on
        /// </summary>
        public Int32 Line;
        /// <summary>
        /// The breakpoint's condition
        /// </summary>
        public string Condition;
        /// <summary>
        /// The expected result of the condition
        /// </summary>
        public Byte Result;
        /// <summary>
        /// Use the function's environment or _G
        /// </summary>
        public Byte UseFunctionEnvironment;
    }

    /// <summary>
    /// Breakpoint begin message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class BreakpointBegin : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BreakpointBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.BreakpointBegin;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="iBreakPluginId">Plugin that hit the breakpoint</param>
        /// <param name="szRelFilePath">Relative path to file the breakpoint is in</param>
        /// <param name="iLine">Line number of the breakpoint</param>
        public BreakpointBegin(UInt16 iPluginId, UInt16 iBreakPluginId, string szRelFilePath, Int32 iLine)
            : this()
        {
            PluginId = iPluginId;

            BreakPluginId = iBreakPluginId;
            RelFilePath = SledUtil.NetSlashes(szRelFilePath);
            Line = iLine;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2 + (2 + Encoding.UTF8.GetBytes(RelFilePath).Length) + 4];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackUInt16(BreakPluginId);
            packer.PackString(RelFilePath);
            packer.PackInt32(Line);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            BreakPluginId = reader.ReadUInt16();
            RelFilePath = SledUtil.FixSlashes(reader.ReadString());
            Line = reader.ReadInt32();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as BreakpointBegin;
            if (scmpCastItem == null)
                return false;

            return ((BreakPluginId == scmpCastItem.BreakPluginId) &&
                (string.Compare(SledUtil.FixSlashes(RelFilePath), scmpCastItem.RelFilePath, StringComparison.Ordinal) == 0) &&
                (Line == scmpCastItem.Line));
        }

        /// <summary>
        /// Plugin that hit the breakpoint
        /// </summary>
        public UInt16 BreakPluginId;
        /// <summary>
        /// Relative path to file the breakpoint is in
        /// </summary>
        public string RelFilePath;
        /// <summary>
        /// Line number of the breakpoint
        /// </summary>
        public Int32 Line;
    }

    /// <summary>
    /// Breakpoint sync message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class BreakpointSync : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BreakpointSync()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.BreakpointSync;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="iBreakPluginId">Plugin that hit the breakpoint</param>
        /// <param name="szRelFilePath">Relative path to file the breakpoint is in</param>
        /// <param name="iLine">Line number of the breakpoint</param>
        public BreakpointSync(UInt16 iPluginId, UInt16 iBreakPluginId, string szRelFilePath, Int32 iLine)
            : this()
        {
            PluginId = iPluginId;

            BreakPluginId = iBreakPluginId;
            RelFilePath = SledUtil.NetSlashes(szRelFilePath);
            Line = iLine;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2 + (2 + Encoding.UTF8.GetBytes(RelFilePath).Length) + 4];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackUInt16(BreakPluginId);
            packer.PackString(RelFilePath);
            packer.PackInt32(Line);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            BreakPluginId = reader.ReadUInt16();
            RelFilePath = SledUtil.FixSlashes(reader.ReadString());
            Line = reader.ReadInt32();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as BreakpointSync;
            if (scmpCastItem == null)
                return false;

            return ((BreakPluginId == scmpCastItem.BreakPluginId) &&
                (string.Compare(SledUtil.FixSlashes(RelFilePath), scmpCastItem.RelFilePath, StringComparison.Ordinal) == 0) &&
                (Line == scmpCastItem.Line));
        }

        /// <summary>
        /// Plugin that hit the breakpoint
        /// </summary>
        public UInt16 BreakPluginId;
        /// <summary>
        /// Relative path to file the breakpoint is in
        /// </summary>
        public string RelFilePath;
        /// <summary>
        /// Line number of the breakpoint
        /// </summary>
        public Int32 Line;
    }

    /// <summary>
    /// Breakpoint end message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class BreakpointEnd : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BreakpointEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.BreakpointEnd;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="iBreakPluginId">Plugin that hit the breakpoint</param>
        /// <param name="szRelFilePath">Relative path to file the breakpoint is in</param>
        /// <param name="iLine">Line number of the breakpoint</param>
        public BreakpointEnd(UInt16 iPluginId, UInt16 iBreakPluginId, string szRelFilePath, Int32 iLine)
            : this()
        {
            PluginId = iPluginId;

            BreakPluginId = iBreakPluginId;
            RelFilePath = SledUtil.NetSlashes(szRelFilePath);
            Line = iLine;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2 + (2 + Encoding.UTF8.GetBytes(RelFilePath).Length) + 4];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackUInt16(BreakPluginId);
            packer.PackString(RelFilePath);
            packer.PackInt32(Line);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            BreakPluginId = reader.ReadUInt16();
            RelFilePath = SledUtil.FixSlashes(reader.ReadString());
            Line = reader.ReadInt32();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as BreakpointEnd;
            if (scmpCastItem == null)
                return false;

            return ((BreakPluginId == scmpCastItem.BreakPluginId) &&
                (string.Compare(SledUtil.FixSlashes(RelFilePath), scmpCastItem.RelFilePath, StringComparison.Ordinal) == 0) &&
                (Line == scmpCastItem.Line));
        }

        /// <summary>
        /// Plugin that hit the breakpoint
        /// </summary>
        public UInt16 BreakPluginId;
        /// <summary>
        /// Relative path to file the breakpoint is in
        /// </summary>
        public string RelFilePath;
        /// <summary>
        /// Line number of the breakpoint
        /// </summary>
        public Int32 Line;
    }

    /// <summary>
    /// Breakpoint continue message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class BreakpointContinue : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BreakpointContinue()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.BreakpointContinue;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="iBreakPluginId">Plugin that hit the breakpoint</param>
        /// <param name="szRelFilePath">Relative path to file the breakpoint is in</param>
        /// <param name="iLine">Line number of the breakpoint</param>
        public BreakpointContinue(UInt16 iPluginId, UInt16 iBreakPluginId, string szRelFilePath, Int32 iLine)
            : this()
        {
            PluginId = iPluginId;

            BreakPluginId = iBreakPluginId;
            RelFilePath = SledUtil.NetSlashes(szRelFilePath);
            Line = iLine;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2 + (2 + Encoding.UTF8.GetBytes(RelFilePath).Length) + 4];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackUInt16(BreakPluginId);
            packer.PackString(RelFilePath);
            packer.PackInt32(Line);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            BreakPluginId = reader.ReadUInt16();
            RelFilePath = SledUtil.FixSlashes(reader.ReadString());
            Line = reader.ReadInt32();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as BreakpointContinue;
            if (scmpCastItem == null)
                return false;

            return ((BreakPluginId == scmpCastItem.BreakPluginId) &&
                (string.Compare(SledUtil.FixSlashes(RelFilePath), scmpCastItem.RelFilePath, StringComparison.Ordinal) == 0) &&
                (Line == scmpCastItem.Line));
        }

        /// <summary>
        /// Plugin that hit the breakpoint
        /// </summary>
        public UInt16 BreakPluginId;
        /// <summary>
        /// Relative path to file the breakpoint is in
        /// </summary>
        public string RelFilePath;
        /// <summary>
        /// Line number of the breakpoint
        /// </summary>
        public Int32 Line;
    }

    /// <summary>
    /// Heartbeat message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class Heartbeat : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Heartbeat()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Heartbeat;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Heartbeat(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Success message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class Success : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Success()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Success;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Success(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Failure message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class Failure : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Failure()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Failure;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Failure(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Version message
    /// </summary>
    [ScmpReceive]
    public class Version : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Version()
        {
            Length = SizeOf + 2 + 2 + 2;
            TypeCode = (UInt16)TypeCodes.Version;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="major">Major version number</param>
        /// <param name="minor">Minor version number</param>
        /// <param name="revision">Revision number</param>
        public Version(UInt16 iPluginId, UInt16 major, UInt16 minor, UInt16 revision)
            : this()
        {
            PluginId = iPluginId;

            Major = major;
            Minor = minor;
            Revision = revision;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2 + 2 + 2];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackUInt16(Major);
            packer.PackUInt16(Minor);
            packer.PackUInt16(Revision);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Major = reader.ReadUInt16();
            Minor = reader.ReadUInt16();
            Revision = reader.ReadUInt16();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as Version;
            if (scmpCastItem == null)
                return false;

            return ((Major == scmpCastItem.Major) &&
                (Minor == scmpCastItem.Minor) &&
                (Revision == scmpCastItem.Revision));
        }

        /// <summary>
        /// Major version number
        /// </summary>
        public UInt16 Major;
        /// <summary>
        /// Minor version number
        /// </summary>
        public UInt16 Minor;
        /// <summary>
        /// Revision number
        /// </summary>
        public UInt16 Revision;
    }

    /// <summary>
    /// Debug start message
    /// </summary>
    [ScmpSend]
    public class DebugStart : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugStart()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DebugStart;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public DebugStart(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Debug step into message
    /// </summary>
    [ScmpSend]
    public class DebugStepInto : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugStepInto()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DebugStepInto;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public DebugStepInto(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Debug step over message
    /// </summary>
    [ScmpSend]
    public class DebugStepOver : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugStepOver()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DebugStepOver;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public DebugStepOver(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Debug step out message
    /// </summary>
    [ScmpSend]
    public class DebugStepOut : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugStepOut()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DebugStepOut;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public DebugStepOut(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Debug stop message
    /// </summary>
    [ScmpSend]
    public class DebugStop : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugStop()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DebugStop;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public DebugStop(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Script cache message
    /// </summary>
    [ScmpReceive]
    public class ScriptCache : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ScriptCache()
        {
            Length = SizeOf + StringLen;
            TypeCode = (UInt16)TypeCodes.ScriptCache;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="szRelScriptPath">Relative path to script file</param>
        public ScriptCache(UInt16 iPluginId, string szRelScriptPath)
        {
            PluginId = iPluginId;

            RelScriptPath = SledUtil.NetSlashes(szRelScriptPath);
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            RelScriptPath = SledUtil.FixSlashes(reader.ReadString());
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(RelScriptPath).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(RelScriptPath);

            return buffer;
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as ScriptCache;
            if (scmpCastItem == null)
                return false;

            return (string.Compare(SledUtil.FixSlashes(RelScriptPath), scmpCastItem.RelScriptPath, StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// Relative path to script file
        /// </summary>
        public string RelScriptPath;
    }

    /// <summary>
    /// Authenticated message
    /// </summary>
    [ScmpReceive]
    public class Authenticated : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Authenticated()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Authenticated;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Authenticated(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Ready message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class Ready : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Ready()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Ready;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Ready(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Plugins ready message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class PluginsReady : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PluginsReady()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.PluginsReady;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public PluginsReady(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Function info message
    /// </summary>
    [ScmpSend]
    public class FunctionInfo : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FunctionInfo()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.FunctionInfo;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iPluginid">Plugin that message belongs to</param>
        /// <param name="szRelScriptPath">Relative path to script file</param>
        /// <param name="szFuncName">Function name</param>
        /// <param name="iLineDefined">Script line on which function is defined</param>
        public FunctionInfo(UInt16 iPluginid, string szRelScriptPath, string szFuncName, Int32 iLineDefined)
            : this()
        {
            PluginId = iPluginid;

            RelScriptPath = SledUtil.NetSlashes(szRelScriptPath);
            FunctionName = szFuncName;
            LineDefined = iLineDefined;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(RelScriptPath).Length) + (2 + Encoding.UTF8.GetBytes(FunctionName).Length) + 4];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(RelScriptPath);
            packer.PackString(FunctionName);
            packer.PackInt32(LineDefined);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            RelScriptPath = SledUtil.FixSlashes(reader.ReadString());
            FunctionName = reader.ReadString();
            LineDefined = reader.ReadInt32();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as FunctionInfo;
            if (scmpCastItem == null)
                return false;

            return ((string.Compare(SledUtil.FixSlashes(RelScriptPath), scmpCastItem.RelScriptPath, StringComparison.Ordinal) == 0) &&
                (string.Compare(FunctionName, scmpCastItem.FunctionName, StringComparison.Ordinal) == 0) &&
                (LineDefined == scmpCastItem.LineDefined));
        }

        /// <summary>
        /// Relative path to script file
        /// </summary>
        public string RelScriptPath;
        /// <summary>
        /// Function name
        /// </summary>
        public string FunctionName;
        /// <summary>
        /// Line number on which function defined
        /// </summary>
        public Int32 LineDefined;
    }

    /// <summary>
    /// TTY begin message
    /// </summary>
    [ScmpReceive]
    public class TtyBegin : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TtyBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.TtyBegin;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public TtyBegin(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// TTY message
    /// </summary>
    [ScmpReceive]
    public class Tty : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Tty()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Tty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iPluginid">Plugin that message belongs to</param>
        /// <param name="szMessage">Data, max length is Scmp.Base.StringLen</param>
        public Tty(UInt16 iPluginid, string szMessage)
            : this()
        {
            PluginId = iPluginid;

            Message = szMessage;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(Message).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(Message);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Message = reader.ReadString();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as Tty;
            if (scmpCastItem == null)
                return false;

            return (string.Compare(Message, scmpCastItem.Message, StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// Message
        /// </summary>
        public string Message;
    }

    /// <summary>
    /// TTY end message
    /// </summary>
    [ScmpReceive]
    public class TtyEnd : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TtyEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.TtyEnd;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public TtyEnd(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// Developer command message
    /// </summary>
    [ScmpSend]
    public class DevCmd : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DevCmd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.DevCmd;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="szCommand">Developer command text</param>
        public DevCmd(UInt16 iPluginId, string szCommand)
            : this()
        {
            PluginId = iPluginId;

            Command = szCommand;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(Command).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(Command);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Command = reader.ReadString();
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as DevCmd;
            if (scmpCastItem == null)
                return false;

            return (string.Compare(Command, scmpCastItem.Command, StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// The developer entered command
        /// </summary>
        public string Command;
    }

    /// <summary>
    /// Edit and continue command message
    /// </summary>
    [ScmpSend]
    public class EditAndContinue : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EditAndContinue()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.EditAndContinue;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        /// <param name="szRelScriptPath">Relative path to script to load</param>
        public EditAndContinue(UInt16 iPluginId, string szRelScriptPath)
            : this()
        {
            PluginId = iPluginId;

            RelScriptPath = SledUtil.NetSlashes(szRelScriptPath);
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        /// <returns>Packed data</returns>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(RelScriptPath).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(RelScriptPath);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            RelScriptPath = SledUtil.FixSlashes(reader.ReadString());
        }

        /// <summary>
        /// Determine whether this SLED Control Message Protocol (SCMP) item is equal to another SCMP item
        /// </summary>
        /// <param name="scmpItem">Item to compare to</param>
        /// <returns>True iff this SCMP item is equal to the SCMP item it is being compared to</returns>
        public override bool IsEqualTo(IScmp scmpItem)
        {
            if (!base.IsEqualTo(scmpItem))
                return false;

            var scmpCastItem = scmpItem as EditAndContinue;
            if (scmpCastItem == null)
                return false;

            return (string.Compare(SledUtil.FixSlashes(RelScriptPath), scmpCastItem.RelScriptPath, StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// Path to script to reload
        /// </summary>
        public string RelScriptPath;
    }

    /// <summary>
    /// Endianness command message
    /// </summary>
    [ScmpReceive]
    public class Endianness : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Endianness()
        {
            Length = SizeOf;
            TypeCode = (UInt16)TypeCodes.Endianness;

            m_endianness = Endian.Unknown;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public Endianness(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }

        /// <summary>
        /// Unpack buffer and determine endianness
        /// </summary>
        /// <param name="buffer">Received data to unpack</param>
        public override void Unpack(byte[] buffer)
        {
            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer);

            Length = reader.ReadInt32();
            TypeCode = reader.ReadUInt16();
            PluginId = reader.ReadUInt16();

            // Data is potentially big endian and we need to compensate!
            if (TypeCode != (UInt16)TypeCodes.Endianness)
            {
                // Swap and re-check
                const int iSizeOfI32 = SledNetworkBufferReader.SizeOfInt32;
                const int iSizeOfU16 = SledNetworkBufferReader.SizeOfUInt16;

                Array.Reverse(buffer, 0, iSizeOfI32);
                Array.Reverse(buffer, iSizeOfI32, iSizeOfU16);
                Array.Reverse(buffer, iSizeOfI32 + iSizeOfU16, iSizeOfU16);

                Length = BitConverter.ToInt32(buffer, 0);
                TypeCode = BitConverter.ToUInt16(buffer, iSizeOfI32);
                PluginId = BitConverter.ToUInt16(buffer, iSizeOfI32 + iSizeOfU16);

                // Verify data is big endian
                if (TypeCode == (UInt16)TypeCodes.Endianness)
                    m_endianness = Endian.Big;
            }
            else
            {
                // Data is little endian
                m_endianness = Endian.Little;
            }
        }

        /// <summary>
        /// Get endianness
        /// </summary>
        public Endian Order
        {
            get { return m_endianness; }
        }

        private Endian m_endianness;
    }

    /// <summary>
    /// Protocol debug mark command message
    /// </summary>
    [ScmpSend]
    [ScmpReceive]
    public class ProtocolDebugMark : Base
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProtocolDebugMark()
        {
            Length = SizeOf;
            TypeCode = (UInt16) TypeCodes.ProtocolDebugMark;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="iPluginId">Plugin that message belongs to</param>
        public ProtocolDebugMark(UInt16 iPluginId)
            : this()
        {
            PluginId = iPluginId;
        }
    }

    /// <summary>
    /// SLED Control Message Protocol (SCMP) extension methods
    /// </summary>
    public static class ScmpExtension
    {
        /// <summary>
        /// Convert version to number
        /// </summary>
        /// <param name="version">Version</param>
        /// <returns>Number</returns>
        public static Int32 ToInt32(this Version version)
        {
            var result = (version.Major * 10) + version.Minor;
            return result;
        }
    }
}
