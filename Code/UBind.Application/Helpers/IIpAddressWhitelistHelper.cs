// <copyright file="IIpAddressWhitelistHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System.Net;

    /// <summary>
    /// Service for authorizing IP Addresses according to a whitelist.
    /// </summary>
    public interface IIpAddressWhitelistHelper
    {
        /// <summary>
        /// Check if an IP address is whitelisted.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        /// <returns>true if the IP address is whitelisted, otherwise false.</returns>
        bool IsWhitelisted(IPAddress ipAddress);
    }
}
