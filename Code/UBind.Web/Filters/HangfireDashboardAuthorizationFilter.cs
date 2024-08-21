// <copyright file="HangfireDashboardAuthorizationFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Hangfire.Annotations;
    using Hangfire.Dashboard;
    using UBind.Application.Configuration;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;

    /// <inheritdoc/>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly ICustomHeaderConfiguration headerConfiguration;
        private readonly IIpAddressWhitelistHelper whitelistHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireDashboardAuthorizationFilter"/> class.
        /// </summary>
        /// <param name="whitelistHelper">Service for authorizing IP addresses based on whitelist.</param>
        /// <param name="headerConfiguration">the header configuration.</param>
        public HangfireDashboardAuthorizationFilter(IIpAddressWhitelistHelper whitelistHelper, ICustomHeaderConfiguration headerConfiguration)
        {
            this.headerConfiguration = headerConfiguration;
            this.whitelistHelper = whitelistHelper;
        }

        /// <inheritdoc/>
        public bool Authorize([NotNull] DashboardContext context)
        {
            var clientIPAddress = context.GetHttpContext().GetClientIPAddress(this.headerConfiguration.ClientIpCode);
            return this.whitelistHelper.IsWhitelisted(clientIPAddress);
        }
    }
}
