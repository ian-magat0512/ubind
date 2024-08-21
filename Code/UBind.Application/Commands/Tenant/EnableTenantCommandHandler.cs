// <copyright file="EnableTenantCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class EnableTenantCommandHandler : ICommandHandler<EnableTenantCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;

        public EnableTenantCommandHandler(
            ITenantRepository tenantRepository,
            IAutomationPeriodicTriggerScheduler periodicTriggerScheduler,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.periodicTriggerScheduler = periodicTriggerScheduler;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
        }

        public Task<Unit> Handle(EnableTenantCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var details = new TenantDetails(tenant.Details, now);
            details.UpdateDetails(
                details.Name,
                details.Alias,
                details.CustomDomain,
                false,
                details.Deleted,
                details.DefaultOrganisationId);
            tenant.Update(details);

            // clear the cache
            this.cachingResolver.RemoveCachedTenants(
                tenant.Id,
                new List<string> { tenant.Details.Alias });

            this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(tenant.Id);
            return Task.FromResult(Unit.Value);
        }
    }
}
