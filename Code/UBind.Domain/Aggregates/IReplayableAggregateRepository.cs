// <copyright file="IReplayableAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    public interface IReplayableAggregateRepository<TId>
    {
        /// <summary>
        /// Replay a single aggregate event.
        /// This can be useful for the purpose of re-triggering associated automations.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID.</param>
        /// <param name="sequenceNumber">The sequence number of the event.</param>
        /// <param name="observerTypes">A list of observer types that this event will be dispatched to,
        /// or null to dispatch it to all event observer types.</param>
        Task ReplayEventByAggregateId(Guid tenantId, TId id, int sequenceNumber, IEnumerable<Type> observerTypes = null);

        /// <summary>
        /// Replay all persisted events by aggregate ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID.</param>
        /// <param name="observerTypes">A list of observer types that the events will be dispatched to,
        /// or null to dispatch it them all event observer types.</param>
        /// <param name="overrideTenantId">Override tenant id of events, only used for UB-7141 migration.</param>
        /// <returns>An awaitable task.</returns>
        Task ReplayAllEventsByAggregateId(Guid tenantId, TId id, IEnumerable<Type> observerTypes = null, Guid? overrideTenantId = null);

        /// <summary>
        /// Replay a particular event by aggregate ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID.</param>
        /// <param name="eventTypes">The event types.</param>
        /// <returns>An awaitable task.</returns>
        Task ReplayEventsOfTypeByAggregateId(Guid tenantId, TId id, Type[] eventTypes);
    }
}
