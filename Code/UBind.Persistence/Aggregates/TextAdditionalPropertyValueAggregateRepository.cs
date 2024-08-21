// <copyright file="TextAdditionalPropertyValueAggregateRepository.cs" company="uBind">
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
    /// Aggregate repository for text additional property value.
    /// </summary>
    public class TextAdditionalPropertyValueAggregateRepository
        : AggregateRepository<TextAdditionalPropertyValue, Guid, EventRecordWithGuidId>,
        ITextAdditionalPropertyValueAggregateRepository
    {
        private readonly IAggregateSnapshotService<TextAdditionalPropertyValue> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAdditionalPropertyValueAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">db context for ubind database.</param>
        /// <param name="observer">event observer of text additional property value.</param>
        /// <param name="clock">NodeTime clock.</param>
        /// <param name="logger">Microsoft logger.</param>
        public TextAdditionalPropertyValueAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            ITextAdditionalPropertyValueEventObserver observer,
            IAggregateSnapshotService<TextAdditionalPropertyValue> aggregateSnapshotService,
            IClock clock,
            ILogger<TextAdditionalPropertyValueAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => TextAdditionalPropertyValue.LoadFromEvent(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(TextAdditionalPropertyValue aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<TextAdditionalPropertyValue>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<TextAdditionalPropertyValue>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.TextAdditionalPropertyValue);
        }

        protected override async Task<AggregateSnapshotResult<TextAdditionalPropertyValue>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.TextAdditionalPropertyValue);
        }

        protected override async Task<AggregateSnapshotResult<TextAdditionalPropertyValue>?> GetAggregateSnapshotByVersion(Guid tenantId, Guid aggregateId, int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(tenantId, aggregateId, version, AggregateType.TextAdditionalPropertyValue);
        }
    }
}
