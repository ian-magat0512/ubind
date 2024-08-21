// <copyright file="PortalAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Aggregates
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    public class PortalAggregateRepository
        : AggregateRepository<PortalAggregate, Guid, EventRecordWithGuidId>, IPortalAggregateRepository
    {
        private readonly IAggregateSnapshotService<PortalAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        public PortalAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IPortalEventObserver observer,
            IAggregateSnapshotService<PortalAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<PortalAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => new PortalAggregate(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(PortalAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<PortalAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<PortalAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.Portal);
        }

        protected override async Task<AggregateSnapshotResult<PortalAggregate>?> GetAggregateSnapshotAsync(
            Guid tenantId,
            Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(
                tenantId,
                aggregateId,
                AggregateType.Portal);
        }

        protected override async Task<AggregateSnapshotResult<PortalAggregate>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.Portal);
        }
    }
}
