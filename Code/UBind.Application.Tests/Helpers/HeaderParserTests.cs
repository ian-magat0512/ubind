// <copyright file="HeaderParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Helpers
{
    using UBind.Application.Helpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="HeaderParserTests" />.
    /// </summary>
    public class HeaderParserTests
    {
        /// <summary>
        /// The GetClientIpAddress_ReturnsCorrectIpAddress_WhenHeaderFormatIsValid.
        /// </summary>
        /// <param name="headerValue">The headerValue<see cref="string"/>.</param>
        /// <param name="expectedIpaddress">The expectedIpaddress<see cref="string"/>.</param>
        [Theory]
        [InlineData("127.0.0.1", "127.0.0.1")] // IPv4 address only")]
        [InlineData("127.0.0.1:12345", "127.0.0.1")] // IPv4 address and port")]
        [InlineData("127.0.0.1, 255.0.0.1", "127.0.0.1")] // IPv4 address only, followed by IPv4 address only.")]
        [InlineData("127.0.0.1:12345, 255.0.0.1", "127.0.0.1")] // IPv4 address and port, followed by IPv4 address only.")]
        [InlineData("127.0.0.1, 255.0.0.1:12345", "127.0.0.1")] // IPv4 address only, followed by IPv4 address and port.")]
        [InlineData("127.0.0.1:12345, 255.0.0.1:12345", "127.0.0.1")] // IPv4 address and port, followed by IPv4 address and port.")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address (without brackets) only")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 only")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]:12345", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 in brackets with port")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348, 255.0.0.1", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address (without brackets) only, followed by IPv4v4 address only.")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348], 255.0.0.1", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address only, followed by IPv4v4 address only.")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]:12345, 255.0.0.1", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address and port, followed by IPv4 address and port.")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348, 255.0.0.1:12345", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address (without brackets) only, followed by IPv4 address and port.")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348], 255.0.0.1:12345", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address only, followed by IPv4 address and port.")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]:12345, 255.0.0.1:12345", "2001:db8:85a3:8d3:1319:8a2e:370:7348")] // IPv6 address and port, followed by IPv4 address and port.")]
        public void GetClientIpAddress_ReturnsCorrectIpAddress_WhenHeaderFormatIsValid(string headerValue, string expectedIpaddress)
        {
            // Act
            var ipAddress = HeaderParser.GetClientIpAddress(headerValue);

            // Assert
            Assert.Equal(expectedIpaddress, ipAddress.ToString());
        }
    }
}
