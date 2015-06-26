/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Text;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Shared.Scmp
{
    /// <summary>
    /// Utility class to help read the network buffer
    /// </summary>
    public class SledNetworkBufferReader
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
        /// Size of a float in bytes
        /// </summary>
        public const int SizeOfFloat = 4;
        /// <summary>
        /// Size of a double in bytes
        /// </summary>
        public const int SizeOfDouble = 8;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">buffer to read</param>
        public SledNetworkBufferReader(byte[] buffer)
            : this(buffer, 0)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer to read</param>
        /// <param name="iInitialOffset">Initial offset in buffer to start reading (if not supposed to be zero)</param>
        public SledNetworkBufferReader(byte[] buffer, int iInitialOffset)
        {
            m_buffer = buffer;
            m_offset = iInitialOffset;
        }

        /// <summary>
        /// Read a Byte from the network buffer
        /// </summary>
        /// <returns>Byte data read</returns>
        public Byte ReadByte()
        {
            var ret = m_buffer[m_offset];
            m_offset += SizeOfByte;
            return ret;
        }

        /// <summary>
        /// Read a UInt16 from the network buffer
        /// </summary>
        /// <returns>UInt16 data read</returns>
        public UInt16 ReadUInt16()
        {
            UInt16 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToUInt16(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfUInt16];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfUInt16);
                Array.Reverse(buf);
                ret = BitConverter.ToUInt16(buf, 0);
            }

            m_offset += SizeOfUInt16;
            return ret;
        }

        /// <summary>
        /// Read a UInt32 from the network buffer
        /// </summary>
        /// <returns>UInt32 data read</returns>
        public UInt32 ReadUInt32()
        {
            UInt32 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToUInt32(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfUInt32];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfUInt32);
                Array.Reverse(buf);
                ret = BitConverter.ToUInt32(buf, 0);
            }

            m_offset += SizeOfUInt32;
            return ret;
        }

        /// <summary>
        /// Read a UInt64 from the network buffer
        /// </summary>
        /// <returns>UInt64 data read</returns>
        public UInt64 ReadUInt64()
        {
            UInt64 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToUInt64(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfUInt64];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfUInt64);
                Array.Reverse(buf);
                ret = BitConverter.ToUInt64(buf, 0);
            }

            m_offset += SizeOfUInt64;
            return ret;
        }

        /// <summary>
        /// Read a Int16 from the network buffer
        /// </summary>
        /// <returns>Int16 data read</returns>
        public Int16 ReadInt16()
        {
            Int16 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToInt16(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfInt16];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfInt16);
                Array.Reverse(buf);
                ret = BitConverter.ToInt16(buf, 0);
            }

            m_offset += SizeOfInt16;
            return ret;
        }

        /// <summary>
        /// Read a Int32 from the network buffer
        /// </summary>
        /// <returns>Int32 data read</returns>
        public Int32 ReadInt32()
        {
            Int32 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToInt32(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfInt32];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfInt32);
                Array.Reverse(buf);
                ret = BitConverter.ToInt32(buf, 0);
            }

            m_offset += SizeOfInt32;
            return ret;
        }

        /// <summary>
        /// Read a Int64 from the network buffer
        /// </summary>
        /// <returns>Int64 data read</returns>
        public Int64 ReadInt64()
        {
            Int64 ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToInt64(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfInt64];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfInt64);
                Array.Reverse(buf);
                ret = BitConverter.ToInt64(buf, 0);
            }

            m_offset += SizeOfInt64;
            return ret;
        }

        /// <summary>
        /// Read a string from the network buffer
        /// </summary>
        /// <returns>String data read</returns>
        public string ReadString()
        {
            // Strings in the buffer are all in the same format: a 2 byte UInt16 to specify the length and then the string immediately following
            var len = ReadUInt16();
            var ret = Encoding.UTF8.GetString(m_buffer, m_offset, len);
            m_offset += len;
            return ret;
        }

        /// <summary>
        /// Read a byte array from the network buffer
        /// </summary>
        /// <returns>Byte array data read</returns>
        public Byte[] ReadByteArray()
        {
            // Arrays in the buffer are all in the same format: a 2 byte UInt16 to specify the length and then the byte array immediately following
            var len = ReadUInt16();
            var ret = new Byte[len];
            Buffer.BlockCopy(m_buffer, m_offset, ret, 0, len);
            m_offset += len;
            return ret;
        }

        /// <summary>
        /// Read a float from the network buffer
        /// </summary>
        /// <returns>Float data read</returns>
        public float ReadFloat()
        {
            float ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToSingle(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfFloat];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfFloat);
                Array.Reverse(buf);
                ret = BitConverter.ToSingle(buf, 0);
            }

            m_offset += SizeOfFloat;
            return ret;
        }

        /// <summary>
        /// Read a double from the network buffer
        /// </summary>
        /// <returns>Double data read</returns>
        public double ReadDouble()
        {
            double ret;

            if (s_endianness == Endian.Little)
                ret = BitConverter.ToDouble(m_buffer, m_offset);
            else
            {
                var buf = new byte[SizeOfDouble];
                Buffer.BlockCopy(m_buffer, m_offset, buf, 0, SizeOfDouble);
                Array.Reverse(buf);
                ret = BitConverter.ToDouble(buf, 0);
            }

            m_offset += SizeOfDouble;
            return ret;
        }

        private readonly byte[] m_buffer;
        private int m_offset;

        /// <summary>
        /// Get or set endianness that the buffer readers should use
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
