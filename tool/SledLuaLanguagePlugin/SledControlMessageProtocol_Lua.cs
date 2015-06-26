/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Scmp;

namespace Sce.Sled.Lua.Scmp
{
    internal enum LuaTypeCodes
    {
        LuaMemoryTraceBegin = 200,
        LuaMemoryTrace = 201,
        LuaMemoryTraceEnd = 202,
        LuaMemoryTraceStreamBegin = 203,
        LuaMemoryTraceStream = 204,
        LuaMemoryTraceStreamEnd = 205,
        
        LuaProfileInfoBegin = 207,
        LuaProfileInfo = 208,
        LuaProfileInfoEnd = 209,
        LuaProfileInfoLookupPerform = 210,
        LuaProfileInfoLookupBegin = 211,
        LuaProfileInfoLookup = 212,
        LuaProfileInfoLookupEnd = 213,

        LuaVarFilterStateTypeBegin = 214,
        LuaVarFilterStateType = 215,
        LuaVarFilterStateTypeEnd = 216,
        LuaVarFilterStateNameBegin = 217,
        LuaVarFilterStateName = 218,
        LuaVarFilterStateNameEnd = 219,

        LuaVarGlobalBegin = 220,
        LuaVarGlobal = 221,
        LuaVarGlobalEnd = 222,
        LuaVarGlobalLookupBegin = 223,
        LuaVarGlobalLookupEnd = 224,

        LuaVarLocalBegin = 230,
        LuaVarLocal = 231,
        LuaVarLocalEnd = 232,
        LuaVarLocalLookupBegin = 233,
        LuaVarLocalLookupEnd = 234,

        LuaVarUpvalueBegin = 240,
        LuaVarUpvalue = 241,
        LuaVarUpvalueEnd = 242,
        LuaVarUpvalueLookupBegin = 243,
        LuaVarUpvalueLookupEnd = 244,

        LuaVarEnvVarBegin = 250,
        LuaVarEnvVar = 251,
        LuaVarEnvVarEnd = 252,
        LuaVarEnvVarLookupBegin = 253,
        LuaVarEnvVarLookupEnd = 254,

        LuaVarLookUp = 255,
        LuaVarUpdate = 256,

        LuaCallStackBegin = 260,
        LuaCallStack = 261,
        LuaCallStackEnd = 262,
        LuaCallStackLookupPerform = 263,
        LuaCallStackLookupBegin = 264,
        LuaCallStackLookup = 265,
        LuaCallStackLookupEnd = 266,

        LuaWatchLookupBegin = 270,
        LuaWatchLookupEnd = 271,
        LuaWatchLookupClear = 272,

        LuaWatchLookupProjectBegin = 280,
        LuaWatchLookupProjectEnd = 281,
        LuaWatchLookupCustomBegin = 282,
        LuaWatchLookupCustomEnd = 283,
        
        LuaStateBegin = 290,
        LuaStateAdd = 291,
        LuaStateRemove = 292,
        LuaStateEnd = 293,
        LuaStateToggle = 294,

        LuaMemoryTraceToggle = 300,
        LuaProfilerToggle = 301,

        LuaLimits = 310,
    }

    [ScmpReceive]
    internal class LuaMemoryTraceBegin : Base
    {
        public LuaMemoryTraceBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceBegin;
        }

        public LuaMemoryTraceBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaMemoryTrace : Base
    {
        public LuaMemoryTrace()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTrace;
        }

