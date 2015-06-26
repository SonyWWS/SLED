/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Linq;
using System.Text;

using Sce.Sled.Shared.Scmp;

using NUnit.Framework;

namespace Sce.Sled.Shared.UnitTests
{
    [TestFixture]
    public class TestScmp
    {
        [Test]
        public void NetworkBufferReaderAndPacker()
        {
            NetworkBufferReaderAndPacker(Services.Endian.Little);
            NetworkBufferReaderAndPacker(Services.Endian.Big);
        }

        private void NetworkBufferReaderAndPacker(Services.Endian endianness)
        {
            SledNetworkBufferPacker.Endian = endianness;
            SledNetworkBufferReader.Endian = endianness;

            // Create some stuff to store in a buffer
            const Byte u8 = (byte)253;                // 0 to 255

            const UInt16 u16 = 65533;                 // 0 to 65,535
            const UInt32 u32 = 4294967293;            // 0 to 4,294,967,295 
            const UInt64 u64 = 18446744073709551613;  // 0 to 18,446,744,073,709,551,615

            const Int16 i16P = 32765;                 // negative 32,768 through positive 32,767
            const Int16 i16N = -32766;
            const Int32 i32P = 2147483645;            // negative 2,147,483,648 through positive 2,147,483,647
            const Int32 i32N = -2147483646;
            const Int64 i64P = 9223372036854775805;   // negative 9,223,372,036,854,775,808 through positive 9,223,372,036,854,775,807
            const Int64 i64N = -9223372036854775806;

            const Single f32P = 3.4123456f;
            const Single f32N = -3.4123456f;

            const Double f64P = 1.7123456789012345;
            const Double f64N = -1.7123456789012345;

            const string strTest = "Hello world! World hello! hello World! world Hello!";

            var bytesAscii = Encoding.ASCII.GetBytes(strTest);
            var bytesUtf8 = Encoding.UTF8.GetBytes(strTest);
            var bytesUtf32 = Encoding.UTF32.GetBytes(strTest);
            var bytesUnicode = Encoding.Unicode.GetBytes(strTest);

            // Length of everything going in the buffer
            var iSizeOfBuffer =
                SledNetworkBufferReader.SizeOfByte +        // u8
                SledNetworkBufferReader.SizeOfUInt16 +      // u16
                SledNetworkBufferReader.SizeOfUInt32 +      // u32
                SledNetworkBufferReader.SizeOfUInt64 +      // u64
                SledNetworkBufferReader.SizeOfInt16 +       // i16p
                SledNetworkBufferReader.SizeOfInt16 +       // i16n
                SledNetworkBufferReader.SizeOfInt32 +       // i32p
                SledNetworkBufferReader.SizeOfInt32 +       // i32n
                SledNetworkBufferReader.SizeOfInt64 +       // i64p
                SledNetworkBufferReader.SizeOfInt64 +       // i64n
                SledNetworkBufferReader.SizeOfFloat +       // f32p
                SledNetworkBufferReader.SizeOfFloat +       // f32n
                SledNetworkBufferReader.SizeOfDouble +      // f64p
                SledNetworkBufferReader.SizeOfDouble +      // f64n
                (Encoding.UTF8.GetBytes(strTest).Length + SledNetworkBufferReader.SizeOfUInt16) +   // str          + length
                (bytesAscii.Length + SledNetworkBufferReader.SizeOfUInt16) +                    // bytesAscii   + length
                (bytesUtf8.Length + SledNetworkBufferReader.SizeOfUInt16) +                     // bytesUtf8    + length
                (bytesUtf32.Length + SledNetworkBufferReader.SizeOfUInt16) +                    // bytesUtf32   + length
                (bytesUnicode.Length + SledNetworkBufferReader.SizeOfUInt16);                   // bytesUnicode + length

            // Create buffer to store everything in
            var buffer1 = new byte[iSizeOfBuffer];

            // Set endianness
            const Services.Endian endian = Services.Endian.Little;
            SledNetworkBufferPacker.Endian = endian;
            SledNetworkBufferReader.Endian = endian;

            // Pack everything into the buffer
            var packer1 = new SledNetworkBufferPacker(ref buffer1);
            packer1.PackByte(u8);
            packer1.PackUInt16(u16);
            packer1.PackUInt32(u32);
            packer1.PackUInt64(u64);
            packer1.PackInt16(i16P);
            packer1.PackInt16(i16N);
            packer1.PackInt32(i32P);
            packer1.PackInt32(i32N);
            packer1.PackInt64(i64P);
            packer1.PackInt64(i64N);
            packer1.PackFloat(f32P);
            packer1.PackFloat(f32N);
            packer1.PackDouble(f64P);
            packer1.PackDouble(f64N);
            packer1.PackString(strTest);
            packer1.PackByteArray(bytesAscii);
            packer1.PackByteArray(bytesUtf8);
            packer1.PackByteArray(bytesUtf32);
            packer1.PackByteArray(bytesUnicode);

            // Read everything from the buffer & validate
            var reader1 = new SledNetworkBufferReader(buffer1);
            Assert.That(u8 == reader1.ReadByte());
            Assert.That(u16 == reader1.ReadUInt16());
            Assert.That(u32 == reader1.ReadUInt32());
            Assert.That(u64 == reader1.ReadUInt64());
            Assert.That(i16P == reader1.ReadInt16());
            Assert.That(i16N == reader1.ReadInt16());
            Assert.That(i32P == reader1.ReadInt32());
            Assert.That(i32N == reader1.ReadInt32());
            Assert.That(i64P == reader1.ReadInt64());
            Assert.That(i64N == reader1.ReadInt64());
            Assert.That(f32P == reader1.ReadFloat());
            Assert.That(f32N == reader1.ReadFloat());
            Assert.That(f64P == reader1.ReadDouble());
            Assert.That(f64N == reader1.ReadDouble());
            Assert.That(string.Compare(strTest, reader1.ReadString()) == 0);
            Assert.That(AreByteArraysEqual(bytesAscii, reader1.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUtf8, reader1.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUtf32, reader1.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUnicode, reader1.ReadByteArray()));

            //
            // Do stuff again with an initial offset
            //

            // Copy over buffer1 into buffer2 but leave a gap at the beginning of buffer2
            const int iOffset = 32;
            var buffer2 = new byte[iSizeOfBuffer + iOffset];
            Buffer.BlockCopy(buffer1, 0, buffer2, iOffset, buffer1.Length);

            // Pack everything into the buffer
            var packer2 = new SledNetworkBufferPacker(ref buffer2, iOffset);
            packer2.PackByte(u8);
            packer2.PackUInt16(u16);
            packer2.PackUInt32(u32);
            packer2.PackUInt64(u64);
            packer2.PackInt16(i16P);
            packer2.PackInt16(i16N);
            packer2.PackInt32(i32P);
            packer2.PackInt32(i32N);
            packer2.PackInt64(i64P);
            packer2.PackInt64(i64N);
            packer2.PackFloat(f32P);
            packer2.PackFloat(f32N);
            packer2.PackDouble(f64P);
            packer2.PackDouble(f64N);
            packer2.PackString(strTest);
            packer2.PackByteArray(bytesAscii);
            packer2.PackByteArray(bytesUtf8);
            packer2.PackByteArray(bytesUtf32);
            packer2.PackByteArray(bytesUnicode);

            // Read everything from the buffer & validate
            var reader2 = new SledNetworkBufferReader(buffer2, iOffset);
            Assert.That(u8 == reader2.ReadByte());
            Assert.That(u16 == reader2.ReadUInt16());
            Assert.That(u32 == reader2.ReadUInt32());
            Assert.That(u64 == reader2.ReadUInt64());
            Assert.That(i16P == reader2.ReadInt16());
            Assert.That(i16N == reader2.ReadInt16());
            Assert.That(i32P == reader2.ReadInt32());
            Assert.That(i32N == reader2.ReadInt32());
            Assert.That(i64P == reader2.ReadInt64());
            Assert.That(i64N == reader2.ReadInt64());
            Assert.That(f32P == reader2.ReadFloat());
            Assert.That(f32N == reader2.ReadFloat());
            Assert.That(f64P == reader2.ReadDouble());
            Assert.That(f64N == reader2.ReadDouble());
            Assert.That(string.Compare(strTest, reader2.ReadString()) == 0);
            Assert.That(AreByteArraysEqual(bytesAscii, reader2.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUtf8, reader2.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUtf32, reader2.ReadByteArray()));
            Assert.That(AreByteArraysEqual(bytesUnicode, reader2.ReadByteArray()));
        }

