// <copyright file="PropertyUpdateEvent.cs" company="uBind">
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
    /// An event which represents an update to a property on an aggregate.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being updated.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate the event belongs to.</typeparam>
    /// <typeparam name="TAggregateId">The type of the aggregate's ID.</typeparam>"
    public abstract class PropertyUpdateEvent<TProperty, TAggregate, TAggregateId> : Event<TAggregate, TAggregateId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyUpdateEvent{TProperty, TAggregate, TAggregateId}"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="aggregateId">The ID of the user aggregate the event belongs to.</param>
        /// <param name="value">The value the property was updated to.</param>
        /// <param name="performingUserId ">The userId who performed update.</param>
        /// <param name="timestamp">The time the event was created.</param>
        public PropertyUpdateEvent(Guid tenantId, TAggregateId aggregateId, TProperty value, Guid? performingUserId, Instant timestamp)
            : base(tenantId, aggregateId, performingUserId, timestamp)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyUpdateEvent{TProperty, TAggregate, TAggregateId}"/> class.
        /// </summary>
        [JsonConstructor]
        protected PropertyUpdateEvent()
            : base(default, default(TAggregateId), default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets or sets the new value for the property.
        /// </summary>
        [JsonProperty]
        public TProperty Value { get; protected set; }
    }
}