        public LuaMemoryTrace(UInt16 iPluginid, char chWhat, string szOldPtr, string szNewPtr, Int32 iOldSize, Int32 iNewSize)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
            OldPtr = szOldPtr;
            NewPtr = szNewPtr;
            OldSize = iOldSize;
            NewSize = iNewSize;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            What = reader.ReadByte();
            OldPtr = reader.ReadString();
            NewPtr = reader.ReadString();
            OldSize = reader.ReadInt32();
            NewSize = reader.ReadInt32();
        }

        public Byte What;
        public string OldPtr;
        public string NewPtr;
        public Int32 OldSize;
        public Int32 NewSize;
    }

    [ScmpReceive]
    internal class LuaMemoryTraceEnd : Base
    {
        public LuaMemoryTraceEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceEnd;
        }

        public LuaMemoryTraceEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaMemoryTraceStreamBegin : Base
    {
        public LuaMemoryTraceStreamBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceStreamBegin;
        }

        public LuaMemoryTraceStreamBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaMemoryTraceStream : Base
    {
        public LuaMemoryTraceStream()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceStream;
        }

        public LuaMemoryTraceStream(UInt16 iPluginid, char chWhat, string szOldPtr, string szNewPtr, Int32 iOldSize, Int32 iNewSize)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
            OldPtr = szOldPtr;
            NewPtr = szNewPtr;
            OldSize = iOldSize;
            NewSize = iNewSize;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            What = reader.ReadByte();
            OldPtr = reader.ReadString();
            NewPtr = reader.ReadString();
            OldSize = reader.ReadInt32();
            NewSize = reader.ReadInt32();
        }

        public Byte What;
        public string OldPtr;
        public string NewPtr;
        public Int32 OldSize;
        public Int32 NewSize;
    }

    [ScmpReceive]
    internal class LuaMemoryTraceStreamEnd : Base
    {
        public LuaMemoryTraceStreamEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceStreamEnd;
        }

        public LuaMemoryTraceStreamEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaProfileInfoBegin : Base
    {
        public LuaProfileInfoBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoBegin;
        }

        public LuaProfileInfoBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaProfileInfo : Base
    {
        public LuaProfileInfo()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfo;
        }

        public LuaProfileInfo(UInt16 iPluginid, string szFuncName, string szRelScriptPath,
            float flFnTimeElapsed, float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest,
            float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest, 
            UInt32 iFnCallCount, Int32 iFnLine, Int32 iFnCalls)
            : this()
        {
            PluginId = iPluginid;

            FunctionName = szFuncName;
            RelScriptPath = szRelScriptPath;
            FnTimeElapsed = flFnTimeElapsed;
            FnTimeElapsedAvg = flFnTimeElapsedAvg;
            FnTimeElapsedShortest = flFnTimeElapsedShortest;
            FnTimeElapsedLongest = flFnTimeElapsedLongest;
            FnTimeInnerElapsed = flFnTimeInnerElapsed;
            FnTimeInnerElapsedAvg = flFnTimeInnerElapsedAvg;
            FnTimeInnerElapsedShortest = flFnTimeInnerElapsedShortest;
            FnTimeInnerElapsedLongest = flFnTimeInnerElapsedLongest;
            FnCallCount = iFnCallCount;
            FnLine = iFnLine;
            FnCalls = iFnCalls;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            FunctionName = reader.ReadString();
            RelScriptPath = reader.ReadString();
            FnTimeElapsed = reader.ReadFloat();
            FnTimeElapsedAvg = reader.ReadFloat();
            FnTimeElapsedShortest = reader.ReadFloat();
            FnTimeElapsedLongest = reader.ReadFloat();
            FnTimeInnerElapsed = reader.ReadFloat();
            FnTimeInnerElapsedAvg = reader.ReadFloat();
            FnTimeInnerElapsedShortest = reader.ReadFloat();
            FnTimeInnerElapsedLongest = reader.ReadFloat();
            FnCallCount = reader.ReadUInt32();
            FnLine = reader.ReadInt32();
            FnCalls = reader.ReadInt32();
        }

        public string FunctionName;
        public string RelScriptPath;
        public float FnTimeElapsed;
        public float FnTimeElapsedAvg;
        public float FnTimeElapsedShortest;
        public float FnTimeElapsedLongest;
        public float FnTimeInnerElapsed;
        public float FnTimeInnerElapsedAvg;
        public float FnTimeInnerElapsedShortest;
        public float FnTimeInnerElapsedLongest;
        public UInt32 FnCallCount;
        public Int32 FnLine;
        public Int32 FnCalls;
    }

    [ScmpReceive]
    internal class LuaProfileInfoEnd : Base
    {
        public LuaProfileInfoEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoEnd;
        }

        public LuaProfileInfoEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    internal class LuaProfileInfoLookupPerform : Base
    {
        public LuaProfileInfoLookupPerform()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoLookupPerform;
        }

        public LuaProfileInfoLookupPerform(UInt16 iPluginid, string szLookup)
            : this()
        {
            PluginId = iPluginid;

            var split = szLookup.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            FunctionName = split[0].Remove(0, 11); // Remove "{pi_lookup:"
            What = (Byte)((split[1])[0]);
            Line = Int32.Parse(split[2]);
            RelScriptPath = split[3].Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            RelScriptPath = RelScriptPath.Remove(RelScriptPath.Length - 1, 1);
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(FunctionName).Length) + 1 + 4 + (2 + Encoding.UTF8.GetBytes(RelScriptPath).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(FunctionName);
            packer.PackByte(What);
            packer.PackInt32(Line);
            packer.PackString(RelScriptPath);

            return buffer;
        }

        public string FunctionName;
        public Byte What;
        public Int32 Line;
        public string RelScriptPath;
    }

    [ScmpReceive]
    internal class LuaProfileInfoLookupBegin : Base
    {
        public LuaProfileInfoLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoLookupBegin;
        }

        public LuaProfileInfoLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaProfileInfoLookup : Base
    {
        public LuaProfileInfoLookup()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoLookup;
        }

        public LuaProfileInfoLookup(UInt16 iPluginid, string szFuncName, string szRelScriptPath,
            float flFnTimeElapsed, float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest,
            float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest,
            UInt32 iFnCallCount, Int32 iFnLine, Int32 iFnCalls)
            : this()
        {
            PluginId = iPluginid;

            FunctionName = szFuncName;
            RelScriptPath = szRelScriptPath;
            FnTimeElapsed = flFnTimeElapsed;
            FnTimeElapsedAvg = flFnTimeElapsedAvg;
            FnTimeElapsedShortest = flFnTimeElapsedShortest;
            FnTimeElapsedLongest = flFnTimeElapsedLongest;
            FnTimeInnerElapsed = flFnTimeInnerElapsed;
            FnTimeInnerElapsedAvg = flFnTimeInnerElapsedAvg;
            FnTimeInnerElapsedShortest = flFnTimeInnerElapsedShortest;
            FnTimeInnerElapsedLongest = flFnTimeInnerElapsedLongest;
            FnCallCount = iFnCallCount;
            FnLine = iFnLine;
            FnCalls = iFnCalls;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            FunctionName = reader.ReadString();
            RelScriptPath = reader.ReadString();
            FnTimeElapsed = reader.ReadFloat();
            FnTimeElapsedAvg = reader.ReadFloat();
            FnTimeElapsedShortest = reader.ReadFloat();
            FnTimeElapsedLongest = reader.ReadFloat();
            FnTimeInnerElapsed = reader.ReadFloat();
            FnTimeInnerElapsedAvg = reader.ReadFloat();
            FnTimeInnerElapsedShortest = reader.ReadFloat();
            FnTimeInnerElapsedLongest = reader.ReadFloat();
            FnCallCount = reader.ReadUInt32();
            FnLine = reader.ReadInt32();
            FnCalls = reader.ReadInt32();
        }

        public string FunctionName;
        public string RelScriptPath;
        public float FnTimeElapsed;
        public float FnTimeElapsedAvg;
        public float FnTimeElapsedShortest;
        public float FnTimeElapsedLongest;
        public float FnTimeInnerElapsed;
        public float FnTimeInnerElapsedAvg;
        public float FnTimeInnerElapsedShortest;
        public float FnTimeInnerElapsedLongest;
        public UInt32 FnCallCount;
        public Int32 FnLine;
        public Int32 FnCalls;
    }

    [ScmpReceive]
    internal class LuaProfileInfoLookupEnd : Base
    {
        public LuaProfileInfoLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfileInfoLookupEnd;
        }

        public LuaProfileInfoLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    internal class LuaVarFilterStateTypeBegin : Base
    {
        public LuaVarFilterStateTypeBegin()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateTypeBegin;
        }

        public LuaVarFilterStateTypeBegin(UInt16 iPluginid, char chWhat)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);

            return buffer;
        }

        public Byte What;
    }

    [ScmpSend]
    internal class LuaVarFilterStateType : Base
    {
        public LuaVarFilterStateType()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateType;
        }

        public LuaVarFilterStateType(UInt16 iPluginid, char chWhat, bool[] filter)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;

            for (var i = 0; i < filter.Length; i++)
                Filter[i] = (Byte)(filter[i] ? 1 : 0);
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1 + 2 + 9];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);
            packer.PackByteArray(Filter);

            return buffer;
        }

        public Byte What;
        public Byte[] Filter = new Byte[9];
    }

    [ScmpSend]
    internal class LuaVarFilterStateTypeEnd : Base
    {
        public LuaVarFilterStateTypeEnd()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateTypeEnd;
        }

        public LuaVarFilterStateTypeEnd(UInt16 iPluginid, char chWhat)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);

            return buffer;
        }

        public Byte What;
    }

    [ScmpSend]
    internal class LuaVarFilterStateNameBegin : Base
    {
        public LuaVarFilterStateNameBegin()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateNameBegin;
        }

        public LuaVarFilterStateNameBegin(UInt16 iPluginid, char chWhat)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);

            return buffer;
        }

        public Byte What;
    }

    [ScmpSend]
    internal class LuaVarFilterStateName : Base
    {
        public LuaVarFilterStateName()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateName;
        }

        public LuaVarFilterStateName(UInt16 iPluginid, char chWhat, string filter)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
            Filter = filter;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1 + (2 + Encoding.UTF8.GetBytes(Filter).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);
            packer.PackString(Filter);

            return buffer;
        }

        public Byte What;
        public string Filter;
    }

    [ScmpSend]
    internal class LuaVarFilterStateNameEnd : Base
    {
        public LuaVarFilterStateNameEnd()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarFilterStateNameEnd;
        }

        public LuaVarFilterStateNameEnd(UInt16 iPluginid, char chWhat)
            : this()
        {
            PluginId = iPluginid;

            What = (Byte)chWhat;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte(What);

            return buffer;
        }

        public Byte What;
    }

    [ScmpReceive]
    internal class LuaVarGlobalBegin : Base
    {
        public LuaVarGlobalBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarGlobalBegin;
        }

        public LuaVarGlobalBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarGlobal : Base
    {
        public LuaVarGlobal()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarGlobal;
            Hierarchy = new List<KeyValuePair<string, int>>();
        }

        public LuaVarGlobal(UInt16 iPluginid, string name, Int16 nameType, string value, Int16 valueType)
            : this()
        {
            PluginId = iPluginid;

            Name = name;
            KeyType = nameType;
            Value = value;
            What = valueType;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Name = reader.ReadString();
            KeyType = reader.ReadInt16();
            Value = reader.ReadString();
            What = reader.ReadInt16();

            var count = reader.ReadUInt16();
            for (UInt16 i = 0; i < count; ++i)
            {
                var name = reader.ReadString();
                var type = reader.ReadInt16();
                Hierarchy.Add(new KeyValuePair<string, int>(name, type));
            }
        }

        public string Name;
        public Int16 KeyType;
        public string Value;
        public Int16 What;
        public List<KeyValuePair<string, int>> Hierarchy;
    }

    [ScmpReceive]
    internal class LuaVarGlobalEnd : Base
    {
        public LuaVarGlobalEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarGlobalEnd;
        }

        public LuaVarGlobalEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarGlobalLookupBegin : Base
    {
        public LuaVarGlobalLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarGlobalLookupBegin;
        }

        public LuaVarGlobalLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarGlobalLookupEnd : Base
    {
        public LuaVarGlobalLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarGlobalLookupEnd;
        }

        public LuaVarGlobalLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarLocalBegin : Base
    {
        public LuaVarLocalBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLocalBegin;
        }

        public LuaVarLocalBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarLocal : Base
    {
        public LuaVarLocal()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLocal;
            Hierarchy = new List<KeyValuePair<string, int>>();
        }

        public LuaVarLocal(UInt16 iPluginid, string name, Int16 nameType, string value, Int16 valueType, Int16 stackLevel, Int32 index)
            : this()
        {
            PluginId = iPluginid;

            Name = name;
            KeyType = nameType;
            Value = value;
            What = valueType;

            StackLevel = stackLevel;
            Index = index;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Name = reader.ReadString();
            KeyType = reader.ReadInt16();
            Value = reader.ReadString();
            What = reader.ReadInt16();

            StackLevel = reader.ReadInt16();
            Index = reader.ReadInt32();

            var count = reader.ReadUInt16();
            for (UInt16 i = 0; i < count; ++i)
            {
                var name = reader.ReadString();
                var type = reader.ReadInt16();
                Hierarchy.Add(new KeyValuePair<string, int>(name, type));
            }
        }

        public string Name;
        public Int16 KeyType;
        public string Value;
        public Int16 What;
        public Int16 StackLevel;
        public Int32 Index;
        public List<KeyValuePair<string, int>> Hierarchy;
    }

    [ScmpReceive]
    internal class LuaVarLocalEnd : Base
    {
        public LuaVarLocalEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLocalEnd;
        }

        public LuaVarLocalEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarLocalLookupBegin : Base
    {
        public LuaVarLocalLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLocalLookupBegin;
        }

        public LuaVarLocalLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarLocalLookupEnd : Base
    {
        public LuaVarLocalLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLocalLookupEnd;
        }

        public LuaVarLocalLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarUpvalueBegin : Base
    {
        public LuaVarUpvalueBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarUpvalueBegin;
        }

        public LuaVarUpvalueBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarUpvalue : Base
    {
        public LuaVarUpvalue()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarUpvalue;
            Hierarchy = new List<KeyValuePair<string, int>>();
        }

        public LuaVarUpvalue(UInt16 iPluginid, string name, Int16 nameType, string value, Int16 valueType, Int16 stackLevel, Int32 index)
            : this()
        {
            PluginId = iPluginid;

            Name = name;
            KeyType = nameType;
            Value = value;
            What = valueType;

            StackLevel = stackLevel;
            Index = index;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Name = reader.ReadString();
            KeyType = reader.ReadInt16();
            Value = reader.ReadString();
            What = reader.ReadInt16();

            StackLevel = reader.ReadInt16();
            Index = reader.ReadInt32();

            var count = reader.ReadUInt16();
            for (UInt16 i = 0; i < count; ++i)
            {
                var name = reader.ReadString();
                var type = reader.ReadInt16();
                Hierarchy.Add(new KeyValuePair<string, int>(name, type));
            }
        }

        public string Name;
        public Int16 KeyType;
        public string Value;
        public Int16 What;
        public Int16 StackLevel;
        public Int32 Index;
        public List<KeyValuePair<string, int>> Hierarchy;
    }

    [ScmpReceive]
    internal class LuaVarUpvalueEnd : Base
    {
        public LuaVarUpvalueEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarUpvalueEnd;
        }

        public LuaVarUpvalueEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarUpvalueLookupBegin : Base
    {
        public LuaVarUpvalueLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarUpvalueLookupBegin;
        }

        public LuaVarUpvalueLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarUpvalueLookupEnd : Base
    {
        public LuaVarUpvalueLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarUpvalueLookupEnd;
        }

        public LuaVarUpvalueLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarEnvVarBegin : Base
    {
        public LuaVarEnvVarBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarEnvVarBegin;
        }

        public LuaVarEnvVarBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarEnvVar : Base
    {
        public LuaVarEnvVar()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarEnvVar;
            Hierarchy = new List<KeyValuePair<string, int>>();
        }

        public LuaVarEnvVar(UInt16 iPluginid, string name, Int16 nameType, string value, Int16 valueType, Int16 stackLevel)
            : this()
        {
            PluginId = iPluginid;

            Name = name;
            KeyType = nameType;
            Value = value;
            What = valueType;

            StackLevel = stackLevel;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Name = reader.ReadString();
            KeyType = reader.ReadInt16();
            Value = reader.ReadString();
            What = reader.ReadInt16();

            StackLevel = reader.ReadInt16();

            var count = reader.ReadUInt16();
            for (UInt16 i = 0; i < count; ++i)
            {
                var name = reader.ReadString();
                var type = reader.ReadInt16();
                Hierarchy.Add(new KeyValuePair<string, int>(name, type));
            }
        }

        public string Name;
        public Int16 KeyType;
        public string Value;
        public Int16 What;
        public Int16 StackLevel;
        public List<KeyValuePair<string, int>> Hierarchy;
    }

    [ScmpReceive]
    internal class LuaVarEnvVarEnd : Base
    {
        public LuaVarEnvVarEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarEnvVarEnd;
        }

        public LuaVarEnvVarEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarEnvVarLookupBegin : Base
    {
        public LuaVarEnvVarLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarEnvVarLookupBegin;
        }

        public LuaVarEnvVarLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaVarEnvVarLookupEnd : Base
    {
        public LuaVarEnvVarLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarEnvVarLookupEnd;
        }

        public LuaVarEnvVarLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    internal class LuaVarLookUp : Base
    {
        public LuaVarLookUp()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaVarLookUp;
        }

        public LuaVarLookUp(UInt16 iPluginId, SledLuaVarLookUpType lookUp)
            : this(iPluginId, lookUp, false)
        {
        }

        public LuaVarLookUp(UInt16 iPluginId, SledLuaVarLookUpType lookUp, bool bExtra)
            : this()
        {
            PluginId = iPluginId;
            LookUp = lookUp;
            Extra = (Byte)(bExtra ? 1 : 0);
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            int size
                = SizeOf            // Base
                + 1                 // Byte     - what
                + 1                 // Byte     - context
                + 2                 // UInt16   - key value pairs count
                + 2                 // Int16    - stack level
                + 4                 // Int32    - index
                + 1;                // Byte     - extra

            foreach (var kv in LookUp.NamesAndTypes)
            {
                var bytes = Encoding.UTF8.GetBytes(kv.Name);

                size += 2;              // UInt16   - length of string that follows
                size += bytes.Length;   // UInt16   - string
                size += 2;              // UInt16   - name lua_t<type>
            }

            var buffer = new byte[size];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte((Byte)((int)LookUp.Scope));
            packer.PackByte((Byte)((int)LookUp.Context));
            packer.PackUInt16((UInt16)LookUp.NamesAndTypes.Count);
            packer.PackInt16((Int16)LookUp.StackLevel);
            packer.PackInt32(LookUp.Index);
            packer.PackByte(Extra);

            foreach (var kv in LookUp.NamesAndTypes)
            {
                packer.PackString(kv.Name);
                packer.PackUInt16((UInt16)kv.NameType);
            }

            return buffer;
        }

        public SledLuaVarLookUpType LookUp;
        public Byte Extra;
    }

    [ScmpSend]
    internal class LuaVarUpdate : Base
    {
        public LuaVarUpdate()
        {
            Length = SizeOf;
            TypeCode = (UInt16) LuaTypeCodes.LuaVarUpdate;
        }

        public LuaVarUpdate(UInt16 iPluginId, SledLuaVarLookUpType lookUp, string value, int valueType)
            : this()
        {
            PluginId = iPluginId;

            LookUp = lookUp;
            Value = value;
            ValueType = (Int16)valueType;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            int size
                = SizeOf            // Base
                + 1                 // Byte     - what
                + 1                 // Byte     - context
                + 2                 // UInt16   - key value pairs count
                + 2                 // Int16    - stack level
                + 4                 // Int32    - index
                + 2 + Encoding.UTF32.GetBytes(Value).Length // string length + string
                + 2;                // Int16    - value type

            foreach (var kv in LookUp.NamesAndTypes)
            {
                var bytes = Encoding.UTF8.GetBytes(kv.Name);

                size += 2;              // UInt16   - length of string that follows
                size += bytes.Length;   // UInt16   - string
                size += 2;              // UInt16   - name lua_t<type>
            }

            var buffer = new byte[size];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte((Byte)(int)LookUp.Scope);
            packer.PackByte((Byte)(int)LookUp.Context);
            packer.PackUInt16((UInt16)LookUp.NamesAndTypes.Count);
            packer.PackInt16((Int16)LookUp.StackLevel);
            packer.PackInt32(LookUp.Index);
            packer.PackString(Value);
            packer.PackInt16(ValueType);

            foreach (var kv in LookUp.NamesAndTypes)
            {
                packer.PackString(kv.Name);
                packer.PackUInt16((UInt16)kv.NameType);
            }

            return buffer;
        }

        public SledLuaVarLookUpType LookUp;
        public string Value;
        public Int16 ValueType;
    }

    [ScmpReceive]
    internal class LuaCallStackBegin : Base
    {
        public LuaCallStackBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackBegin;
        }

        public LuaCallStackBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaCallStack : Base
    {
        public LuaCallStack()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStack;
        }

        public LuaCallStack(UInt16 iPluginid, string szRelScriptPath, Int32 iCurrentLine, Int32 iLineDefined, Int32 iLastLineDefined, string szFunctionName, Int16 iStackLevel)
            : this()
        {
            PluginId = iPluginid;

            RelScriptPath = szRelScriptPath;
            CurrentLine = iCurrentLine;
            LineDefined = iLineDefined;
            LastLineDefined = iLastLineDefined;
            FunctionName = szFunctionName;
            StackLevel = iStackLevel;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            RelScriptPath = reader.ReadString();
            CurrentLine = reader.ReadInt32();
            LineDefined = reader.ReadInt32();
            LastLineDefined = reader.ReadInt32();
            FunctionName = reader.ReadString();
            StackLevel = reader.ReadInt16();
        }

        public string RelScriptPath;
        public Int32 CurrentLine;
        public Int32 LineDefined;
        public Int32 LastLineDefined;
        public string FunctionName;
        public Int16 StackLevel;
    }

    [ScmpReceive]
    internal class LuaCallStackEnd : Base
    {
        public LuaCallStackEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackEnd;
        }

        public LuaCallStackEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    internal class LuaCallStackLookupPerform : Base
    {
        public LuaCallStackLookupPerform()
        {
            Length = SizeOf + 2;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackLookupPerform;
        }

        public LuaCallStackLookupPerform(UInt16 iPluginid, Int16 iStackLevel)
            : this()
        {
            PluginId = iPluginid;
            StackLevel = iStackLevel;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 2];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackInt16(StackLevel);

            return buffer;
        }

        public Int16 StackLevel;
    }

    [ScmpReceive]
    internal class LuaCallStackLookupBegin : Base
    {
        public LuaCallStackLookupBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackLookupBegin;
        }

        public LuaCallStackLookupBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaCallStackLookup : Base
    {
        public LuaCallStackLookup()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackLookup;
        }

        public LuaCallStackLookup(UInt16 iPluginid, string szFunctionName, Int32 iLineDefined, Int16 iStackLevel)
            : this()
        {
            PluginId = iPluginid;
            LineDefined = iLineDefined;
            FunctionName = szFunctionName;
            StackLevel = iStackLevel;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            FunctionName = reader.ReadString();
            LineDefined = reader.ReadInt32();
            StackLevel = reader.ReadInt16();
        }

        public string FunctionName;
        public Int32 LineDefined;
        public Int16 StackLevel;
    }

    [ScmpReceive]
    internal class LuaCallStackLookupEnd : Base
    {
        public LuaCallStackLookupEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaCallStackLookupEnd;
        }

        public LuaCallStackLookupEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    [ScmpReceive]
    internal class LuaWatchLookupBegin : Base
    {
        public LuaWatchLookupBegin()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupBegin;
        }

        public LuaWatchLookupBegin(UInt16 iPluginid, SledLuaVarScopeType scope)
            : this()
        {
            PluginId = iPluginid;
            Scope = scope;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte((Byte)(int)Scope);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Scope = (SledLuaVarScopeType)reader.ReadByte();
        }

        public SledLuaVarScopeType Scope;
    }

    [ScmpSend]
    [ScmpReceive]
    internal class LuaWatchLookupEnd : Base
    {
        public LuaWatchLookupEnd()
        {
            Length = SizeOf + 1;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupEnd;
        }

        public LuaWatchLookupEnd(UInt16 iPluginid, SledLuaVarScopeType scope)
            : this()
        {
            PluginId = iPluginid;
            Scope = scope;
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + 1];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackByte((Byte)(int)Scope);

            return buffer;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Scope = (SledLuaVarScopeType)reader.ReadByte();
        }

        public SledLuaVarScopeType Scope;
    }

    [ScmpReceive]
    internal class LuaWatchLookupClear : Base
    {
        public LuaWatchLookupClear()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupClear;
        }

        public LuaWatchLookupClear(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaWatchLookupProjectBegin : Base
    {
        public LuaWatchLookupProjectBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupProjectBegin;
        }

        public LuaWatchLookupProjectBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaWatchLookupProjectEnd : Base
    {
        public LuaWatchLookupProjectEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupProjectEnd;
        }

        public LuaWatchLookupProjectEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaWatchLookupCustomBegin : Base
    {
        public LuaWatchLookupCustomBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupCustomBegin;
        }

        public LuaWatchLookupCustomBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaWatchLookupCustomEnd : Base
    {
        public LuaWatchLookupCustomEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaWatchLookupCustomEnd;
        }

        public LuaWatchLookupCustomEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }
    
    [ScmpReceive]
    internal class LuaStateBegin : Base
    {
        public LuaStateBegin()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaStateBegin;
        }

        public LuaStateBegin(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaStateAdd : Base
    {
        public LuaStateAdd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaStateAdd;
        }

        public LuaStateAdd(UInt16 iPluginid, string szAddress, string szName, bool bDebugging)
            : this()
        {
            PluginId = iPluginid;
            
            Address = szAddress;
            Name = szName;
            Debugging = (Byte)(bDebugging ? 1 : 0);
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Address = reader.ReadString();
            Name = reader.ReadString();
            Debugging = reader.ReadByte();
        }

        public string Address;
        public string Name;
        public Byte Debugging;
    }

    [ScmpReceive]
    internal class LuaStateRemove : Base
    {
        public LuaStateRemove()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaStateRemove;
        }

        public LuaStateRemove(UInt16 iPluginid, string szAddress)
            : this()
        {
            PluginId = iPluginid;

            Address = szAddress;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            Address = reader.ReadString();
        }

        public string Address;
    }

    [ScmpReceive]
    internal class LuaStateEnd : Base
    {
        public LuaStateEnd()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaStateEnd;
        }

        public LuaStateEnd(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    internal class LuaStateToggle : Base
    {
        public LuaStateToggle()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaStateToggle;
        }

        public LuaStateToggle(UInt16 iPluginid, string address)
            : this()
        {
            PluginId = iPluginid;
            
            Address = address.Replace(Resource.HexAddress, string.Empty);
        }

        /// <summary>
        /// Wrap all contents into a byte array for sending
        /// </summary>
        public override byte[] Pack()
        {
            var buffer = new byte[SizeOf + (2 + Encoding.UTF8.GetBytes(Address).Length)];
            base.Pack(ref buffer, buffer.Length);

            var packer = new SledNetworkBufferPacker(ref buffer, SizeOf);
            packer.PackString(Address);

            return buffer;
        }

        public string Address;
    }

    [ScmpSend]
    [ScmpReceive]
    internal class LuaMemoryTraceToggle : Base
    {
        public LuaMemoryTraceToggle()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaMemoryTraceToggle;
        }

        public LuaMemoryTraceToggle(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpSend]
    [ScmpReceive]
    internal class LuaProfilerToggle : Base
    {
        public LuaProfilerToggle()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaProfilerToggle;
        }

        public LuaProfilerToggle(UInt16 iPluginid)
            : this()
        {
            PluginId = iPluginid;
        }
    }

    [ScmpReceive]
    internal class LuaLimits : Base
    {
        public LuaLimits()
        {
            Length = SizeOf;
            TypeCode = (UInt16)LuaTypeCodes.LuaLimits;
        }

        public LuaLimits(UInt16 iPluginId, UInt16 iMaxBreakpoints, UInt16 iMaxVarFilters)
            : this()
        {
            PluginId = iPluginId;

            MaxBreakpoints = iMaxBreakpoints;
            MaxVarFilters = iMaxVarFilters;
        }

        /// <summary>
        /// Upon receiving data, unpack the byte array into the class members
        /// </summary>
        public override void Unpack(byte[] buffer)
        {
            // Read the Scmp.Base structure
            base.Unpack(buffer);

            // Create reader to easily unpack the network buffer
            var reader = new SledNetworkBufferReader(buffer, SizeOf);

            MaxBreakpoints = reader.ReadUInt16();
            MaxVarFilters = reader.ReadUInt16();
            ProfilerEnabled = (reader.ReadByte() == 1);
            MemoryTracerEnabled = (reader.ReadByte() == 1);
        }

        public UInt16 MaxBreakpoints;
        public UInt16 MaxVarFilters;
        public bool ProfilerEnabled;
        public bool MemoryTracerEnabled;
    }
}
