/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Diagnostics;
using System.Text;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Shared.Scmp
{
    /// <summary>
    /// Utility class to help pack a network byte array
    /// </summary>
    public class SledNetworkBufferPacker
    {
        /// <summary>
        /// Size of a byte in bytes
        /// </summary>
        public const int SizeOfByte = 1;
        /// <summary>
        /// Size of a UInt16 in bytes
        /// </summary>
        public const int SizeOfUInt16 = 2;
        /// <summary>
        /// Size of a UInt32 in bytes
        /// </summary>
        public const int SizeOfUInt32 = 4;
        /// <summary>
        /// Size of a UInt64 in bytes
        /// </summary>
        public const int SizeOfUInt64 = 8;
        /// <summary>
        /// Size of a Int16 in bytes
        /// </summary>
        public const int SizeOfInt16 = 2;
        /// <summary>
        /// Size of a Int32 in bytes
        /// </summary>
        public const int SizeOfInt32 = 4;
        /// <summary>
        /// Size of a Int64 in bytes
        /// </summary>
        public const int SizeOfInt64 = 8;
        /// <summary>
        /// Size of float in bytes
        /// </summary>
        public const int SizeOfFloat = 4;
        /// <summary>
        /// Size of double in bytes
        /// </summary>
        public const int SizeOfDouble = 8;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer to pack. It must be initialized and be large enough to fit all elements.</param>
        public SledNetworkBufferPacker(ref byte[] buffer)
            : this(ref buffer, 0)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer to pack. It must be initialized and be large enough to fit all elements.</param>
        /// <param name="iInitialOffset">Initial offset to start packing from, if not zero</param>
        public SledNetworkBufferPacker(ref byte[] buffer, int iInitialOffset)
        {
            m_buffer = buffer;
            m_offset = iInitialOffset;
        }

        /// <summary>
        /// Pack a byte
        /// </summary>
        /// <param name="value">Byte to pack</param>
        public void PackByte(Byte value)
        {
            m_buffer[m_offset] = value;
            m_offset += SizeOfByte;
        }

        /// <summary>
        /// Pack a UInt16
        /// </summary>
        /// <param name="value">UInt16 to pack</param>
        public void PackUInt16(UInt16 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfUInt16);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfUInt16);
            m_offset += SizeOfUInt16;
        }

        /// <summary>
        /// Pack a UInt32
        /// </summary>
        /// <param name="value">UInt32 to pack</param>
        public void PackUInt32(UInt32 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfUInt32);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfUInt32);
            m_offset += SizeOfUInt32;
        }

        /// <summary>
        /// Pack a UInt64
        /// </summary>
        /// <param name="value">UInt64 to pack</param>
        public void PackUInt64(UInt64 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfUInt64);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfUInt64);
            m_offset += SizeOfUInt64;
        }

        /// <summary>
        /// Pack a Int16
        /// </summary>
        /// <param name="value">Int16 to pack</param>
        public void PackInt16(Int16 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfInt16);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfInt16);
            m_offset += SizeOfInt16;
        }

        /// <summary>
        /// Pack a Int32
        /// </summary>
        /// <param name="value">Int32 to pack</param>
        public void PackInt32(Int32 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfInt32);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfInt32);
            m_offset += SizeOfInt32;
        }

        /// <summary>
        /// Pack a Int64
        /// </summary>
        /// <param name="value">Int64 to pack</param>
        public void PackInt64(Int64 value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfInt64);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfInt64);
            m_offset += SizeOfInt64;
        }

        /// <summary>
        /// Pack a string
        /// </summary>
        /// <param name="value">String to pack</param>
        public void PackString(string value)
        {
            var buf = Encoding.UTF8.GetBytes(value);
            PackUInt16((UInt16)buf.Length);
            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, buf.Length);
            m_offset += buf.Length;
        }

        /// <summary>
        /// Pack a byte array
        /// </summary>
        /// <param name="value">Byte array to pack</param>
        public void PackByteArray(byte[] value)
        {
            PackUInt16((UInt16)value.Length);
            Buffer.BlockCopy(value, 0, m_buffer, m_offset, value.Length);
            m_offset += value.Length;
        }

        /// <summary>
        /// Pack a float
        /// </summary>
        /// <param name="value">Float to pack</param>
        public void PackFloat(float value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfFloat);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfFloat);
            m_offset += SizeOfFloat;
        }

        /// <summary>
        /// Pack a double
        /// </summary>
        /// <param name="value">Double to pack</param>
        public void PackDouble(double value)
        {
            var buf = BitConverter.GetBytes(value);
            CheckLengthAgainstSize(buf.Length, SizeOfDouble);

            if (s_endianness == Endian.Big)
                Array.Reverse(buf);

            Buffer.BlockCopy(buf, 0, m_buffer, m_offset, SizeOfDouble);
            m_offset += SizeOfDouble;
        }

        [Conditional("DEBUG")]
        private static void CheckLengthAgainstSize(int length, int size)
        {
            if (length == size)
                return;

            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private readonly byte[] m_buffer;
        private int m_offset;

        /// <summary>
        /// Get or set endianness that the buffer packers should use
        /// </summary>
        public static Endian Endian
        {
            get { return s_endianness; }
            set
            {
                s_endianness = value;

                // Default to little endian
                if (value == Endian.Unknown)
                    s_endianness = Endian.Little;
            }
        }

        private static Endian s_endianness;
    }
}
