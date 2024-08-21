// <copyright file="ResourcePoolEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ResourcePool
{
    using NodaTime;

    /// <summary>
    /// Represents an event that has happened in relation to a resource pool, so that we can generate
    /// statistics and reports.
    /// </summary>
    public class ResourcePoolEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePoolEvent"/> class.
        /// </summary>
        /// <param name="type">The event type.</param>
        /// <param name="timestamp">The instant the event happened.</param>
        public ResourcePoolEvent(EventType type, Instant timestamp)
        {
            this.Type = type;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// The event types.
        /// </summary>
        public enum EventType
        {
#pragma warning disable SA1602 // Enumeration items should be documented
            ResourceAquired,
            ResourceReleased,
            PoolExhausted,
            ResourceAdded,
            ResourceRemoved,
            PoolGrown,
            ResourceWasted,
#pragma warning restore SA1602 // Enumeration items should be documented
        }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public EventType Type { get; private set; }

        /// <summary>
        /// Gets the instant at which the event occurred.
        /// </summary>
        public Instant Timestamp { get; private set; }
    }
}
