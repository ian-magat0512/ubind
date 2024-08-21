// <copyright file="IPortalService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System.Threading.Tasks;
    using Flurl;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    public interface IPortalService
    {
        /// <summary>
        /// Generates the default URL for a specific portal.
        /// E.g. https://app.ubind.com.au/portal/{tenantAlias}/{organisationAlias}/{portalAlias}
        /// The URL is always fully qualified. It's not shortened if it's the default portal for an organisation
        /// or for the default organisation.
        /// </summary>
        Task<Url> GenerateDefaultUrlForPortal(PortalReadModel portal, DeploymentEnvironment? environment = null);

        /// <summary>
        /// Generates the default portal URL for an organisation.
        /// E.g. https://app.ubind.com.au/portal/{tenantAlias}/{organisationAlias}
        /// This is typically used when generating a link to the portal that is configured to be the default agent
        /// portal for an organisation.
        /// </summary>
        Task<Url> GenerateDefaultUrlForOrganisation(OrganisationReadModel organisation, DeploymentEnvironment? environment = null);

        /// <summary>
        /// Generates the default portal URL for the default organisation of a tenant.
        /// E.g. https://app.ubind.com.au/portal/{tenantAlias}
        /// This is typically used when generating a link to the portal that is configured to be the default agent
        /// portal for the default organisation of a tenant.
        /// </summary>
        Url GenerateDefaultUrlForTenant(Tenant tenant, DeploymentEnvironment? environment = null);

        Url AddEnvironmentToUrl(Url url, DeploymentEnvironment? environment = null);

        Url AddPathToDefaultUrl(Url url, string path);

        Url AddPathToEmbeddedUrl(Url url, string path);

        void AddMissingAuthenticationMethodsToPortalSignInMethods(
            Guid tenantId,
            Guid portalId,
            IList<AuthenticationMethodReadModelSummary> authenticationMethods,
            IList<PortalSignInMethodReadModel> portalSignInMethods);

        void AddMissingSortOrderValues(IList<PortalSignInMethodReadModel> portalSignInMethods);

        void RemoveDuplicateLocalAccountMethod(
            IList<AuthenticationMethodReadModelSummary> authenticationMethods, Guid portalOrganisationId);

        Task<Guid?> GetDefaultPortalIdByUserType(Guid tenantId, Guid organisationId, PortalUserType userType);
    }
}
