// <copyright file="Message.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;

    /// <summary>
    /// Base class for message type entities.
    /// </summary>
    public abstract class Message : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the entity.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public Message(Guid id, Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
        }
    }
}
