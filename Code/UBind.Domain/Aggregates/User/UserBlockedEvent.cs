// <copyright file="UserBlockedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// The user has been blocked.
        /// </summary>
        public class UserBlockedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserBlockedEvent"/> class for a user.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="customerId">The ID of the customer associated with the user, if any.</param>
            /// <param name="personId">The ID of the person associated with the user, if any.</param>
            /// <param name="performingUserId">The userId who the block the user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserBlockedEvent(Guid tenantId, Guid userId, Guid? customerId, Guid personId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
                this.PersonId = personId;
            }

            [JsonConstructor]
            private UserBlockedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the customer associated with the user, if any, otherwise default.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the Person associated with the user, if any, otherwise default.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }
        }
    }
}
