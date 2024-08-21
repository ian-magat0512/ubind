// <copyright file="IpAddressWhitelistHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using NetTools;
    using UBind.Application.Configuration;

    /// <inheritdoc/>
    public class IpAddressWhitelistHelper : IIpAddressWhitelistHelper
    {
        private readonly IEnumerable<IPAddressRange> authorizedRanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpAddressWhitelistHelper"/> class.
        /// </summary>
        /// <param name="configuration">Configuration containing IP address whitelist.</param>
        public IpAddressWhitelistHelper(IIpWhitelistConfiguration configuration)
        {
            this.authorizedRanges = configuration.AuthorizedIpAddresses.Select(item => IPAddressRange.Parse(item));
        }

        /// <inheritdoc/>
        public bool IsWhitelisted(IPAddress ipAddress)
        {
            return this.authorizedRanges.Any(range => range.Contains(ipAddress));
        }
    }
}
