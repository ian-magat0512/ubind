// <copyright file="CustomerAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for user aggregates.
    /// </summary>
    public class CustomerAggregateRepository
        : AggregateRepository<CustomerAggregate, Guid, EventRecordWithGuidId>,
            ICustomerAggregateRepository
    {
        private readonly IAggregateSnapshotService<CustomerAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        public CustomerAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            ICustomerEventObserver observer,
            IAggregateSnapshotService<CustomerAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<CustomerAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => CustomerAggregate.LoadFromEvents(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(CustomerAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<CustomerAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<CustomerAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.Customer);
        }

        protected override async Task<AggregateSnapshotResult<CustomerAggregate>?> GetAggregateSnapshotAsync(
            Guid tenantId,
            Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(
                tenantId,
                aggregateId,
                AggregateType.Customer);
        }

        protected override async Task<AggregateSnapshotResult<CustomerAggregate>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.Customer);
        }
    }
}
