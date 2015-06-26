/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Runtime.InteropServices;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// Wrapper for kernel32.dll flags and functions</summary>
    public static class SledKernel32
    {
        /// <summary>
        /// Page protection of file mapping object</summary>
        /// <remarks>For more details, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366537%28v=vs.85%29.aspx </remarks>
        [Flags]
        public enum FileMapProtection : uint
        {
            /// <summary>
            /// Allow views to be mapped for read-only or copy-on-write access</summary>
            PageReadonly = 0x02,

            /// <summary>
            /// Allow views to be mapped for read-only, copy-on-write, or read/write access</summary>
            PageReadWrite = 0x04,

            /// <summary>
            /// Allow views to be mapped for read-only or copy-on-write access</summary>
            PageWriteCopy = 0x08,

            /// <summary>
            /// Allow views to be mapped for read-only, copy-on-write, or execute access</summary>
            PageExecuteRead = 0x20,

            /// <summary>
            /// Allow views to be mapped for read-only, copy-on-write, read/write, or execute access</summary>
            PageExecuteReadWrite = 0x40,

            /// <summary>
            /// If file mapping object is backed by the operating system paging file (hfile parameter is INVALID_HANDLE_VALUE), 
            /// when a view of the file is mapped into process address space, 
            /// the entire range of pages is committed rather than reserved</summary>
            SectionCommit = 0x8000000,

            /// <summary>
            /// Specifies that file that hFile parameter specifies is an executable image file</summary>
            SectionImage = 0x1000000,

            /// <summary>
            /// Sets all pages to be non-cachable</summary>
            SectionNoCache = 0x10000000,

            /// <summary>
            /// If file mapping object is backed by operating system paging file (hfile parameter is INVALID_HANDLE_VALUE), 
            /// specifies that when a view of the file is mapped into a process address space, 
            /// the entire range of pages is reserved for later use by the process rather than committed</summary>
            SectionReserve = 0x4000000,
        }

        /// <summary>
        /// File mapping security and access rights</summary>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366559%28v=vs.85%29.aspx </remarks>
        [Flags]
        public enum FileMapAccess : uint
        {
            /// <summary>
            /// Mapping a copy-on-write view of a file mapping object requires same access as mapping read-only view. 
            /// Not an actual access right and should not be specified in security descriptor.</summary>
            FileMapCopy = 0x0001,

            /// <summary>
            /// Allows mapping of read-only, copy-on-write, or read/write views of file mapping object</summary>
            FileMapWrite = 0x0002,

            /// <summary>
            /// Allows mapping of read-only or copy-on-write views of file mapping object</summary>
            FileMapRead = 0x0004,

            /// <summary>
            /// Includes all access rights to file mapping object except FileMapExecute</summary>
            FileMapAllAccess = 0x001f,

            /// <summary>
            /// Allows mapping of executable views of the file mapping object</summary>
            FileMapExecute = 0x0020,
        }

        /// <summary>
        /// Create or open named or unnamed file mapping object for specified file</summary>
        /// <param name="hFile">Handle to file from which to create file mapping object</param>
        /// <param name="lpFileMappingAttributes">Pointer to a SECURITY_ATTRIBUTES structure that determines whether
        /// returned handle can be inherited by child processes</param>
        /// <param name="flProtect">Page protection of file mapping object</param>
        /// <param name="dwMaximumSizeHigh">High-order DWORD of maximum size of file mapping object</param>
        /// <param name="dwMaximumSizeLow">Low-order DWORD of maximum size of file mapping object</param>
        /// <param name="lpName">Name of file mapping object</param>
        /// <returns>If function succeeds, handle to the newly created file mapping object. NULL otherwise.</returns>
        /// <remarks>For more details, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366537%28v=vs.85%29.aspx </remarks>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, FileMapProtection flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        /// <summary>
        /// Open named file mapping object</summary>
        /// <param name="dwDesiredAccess">Access to the file mapping object</param>
        /// <param name="bInheritHandle">If TRUE, process created by CreateProcess function can inherit handle; 
        /// otherwise, handle cannot be inherited</param>
        /// <param name="lpName">Name of the file mapping object to be opened</param>
        /// <returns>If succeeds, open handle to specified file mapping object; otherwise NULL.</returns>
        /// <remarks>For more information, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366791%28v=vs.85%29.aspx </remarks>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFileMapping(FileMapAccess dwDesiredAccess, bool bInheritHandle, string lpName);

        /// <summary>
        /// Map view of file mapping into address space of calling process</summary>
        /// <param name="hFileMappingObject">Handle to file mapping object</param>
        /// <param name="dwDesiredAccess">Type of access to file mapping object, which determines protection of pages</param>
        /// <param name="dwFileOffsetHigh">High-order DWORD of file offset where view begins</param>
        /// <param name="dwFileOffsetLow">Low-order DWORD of file offset where view begins</param>
        /// <param name="dwNumberOfBytesToMap">Number of bytes of file mapping to map to view</param>
        /// <returns>If succeeds, starting address of the mapped view; NULL otherwise</returns>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366761%28v=vs.85%29.aspx </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, FileMapAccess dwDesiredAccess, UInt32 dwFileOffsetHigh, UInt32 dwFileOffsetLow, UInt32 dwNumberOfBytesToMap);

        /// <summary>
        /// Unmap mapped view of file from calling process's address space</summary>
        /// <param name="lpBaseAddress">Pointer to base address of mapped view of file that is to be unmapped</param>
        /// <returns>If succeeds, nonzero value; otherwise zero</returns>
        /// <remarks>For details, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366882%28v=vs.85%29.aspx </remarks>
        [DllImport("kernel32")]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        /// <summary>
        /// Close open object handle</summary>
        /// <param name="hFile">Valid handle to an open object</param>
        /// <returns>If succeeds, nonzero value; otherwise zero</returns>
        /// <remarks>For more information, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms724211%28v=vs.85%29.aspx </remarks>
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hFile);
    }
}
