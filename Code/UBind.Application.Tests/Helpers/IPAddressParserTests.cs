// <copyright file="IPAddressParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Helpers
{
    using System.Net;
    using UBind.Application.Helpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="IPAddressParserTests" />.
    /// </summary>
    public class IPAddressParserTests
    {
        /// <summary>
        /// The TryGetHostIPAddress_Succeeds_ForValidIPAddress.
        /// </summary>
        /// <param name="ipAddress">The ipAddress<see cref="string"/>.</param>
        /// <param name="expectedHost">The expectedHost<see cref="string"/>.</param>
        [Theory]
        [InlineData("127.0.0.1:12345", "127.0.0.1")]
        [InlineData("127.0.0.1", "127.0.0.1")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("[::1]:100", "::1")]
        [InlineData("[::1]:0", "::1")]
        [InlineData("::1", "::1")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]", "2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]:100", "2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348", "2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        public void TryGetHostIPAddress_Succeeds_ForValidIPAddress(string ipAddress, string expectedHost)
        {
            // Act
            var result = IpAddressParser.TryGetIPAddress(ipAddress, out IPAddress host);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedHost, host.ToString());
        }
    }
}
