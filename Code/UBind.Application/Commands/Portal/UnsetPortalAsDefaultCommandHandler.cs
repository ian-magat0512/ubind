// <copyright file="UnsetPortalAsDefaultCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    public class UnsetPortalAsDefaultCommandHandler : ICommandHandler<UnsetPortalAsDefaultCommand, PortalReadModel>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalReadModelRepository portalReadModelRepository;

        public UnsetPortalAsDefaultCommandHandler(
            ITenantRepository tenantRepository,
            IPortalAggregateRepository portalAggregateRepository,
            IPortalReadModelRepository portalReadModelRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICachingResolver cachingResolver)
        {
            this.tenantRepository = tenantRepository;
            this.portalAggregateRepository = portalAggregateRepository;
            this.portalReadModelRepository = portalReadModelRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
        }

        public async Task<PortalReadModel> Handle(UnsetPortalAsDefaultCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var organisationAggregate = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == command.OrganisationId;
            if (!portalAggregate.IsDefault && organisationAggregate.DefaultPortalId != command.PortalId
                && (!isDefaultOrganisation || tenant.Details.DefaultPortalId != command.PortalId))
            {
                // it's already not the default and not set as default on all of the related entities.
                return this.portalReadModelRepository.GetPortalById(tenant.Id, command.PortalId);
            }

            if (isDefaultOrganisation && tenant.Details.DefaultPortalId == command.PortalId
                && portalAggregate.UserType == PortalUserType.Agent)
            {
                // you can't unset the default agent portal for a tenant, otherwise nobody would be able
                // to log in to that tenant to administer it.
                throw new ErrorException(Errors.Portal.CannotUnsetAsDefaultWithoutSettingAnotherAsDefaultFirst(
                    portalAggregate.Name,
                    organisationAggregate.Name,
                    tenant.Details.Name));
            }

            if (portalAggregate.IsDefault)
            {
                portalAggregate.SetDefault(false, performingUserId, now);
                await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            }

            if (organisationAggregate.DefaultPortalId == command.PortalId)
            {
                organisationAggregate.SetDefaultPortal(null, performingUserId, now);
                await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(
                    command.TenantId, organisationAggregate.Id, new List<string> { organisationAggregate.Alias });
            }

            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            return portalAggregate.ReadModel
                ?? this.portalReadModelRepository.GetPortalById(tenant.Id, command.PortalId);
        }
    }
}
