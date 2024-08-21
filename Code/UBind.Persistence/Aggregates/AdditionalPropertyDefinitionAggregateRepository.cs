// <copyright file="AdditionalPropertyDefinitionAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Aggregates
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Aggregate repository for additional property definition.
    /// </summary>
    public class AdditionalPropertyDefinitionAggregateRepository :
        AggregateRepository<AdditionalPropertyDefinition, Guid, EventRecordWithGuidId>,
            IAdditionalPropertyDefinitionAggregateRepository
    {
        private readonly IAggregateSnapshotService<AdditionalPropertyDefinition> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db context for ubind database.</param>
        /// <param name="additionalPropertyDefinitionEventObserver">Event observer of additiona property definition.</param>
        /// <param name="clock">NodeTime clock.</param>
        /// <param name="logger">Microsoft logger.</param>
        public AdditionalPropertyDefinitionAggregateRepository(
           IUBindDbContext dbContext,
           IEventRecordRepository eventRecordRepository,
           IAdditionalPropertyDefinitionEventObserver additionalPropertyDefinitionEventObserver,
           IAggregateSnapshotService<AdditionalPropertyDefinition> aggregateSnapshotService,
           IClock clock,
           ILogger<AdditionalPropertyDefinitionAggregateRepository> logger,
           IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => AdditionalPropertyDefinition.LoadFromEvents(events),
                  EventRecordWithGuidId.Create,
                  additionalPropertyDefinitionEventObserver,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(AdditionalPropertyDefinition aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<AdditionalPropertyDefinition>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<AdditionalPropertyDefinition>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.AdditionalPropertyDefinition);
        }

        protected override async Task<AggregateSnapshotResult<AdditionalPropertyDefinition>?> GetAggregateSnapshotAsync(
            Guid tenantId,
            Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(
                tenantId,
                aggregateId,
                AggregateType.AdditionalPropertyDefinition);
        }

        protected override async Task<AggregateSnapshotResult<AdditionalPropertyDefinition>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.AdditionalPropertyDefinition);
        }
    }
}
