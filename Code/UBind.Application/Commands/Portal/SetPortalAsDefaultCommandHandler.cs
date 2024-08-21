// <copyright file="SetPortalAsDefaultCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System;
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

    public class SetPortalAsDefaultCommandHandler : ICommandHandler<SetPortalAsDefaultCommand, PortalReadModel>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IPortalReadModelRepository portalReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;

        public SetPortalAsDefaultCommandHandler(
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

        public async Task<PortalReadModel> Handle(SetPortalAsDefaultCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var organisationAggregate = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            if (portalAggregate.Disabled)
            {
                throw new ErrorException(Errors.Portal.CannotSetDisabledPortalAsDefault(
                    portalAggregate.Name, organisationAggregate.Name));
            }

            bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == command.OrganisationId;
            if (portalAggregate.IsDefault && organisationAggregate.DefaultPortalId == command.PortalId
                && (!isDefaultOrganisation || tenant.Details.DefaultPortalId == command.PortalId))
            {
                // it's already the default and set as default on the related entities.
                throw new ErrorException(Errors.Portal.AlreadyDefaultForOrganisation(
                    portalAggregate.Name,
                    organisationAggregate.Name));
            }

            // remove the default flag on the existing portal
            Guid? existingDefaultPortalId = this.portalReadModelRepository.GetDefaultPortalId(
                command.TenantId, command.OrganisationId, portalAggregate.UserType);
            if (existingDefaultPortalId.HasValue)
            {
                var existingDefaultPortalAggregate
                    = this.portalAggregateRepository.GetById(tenant.Id, existingDefaultPortalId.Value);
                existingDefaultPortalAggregate.SetDefault(false, performingUserId, now);
                await this.portalAggregateRepository.ApplyChangesToDbContext(existingDefaultPortalAggregate);
                this.cachingResolver.RemoveCachedPortals(
                    command.TenantId,
                    existingDefaultPortalAggregate.Id,
                    new List<string> { existingDefaultPortalAggregate.Alias });
            }

            // set the default flag on the new portal
            portalAggregate.SetDefault(true, performingUserId, now);
            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);

            if (portalAggregate.UserType == PortalUserType.Agent)
            {
                // set that this is the new default portal on the organisation
                organisationAggregate.SetDefaultPortal(command.PortalId, performingUserId, now);
                await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(
                    command.TenantId, organisationAggregate.Id, new List<string> { organisationAggregate.Alias });

                if (isDefaultOrganisation)
                {
                    // set the default portal ID on the tenant
                    var newTenantDetails = new TenantDetails(tenant.Details, now);
                    newTenantDetails.DefaultPortalId = command.PortalId;
                    tenant.Update(newTenantDetails);
                    this.cachingResolver.RemoveCachedTenants(
                        command.TenantId, new List<string> { tenant.Details.Alias });
                }
            }

            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            return portalAggregate.ReadModel;
        }
    }
}
