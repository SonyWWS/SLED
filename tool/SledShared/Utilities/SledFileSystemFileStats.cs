/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// File statistics class
    /// </summary>
    public class SledFileSystemFileStats
    {
        private SledFileSystemFileStats(string filePath)
        {
            try
            {
                m_attributes = File.GetAttributes(filePath);
                m_lastWrite = File.GetLastWriteTime(filePath);
                m_bValid = true;
            }
            catch (Exception ex)
            {
                m_exception = ex;
                m_bValid = false;
            }
        }

        /// <summary>
        /// Get whether file statistics were successfully obtained
        /// </summary>
        public bool Valid
        {
            get { return m_bValid; }
        }

        /// <summary>
        /// Get exception (if any) that occurred while obtaining file statistics
        /// </summary>
        public Exception Exception
        {
            get { return m_exception; }
        }

        /// <summary>
        /// Get last time the file was written to
        /// </summary>
        public DateTime LastWrite
        {
            get { return m_lastWrite; }
        }

        /// <summary>
        /// Get file attributes
        /// </summary>
        public FileAttributes Attributes
        {
            get { return m_attributes; }
        }

        /// <summary>
        /// Create an instance of the class
        /// </summary>
        /// <param name="filePath">Path</param>
        /// <returns>Statistics</returns>
        public static SledFileSystemFileStats GetStats(string filePath)
        {
            return new SledFileSystemFileStats(filePath);
        }

        /// <summary>
        /// Compare two SledFileSystemFileStats classes and find
        /// out what's different between the two
        /// </summary>
        /// <param name="stat1">SledFileSystemFileStats to compare</param>
        /// <param name="stat2">SledFileSystemFileStats to compare</param>
        /// <returns>SledFileSystemFileStatsChange describing differences</returns>
        public static SledFileSystemFileStatsChange Compare(SledFileSystemFileStats stat1, SledFileSystemFileStats stat2)
        {
            // If either is invalid then ignore
            if (!stat1.Valid || !stat2.Valid)
                return SledFileSystemFileStatsChange.Nothing;

            // Check LastWrite and if it differs check attributes
            return
                !stat1.LastWrite.Equals(stat2.LastWrite)
                    ? SledFileSystemFileStatsChange.LastWrite
                    : CompareAttributes(stat1.Attributes, stat2.Attributes);
        }

        private static SledFileSystemFileStatsChange CompareAttributes(FileAttributes attr1, FileAttributes attr2)
        {
            // Only care about ReadOnly attribute at this time...
            return
                ((attr1 & FileAttributes.ReadOnly) !=
                (attr2 & FileAttributes.ReadOnly))
                    ? SledFileSystemFileStatsChange.Attribute 
                    : SledFileSystemFileStatsChange.Nothing;
        }

        private readonly bool m_bValid;
        private readonly DateTime m_lastWrite;
        private readonly Exception m_exception;
        private readonly FileAttributes m_attributes;
    }

    /// <summary>
    /// SLED file system stats change enumeration
    /// </summary>
    public enum SledFileSystemFileStatsChange
    {
        /// <summary>
        /// Nothing is different between two SledFileSystemStats classes
        /// </summary>
        Nothing,

        /// <summary>
        /// There's an attribute change between two SledFileSystemStats classes
        /// </summary>
        Attribute,

        /// <summary>
        /// There's a LastWrite change between two SledFileSystemStats classes
        /// </summary>
        LastWrite,
    }
}