        private static bool AreByteArraysEqual(byte[] byteArray1, byte[] byteArray2)
        {
            if ((byteArray1 == null) || (byteArray2 == null))
                return false;

            return
                (byteArray1.Length == byteArray2.Length) &&
                byteArray1.SequenceEqual(byteArray2);
        }

        [Test]
        public void ScmpMessages()
        {
            const UInt16 iPluginId = 1;

            const string someString1 = @"path\to\some\file.txt";
            const string someString2 = "Hello world!";
            //Guid someGuid = Guid.NewGuid();
            const UInt16 someUInt161 = 24;
            const UInt16 someUInt162 = 11;
            const UInt16 someUInt163 = 42;
            const int someInt1 = 32;
            const bool someBool1 = true;
            const bool someBool2 = false;

            var buffer = new byte[0];

            //
            // Make sure some Pack() & Read() functions create the same data
            //

            Assert
                .That(PackAndReturn(new BreakpointDetails(iPluginId, someString1, someInt1, someString2, someBool1, someBool2), ref buffer)
                .IsEqualTo(CreateFromBuffer<BreakpointDetails>(buffer)));

            Assert
                .That(PackAndReturn(new BreakpointBegin(iPluginId, iPluginId, someString1, someInt1), ref buffer)
                .IsEqualTo(CreateFromBuffer<BreakpointBegin>(buffer)));

            Assert
                .That(PackAndReturn(new BreakpointSync(iPluginId, iPluginId, someString1, someInt1), ref buffer)
                .IsEqualTo(CreateFromBuffer<BreakpointSync>(buffer)));

            Assert
                .That(PackAndReturn(new BreakpointEnd(iPluginId, iPluginId, someString1, someInt1), ref buffer)
                .IsEqualTo(CreateFromBuffer<BreakpointEnd>(buffer)));

            Assert
                .That(PackAndReturn(new BreakpointContinue(iPluginId, iPluginId, someString1, someInt1), ref buffer)
                .IsEqualTo(CreateFromBuffer<BreakpointContinue>(buffer)));

            Assert
                .That(PackAndReturn(new Heartbeat(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<Heartbeat>(buffer)));

            Assert
                .That(PackAndReturn(new Success(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<Success>(buffer)));

            Assert
                .That(PackAndReturn(new Failure(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<Failure>(buffer)));

            Assert
                .That(PackAndReturn(new Scmp.Version(iPluginId, someUInt161, someUInt162, someUInt163), ref buffer)
                .IsEqualTo(CreateFromBuffer<Scmp.Version>(buffer)));

            Assert
                .That(PackAndReturn(new DebugStart(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<DebugStart>(buffer)));

            Assert
                .That(PackAndReturn(new DebugStepInto(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<DebugStepInto>(buffer)));

            Assert
                .That(PackAndReturn(new DebugStepOver(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<DebugStepOver>(buffer)));

            Assert
                .That(PackAndReturn(new DebugStepOut(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<DebugStepOut>(buffer)));

            Assert
                .That(PackAndReturn(new DebugStop(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<DebugStop>(buffer)));

            Assert
                .That(PackAndReturn(new ScriptCache(iPluginId, someString1), ref buffer)
                .IsEqualTo(CreateFromBuffer<ScriptCache>(buffer)));

            Assert
                .That(PackAndReturn(new Authenticated(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<Authenticated>(buffer)));

            Assert
                .That(PackAndReturn(new Ready(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<Ready>(buffer)));

            Assert
                .That(PackAndReturn(new PluginsReady(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<PluginsReady>(buffer)));

            Assert
                .That(PackAndReturn(new FunctionInfo(iPluginId, someString1, someString2, someInt1), ref buffer)
                .IsEqualTo(CreateFromBuffer<FunctionInfo>(buffer)));

            Assert
                .That(PackAndReturn(new TtyBegin(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<TtyBegin>(buffer)));

            Assert
                .That(PackAndReturn(new Tty(iPluginId, someString1), ref buffer)
                .IsEqualTo(CreateFromBuffer<Tty>(buffer)));

            Assert
                .That(PackAndReturn(new TtyEnd(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<TtyEnd>(buffer)));

            Assert
                .That(PackAndReturn(new DevCmd(iPluginId, someString1), ref buffer)
                .IsEqualTo(CreateFromBuffer<DevCmd>(buffer)));

            Assert
                .That(PackAndReturn(new EditAndContinue(iPluginId, someString1), ref buffer)
                .IsEqualTo(CreateFromBuffer<EditAndContinue>(buffer)));

            // Skip Endianness

            Assert
                .That(PackAndReturn(new ProtocolDebugMark(iPluginId), ref buffer)
                .IsEqualTo(CreateFromBuffer<ProtocolDebugMark>(buffer)));
        }

        [Test]
        public void ConvertVersionToInt()
        {
            {
                const int expected = 10;
                var ver1 = new Scmp.Version(0, 1, 0, 0);
                Assert.AreEqual(expected, ver1.ToInt32());
            }

            {
                const int expected = 35;
                var ver2 = new Scmp.Version(0, 3, 5, 24);
                Assert.AreEqual(expected, ver2.ToInt32());
            }
        }

        private static IScmp PackAndReturn(IScmp item, ref byte[] buffer)
        {
            buffer = item.Pack();
            return item;
        }

        private static IScmp CreateFromBuffer<T>(byte[] buffer) where T : IScmp, new()
        {
            var scmpItem = new T();
            scmpItem.Unpack(buffer);
            return scmpItem;
        }
    }
}
