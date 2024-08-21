// <copyright file="UserAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for user aggregates.
    /// </summary>
    public class UserAggregateRepository
        : AggregateRepository<UserAggregate, Guid, EventRecordWithGuidId>, IUserAggregateRepository
    {
        private readonly IAggregateSnapshotService<UserAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind DbContext.</param>
        /// <param name="observer">An observer for event dispatching.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public UserAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IUserEventObserver observer,
            IAggregateSnapshotService<UserAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<UserAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => new UserAggregate(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(UserAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<UserAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<UserAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.User);
        }

        protected override async Task<AggregateSnapshotResult<UserAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.User);
        }

        protected override async Task<AggregateSnapshotResult<UserAggregate>?> GetAggregateSnapshotByVersion(Guid tenantId, Guid aggregateId, int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(tenantId, aggregateId, version, AggregateType.User);
        }
    }
}
