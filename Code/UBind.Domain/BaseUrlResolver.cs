// <copyright file="BaseUrlResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;

    public class BaseUrlResolver : IBaseUrlResolver
    {
        private readonly IEmailInvitationConfiguration configuration;

        public BaseUrlResolver(IEmailInvitationConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetBaseUrl(Tenant tenant)
        {
            string baseUrl = this.configuration.InvitationLinkHost;
            if (tenant.Details.CustomDomain.IsNotNullOrEmpty())
            {
                baseUrl = $"https://{tenant.Details.CustomDomain}/";
            }

            return baseUrl;
        }
    }
}
