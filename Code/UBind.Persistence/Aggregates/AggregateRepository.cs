// <copyright file="AggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <inheritdoc/>
    public abstract class AggregateRepository<TAggregate, TId, TEventRecord>
            : IAggregateRepository<TAggregate, TId>, IReplayableAggregateRepository<TId>
        where TAggregate : IAggregateRootEntity<TAggregate, TId>
        where TId : IEquatable<TId>
        where TEventRecord : EventRecord<TId>
    {
        private readonly IUBindDbContext dbContext;
        private readonly IEventRecordRepository eventRecordRepository;
        private readonly Func<IEnumerable<IEvent<TAggregate, TId>>, TAggregate> factoryMethod;
        private readonly Func<Guid, TId, int, IEvent<TAggregate, TId>, AggregateType, Instant, TEventRecord> eventRecordFactoryMethod;
        private readonly IAggregateEventObserver<TAggregate, IEvent<TAggregate, TId>> observer;
        private readonly IClock clock;
        private readonly ILogger logger;
        private readonly int snapshotSaveInterval = 200; // saves a new snapshot every 200 events

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRepository{TAggregate,TId,TEventRecord}"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind dbContext.</param>
        /// <param name="aggregateFactoryMethod">Method for constructing aggregate from events.</param>
        /// <param name="eventRecordFactoryMethod">Method for constructing event records.</param>
        /// <param name="observer">An event observer (for updating read model).</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public AggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            Func<IEnumerable<IEvent<TAggregate, TId>>, TAggregate> aggregateFactoryMethod,
            Func<Guid, TId, int, IEvent<TAggregate, TId>, AggregateType, Instant, TEventRecord> eventRecordFactoryMethod,
            IAggregateEventObserver<TAggregate, IEvent<TAggregate, TId>> observer,
            IClock clock,
            ILogger logger)
        {
            this.dbContext = dbContext;
            this.eventRecordRepository = eventRecordRepository;
            this.factoryMethod = aggregateFactoryMethod;
            this.eventRecordFactoryMethod = eventRecordFactoryMethod;
            this.observer = observer;
            this.clock = clock;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        protected IUBindDbContext DbContext => this.dbContext;

        /// <summary>
        /// Gets the clock.
        /// </summary>
        protected IClock Clock => this.clock;

        /// <inheritdoc/>
        public virtual TAggregate? GetById(Guid tenantId, TId id)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById)))
            {
                HashSet<TAggregate>? aggregateCache = null;
                AggregateSnapshotResult<TAggregate>? aggregateSnapshot = null;

                // If we're in a transaction then use the aggregate cache
                if (this.DbContext.HasTransaction())
                {
                    aggregateCache = this.dbContext.GetContextAggregates<TAggregate>();
                    var cachedAggregate = aggregateCache.FirstOrDefault(a => a.Id.Equals(id));
                    if (cachedAggregate != null)
                    {
                        return cachedAggregate;
                    }
                }

                if (typeof(TAggregate) == typeof(QuoteAggregate))
                {
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (aggregate snapshot database query)"))
                    {
                        aggregateSnapshot = this.GetAggregateSnapshot(tenantId, id);
                    }
                }

                IEnumerable<TEventRecord> records = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (database query)"))
                {
                    if (aggregateSnapshot?.Version != null)
                    {
                        records = this.eventRecordRepository.GetEventRecordsAfterSequence<TEventRecord, TId>(tenantId, id, aggregateSnapshot.Version);
                    }
                    else
                    {
                        records = this.eventRecordRepository.GetEventRecords<TEventRecord, TId>(tenantId, id);
                    }
                }

                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (event deserialization + factory method)"))
                {
                    IEnumerable<IEvent<TAggregate, TId>> events = records.Select(r => r.GetEvent<IEvent<TAggregate, TId>, TAggregate>());
                    if (!events.Any() && aggregateSnapshot == null)
                    {
                        // TODO: generic class constraint on TAggregate.
                        return default;
                    }

                    var aggregate = this.LoadAggregateWithEvents(aggregateSnapshot, events);
                    if (this.dbContext.HasTransaction() && aggregateCache != null)
                    {
                        aggregateCache.Add(aggregate);
                    }

                    return aggregate;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TAggregate?> GetByIdAtSequenceNumber(Guid tenantId, TId id, int atSequenceNumber)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById)))
            {
                AggregateSnapshotResult<TAggregate>? aggregateSnapshot = null;
                if (typeof(TAggregate) == typeof(QuoteAggregate))
                {
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (aggregate snapshot database query)"))
                    {
                        aggregateSnapshot = await this.GetAggregateSnapshotByVersion(tenantId, id, atSequenceNumber);
                    }
                }

                IEnumerable<TEventRecord> records = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAtSequence<TEventRecord, TId>(tenantId, id, atSequenceNumber, aggregateSnapshot?.Version);
                }

                IEnumerable<IEvent<TAggregate, TId>> events = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (event deserialization + factory method)"))
                {
                    events = records.Select(r => r.GetEvent<IEvent<TAggregate, TId>, TAggregate>());

                    if (!events.Any() && aggregateSnapshot == null)
                    {
                        return default;
                    }

                    return this.LoadAggregateWithEvents(aggregateSnapshot, events);
                }
            }
        }

        /// <inheritdoc/>
        public virtual Task DeleteById(Guid tenantId, TId id)
        {
            var rowsToDelete = this.dbContext
                .Set<TEventRecord>()
                .Where(r => r.TenantId == tenantId && r.AggregateId.Equals(id));
            this.dbContext
                .Set<TEventRecord>()
                .RemoveRange(rowsToDelete);
            return Task.CompletedTask;
        }

        public virtual async Task ReplayEventByAggregateId(Guid tenantId, TId id, int sequenceNumber, IEnumerable<Type> observerTypes = null)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayEventByAggregateId)))
            {
                AggregateSnapshotResult<TAggregate>? aggregateSnapshot = null;
                if (typeof(TAggregate) == typeof(QuoteAggregate))
                {
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetById) + " (aggregate snapshot database query)"))
                    {
                        aggregateSnapshot = await this.GetAggregateSnapshotByVersion(tenantId, id, sequenceNumber);
                    }
                }

                IEnumerable<TEventRecord>? records = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayEventByAggregateId) + " (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAtSequence<TEventRecord, TId>(tenantId, id, sequenceNumber, aggregateSnapshot?.Version);
                }

                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayEventByAggregateId) + " (event deserialization)"))
                {
                    var eventsWithSequence = records.Select(r => new { eventRecord = r.GetEvent<IEvent<TAggregate, TId>, TAggregate>(), sequence = r.Sequence });
                    var events = eventsWithSequence.Select(ews => ews.eventRecord);
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayEventByAggregateId) + " (event execution)"))
                    {
                        var @event = eventsWithSequence.SingleOrDefault(ews => ews.sequence == sequenceNumber);
                        if (@event == null)
                        {
                            throw new ErrorException(Errors.General.NotFound("event record", sequenceNumber, "sequence number"));
                        }

                        var aggregate = this.LoadAggregateWithEvents(aggregateSnapshot, events);
                        aggregate.IsBeingReplayed = true;

                        using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                        {
                            this.observer.Dispatch(aggregate, @event.eventRecord, @event.sequence, observerTypes);
                            transaction.Complete();
                        }

                        // since there's nothing to save, we still need to call OnSavedChanges to signify to system event handlers
                        // that they can start emitting the system event (which allows automations to run).
                        aggregate.OnSavedChanges();

                        aggregate.IsBeingReplayed = false;
                    }

                    await Task.CompletedTask;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task ReplayAllEventsByAggregateId(
            Guid tenantId,
            TId id,
            IEnumerable<Type>? observerTypes = null,
            Guid? overrideTenantId = null)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId)))
            {
                IEnumerable<TEventRecord>? records = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAsync<TEventRecord, TId>(tenantId, id);
                }

                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (event deserialization)"))
                {
                    var eventsWithSequence = records.Select(r => new { eventRecord = r.GetEvent<IEvent<TAggregate, TId>, TAggregate>(), sequence = r.Sequence });
                    var events = eventsWithSequence.Select(ews => ews.eventRecord);
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (event execution)"))
                    {
                        if (events.Any())
                        {
                            var aggregate = this.factoryMethod(events);
                            aggregate.IsBeingReplayed = true;
                            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                            {
                                foreach (var @event in eventsWithSequence)
                                {
                                    if (overrideTenantId != null)
                                    {
                                        @event.eventRecord.TenantId = overrideTenantId.Value;
                                    }

                                    this.observer.Dispatch(aggregate, @event.eventRecord, @event.sequence, observerTypes);
                                }

                                transaction.Complete();
                            }

                            // since there's nothing to save, we still need to call OnSavedChanges to signify to system event handlers
                            // that they can start emitting the system event (which allows automations to run).
                            aggregate.OnSavedChanges();

                            aggregate.IsBeingReplayed = false;
                        }
                    }

                    await Task.CompletedTask;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task ReplayEventsOfTypeByAggregateId(Guid tenantId, TId id, Type[] eventTypes)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId)))
            {
                IEnumerable<TEventRecord>? records = null;
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAsync<TEventRecord, TId>(tenantId, id);
                }

                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (event deserialization)"))
                {
                    var eventsWithSequence = records.Select(r => new { eventRecord = r.GetEvent<IEvent<TAggregate, TId>, TAggregate>(), sequence = r.Sequence });
                    eventsWithSequence = eventsWithSequence.Where(e => eventTypes.Any(a => a.Name == e.eventRecord.GetType().Name)).ToList();
                    var events = eventsWithSequence.Select(ews => ews.eventRecord);
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ReplayAllEventsByAggregateId) + " (event execution)"))
                    {
                        if (eventsWithSequence.Any())
                        {
                            var aggregate = this.factoryMethod(events);
                            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                            {
                                foreach (var @event in eventsWithSequence)
                                {
                                    this.observer.Dispatch(aggregate, @event.eventRecord, @event.sequence);
                                }

                                transaction.Complete();
                            }
                        }
                    }

                    await Task.CompletedTask;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task Save(TAggregate aggregate)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.Save)))
            {
                if (!this.dbContext.HasTransaction())
                {
                    // Having all db updates inside transaction guarantees hangfire events will not fire before data is ready.
                    using (var transaction = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                    {
                        this.dbContext.TransactionStack.Push(transaction);
                        try
                        {
                            await this.ApplyChangesToDbContext(aggregate);
                            this.SaveChanges(aggregate);
                            transaction.Complete();
                        }
                        finally
                        {
                            this.dbContext.TransactionStack.Pop();
                        }
                    }
                }
                else
                {
                    // Since we are already operating under a transaction, we will not actually write to the db, we'll let
                    // the code managing that transaction decide when to write to the db. So we will just make sure the changes
                    // are applied to the dbContext.
                    await this.ApplyChangesToDbContext(aggregate);
                }
            }
        }

        /// <summary>
        /// Sets the changes on the DbContext but does not commit the change to the database yet.
        /// The intention here is to prepar the dbcontext for saving, by adding all of the unsaved events to the
        /// DbContext and processing any event observers for those events, e.g. to write ReadModels.
        /// You can call this and also make any other changes to the DbContext inside a transaction scope, and then
        /// manually call dbContext.SaveChanges() at the point you want to commit the changes to the database.
        /// </summary>
        /// <param name="aggregate">The aggregate to persist.</param>
        /// <returns>An awaitable task.</returns>
        public virtual async Task ApplyChangesToDbContext(TAggregate aggregate)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.ApplyChangesToDbContext)))
            {
                bool shouldTriggerSnapshotSave = false;
                var sequenceNumber = aggregate.PersistedEventCount;
                foreach (IEvent<TAggregate, TId> @event in aggregate.UnsavedEvents)
                {
                    using (MiniProfiler.Current.Step("Dispatch " + @event.GetType().Name))
                    {
                        this.observer.Dispatch(aggregate, @event, sequenceNumber);
                    }
                    TEventRecord record = this.eventRecordFactoryMethod(@event.TenantId, aggregate.Id, sequenceNumber, @event, aggregate.AggregateType, @event.Timestamp);
                    this.logger.LogInformation($"Adding event for aggregate {aggregate.Id} with sequence number {record.Sequence}.");
                    this.dbContext.GetDbSet<TEventRecord>().Add(record);

                    if (typeof(TAggregate) == typeof(QuoteAggregate) && sequenceNumber != 0 && (sequenceNumber % this.snapshotSaveInterval == 0))
                    {
                        shouldTriggerSnapshotSave = true;
                    }

                    sequenceNumber++;
                }

                // This should trigger the snapshot save after the last event is added to the dbContext.
                // If the sequence number is equal to the snapshot save interval, then we need to save the snapshot.
                // This ensures that the aggregate is updated with the latest sequence number before the snapshot is saved.
                if (shouldTriggerSnapshotSave)
                {
                    this.InitiateAggregateSnapshotSave(aggregate, sequenceNumber - 1);
                }

                aggregate.PersistedEventCount = sequenceNumber;
                aggregate.UnsavedEvents.Clear();

                // store this as a context aggregate so it can be fetched and updated before saving of the dbContext
                // actually happens.
                var contextAggregates = this.dbContext.GetContextAggregates<TAggregate>();

                // if it already exists in the set then we remove and re-add it to ensure it's the latest version.
                contextAggregates.RemoveWhere(c => c.Id.Equals(aggregate.Id));
                contextAggregates.Add(aggregate);

                // trigger an aggregate saved changes event when the db context saving changes completes
                // This is needed because some event handlers shouldn't do certain things until AFTER the saving
                // is completed, such as triggering integrations or automations. That's critical because
                // if they request the aggregate from the db, it won't be there yet - so it needs to be AFTER save.
                this.dbContext.SavedChanges += (object instance, EventArgs args) => aggregate.OnSavedChanges();

                this.OnAggregateSave(aggregate);
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TAggregate?> GetByIdAsync(Guid tenantId, TId id)
        {
            using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdAsync)}"))
            {
                HashSet<TAggregate>? aggregateCache = null;
                AggregateSnapshotResult<TAggregate>? aggregateSnapshot = null;

                // If we're in a transaction then use the aggregate cache
                if (this.DbContext.HasTransaction())
                {
                    aggregateCache = this.dbContext.GetContextAggregates<TAggregate>();
                    var cachedAggregate = aggregateCache.FirstOrDefault(a => a.Id.Equals(id));
                    if (cachedAggregate != null)
                    {
                        return cachedAggregate;
                    }
                }

                if (typeof(TAggregate) == typeof(QuoteAggregate))
                {
                    using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.GetByIdAsync) + " (aggregate snapshot database query)"))
                    {
                        aggregateSnapshot = await this.GetAggregateSnapshotAsync(tenantId, id);
                    }
                }

                IEnumerable<TEventRecord> records = null;
                using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdAsync)} (database query)"))
                {
                    if (aggregateSnapshot?.Version != null)
                    {
                        records = await this.eventRecordRepository.GetEventRecordsAfterSequenceAsync<TEventRecord, TId>(tenantId, id, aggregateSnapshot.Version);
                    }
                    else
                    {
                        records = await this.eventRecordRepository.GetEventRecordsAsync<TEventRecord, TId>(tenantId, id);
                    }
                }

                using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdAsync)} (event deserialization + factory method)"))
                {
                    IEnumerable<IEvent<TAggregate, TId>> events = records.Select(r => r.GetEvent<IEvent<TAggregate, TId>, TAggregate>());
                    if (!events.Any() && aggregateSnapshot == null)
                    {
                        // TODO: generic class constraint on TAggregate.
                        return default;
                    }

                    var aggregate = this.LoadAggregateWithEvents(aggregateSnapshot, events);
                    if (this.dbContext.HasTransaction() && aggregateCache != null)
                    {
                        aggregateCache.Add(aggregate);
                    }

                    return aggregate;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TAggregate?> GetByIdWithoutUsingSnapshot(Guid tenantId, TId id)
        {
            using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdWithoutUsingSnapshot)}"))
            {
                HashSet<TAggregate>? aggregateCache = null;

                // If we're in a transaction then use the aggregate cache
                if (this.DbContext.HasTransaction())
                {
                    aggregateCache = this.dbContext.GetContextAggregates<TAggregate>();
                    var cachedAggregate = aggregateCache.FirstOrDefault(a => a.Id.Equals(id));
                    if (cachedAggregate != null)
                    {
                        return cachedAggregate;
                    }
                }
                IEnumerable<TEventRecord> records = null;
                using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdWithoutUsingSnapshot)} (database query)"))
                {
                    records = await this.eventRecordRepository.GetEventRecordsAsync<TEventRecord, TId>(tenantId, id);
                }

                using (MiniProfiler.Current.Step($"{nameof(AggregateRepository<TAggregate, TId, TEventRecord>)}.{nameof(this.GetByIdWithoutUsingSnapshot)} (event deserialization + factory method)"))
                {
                    IEnumerable<IEvent<TAggregate, TId>> events = records.Select(r => r.GetEvent<IEvent<TAggregate, TId>, TAggregate>());
                    if (!events.Any())
                    {
                        // TODO: generic class constraint on TAggregate.
                        return default;
                    }

                    var aggregate = this.factoryMethod(events);
                    if (this.dbContext.HasTransaction() && aggregateCache != null)
                    {
                        aggregateCache.Add(aggregate);
                    }

                    return aggregate;
                }
            }
        }

        /// <inheritdoc/>
        public int GetSnapshotSaveInterval()
        {
            return this.snapshotSaveInterval;
        }

        protected abstract void InitiateAggregateSnapshotSave(TAggregate aggregate, int version);

        protected abstract AggregateSnapshotResult<TAggregate>? GetAggregateSnapshot(Guid tenantId, TId aggregateId);

        protected abstract Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, TId aggregateId);

        protected abstract Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotByVersion(Guid tenantId, TId aggregateId, int version);

        /// <summary>
        /// Extension point for subclasses.
        /// </summary>
        /// <param name="aggregate">The aggregate being saved.</param>
        protected virtual void OnAggregateSave(TAggregate aggregate)
        {
            // Nop.
        }

        /// <summary>
        /// Persists changes to the database.
        /// </summary>
        /// <param name="aggregate">The aggregate whose changes are being persisted.</param>
        protected virtual void SaveChanges(TAggregate aggregate)
        {
            using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.SaveChanges)))
            {
                try
                {
                    this.dbContext.SaveChanges();
                }
                catch (DbUpdateException ex) when (ex.IsDuplicateKeyException())
                {
                    ((UBindDbContext)this.dbContext).Reset(this.logger);
                    throw new ConcurrencyException($"Aggregate '{aggregate.GetType()}' with Id '{aggregate.Id}' has been updated since it was read from the database (there were two entries to be saved with the same key)", ex);
                }
            }
        }

        private TAggregate LoadAggregateWithEvents(AggregateSnapshotResult<TAggregate>? aggregateSnapshot, IEnumerable<IEvent<TAggregate, TId>> events)
        {
            TAggregate aggregate;
            if (aggregateSnapshot != null)
            {
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.LoadAggregateWithEvents) + "/apply events by snapshot/total events: " + events.Count()))
                {
                    aggregate = aggregateSnapshot.Aggregate;
                    aggregate = aggregate.ApplyEventsAfterSnapshot(events, aggregateSnapshot.Version);
                }
            }
            else
            {
                using (MiniProfiler.Current.Step(nameof(AggregateRepository<TAggregate, TId, TEventRecord>) + "." + nameof(this.LoadAggregateWithEvents) + "/apply all events/total events: " + events.Count()))
                {
                    aggregate = this.factoryMethod(events);
                }
            }

            return aggregate;
        }
    }
}