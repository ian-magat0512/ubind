// <copyright file="IPAddressWhiteListTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using UBind.Application.Configuration;
    using UBind.Application.Helpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="IpAddressWhiteListTests" />.
    /// </summary>
    public class IpAddressWhiteListTests
    {
        /// <summary>
        /// The IsWhiteListed_IsTrue_IfIPAddressWithinAuthorizedList.
        /// </summary>
        /// <param name="ipString">the test ip address.</param>
        [Theory]
        [InlineData("127.0.0.1")]
        [InlineData("::1")]
        [InlineData("10.5.0.0")]
        [InlineData("10.5.255.255")]
        [InlineData("10.5.150.20")]
        [InlineData("10.5.10.255")]
        [InlineData("192.168.0.0")]
        [InlineData("192.168.250.230")]
        [InlineData("192.168.10.240")]
        [InlineData("192.168.123.3")]
        [InlineData("192.168.255.255")]
        [InlineData("210.10.238.100")]
        [InlineData("54.206.58.132")]
        public void IsWhiteListed_IsTrue_IfIPAddressWithinAuthorizedList(string ipString)
        {
            // Arrange
            IpWhitelistConfiguration ipWhitelistConfiguration = new IpWhitelistConfiguration();
            ipWhitelistConfiguration.AuthorizedIpAddresses = new List<string>()
            {
                "::1", // IPv6 localhost
                "127.0.0.1", // IPv4 localhost
                "192.168.0.0/16", // Local network
                "10.5.0.0/16", // Local network
                "210.10.238.100", // Aptiture Collins Street Office
                "54.206.58.132", // PRTG2 Monitoring
            };

            var ipWhiteList = new IpAddressWhitelistHelper(ipWhitelistConfiguration);

            // Trim port, if present.
            if (IpAddressParser.TryGetIPAddress(ipString, out IPAddress ipAddress))
            {
                // Act
                var result = ipWhiteList.IsWhitelisted(ipAddress);

                // Assert
                Assert.True(result);
            }
        }

        /// <summary>
        /// The IsWhiteListed_IsFalse_IfIPAddressOutsideAuthorizedList.
        /// </summary>
        /// <param name="ipString">the test ip address.</param>
        [Theory]
        [InlineData("127.0.0.2")]
        [InlineData("::10")]
        [InlineData("10.2.0.0")]
        [InlineData("10.9.255.255")]
        [InlineData("10.1.150.20")]
        [InlineData("10.22.10.255")]
        [InlineData("192.158.0.0")]
        [InlineData("192.128.250.230")]
        [InlineData("191.168.10.240")]
        [InlineData("192.188.123.3")]
        [InlineData("112.168.255.255")]
        [InlineData("210.10.228.100")]
        [InlineData("53.206.58.132")]
        public void IsWhiteListed_IsFalse_IfIPAddressOutsideAuthorizedList(string ipString)
        {
            // Arrange
            IpWhitelistConfiguration ipWhitelistConfiguration = new IpWhitelistConfiguration();
            ipWhitelistConfiguration.AuthorizedIpAddresses = new List<string>()
            {
                "::1", // IPv6 localhost
                "127.0.0.1", // IPv4 localhost
                "192.168.0.0/16", // Local network
                "10.5.0.0/16", // Local network
                "210.10.238.100", // Aptiture Collins Street Office
                "54.206.58.132", // PRTG2 Monitoring
            };

            var ipWhiteList = new IpAddressWhitelistHelper(ipWhitelistConfiguration);

            // Trim port, if present.
            if (IpAddressParser.TryGetIPAddress(ipString, out IPAddress ipAddress))
            {
                // Act
                var result = ipWhiteList.IsWhitelisted(ipAddress);

                // Assert
                Assert.False(result);
            }
        }
    }
}
