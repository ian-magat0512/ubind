// <copyright file="PersonAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for user aggregates.
    /// </summary>
    public class PersonAggregateRepository
        : AggregateRepository<PersonAggregate, Guid, EventRecordWithGuidId>, IPersonAggregateRepository
    {
        private readonly IAggregateSnapshotService<PersonAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind DbContext.</param>
        /// <param name="observer">Observer for handling events.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public PersonAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IPersonEventObserver observer,
            IAggregateSnapshotService<PersonAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<PersonAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(dbContext, eventRecordRepository, events => PersonAggregate.LoadFromEvents(events), EventRecordWithGuidId.Create, observer, clock, logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(PersonAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<PersonAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<PersonAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.Person);
        }

        protected override async Task<AggregateSnapshotResult<PersonAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.Person);
        }

        protected override async Task<AggregateSnapshotResult<PersonAggregate>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.Person);
        }
    }
}
