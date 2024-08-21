// <copyright file="ClientIPAddressFilterAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Application.Configuration;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Web.Extensions;

    /// <summary>
    /// Filter attribute for authorizing based on client IP address.
    /// </summary>
    public class ClientIPAddressFilterAttribute : ActionFilterAttribute
    {
        private readonly ICustomHeaderConfiguration headerConfiguration;
        private readonly IIpAddressWhitelistHelper whitelistHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIPAddressFilterAttribute"/> class.
        /// </summary>
        /// <param name="whitelistHelper">Service for authorizing IP addresses based on whitelist.</param>
        /// <param name="headerConfiguration">the header configuration.</param>
        public ClientIPAddressFilterAttribute(IIpAddressWhitelistHelper whitelistHelper, ICustomHeaderConfiguration headerConfiguration)
        {
            this.headerConfiguration = headerConfiguration;
            this.whitelistHelper = whitelistHelper;
        }

        /// <inheritdoc/>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var clientIPAddress = context.HttpContext.GetClientIPAddress(this.headerConfiguration.ClientIpCode);

            if (!this.whitelistHelper.IsWhitelisted(clientIPAddress))
            {
                context.Result = Errors.General.Forbidden().ToProblemJsonResult();
            }
        }
    }
}
