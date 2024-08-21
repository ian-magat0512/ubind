// <copyright file="GetPortalLoginMethodsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Models.PortalLoginMethod;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;

    public class GetPortalLoginMethodsQueryHandler
        : IQueryHandler<GetPortalLoginMethodsQuery, IList<PortalLoginMethodModel>>
    {
        private readonly IPortalSignInMethodReadModelRepository portalSignInMethodReadModelRepository;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly IEntitySettingsRepository portalEntitySettingsRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalService portalService;

        public GetPortalLoginMethodsQueryHandler(
            IPortalSignInMethodReadModelRepository portalSignInMethodReadModelRepository,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            IEntitySettingsRepository portalEntitySettingsRepository,
            ICachingResolver cachingResolver,
            IPortalService portalService)
        {
            this.portalSignInMethodReadModelRepository = portalSignInMethodReadModelRepository;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.portalEntitySettingsRepository = portalEntitySettingsRepository;
            this.cachingResolver = cachingResolver;
            this.portalService = portalService;
        }

        public async Task<IList<PortalLoginMethodModel>> Handle(
            GetPortalLoginMethodsQuery query, CancellationToken cancellationToken)
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
            this.portalService.AddMissingSortOrderValues(portalSignInMethods);
            var portalEntitySettings = this.portalEntitySettingsRepository
                .GetEntitySettings<PortalEntitySettings>(query.TenantId, EntityType.Portal, query.PortalId);

            var results = new List<PortalLoginMethodModel>();
            foreach (var authenticationMethod in authenticationMethods)
            {
                if (authenticationMethod.Disabled)
                {
                    // hard pass - we can't use it.
                    continue;
                }

                var portalSignInMethod = portalSignInMethods.FirstOrDefault(x => x.AuthenticationMethodId == authenticationMethod.Id);
                if (portalSignInMethod == null)
                {
                    portalSignInMethod = new PortalSignInMethodReadModel
                    {
                        AuthenticationMethodId = authenticationMethod.Id,
                        Name = authenticationMethod.Name,
                        PortalId = query.PortalId,
                        SortOrder = -1,
                        TenantId = query.TenantId,
                        TypeName = authenticationMethod.TypeName,
                    };

                    if (authenticationMethod is LocalAccountAuthenticationMethodReadModel localAccountAuthenticationMethod)
                    {
                        portalSignInMethod.IsEnabled = true;
                        if (portalEntitySettings != null)
                        {
                            localAccountAuthenticationMethod.AllowCustomerSelfRegistration = portalEntitySettings.AllowCustomerSelfAccountCreation;
                        }
                    }
                }

                if (!portalSignInMethod.IsEnabled)
                {
                    // hard pass - we can't use it.
                    continue;
                }

                results.Add(this.CreatePortalLoginMethodModel(portalSignInMethod, authenticationMethod, portal));
            }

            return results;
        }

        private PortalLoginMethodModel CreatePortalLoginMethodModel(
            PortalSignInMethodReadModel portalSignInMethod,
            AuthenticationMethodReadModelSummary authenticationMethod,
            PortalReadModel portal)
        {
            switch (authenticationMethod)
            {
                case LocalAccountAuthenticationMethodReadModel local:
                    return new PortalLocalAccountLoginMethodModel(portalSignInMethod, local, portal);
                case SamlAuthenticationMethodReadModel saml:
                    return new PortalSamlLoginMethodModel(portalSignInMethod, saml, portal);
            }

            throw new ErrorException(Errors.General.Unexpected($"Came across a type of "
                + "AuthenticationMethodReadModelSummary which is not supported for conversion to a "
                + "PortalLoginMethodModel."));
        }
    }
}
