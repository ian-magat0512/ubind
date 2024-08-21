// <copyright file="HttpContextPropertiesResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Net;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using UBind.Application.Configuration;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Domain;

    /// <summary>
    /// Makes available certain properties which are resolvable from the http context.
    /// For example: we need to know the user who triggered the event, so we can trace up which user performed which
    /// action.
    /// This is needed so that we don't have to pass in these properties to each command/query from the controller,
    /// instead commands and queries can resolve them directly using this injected service.
    /// </summary>
    public class HttpContextPropertiesResolver : IHttpContextPropertiesResolver
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ICustomHeaderConfiguration headerConfiguration;
        private readonly IIpAddressWhitelistHelper ipAddressWhitelistHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPropertiesResolver"/> class.
        /// The Performing User Resolver.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public HttpContextPropertiesResolver(
            IHttpContextAccessor contextAccessor,
            ICustomHeaderConfiguration headerConfiguration,
            IIpAddressWhitelistHelper ipAddressWhitelistHelper)
        {
            this.contextAccessor = contextAccessor;
            this.headerConfiguration = headerConfiguration;
            this.ipAddressWhitelistHelper = ipAddressWhitelistHelper;
            if (contextAccessor.HttpContext != null)
            {
                this.PerformingUser = contextAccessor.HttpContext.User;
                if (this.PerformingUser != null)
                {
                    this.PerformingUserId = this.PerformingUser.GetId();
                }
            }
        }

        /// <summary>
        /// Gets User Id.
        /// </summary>
        public Guid? PerformingUserId { get; }

        public ClaimsPrincipal PerformingUser { get; }

        public IPAddress? ClientIpAddress
            => this.contextAccessor.HttpContext?.GetClientIPAddress(this.headerConfiguration.ClientIpCode);

        public bool IsIpAddressWhitelisted
            => this.ipAddressWhitelistHelper.IsWhitelisted(this.ClientIpAddress);

        /// <inheritdoc/>
        public bool IsValidSecretKey()
        {
            this.contextAccessor.HttpContext.Request.Headers.TryGetValue("ubind-secret-key", out var passedSecretKey);
            var officialSecretKey = this.headerConfiguration.SecretKey;

            return passedSecretKey.ToString() == officialSecretKey;
        }
    }
}
