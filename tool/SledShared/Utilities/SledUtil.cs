/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Sce.Atf;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// Class of SLED static utilities
    /// </summary>
    public static class SledUtil
    {
        /// <summary>
        /// Clamp a value within a specified range
        /// </summary>
        /// <typeparam name="T">Type of value to clamp</typeparam>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped value</returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;

            return
                value.CompareTo(max) > 0
                    ? max
                    : value;
        }

        /// <summary>
        /// Try to open a path
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <returns>Process</returns>
        public static Process ShellOpen(string path)
        {
            return ShellOpen(path, null);
        }

        /// <summary>
        /// Try to open a path, supplying arguments
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <param name="arguments">Arguments to pass on the command line</param>
        /// <returns>Process</returns>
        public static Process ShellOpen(string path, string arguments)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            var psi = new ProcessStartInfo(path);
            if (!string.IsNullOrEmpty(arguments))
                psi.Arguments = arguments;
            
            return Process.Start(psi);
        }

        /// <summary>
        /// Start Windows Explorer and navigate to the specified path
        /// </summary>
        /// <param name="path">Path to navigate to. If null, Explorer is simply opened with no parameters passed to it.</param>
        /// <returns>New System.Diagnostics.Process component associated with the process
        /// resource, or null if no process resource is started</returns>
        public static Process ShellOpenExplorerPath(string path)
        {
            var psi = new ProcessStartInfo("explorer.exe");

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var fa = File.GetAttributes(path);
                    var name = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(name))
                        psi.Arguments = (fa & FileAttributes.Directory) != 0 ? path : name;
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(SledMessageType.Error, ex.Message);
                }
            }
            
            return Process.Start(psi);
        }

        /// <summary>
        /// Display the "Open with" dialog, sending it file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>New System.Diagnostics.Process component associated with the process
        /// resource, or null if no process resource is started</returns>
        public static Process ShellOpenWith(string filePath)
        {
            try
            {   
                var psi =
                    new ProcessStartInfo
                        {
                            FileName = "rundll32.exe",
                            Arguments = "shell32.dll, OpenAs_RunDLL " + filePath
                        };
                return Process.Start(psi);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(SledMessageType.Error, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Replace "%sN" in input string with "varArgs[N]"
        /// </summary>
        /// <param name="inputString">String containing "%sN", so translated items can maintain the proper word order and contain variable contents</param>
        /// <param name="args">Optional arguments to insert into the string</param>
        /// <returns>String where "%sN" items have been replaced by "args[N]" items</returns>
        public static string TransSub(string inputString, params object[] args)
        {
            var sb = new StringBuilder(inputString);

            for (var i = 0; i < args.Length; i++)
            {
                // Replace %sN with args[N]

                var curArg = args[i].ToString();
                sb.Replace("%s" + i, curArg);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Check if a file ends with a certain extension
        /// </summary>
        /// <param name="szAbsFilePath">Absolute path to file</param>
        /// <param name="extensions">Array of extensions, including the period "." in the extension (e.g. ".lpf", ".spf")</param>
        /// <returns>True iff file ends with one of the extensions</returns>
        public static bool FileEndsWithExtension(string szAbsFilePath, string[] extensions)
        {
            string extension;
            return FileEndsWithExtension(szAbsFilePath, extensions, out extension);
        }

        /// <summary>
        /// Check if a file ends with a certain extension, and if true, return the extension it ended with
        /// </summary>
        /// <param name="szAbsFilePath">Absolute path to file</param>
        /// <param name="extensions">Array of extensions, including the period "." in the extension (e.g. ".lpf", ".spf")</param>
        /// <param name="extension">The extension from the extensions array that the file ended with</param>
        /// <returns>True iff file ends with one of the extensions</returns>
        public static bool FileEndsWithExtension(string szAbsFilePath, string[] extensions, out string extension)
        {
            extension = string.Empty;
            var szFileExt = Path.GetExtension(szAbsFilePath);

            foreach (var ext in extensions)
            {
                if (string.Compare(szFileExt, ext, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                extension = ext;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a string ends with a certain extension
        /// </summary>
        /// <param name="szString">String to check</param>
        /// <param name="extensions">Array of extensions, including the period "." in the extension (e.g. ".lpf", ".spf")</param>
        /// <returns>True iff string ends with one of the extensions</returns>
        public static bool StringEndsWithExtension(string szString, string[] extensions)
        {
            string extension;
            return StringEndsWithExtension(szString, extensions, out extension);
        }

        /// <summary>
        /// Check if a string ends with a certain extension or not and, if true, return the extension it ended with
        /// </summary>
        /// <param name="szString">String to check</param>
        /// <param name="extensions">Array of extensions, including the period "." in the extension (e.g. ".lpf", ".spf")</param>
        /// <param name="extension">The extension from the extensions array that the string ended with</param>
        /// <returns>True iff string ends with one of the extensions</returns>
        public static bool StringEndsWithExtension(string szString, string[] extensions, out string extension)
        {
            extension = string.Empty;

            if (string.IsNullOrEmpty(szString))
                return false;

            if (extensions == null)
                return false;

            foreach (var ext in extensions)
            {
                if (!szString.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) 
                    continue;

                extension = ext;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate an MD5 hash for a file
        /// </summary>
        /// <param name="szAbsPath">Absolute path to file</param>
        /// <returns>MD5 hash</returns>
        public static string GetMd5Hash(string szAbsPath)
        {
            var md5 = string.Empty;

            if (!File.Exists(szAbsPath))
                return md5;

            using (var stream = new FileStream(szAbsPath, FileMode.Open, FileAccess.Read))
            {
                md5 = GetMd5Hash(stream);
            }

            return md5;
        }

        /// <summary>
        /// Generate an MD5 hash for an input stream
        /// </summary>
        /// <param name="stream">Already opened stream</param>
        /// <returns>MD5 hash</returns>
        public static string GetMd5Hash(Stream stream)
        {
            var sb = new StringBuilder();

            if (stream != Stream.Null)
            {
                var buffer = s_md5.ComputeHash(stream);
                for (var i = 0; i < buffer.Length; i++)
                {
                    sb.Append(buffer[i].ToString("x2"));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate an MD5 hash for a string of text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>MD5 hash for a text string</returns>
        public static Int64 GetMd5HashForText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                var input = Encoding.UTF8.GetBytes(text);
                var output = s_md5.ComputeHash(input);

                return BitConverter.ToInt64(output, 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static readonly System.Security.Cryptography.MD5 s_md5 =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        /// <summary>
        /// Check if a file has a byte order mark
        /// </summary>
        /// <param name="filePath">Absolute path to file</param>
        /// <returns>True iff the file has a byte order mark</returns>
        public static bool FileHasBom(string filePath)
        {
            FileStream file = null;

            try
            {
                file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (file == Stream.Null)
                    return false;

                if (!file.CanRead)
                    return false;

                var bom = new byte[4];
                
                var iRead = file.Read(bom, 0, 4);
                if (iRead != 4)
                    return false;

                if ((bom[0] == 0xFF) && (bom[1] == 0xFE)) // UTF-16 
                    return true;
                else if ((bom[0] == 0xFE) && (bom[1] == 0xFF)) // UTF-16 Big-Endian 
                    return true;
                else if ((bom[0] == 0xFF) && (bom[1] == 0xFE) && (bom[2] == 0x00) && (bom[3] == 0x00)) // UTF-32 
                    return true;
                else if ((bom[0] == 0x00) && (bom[1] == 0x00) && (bom[2] == 0xFE) && (bom[3] == 0xFF)) // UTF-32 Big-Endian 
                    return true;
                else if ((bom[0] == 0xEF) && (bom[1] == 0xBB) && (bom[2] == 0xBF)) // UTF-8 
                    return true;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(SledMessageType.Error, "{0}: Exception in FileHasBom: {1}", typeof(SledUtil), ex.Message);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                    file.Dispose();
                }
            }

            return false;
        }

        /// <summary>
        /// Return whether or not a file is read only
        /// <remarks>The default return value is false, even if an exception is thrown</remarks>
        /// </summary>
        /// <param name="filePath">Absolute path to file</param>
        /// <returns>True iff the file has the read only attribute on it</returns>
        public static bool IsFileReadOnly(string filePath)
        {
            var bRetval = false;

            try
            {
                if (!File.Exists(filePath))
                    return false;

                var fa = File.GetAttributes(filePath);

                if ((fa & FileAttributes.ReadOnly) != 0)
                    bRetval = true;
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (FileNotFoundException) { }
            catch (PathTooLongException) { }
            catch (DirectoryNotFoundException) { }

            return bRetval;
        }

        /// <summary>
        /// Add or remove the read only attribute on a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="value">Whether to add or remove the read only attribute</param>
        public static void SetReadOnly(string filePath, bool value)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            try
            {
                if (!File.Exists(filePath))
                    throw new InvalidOperationException("file not found");

                var attributes = File.GetAttributes(filePath);

                // Check if already the correct value
                if (((value) && ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)) ||
                    ((!value) && ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)))
                    return;

                if (value)
                    attributes |= FileAttributes.ReadOnly;
                else
                    attributes &= (~FileAttributes.ReadOnly);

                File.SetAttributes(filePath, attributes);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception {0} read only " + 
                    "attribute on file \"{1}\": {2}",
                    value ? "adding" : "removing",
                    filePath, ex.Message);
            }
        }

        /// <summary>
        /// Get a relative path to a file. Wrapper function to provide additional support when PathUtil fails.
        /// </summary>
        /// <param name="absolutePath">Absolute path to file</param>
        /// <param name="basePath">Base path to formulate a relative path from</param>
        /// <returns>Relative path to the file, or the absolute path if a relative path can't be generated</returns>
        public static string GetRelativePath(string absolutePath, string basePath)
        {
            BreakIfStringNullOrEmpty(absolutePath);
            BreakIfStringNullOrEmpty(basePath);

            var absolutePathTemp = absolutePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            var szRelPath = PathUtil.GetRelativePath(absolutePathTemp, basePath);

            if (szRelPath == null)
                szRelPath = absolutePathTemp;
            else
            {
                // Strip off ".\" on relative paths
                if ((szRelPath.Length > 2) && (szRelPath[0] == '.') && (szRelPath[1] == Path.DirectorySeparatorChar))
                    szRelPath = szRelPath.Substring(2, szRelPath.Length - 2);
            }

            return szRelPath;
        }

        /// <summary>
        /// Get an absolute path to a file. Wrapper function to provide additional support when PathUtil fails
        /// </summary>
        /// <param name="relativePath">Relative path to file</param>
        /// <param name="basePath">Absolute path to a base path that the relative path is converted from</param>
        /// <returns>Absolute path to file</returns>
        public static string GetAbsolutePath(string relativePath, string basePath)
        {
            BreakIfStringNullOrEmpty(relativePath);
            BreakIfStringNullOrEmpty(basePath);

            var relativePathTemp = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            var szAbsPath = PathUtil.GetAbsolutePath(relativePathTemp, basePath) ?? relativePathTemp;

            return szAbsPath;
        }

        /// <summary>
        /// Convert forward slashes to back slashes, and if a black slash is the last
        /// character in the path, remove it
        /// </summary>
        /// <param name="path">Path to fix up</param>
        /// <returns>string with slashes fixed</returns>
        public static string FixSlashes(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var szRetval = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // Remove trailing directory separator character
            if (szRetval.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
                szRetval = szRetval.TrimEnd(new[] { Path.DirectorySeparatorChar });

            try
            {
                // Paths that are simply drive letters need the extra slash on them
                if (Path.GetFullPath(path) == Path.GetPathRoot(path))
                    szRetval += Path.DirectorySeparatorChar;
            }
            catch (Exception ex)
            {
                // Invalid path somehow...
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[SledUtil.FixSlashes] Exception encountered " +
                    "with path: \"{0}\" - Exception: {1}",
                    path, ex.Message);

                szRetval = string.Empty;
            }

            return szRetval;
        }

        /// <summary>
        /// Convert backslashes to forward slashes
        /// </summary>
        /// <param name="path">Path to replace slashes in</param>
        /// <returns>String with slashes replaced</returns>
        public static string NetSlashes(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var szRetval = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return szRetval;
        }

        /// <summary>
        /// Helper function to test if a character is a white space character
        /// </summary>
        /// <param name="ch">Character to test</param>
        /// <returns>True iff character is whitespace</returns>
        public static bool IsWhiteSpace(char ch)
        {
            return ((ch == ' ') || (ch == '\t') || (ch == '\r') || (ch == '\n'));
        }

        /// <summary>
        /// Helper function to test if a string is composed of all white space characters
        /// </summary>
        /// <param name="szString">String to check whitespace of</param>
        /// <returns>True iff string is entirely composed of whitespace characters</returns>
        public static bool IsWhiteSpace(string szString)
        {
            var iLen = szString.Length;

            for (var i = 0; i < iLen; i++)
            {
                if (!IsWhiteSpace(szString[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Remove white space anywhere in a string
        /// </summary>
        /// <param name="szString">String to remove whitespace from</param>
        /// <returns>String without whitespace</returns>
        public static string RemoveWhiteSpace(string szString)
        {
            var iLen = szString.Length;

            var szTemp = new StringBuilder(iLen);

            for (var i = 0; i < iLen; i++)
            {
                var ch = szString[i];

                if (!IsWhiteSpace(ch))
                    szTemp.Append(ch);
            }

            return szTemp.ToString();
        }

        /// <summary>
        /// Search a string szSearchString for all occurrences
        /// of chSearchChar, and return an array whose indices
        /// are positions in szSearchString where chSearchChar is found
        /// </summary>
        /// <param name="szSearchString">Input string</param>
        /// <param name="chSearchChar">Character to search for</param>
        /// <returns>Array whose elements contains positions in szSearchString where chSearchChar is found</returns>
        public static int[] IndicesOf(string szSearchString, char chSearchChar)
        {
            var lstIndices = new List<int>();

            var iPos = szSearchString.IndexOf(chSearchChar);
            while (iPos != -1)
            {
                lstIndices.Add(iPos);
                iPos = szSearchString.IndexOf(chSearchChar, iPos + 1);
            }

            return lstIndices.ToArray();
        }

        /// <summary>
        /// Create a GUID from a string
        /// </summary>
        /// <param name="szString">Input string</param>
        /// <returns>GUID from string</returns>
        public static Guid GuidFromString(string szString)
        {
            string szGuid;

            // Add up the characters in the string
            var iStringSum = 0;
            for (var i = 0; i < szString.Length; i++)
                iStringSum += szString[i];

            // Make positive if negative
            if (iStringSum < 0)
                iStringSum *= -1;

            // Pick out valid hexadecimal values
            var regex = new Regex("[abcdefABCDEF\\d]+");

            var szHexSubString = "";
            foreach (var ch in szString)
            {
                if (regex.IsMatch(ch.ToString()))
                    szHexSubString += ch.ToString();
            }

            // Check if any hexadecimal values were found
            if (szHexSubString.Length <= 0)
            {
                // None found, use the string sum only
                szGuid = iStringSum.ToString();

                while (szGuid.Length < 32)
                    szGuid += iStringSum.ToString();

                // Truncate to 32 characters
                szGuid = szGuid.Substring(0, 32);
            }
            else
            {
                // Found some hexadecimal values, copy them over to fill in the Guid
                szGuid = (szHexSubString + iStringSum);

                while (szGuid.Length < 32)
                    szGuid += (szHexSubString + iStringSum);

                // Truncate to 32 characters
                szGuid = szGuid.Substring(0, 32);
            }

            return new Guid(szGuid);
        }

        /// <summary>
        /// Create an XML safe GUID
        /// </summary>
        /// <returns>XML safe GUID</returns>
        public static Guid MakeXmlSafeGuid()
        {
            var guid = Guid.NewGuid();
            while (char.IsDigit(guid.ToString(), 0))
                guid = Guid.NewGuid();
            return guid;
        }

        /// <summary>
        /// Test if GUID is safe to use in XML
        /// </summary>
        /// <param name="guid">GUID to test</param>
        /// <returns>True iff GUID is safe to use in XML</returns>
        public static bool IsXmlSafeGuid(Guid guid)
        {
            return !char.IsDigit(guid.ToString(), 0);
        }

        /// <summary>
        /// Compare two strings. During comparison, perform a "natural sort" 
        /// (a sort where numbers in strings are taken into account and compared as numbers instead of strings).
        /// </summary>
        /// <remarks>This doesn't do anything but a normal string compare for strings that are mixed (i.e. have both numbers and letters in them)</remarks>
        /// <param name="szString1">First string to compare</param>
        /// <param name="szString2">Second string to compare</param>
        /// <returns>-1: first string less than second string; 0: strings are identical; 1: first string greater than second string</returns>
        public static int CompareNaturalName(string szString1, string szString2)
        {
            var bString1HasNumbers = false;
            var bString2HasNumbers = false;

            var bAllDigits = true;
            for (var i = 0; i < szString1.Length; i++)
            {
                if (char.IsDigit(szString1[i]))
                    bString1HasNumbers = true;
                else
                    bAllDigits = false;
            }
            var bString1AllNumbers = bAllDigits;

            bAllDigits = true;
            for (var i = 0; i < szString2.Length; i++)
            {
                if (char.IsDigit(szString2[i]))
                    bString2HasNumbers = true;
                else
                    bAllDigits = false;
            }
            var bString2AllNumbers = bAllDigits;

            if (!bString1HasNumbers && !bString2HasNumbers)
            {
                // No numbers in either
                return string.Compare(szString1, szString2, StringComparison.CurrentCulture);
            }

            if (!bString1AllNumbers || !bString2AllNumbers)
            {
                // Mixed. Just do a normal compare for now?
                return string.Compare(szString1, szString2, StringComparison.CurrentCulture);
            }
            
            // All numbers in both
            long lVal1;
            long lVal2;

            // Try and get a number from each string
            if (!long.TryParse(szString1, out lVal1) || !long.TryParse(szString2, out lVal2))
            {
                // Failed to get number from the string
                return string.Compare(szString1, szString2, StringComparison.CurrentCulture);
            }
            
            if (lVal1 < lVal2)
                return -1;

            return lVal1 > lVal2 ? 1 : 0;
        }

        /// <summary>
        /// Convert certain characters to make a string XML safe
        /// <remarks>Not fully tested</remarks>
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>XML safe string</returns>
        public static string MakeXmlSafe(string value)
        {
            var retval = value.Replace("&", "&amp;");
            retval = retval.Replace("<", "&lt;");
            retval = retval.Replace(">", "&gt;");
            return retval;
        }

        /// <summary>
        /// Helper to force a breakpoint if a string is null or empty
        /// </summary>
        /// <param name="value">String to test</param>
        [Conditional("DEBUG")]
        public static void BreakIfStringNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
                Debugger.Break();
        }

        /// <summary>
        /// Open a stream for reading (and catch all "File.Open" exceptions)
        /// </summary>
        /// <param name="absPath">Absolute path to file to open</param>
        /// <returns>Stream or null</returns>
        public static Stream OpenStreamForRead(string absPath)
        {
            try
            {
                Stream strm =
                    File.Open(absPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return strm;
            }
            catch (ArgumentNullException) { return null; }
            catch (PathTooLongException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (ArgumentOutOfRangeException) { return null; }
            catch (FileNotFoundException) { return null; }
            catch (IOException) { return null; }
            catch (NotSupportedException) { return null; }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// Close an opened stream
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public static void CloseStream(Stream stream)
        {
            if (stream == null)
                return;

            if (stream == Stream.Null)
                return;

            stream.Close();
            stream.Dispose();
        }

        /// <summary>
        /// Try to open a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="fileMode">FileMode specifying how to open file</param>
        /// <param name="fileAccess">Read/write access to file</param>
        /// <param name="fileShare">Access allowed to other objects</param>
        /// <param name="maximumAttempts">Maximum attempts to open</param>
        /// <param name="attemptWaitMs">Time between open attempts</param>
        /// <returns>FileStream for opened file</returns>
        public static FileStream TryOpen(
            string filePath,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int maximumAttempts,
            int attemptWaitMs)
        {
            // Based on:
            // http://stackoverflow.com/questions/265953/how-can-you-easily-check-if-access-is-denied-for-a-file-in-net

            FileStream stream;
            var attempts = 0;

            while (true)
            {
                try
                {
                    stream = File.Open(filePath, fileMode, fileAccess, fileShare);

                    break;
                }
                catch (IOException)
                {
                    attempts++;
                    if (attempts > maximumAttempts)
                    {
                        stream = null;
                        break;
                    }

                    Thread.Sleep(attemptWaitMs);
                }

            }

            return stream;
        }

        /// <summary>
        /// Get files from a directory
        /// </summary>
        /// <param name="info">Directory to get files from</param>
        /// <param name="predicate">Predicate to determine which directories to return</param>
        /// <returns>Files found in directory</returns>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo info, Func<FileInfo, bool> predicate)
        {
            if (info == null)
                yield break;

            if (predicate == null)
                yield break;

            string fullName;

            try
            {
                fullName = info.FullName;
            }
            catch (SecurityException)
            {
                fullName = null;
            }

            if (string.IsNullOrEmpty(fullName))
                yield break;

            if (!Directory.Exists(fullName))
                yield break;

            FileInfo[] fileInfos;

            try
            {
                fileInfos = info.GetFiles(All);
            }
            catch (Exception)
            {
                fileInfos = EmptyArray<FileInfo>.Instance;
            }

            foreach (var fileInfo in fileInfos.Where(predicate))
                yield return fileInfo;
        }

        /// <summary>
        /// Get directories from a directory
        /// </summary>
        /// <param name="info">Directory to get directories from</param>
        /// <param name="predicate">Predicate to determine which files to return</param>
        /// <returns>Files found in directory</returns>
        public static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo info, Func<DirectoryInfo, bool> predicate)
        {
            if (info == null)
                yield break;

            if (predicate == null)
                yield break;

            string fullName;

            try
            {
                fullName = info.FullName;
            }
            catch (SecurityException)
            {
                fullName = null;
            }

            if (string.IsNullOrEmpty(fullName))
                yield break;

            if (!Directory.Exists(fullName))
                yield break;

            DirectoryInfo[] dirInfos;

            try
            {
                dirInfos = info.GetDirectories(All);
            }
            catch (Exception)
            {
                dirInfos = EmptyArray<DirectoryInfo>.Instance;
            }

            foreach (var dirInfo in dirInfos.Where(predicate))
                yield return dirInfo;
        }

        /// <summary>
        /// Get files and directories in a directory
        /// </summary>
        /// <param name="info">Directory to get files and directories from</param>
        /// <param name="searchOption">Search option</param>
        /// <param name="filePredicate">Predicate to determine which files to include</param>
        /// <param name="dirPredicate">Predicate to determine which directories to include</param>
        /// <returns>Files and directories in a directory</returns>
        public static IEnumerable<FileSystemInfo> GetFilesAndDirectories(
            this DirectoryInfo info,
            SearchOption searchOption,
            Func<FileInfo, bool> filePredicate,
            Func<DirectoryInfo, bool> dirPredicate)
        {
            if (info == null)
                yield break;

            var results = GetFilesAndDirectoriesTree(info, searchOption, filePredicate, dirPredicate);
            if (results == null)
                yield break;

            foreach (var fsi in results.PreOrder)
                yield return fsi.Value;
        }

        /// <summary>
        /// Get files and directories from a directory and return the results as a tree
        /// </summary>
        /// <param name="info">Directory to look in for files and directories</param>
        /// <param name="searchOption">Search option</param>
        /// <param name="filePredicate">Predicate to determine which files to include</param>
        /// <param name="dirPredicate">Predicate to determine which directories to include</param>
        /// <returns>Files and directories from a directory, as a tree</returns>
        public static Tree<FileSystemInfo> GetFilesAndDirectoriesTree(
            this DirectoryInfo info,
            SearchOption searchOption,
            Func<FileInfo, bool> filePredicate,
            Func<DirectoryInfo, bool> dirPredicate)
        {
            var cancelObject = new BoolWrapper { Value = false };
            return GetFilesAndDirectoriesTree(info, searchOption, filePredicate, dirPredicate, cancelObject);
        }

        /// <summary>
        /// Get files and directories from a directory and return the results as a tree
        /// </summary>
        /// <param name="info">Directory to look in for files and directories</param>
        /// <param name="searchOption">Search option</param>
        /// <param name="filePredicate">Predicate to determine which files to include</param>
        /// <param name="dirPredicate">Predicate to determine which directories to include</param>
        /// <param name="cancel">Flag to indicate if current iteration should cancel</param>
        /// <returns>Files and directories from a directory as a tree</returns>
        public static Tree<FileSystemInfo> GetFilesAndDirectoriesTree(
            this DirectoryInfo info,
            SearchOption searchOption,
            Func<FileInfo, bool> filePredicate,
            Func<DirectoryInfo, bool> dirPredicate,
            BoolWrapper cancel)
        {
            if ((info == null) || (filePredicate == null) || (dirPredicate == null))
                return null;

            if (s_shouldCancel(cancel))
                return null;

            if (!dirPredicate(info))
                return null;

            var root = new Tree<FileSystemInfo>(info);

            foreach (var dirInfo in info.GetDirectories(dirPredicate))
            {
                if (s_shouldCancel(cancel))
                    return null;

                var children =
                    searchOption == SearchOption.AllDirectories
                        ? GetFilesAndDirectoriesTree(dirInfo, searchOption, filePredicate, dirPredicate, cancel)
                        : new Tree<FileSystemInfo>(dirInfo);

                if (children != null)
                    root.Children.Add(children);
            }

            if (s_shouldCancel(cancel))
                return null;

            foreach (var fileInfo in info.GetFiles(filePredicate))
                root.Children.Add(new Tree<FileSystemInfo>(fileInfo));

            return root;
        }

        /// <summary>
        /// Object to wrap a volatile bool property for determining whether an asynchronous activity should be canceled
        /// </summary>
        public class BoolWrapper
        {
            /// <summary>
            /// Get or set the value
            /// </summary>
            public volatile bool Value;
        }

        /// <summary>
        /// Interlocked count down class
        /// </summary>
        /// <remarks>Switch to .NET 4's Countdown class eventually</remarks>
        public class Countdown : IDisposable
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="total">Initial value to count down from</param>
            public Countdown(int total)
            {
                m_total = total;
                m_done = new ManualResetEvent(false);
            }

            /// <summary>
            /// Decrement the count
            /// </summary>
            public void Signal()
            {
                if (Interlocked.Decrement(ref m_total) == 0)
                    m_done.Set();
            }

            /// <summary>
            /// Wait and block until count reaches zero
            /// </summary>
            public void Wait()
            {
                m_done.WaitOne();
            }

            /// <summary>
            /// Perform application-defined tasks associated with freeing, releasing, or
            /// resetting unmanaged resources</summary>
            public void Dispose()
            {
                ((IDisposable)m_done).Dispose();
            }

            private int m_total;

            private readonly ManualResetEvent m_done;
        }

        /// <summary>
        /// Helper class for outputing a string around a function call in a debug build
        /// </summary>
        public class ScopedDebugOnlyOutput : IDisposable
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="type">Message type</param>
            /// <param name="format">Format string</param>
            /// <param name="args">Parameters</param>
            public ScopedDebugOnlyOutput(SledMessageType type, string format, params object[] args)
            {
                m_type = type;
                m_message = string.Format(format, args);

                Output(m_type, m_message);
            }

            #region Implementation of IDisposable

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Output(m_type, m_message);
            }

            #endregion

            [Conditional("DEBUG")]
            private static void Output(SledMessageType type, string message)
            {
                SledOutDevice.OutLine(type, message);
            }

            private readonly SledMessageType m_type;
            private readonly string m_message;
        }

        /// <summary>
        /// An exception free wrapper around creating a DirectoryInfo from a path
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="dirMustExist">Whether null is returned if the directory doesn't exist on disk</param>
        /// <returns>DirectoryInfo or null</returns>
        public static DirectoryInfo CreateDirectoryInfo(string path, bool dirMustExist)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                var di = new DirectoryInfo(path);
                return dirMustExist ? (di.Exists ? di : null) : di;
            }
            catch (ArgumentNullException) { return null; }
            catch (SecurityException) { return null; }
            catch (ArgumentException) { return null; }
            catch (PathTooLongException) { return null; }
        }

        /// <summary>
        /// Create a comma separated string from an enumeration of strings
        /// </summary>
        /// <param name="items">Strings to concatenate together</param>
        /// <param name="includeSpace">Whether or not to include a space between strings</param>
        /// <returns>Comma separated string from enumeration of strings</returns>
        public static string ToCommaSeparatedValues(this IEnumerable<string> items, bool includeSpace)
        {
            if (items == null)
                return string.Empty;

            var first = true;
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                if (!first)
                    sb.Append(includeSpace ? ", " : ",");

                sb.Append(item);

                if (first)
                    first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a collection of items from a CSV (comma separated value) string
        /// </summary>
        /// <param name="csvString">String to break apart</param>
        /// <returns>Collection of items from a CSV string</returns>
        public static IEnumerable<string> FromCommaSeparatedValues(this string csvString)
        {
            if (string.IsNullOrEmpty(csvString))
                yield break;

            var split = csvString.Split(s_csvSplitChars, StringSplitOptions.RemoveEmptyEntries);
            foreach (var piece in split)
                yield return piece.Trim();
        }

        private static readonly Func<BoolWrapper, bool> s_shouldCancel =
            cancel => ((cancel != null) && cancel.Value);

        private static readonly char[] s_csvSplitChars = { ',' };

        private const string All = "*.*";

        /// <summary>
        /// Try to parse out an IP address from a string
        /// </summary>
        /// <param name="ipAddrOrHost">String</param>
        /// <param name="addr">IP address found or null</param>
        /// <returns>True iff IP address found</returns>
        public static bool TryParseIpAddress(string ipAddrOrHost, out IPAddress addr)
        {
            addr = null;

            if (string.IsNullOrEmpty(ipAddrOrHost))
                return false;

            if (!IPAddress.TryParse(ipAddrOrHost, out addr))
            {
                try
                {
                    var hostEntry = Dns.GetHostEntry(ipAddrOrHost);
                    addr = hostEntry.AddressList.FirstOrDefault();
                }
                catch (ArgumentNullException) { }
                catch (ArgumentException) { }
                catch (System.Net.Sockets.SocketException) { }
            }

            return addr != null;
        }
    }

    /// <summary>
    /// Version extension methods
    /// </summary>
    public static class VersionExtension
    {
        /// <summary>
        /// Convert a Version object to a number
        /// </summary>
        /// <param name="version">Version object to convert</param>
        /// <returns>Converted version number</returns>
        public static Int32 ToInt32(this Version version)
        {
            var result = (version.Major * 10) + version.Minor;
            return result;
        }
    }

    /// <summary>
    /// FileSystemInfo extension methods
    /// </summary>
    public static class FileSystemInfoExtension
    {
        /// <summary>
        /// Determine if a FileSystemInfo object is a directory
        /// </summary>
        /// <param name="fsi">FileSystemInfo object</param>
        /// <returns>True iff a directory</returns>
        public static bool IsDirectory(this FileSystemInfo fsi)
        {
            if (fsi == null)
                return false;

            try
            {
                if (fsi is DirectoryInfo)
                    return true;

                return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception checking if item is a directory: {1}",
                    typeof(FileSystemEventArgs).Name, ex.Message);

                return false;
            }
        }
    }
}
