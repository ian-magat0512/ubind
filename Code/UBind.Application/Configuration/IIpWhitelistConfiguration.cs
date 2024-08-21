// <copyright file="IIpWhitelistConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Configuration for specifying permitted client IP addresses for use in IP whitelisting.
    /// </summary>
    public interface IIpWhitelistConfiguration
    {
        /// <summary>
        /// Gets the IP addresses or ranges that are authorized for IP-restricted endppoints.
        /// </summary>
        /// <remarks>
        /// Supported formats include any supported by the IPAddressRange Nuget package:
        /// https://www.nuget.org/packages/IPAddressRange/
        /// This includes IPv4, IPv6, CIDR etc.:
        /// "192.168.0.0/24"
        /// "192.168.0.0/255.255.255.0"
        /// "192.168.0.0-192.168.0.255"
        /// "fe80::/10"
        /// "fe80::d503:4ee:3882:c586%3"
        /// "::1"
        /// .</remarks>
        IEnumerable<string> AuthorizedIpAddresses { get; }
    }
}
