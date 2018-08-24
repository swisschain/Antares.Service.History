using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Core;
using Xunit;

namespace Lykke.Service.History.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void IncrementArray_Test()
        {
            Assert.Equal(new byte[] { 0, 45, 0 }, Utils.IncrementByteArray(new byte[] { 0, 0, 0 }, 300));

            Assert.Equal(new byte[] { 253, 252, 255 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 1));

            Assert.Equal(new byte[] { 254, 0, 0 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 5));

            Assert.Equal(new byte[] { 0, 0, 0 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 100));

            Assert.Equal(new byte[] { 253, 252, 254 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, -10));
        }
    }
}
