/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Reflection;
using System.IO;

using Sce.Sled.Shared.Utilities;

using NUnit.Framework;

namespace Sce.Sled.Shared.UnitTests
{
    [TestFixture]
    public class TestUtils
    {
        [Test]
        public void Clamp()
        {
            const int minValue = 0;
            const int maxValue = 10;
            const int lessThanMinValue = -5;
            const int greaterThanMaxValue = 20;

            const int value1 = 5;

            Assert.AreEqual(value1, SledUtil.Clamp(value1, minValue, maxValue));
            Assert.AreEqual(value1, SledUtil.Clamp(value1, lessThanMinValue, greaterThanMaxValue));
            Assert.AreEqual(minValue, SledUtil.Clamp(lessThanMinValue, minValue, maxValue));
            Assert.AreEqual(maxValue, SledUtil.Clamp(greaterThanMaxValue, minValue, maxValue));
        }

        [Test]
        public void TransSub()
        {
            const string inputString1 = "%s0 %s1 %s2 %s3 %s4 %s5 %s6 %s7 %s8 %s9";
            const string inputString2 = "%s0 %s1 %s1 %s4";

            var trans1 = SledUtil.TransSub(inputString1, "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine");
            var trans2 = SledUtil.TransSub(inputString2, "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine");

            const string result1 = "zero one two three four five six seven eight nine";
            const string result2 = "zero one one four";

            Assert.AreEqual(0, string.Compare(trans1, result1));
            Assert.AreEqual(0, string.Compare(trans2, result2));
            Assert.That(string.Compare(trans1, trans2) != 0);
        }

        [Test]
        public void FileEndsWithExtension()
        {
            var extensions = new[] { ".txt", ".exe" };
            var filenames = new[] { @"C:\text.txt", @"C:\executable.exe", @"C:\image.bmp" };

            Assert.That(!SledUtil.StringEndsWithExtension(null, null));
            Assert.That(!SledUtil.StringEndsWithExtension(null, extensions));
            Assert.That(!SledUtil.StringEndsWithExtension(filenames[0], null));

            string out1, out2;

            Assert.That(!SledUtil.StringEndsWithExtension(null, null, out out1));
            Assert.AreEqual(0, string.Compare(string.Empty, out1));

            Assert.That(!SledUtil.StringEndsWithExtension(null, extensions, out out1));
            Assert.AreEqual(0, string.Compare(string.Empty, out1));

            Assert.That(!SledUtil.StringEndsWithExtension(filenames[1], null, out out1));
            Assert.AreEqual(0, string.Compare(string.Empty, out1));

            Assert.That(SledUtil.StringEndsWithExtension(filenames[0], extensions));
            Assert.That(!SledUtil.StringEndsWithExtension(filenames[2], extensions));
            Assert.That(SledUtil.StringEndsWithExtension(filenames[1], extensions, out out1));
            Assert.That(!SledUtil.StringEndsWithExtension(filenames[2], extensions, out out2));

            Assert.AreEqual(0, string.Compare(extensions[1], out1, true));
            Assert.AreEqual(0, string.Compare(string.Empty, out2, true));
        }

