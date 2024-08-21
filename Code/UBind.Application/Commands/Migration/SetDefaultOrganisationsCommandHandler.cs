// <copyright file="SetDefaultOrganisationsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class SetDefaultOrganisationsCommandHandler : ICommandHandler<SetDefaultOrganisationsCommand, Unit>
    {
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IClock clock;

        public SetDefaultOrganisationsCommandHandler(
            IOrganisationAggregateRepository organisationAggregateRepository,
            ITenantRepository tenantRepository,
            IClock clock)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.tenantRepository = tenantRepository;
            this.clock = clock;
        }

        public Task<Unit> Handle(SetDefaultOrganisationsCommand request, CancellationToken cancellationToken)
        {
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                if (tenant.IsMasterTenant)
                {
                    // the master tenant doesn't have an organisation.
                    continue;
                }

                var now = this.clock.Now();
                var organisation = this.organisationAggregateRepository.GetById(tenant.Id, tenant.Details.DefaultOrganisationId);
                organisation.SetDefault(true, null, now);
                this.organisationAggregateRepository.Save(organisation);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
