// <copyright file="PortalService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System.Threading.Tasks;
    using Flurl;
    using Humanizer;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    public class PortalService : IPortalService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IBaseUrlResolver baseUrlResolver;
        private readonly IPortalReadModelRepository portalReadModelRepository;

        public PortalService(
            ICachingResolver cachingResolver,
            IBaseUrlResolver baseUrlResolver,
            IPortalReadModelRepository portalReadModelRepository)
        {
            this.cachingResolver = cachingResolver;
            this.baseUrlResolver = baseUrlResolver;
            this.portalReadModelRepository = portalReadModelRepository;
        }

        public async Task<Url> GenerateDefaultUrlForPortal(PortalReadModel portal, DeploymentEnvironment? environment = null)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(portal.TenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant, new GuidOrAlias(portal.OrganisationId));
            string baseUrl = this.baseUrlResolver.GetBaseUrl(tenant);
            string portalPath = $"portal/{tenant.Details.Alias}/{organisation.Alias}/{portal.Alias}";
            return this.AddEnvironmentToUrl(Url.Combine(baseUrl, portalPath), environment);
        }

        public async Task<Url> GenerateDefaultUrlForOrganisation(
            OrganisationReadModel organisation,
            DeploymentEnvironment? environment = null)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(organisation.TenantId);
            Url url = this.GenerateDefaultUrlForTenant(tenant, environment);
            if (tenant.Details.DefaultOrganisationId != organisation.Id)
            {
                url = url.AppendPathWithQuery($"/{organisation.Alias}");
            }

            return url;
        }

        public Url GenerateDefaultUrlForTenant(Tenant tenant, DeploymentEnvironment? environment = null)
        {
            string baseUrl = this.baseUrlResolver.GetBaseUrl(tenant);
            string portalPath = $"portal/{tenant.Details.Alias}";
            return this.AddEnvironmentToUrl(Url.Combine(baseUrl, portalPath), environment);
        }

        public Url AddEnvironmentToUrl(Url url, DeploymentEnvironment? environment = null)
        {
            if (environment != null && environment != DeploymentEnvironment.Production)
            {
                return url.SetQueryParam("environment", environment.Value.Humanize().ToLowerInvariant());
            }

            return url;
        }

        public Url AddPathToDefaultUrl(Url url, string path)
        {
            if (path == null)
            {
                return url;
            }

            var newUrl = url.AppendPathSegment("path");
            return newUrl.AppendPathWithQuery(path);
        }

        /// <summary>
        /// adds the path parameter to the embedded url.
        /// </summary>
        public Url AddPathToEmbeddedUrl(Url url, string path)
        {
            if (path == null)
            {
                return url;
            }

            url.QueryParams.Add("path", path);
            url.Query = System.Net.WebUtility.UrlDecode(url.Query);
            url.Query = url.Query.Replace("?", "&");
            return url;
        }

        public void AddMissingAuthenticationMethodsToPortalSignInMethods(
            Guid tenantId,
            Guid portalId,
            IList<AuthenticationMethodReadModelSummary> authenticationMethods,
            IList<PortalSignInMethodReadModel> portalSignInMethods)
        {
            foreach (var authenticationMethod in authenticationMethods)
            {
                if (!portalSignInMethods.Any(x => x.AuthenticationMethodId == authenticationMethod.Id))
                {
                    bool isLocaAccountAuthMethod
                        = authenticationMethod.TypeName == AuthenticationMethodType.LocalAccount.Humanize();
                    portalSignInMethods.Add(new PortalSignInMethodReadModel
                    {
                        AuthenticationMethodId = authenticationMethod.Id,
                        IsEnabled = isLocaAccountAuthMethod, // enable local account by default
                        Name = authenticationMethod.Name,
                        PortalId = portalId,
                        SortOrder = -1,
                        TenantId = tenantId,
                        TypeName = authenticationMethod.TypeName,
                    });
                }
            }
        }

        public void AddMissingSortOrderValues(IList<PortalSignInMethodReadModel> portalSignInMethods)
        {
            // Find the maximum SortOrder value that's not -1, if any.
            int maxSortOrder = portalSignInMethods.Any(x => x.SortOrder != -1)
                               ? portalSignInMethods.Where(x => x.SortOrder != -1).Max(x => x.SortOrder)
                               : -1;

            // Identify all items with a SortOrder of -1 and update their SortOrder.
            foreach (var method in portalSignInMethods.Where(x => x.SortOrder == -1))
            {
                method.SortOrder = ++maxSortOrder;
            }
        }

        /// <summary>
        /// We may often have multiple local account methods, so we need to remove duplicates.
        /// We should keep the most specific one, e.g from the current organisation.
        /// </summary>
        public void RemoveDuplicateLocalAccountMethod(
            IList<AuthenticationMethodReadModelSummary> authenticationMethods, Guid portalOrganisationId)
        {
            int localAccountMethodCount
                = authenticationMethods.Where(p => p.TypeName == AuthenticationMethodType.LocalAccount.Humanize()).Count();
            if (localAccountMethodCount > 1)
            {
                var methodsToRemove = authenticationMethods
                    .Where(m => m.TypeName == AuthenticationMethodType.LocalAccount.Humanize())
                    .Where(m => m.OrganisationId != portalOrganisationId).ToList();
                methodsToRemove.ForEach(m => authenticationMethods.Remove(m));
            }
        }

        public async Task<Guid?> GetDefaultPortalIdByUserType(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            Guid? portalId;
            if (tenantId == Tenant.MasterTenantId)
            {
                // There is no default portal for the master tenant
                return null;
            }

            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
            if (userType == PortalUserType.Agent)
            {
                portalId = organisation.DefaultPortalId ??
                    (await this.cachingResolver.GetTenantOrThrow(tenantId)).Details.DefaultPortalId;
                return portalId != default(Guid) ? portalId : null;
            }

            // Get the default customer portal for this organisation
            portalId = this.portalReadModelRepository.GetDefaultPortalId(tenantId, organisationId, userType);
            if (portalId != null)
            {
                return portalId;
            }

            // Get the default customer portal for the default organisation
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            if (organisationId != tenant.Details.DefaultOrganisationId)
            {
                portalId = this.portalReadModelRepository.GetDefaultPortalId(
                    tenantId, tenant.Details.DefaultOrganisationId, userType);
            }

            // It may still be null, in which case we couldn't find any customer portal for this user to login to.
            return portalId;
        }
    }
}
