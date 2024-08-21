// <copyright file="GetPortalUrlQueryHandler.cs" company="uBind">
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

    public class GetPortalUrlQueryHandler : IQueryHandler<GetPortalUrlQuery, string>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalService portalService;

        public GetPortalUrlQueryHandler(
            ICachingResolver cachingResolver,
            IPortalService portalService)
        {
            this.cachingResolver = cachingResolver;
            this.portalService = portalService;
        }

        public async Task<string> Handle(GetPortalUrlQuery query, CancellationToken cancellationToken)
        {
            Url? url = null;
            if (query.PortalId.HasValue)
            {
                var portal = await this.cachingResolver.GetPortalOrThrow(query.TenantId, new GuidOrAlias(query.PortalId.Value));
                string environmentUrl = portal.GetEnvironmentUrlIfSet(query.Environment);
                if (environmentUrl == null)
                {
                    if (portal.IsDefault && portal.UserType == PortalUserType.Agent)
                    {
                        var tenant = await this.cachingResolver.GetTenantOrThrow(portal.TenantId);
                        if (portal.OrganisationId == tenant.Details.DefaultOrganisationId)
                        {
                            url = this.portalService.GenerateDefaultUrlForTenant(tenant, query.Environment);
                            url = this.portalService.AddPathToDefaultUrl(url, query.Path);
                        }
                        else
                        {
                            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant, new GuidOrAlias(portal.OrganisationId));
                            url = await this.portalService.GenerateDefaultUrlForOrganisation(organisation, query.Environment);
                            url = this.portalService.AddPathToDefaultUrl(url, query.Path);
                        }
                    }
                    else
                    {
                        url = await this.portalService.GenerateDefaultUrlForPortal(portal, query.Environment);
                        url = this.portalService.AddPathToDefaultUrl(url, query.Path);
                    }
                }
                else
                {
                    url = new Url(environmentUrl);
                    url = this.portalService.AddPathToEmbeddedUrl(url, query.Path);
                }
            }
            else if (query.OrganisationId.HasValue)
            {
                var organisation
                    = await this.cachingResolver.GetOrganisationOrThrow(query.TenantId, new GuidOrAlias(query.OrganisationId.Value));
                url = await this.portalService.GenerateDefaultUrlForOrganisation(organisation, query.Environment);
                url = this.portalService.AddPathToDefaultUrl(url, query.Path);
            }
            else
            {
                var tenant = await this.cachingResolver.GetTenantOrThrow(query.TenantId);
                url = this.portalService.GenerateDefaultUrlForTenant(tenant, query.Environment);
                url = this.portalService.AddPathToDefaultUrl(url, query.Path);
            }

            return url;
        }
    }
}
