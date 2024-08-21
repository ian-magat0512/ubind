// <copyright file="DomainEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Notifications
{
    using System;
    using MediatR;
    using NodaTime;

    /// <summary>
    /// A base event class to trigger the addition of default values to a newly created entity with an aggregate.
    /// </summary>
    public abstract class DomainEvent : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="performingUserId">ID of the user.</param>
        /// <param name="timestamp">The time of the event.</param>
        protected DomainEvent(Guid? performingUserId, Instant timestamp)
        {
            this.PerformingUserId = performingUserId;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the ID of the user.
        /// </summary>
        public Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets the created timestamp.
        /// </summary>
        public Instant Timestamp { get; }
    }
}
