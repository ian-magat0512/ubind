// <copyright file="IEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using NodaTime;

    /// <summary>
    /// Defines a method whereby an event may dispatch itself to its aggregate to be applied.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the root entity for the aggregate the event belongs to.</typeparam>
    /// <typeparam name="TId">The type of the aggregate's ID.</typeparam>
    public interface IEvent<TAggregate, TId>
    {
        /// <summary>
        /// Gets or sets the ID of the aggregate the event relates to.
        /// </summary>
        TId AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the event related to this aggregate.
        /// </summary>
        int Sequence { get; set; }

        /// <summary>
        /// Gets the ID of the performing user if any, otherwise null.
        /// </summary>
        Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets or sets the ID of the tenant the aggregate event is under.
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp the event was created.
        /// </summary>
        Instant Timestamp { get; set; }
    }
}