        [Test]
        public void GetRelativePath()
        {
            var basePaths = new[] { @"C:\usr\local\cell", @"C:\usr\local\cell\" };

            var absPath1 = @"C:\usr\local\file.txt";

            var relPath1 = SledUtil.GetRelativePath(absPath1, basePaths[0]);
            var relPath2 = SledUtil.GetRelativePath(absPath1, basePaths[1]);
            var relPath3 = @"..\file.txt";

            Assert.That(
                (string.Compare(relPath1, relPath2) == 0) &&
                (string.Compare(relPath2, relPath3) == 0) &&
                (string.Compare(relPath3, relPath1) == 0));

            absPath1 = @"C:\usr\local\cell\some\sub\directory\file.txt";

            relPath1 = SledUtil.GetRelativePath(absPath1, basePaths[0]);
            relPath2 = SledUtil.GetRelativePath(absPath1, basePaths[1]);
            relPath3 = @"some\sub\directory\file.txt";

            Assert.That(
                (string.Compare(relPath1, relPath2) == 0) &&
                (string.Compare(relPath2, relPath3) == 0) &&
                (string.Compare(relPath3, relPath1) == 0));

            absPath1 = @"D:\usr\local\cell\some\sub\directory\file.txt";

            relPath1 = SledUtil.GetRelativePath(absPath1, basePaths[0]);
            relPath2 = SledUtil.GetRelativePath(absPath1, basePaths[1]);
            relPath3 = absPath1;

            Assert.That(
                (string.Compare(relPath1, relPath2) == 0) &&
                (string.Compare(relPath2, relPath3) == 0) &&
                (string.Compare(relPath3, relPath1) == 0));

            absPath1 = @"C:\some\other\completely\different\directory\file.txt";

            relPath1 = SledUtil.GetRelativePath(absPath1, basePaths[0]);
            relPath2 = SledUtil.GetRelativePath(absPath1, basePaths[1]);
            relPath3 = @"..\..\..\some\other\completely\different\directory\file.txt";

            Assert.That(
                (string.Compare(relPath1, relPath2) == 0) &&
                (string.Compare(relPath2, relPath3) == 0) &&
                (string.Compare(relPath3, relPath1) == 0));
        }

        [Test]
        public void GetAbsolutePath()
        {
            var basePaths = new[] { @"C:\usr\local\cell", @"C:\usr\local\cell\" };

            const string relPath1 = @"..\..\some\folder\file.txt";
            const string relPath2 = @"some\folder\file.txt";

            var absPath1 = SledUtil.GetAbsolutePath(relPath1, basePaths[0]);
            var absPath2 = SledUtil.GetAbsolutePath(relPath1, basePaths[1]);
            var absPath3 = SledUtil.GetAbsolutePath(relPath1, basePaths[0]);
            var absPath4 = SledUtil.GetAbsolutePath(relPath1, basePaths[1]);

            const string absPath5 = @"C:\usr\some\folder\file.txt";
            const string absPath6 = @"C:\usr\local\cell\some\folder\file.txt";

            Assert.That(
                (string.Compare(absPath1, absPath5) == 0) &&
                (string.Compare(absPath2, absPath5) == 0) &&
                (string.Compare(absPath3, absPath5) == 0) &&
                (string.Compare(absPath4, absPath5) == 0));

            absPath1 = SledUtil.GetAbsolutePath(relPath2, basePaths[0]);
            absPath2 = SledUtil.GetAbsolutePath(relPath2, basePaths[1]);
            absPath3 = SledUtil.GetAbsolutePath(relPath2, basePaths[0]);
            absPath4 = SledUtil.GetAbsolutePath(relPath2, basePaths[1]);

            Assert.That(
                (string.Compare(absPath1, absPath6) == 0) &&
                (string.Compare(absPath2, absPath6) == 0) &&
                (string.Compare(absPath3, absPath6) == 0) &&
                (string.Compare(absPath4, absPath6) == 0));
        }

        [Test]
        public void FixSlashes()
        {
            string temp = null;
            const string alreadyFixed = @"C:\usr\local\cell";

            var fix1 = SledUtil.FixSlashes(temp);
            Assert.AreEqual(0, string.Compare(string.Empty, fix1));

            temp = @"C:/usr/local/cell/";
            fix1 = SledUtil.FixSlashes(temp);
            Assert.AreEqual(0, string.Compare(fix1, alreadyFixed));

            temp = @"C:/usr/local/cell";
            fix1 = SledUtil.FixSlashes(temp);
            Assert.AreEqual(0, string.Compare(fix1, alreadyFixed));

            temp = @"C:\usr/local\cell/";
            fix1 = SledUtil.FixSlashes(temp);
            Assert.AreEqual(0, string.Compare(fix1, alreadyFixed));

            temp = @"C:/usr\local/cell";
            fix1 = SledUtil.FixSlashes(temp);
            Assert.AreEqual(0, string.Compare(fix1, alreadyFixed));
        }

        [Test]
        public void IsWhiteSpace()
        {
            var test = "\t\r\n ";
            Assert.That(SledUtil.IsWhiteSpace(test));

            test = "\t\r\n a";
            Assert.That(!SledUtil.IsWhiteSpace(test));
        }

        [Test]
        public void RemoveWhiteSpace()
        {
            var test = "\t\r\n ";
            var remove1 = SledUtil.RemoveWhiteSpace(test);
            Assert.That(string.Compare(remove1, string.Empty) == 0);

            test = "\t\r\n a";
            remove1 = SledUtil.RemoveWhiteSpace(test);
            Assert.That(string.Compare(remove1, "a") == 0);
        }

        [Test]
        public void CreateDirectoryInfo()
        {
            var assem = Assembly.GetExecutingAssembly();
            Assert.That(assem != null);

            var dir1 = Path.GetDirectoryName(assem.Location);
            Assert.That(!string.IsNullOrEmpty(dir1));
            Assert.That(Directory.Exists(dir1));

            {
                var di = SledUtil.CreateDirectoryInfo(dir1, true);
                Assert.That(di != null);
                Assert.That(di.Exists);
            }

            var dir2 = Path.Combine(dir1 + Path.DirectorySeparatorChar, RandomDirName);
            Assert.That(!string.IsNullOrEmpty(dir2));
            Assert.That(!Directory.Exists(dir2));

            {
                // Since the directory doesn't exist we
                // shouldn't get a DirectoryInfo back
                var di = SledUtil.CreateDirectoryInfo(dir2, true);
                Assert.That(di == null);
            }

            {
                // Since the directory doesn't exist we
                // shouldn't get a DirectoryInfo back
                var di = SledUtil.CreateDirectoryInfo(dir2, false);
                Assert.That(di != null);
                Assert.That(!di.Exists);
            }
        }

        private const string RandomDirName = "zzz1234";
    }
}
