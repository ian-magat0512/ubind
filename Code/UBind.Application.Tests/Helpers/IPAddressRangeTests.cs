// <copyright file="IPAddressRangeTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System.Net;
    using NetTools;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="IPAddressRangeTests" />.
    /// </summary>
    public class IPAddressRangeTests
    {
        /// <summary>
        /// The IPAddressRangeSupportsVariousFormats.
        /// </summary>
        [Fact]
        public void IPAddressRangeSupportsVariousFormats()
        {
            // Arrange
            var range = IPAddressRange.Parse("192.168.0.0/16");
            var address = IPAddress.Parse("192.168.0.1");

            // Act
            var result = range.Contains(address);

            // Assert
            Assert.True(result);
        }
    }
}
