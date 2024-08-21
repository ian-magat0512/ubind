// <copyright file="Event.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Representing an event on an aggregate.
    /// </summary>
    /// <typeparam name="TAggregate">The type the aggregate's root entity this event belongs to.</typeparam>
    /// <typeparam name="TAggregateId">THe type of the aggregate's root entity's ID.</typeparam>
    public abstract class Event<TAggregate, TAggregateId> : IEvent<TAggregate, TAggregateId>
    {
        /// <summary>
        /// Constructs the event.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the aggregate belongs to.</param>
        /// <param name="aggregateId">The ID of the aggregate the event belongs to.</param>
        /// <param name="performingUserId">The user Id whow trigger the event.</param>
        /// <param name="timestamp">The time the event was created.</param>
        protected Event(Guid tenantId, TAggregateId aggregateId, Guid? performingUserId, Instant timestamp)
        {
            this.AggregateId = aggregateId;
            this.Timestamp = timestamp;
            this.PerformingUserId = performingUserId;
            this.TenantId = tenantId;
        }

        [JsonConstructor]
        protected Event()
        {
        }

        /// <inheritdoc/>
        [JsonProperty]
        public Guid? PerformingUserId { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the aggregate the event belongs to.
        /// </summary>
        [JsonIgnore]
        public TAggregateId AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp the event was created.
        /// We do not need to serialize this property as it has it's own column "TicksSinceEpoch".
        /// </summary>
        [JsonIgnore]
        public Instant Timestamp { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public Guid TenantId { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public int Sequence { get; set; }
    }
}
