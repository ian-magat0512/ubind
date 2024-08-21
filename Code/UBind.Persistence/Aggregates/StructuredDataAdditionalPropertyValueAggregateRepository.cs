// <copyright file="StructuredDataAdditionalPropertyValueAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Aggregates
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Aggregate repository for structured data additional property value.
    /// </summary>
    public class StructuredDataAdditionalPropertyValueAggregateRepository
        : AggregateRepository<StructuredDataAdditionalPropertyValue, Guid, EventRecordWithGuidId>,
        IStructuredDataAdditionalPropertyValueAggregateRepository
    {
        private readonly IAggregateSnapshotService<StructuredDataAdditionalPropertyValue> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValueAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">db context for ubind database.</param>
        /// <param name="observer">event observer of structured data additional property value.</param>
        /// <param name="clock">NodeTime clock.</param>
        /// <param name="logger">Microsoft logger.</param>
        public StructuredDataAdditionalPropertyValueAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IStructuredDataAdditionalPropertyValueEventObserver observer,
            IAggregateSnapshotService<StructuredDataAdditionalPropertyValue> aggregateSnapshotService,
            IClock clock,
            ILogger<StructuredDataAdditionalPropertyValueAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => StructuredDataAdditionalPropertyValue.LoadFromEvents(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(StructuredDataAdditionalPropertyValue aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<StructuredDataAdditionalPropertyValue>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<StructuredDataAdditionalPropertyValue>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.StructuredDataAdditionalPropertyValue);
        }

        protected override async Task<AggregateSnapshotResult<StructuredDataAdditionalPropertyValue>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.StructuredDataAdditionalPropertyValue);
        }

        protected override async Task<AggregateSnapshotResult<StructuredDataAdditionalPropertyValue>?> GetAggregateSnapshotByVersion(Guid tenantId, Guid aggregateId, int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(tenantId, aggregateId, version, AggregateType.StructuredDataAdditionalPropertyValue);
        }
    }
}
