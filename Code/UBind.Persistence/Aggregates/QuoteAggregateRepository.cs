// <copyright file="QuoteAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for quote aggregates.
    /// </summary>
    public class QuoteAggregateRepository
        : AggregateRepository<QuoteAggregate, Guid, EventRecordWithGuidId>, IQuoteAggregateRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IAggregateEventObserver<QuoteAggregate, IEvent<QuoteAggregate, Guid>> observer;
        private readonly IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;
        private readonly IEventRecordRepository eventRecordRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind DbContext.</param>
        /// <param name="observer">An observer for event dispatching.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public QuoteAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IQuoteEventObserver observer,
            IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService,
            IClock clock,
            ILogger<QuoteAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(
                  dbContext,
                  eventRecordRepository,
                  events => QuoteAggregate.LoadFromEvents(events),
                  EventRecordWithGuidId.Create,
                  observer,
                  clock,
                  logger)
        {
            this.dbContext = dbContext;
            this.eventRecordRepository = eventRecordRepository;
            this.observer = observer;
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        public override async Task ReplayAllEventsByAggregateId(
            Guid tenantId,
            Guid id,
            IEnumerable<Type> observerTypes = null,
            Guid? overrideTenantId = null)
        {
            var currentStep = $"{this.GetType().Name}.{nameof(this.ReplayAllEventsByAggregateId)}";
            using (MiniProfiler.Current.Step(currentStep))
            {
                IEnumerable<EventRecordWithGuidId> records = null;
                using (MiniProfiler.Current.Step($"{currentStep} (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAsync<EventRecordWithGuidId, Guid>(tenantId, id);
                }

                using (MiniProfiler.Current.Step($"{currentStep} (event deserialization)"))
                {
                    var events = records.Select(r => r.GetEvent<IEvent<QuoteAggregate, Guid>, QuoteAggregate>());
                    var quoteAggregate = QuoteAggregate.LoadFromEvents(events);
                    var strippedEvents = quoteAggregate.GetRemainingEventsWithSequenceNumberAfterRollbacks();
                    using (MiniProfiler.Current.Step($"{currentStep} (event execution)"))
                    {
                        if (strippedEvents.Any())
                        {
                            quoteAggregate.IsBeingReplayed = true;
                            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                            {
                                foreach (var @event in strippedEvents)
                                {
                                    if (overrideTenantId != null)
                                    {
                                        @event.Value.TenantId = overrideTenantId.Value;
                                    }

                                    this.observer.Dispatch(quoteAggregate, @event.Value, @event.Key, observerTypes);
                                }

                                transaction.Complete();
                            }

                            quoteAggregate.IsBeingReplayed = false;
                        }
                    }

                    await Task.CompletedTask;
                }
            }
        }

        protected override void InitiateAggregateSnapshotSave(QuoteAggregate aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<QuoteAggregate>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<QuoteAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.Quote);
        }

        protected override async Task<AggregateSnapshotResult<QuoteAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(tenantId, aggregateId, AggregateType.Quote);
        }

        protected override async Task<AggregateSnapshotResult<QuoteAggregate>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.Quote);
        }
    }
}
