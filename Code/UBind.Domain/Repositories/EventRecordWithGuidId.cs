// <copyright file="EventRecordWithGuidId.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;

    /// <summary>
    /// For persisting aggregate events.
    /// </summary>
    public class EventRecordWithGuidId : EventRecord<Guid>
    {
        /// <summary>
        /// Gets an expression mapping private field requiring persistence for EF.
        /// </summary>
        public static readonly Expression<Func<EventRecordWithGuidId, string>> EventJsonExpression =
            rf => rf.EventJson;

        /// <summary>
        /// Gets an expression mapping private field requiring persistence for EF.
        /// </summary>
        public static readonly Expression<Func<EventRecordWithGuidId, long>> TicksSinceEpochExpression =
            rf => rf.TicksSinceEpoch;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecordWithGuidId"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant this aggregate belongs to.</param>
        /// <param name="aggregateId">The ID of the aggregate the event belongs to.</param>
        /// <param name="sequence">The sequence number for ordering the event in the aggregate.</param>
        /// <param name="eventJson">The event serialized as JSON.</param>
        /// <param name="aggregateType">The aggregates type for easier tracking.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public EventRecordWithGuidId(Guid tenantId, Guid aggregateId, int sequence, string eventJson, AggregateType aggregateType, Instant createdTimestamp)
            : base(tenantId, aggregateId, sequence, eventJson, aggregateType, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecordWithGuidId"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        protected EventRecordWithGuidId()
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="EventRecord{TId}"/> class for a given event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="tenantId">The id of the tenant this aggregate event is for.</param>
        /// <param name="aggregateId">The ID of the aggregate the event belongs to.</param>
        /// <param name="sequence">The event's sequence number in its aggregate.</param>
        /// <param name="event">The event.</param>
        /// <param name="aggregateType">The aggregates type for easier tracking.</param>
        /// <param name="createdTimestamp">A record created timestamp.</param>
        /// <returns>The new instance of the <see cref="EventRecord{TId}"/>.</returns>
        public static EventRecordWithGuidId Create<TEvent>(
            Guid tenantId,
            Guid aggregateId,
            int sequence,
            TEvent @event,
            AggregateType aggregateType,
            Instant createdTimestamp)
        {
            var eventJson = JsonConvert.SerializeObject(@event, CustomSerializerSetting.AggregateEventSerializerSettings);
            return new EventRecordWithGuidId(tenantId, aggregateId, sequence, eventJson, aggregateType, createdTimestamp);
        }
    }
}
