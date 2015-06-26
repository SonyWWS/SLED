/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using NUnit.Framework;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.UnitTests
{
    [TestFixture]
    public class TestVersion
    {
        [Test]
        public void ConvertToInt()
        {
            {
                const int expected = 10;
                var ver1 = new Version(1, 0);
                Assert.AreEqual(expected, ver1.ToInt32());
            }

            {
                const int expected = 43;
                var ver2 = new Version(4, 3, 234, 4);
                Assert.AreEqual(expected, ver2.ToInt32());
            }
        }
    }
}