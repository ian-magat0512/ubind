// <copyright file="GetPortalSignInMethodsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;

    public class GetPortalSignInMethodsQueryHandler
        : IQueryHandler<GetPortalSignInMethodsQuery, IList<PortalSignInMethodReadModel>>
    {
        private readonly IPortalSignInMethodReadModelRepository portalSignInMethodReadModelRepository;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalService portalService;

        public GetPortalSignInMethodsQueryHandler(
            IPortalSignInMethodReadModelRepository portalSignInMethodReadModelRepository,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            ICachingResolver cachingResolver,
            IPortalService portalService)
        {
            this.portalSignInMethodReadModelRepository = portalSignInMethodReadModelRepository;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.cachingResolver = cachingResolver;
            this.portalService = portalService;
        }

        public async Task<IList<PortalSignInMethodReadModel>> Handle(GetPortalSignInMethodsQuery query, CancellationToken cancellationToken)
        {
            var portal = await this.cachingResolver.GetPortalOrThrow(query.TenantId, query.PortalId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(query.TenantId, portal.OrganisationId);
            List<Guid> organisationIds = new List<Guid> { organisation.Id };
            if (organisation.ManagingOrganisationId != null)
            {
                organisationIds.Add(organisation.ManagingOrganisationId.Value);
            }

            var filters = new EntityListFilters
            {
                OrganisationIds = organisationIds,
            };
            var authenticationMethods = this.authenticationMethodReadModelRepository.Get(query.TenantId, filters);
            this.portalService.RemoveDuplicateLocalAccountMethod(authenticationMethods, portal.OrganisationId);
            var portalSignInMethods = this.portalSignInMethodReadModelRepository.GetAll(query.TenantId, query.PortalId);

            this.portalService.AddMissingAuthenticationMethodsToPortalSignInMethods(
                query.TenantId,
                query.PortalId,
                authenticationMethods,
                portalSignInMethods);

            this.portalService.AddMissingSortOrderValues(portalSignInMethods);
            return portalSignInMethods;
        }
    }
}
