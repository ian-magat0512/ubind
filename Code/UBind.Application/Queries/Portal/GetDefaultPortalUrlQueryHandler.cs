// <copyright file="GetDefaultPortalUrlQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System.Threading;
    using System.Threading.Tasks;
    using Flurl;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    internal class GetDefaultPortalUrlQueryHandler : IQueryHandler<GetDefaultPortalUrlQuery, string>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalService portalService;

        public GetDefaultPortalUrlQueryHandler(
            ICachingResolver cachingResolver,
            IPortalService portalService)
        {
            this.cachingResolver = cachingResolver;
            this.portalService = portalService;
        }

        public async Task<string> Handle(GetDefaultPortalUrlQuery query, CancellationToken cancellationToken)
        {
            Url? url = null;
            if (query.Portal.IsDefault && query.Portal.UserType == PortalUserType.Agent)
            {
                var tenant = await this.cachingResolver.GetTenantOrThrow(query.Portal.TenantId);
                if (query.Portal.OrganisationId == tenant.Details.DefaultOrganisationId)
                {
                    url = this.portalService.GenerateDefaultUrlForTenant(tenant);
                }
                else
                {
                    var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant, new GuidOrAlias(query.Portal.OrganisationId));
                    url = await this.portalService.GenerateDefaultUrlForOrganisation(organisation);
                }
            }
            else
            {
                url = await this.portalService.GenerateDefaultUrlForPortal(query.Portal);
            }

            return url;
        }
    }
}
