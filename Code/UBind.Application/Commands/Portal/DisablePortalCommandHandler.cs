// <copyright file="DisablePortalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Portal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    public class DisablePortalCommandHandler : ICommandHandler<DisablePortalCommand, PortalReadModel>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver cachingResolver;
        private readonly IUserSessionDeletionService userSessionDeletionService;

        public DisablePortalCommandHandler(
            ITenantRepository tenantRepository,
            IPortalAggregateRepository portalAggregateRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver,
            IUserSessionDeletionService userSessionDeletionService)
        {
            this.tenantRepository = tenantRepository;
            this.portalAggregateRepository = portalAggregateRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.cachingResolver = cachingResolver;
            this.userSessionDeletionService = userSessionDeletionService;
        }

        public async Task<PortalReadModel> Handle(DisablePortalCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Tenant.NotFound(command.TenantId));
            }

            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            if (portalAggregate == null)
            {
                throw new ErrorException(Errors.Portal.NotFound(tenant.Details.Alias, command.PortalId));
            }

            bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == portalAggregate.OrganisationId;
            if (isDefaultOrganisation && tenant.Details.DefaultPortalId == command.PortalId)
            {
                var organisation = this.organisationReadModelRepository.Get(tenant.Id, portalAggregate.OrganisationId);

                if (organisation == null)
                {
                    throw new ErrorException(Errors.Organisation.NotFound(portalAggregate.OrganisationId));
                }

                // you can't disable the default portal for a tenant, otherwise nobody would be able
                // to log in to that tenant to administer it.
                throw new ErrorException(Errors.Portal.CannotDisableDefaultPortalForTenant(
                    portalAggregate.Name,
                    organisation.Name,
                    tenant.Details.Name));
            }

            if (portalAggregate.IsDefault)
            {
                // remove the default portal from the organisation
                var organisationAggregate
                    = this.organisationAggregateRepository.GetById(tenant.Id, portalAggregate.OrganisationId);

                if (organisationAggregate == null)
                {
                    throw new ErrorException(Errors.Organisation.NotFound(portalAggregate.OrganisationId));
                }

                organisationAggregate.SetDefaultPortal(null, performingUserId, now);
                await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(
                    command.TenantId, organisationAggregate.Id, new List<string> { organisationAggregate.Alias });
            }

            portalAggregate.Disable(performingUserId, now);
            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            this.userSessionDeletionService.EnqueueExpireAllUserSessionsByPortalId(command.TenantId, command.PortalId, cancellationToken);
            return portalAggregate.ReadModel;
        }
    }
}
