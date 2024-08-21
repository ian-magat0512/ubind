// <copyright file="ClaimAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for user aggregates.
    /// </summary>
    public class ClaimAggregateRepository
        : AggregateRepository<ClaimAggregate, Guid, EventRecordWithGuidId>,
            IClaimAggregateRepository
    {
        private readonly IAggregateSnapshotService<ClaimAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind DbContext.</param>
        /// <param name="observer">An observer for event dispatching.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public ClaimAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IClaimEventObserver observer,
            IAggregateSnapshotService<ClaimAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<ClaimAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(dbContext, eventRecordRepository, events => ClaimAggregate.LoadFromEvents(events), EventRecordWithGuidId.Create, observer, clock, logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(ClaimAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<ClaimAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<ClaimAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.Claim);
        }

        protected override async Task<AggregateSnapshotResult<ClaimAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.Claim);
        }

        protected override async Task<AggregateSnapshotResult<ClaimAggregate>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.Claim);
        }
    }
}
