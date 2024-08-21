// <copyright file="UpdateTenantCommandHandler.cs" company="uBind">
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
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Notification;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class UpdateTenantCommandHandler : ICommandHandler<UpdateTenantCommand, Unit>
    {
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;
        private readonly ITenantRepository tenantRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;
        private readonly ITenantService tenantService;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;

        public UpdateTenantCommandHandler(
            ITenantRepository tenantRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAutomationPeriodicTriggerScheduler periodicTriggerScheduler,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IClock clock,
            ITenantService tenantService,
            ITenantSystemEventEmitter tenantSystemEventEmitter)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
            this.tenantRepository = tenantRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.periodicTriggerScheduler = periodicTriggerScheduler;
            this.tenantService = tenantService;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        }

        public async Task<Unit> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
        {
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var oldAlias = tenant.Details.Alias;
            var newAlias = command.Alias;
            bool aliasChanged = newAlias != oldAlias;
            if (aliasChanged)
            {
                this.tenantService.ThrowIfTenantAliasIsNull(command.Alias);
                this.tenantService.ThrowIfTenantAliasInUse(command.Alias);
            }

            if (tenant.Details.Name != command.Name)
            {
                this.tenantService.ThrowIfTenantNameInUse(command.Name);
            }

            if (command.CustomDomain.IsNotNullOrEmpty() && tenant.Details.CustomDomain != command.CustomDomain)
            {
                this.tenantService.ThrowIfCustomDomainInUse(command.CustomDomain);
            }

            // retain previous settings of tenant while updating tenant detail
            var tenantDetails = new TenantDetails(tenant.Details, this.clock.GetCurrentInstant());
            tenantDetails.UpdateDetails(
                command.Name,
                command.Alias,
                command.CustomDomain,
                tenant.Details.Disabled,
                tenant.Details.Deleted,
                tenant.Details.DefaultOrganisationId);
            tenant.Update(tenantDetails);
            this.tenantRepository.SaveChanges();

            // clear the cache
            this.cachingResolver.RemoveCachedTenants(
                tenant.Id,
                new List<string> { oldAlias, newAlias });

            // notify others - this causes the folders to be renamed amongst other things.
            if (aliasChanged)
            {
                var onAliasChangeEvent =
                    new TenantAliasChangeDomainEvent(
                      tenant.Id,
                      oldAlias,
                      newAlias,
                      this.httpContextPropertiesResolver.PerformingUserId,
                      this.clock.Now());
                await this.mediator.Publish(onAliasChangeEvent);
            }

            // recreate automation perioding trigger jobs
            await this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(tenant.Id);
            await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(tenant.Id, SystemEventType.TenantModified);
            return Unit.Value;
        }
    }
}
