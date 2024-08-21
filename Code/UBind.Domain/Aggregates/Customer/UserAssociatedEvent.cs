// <copyright file="UserAssociatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class CustomerAggregate
    {
        /// <summary>
        /// A customer has been associated to a user account.
        /// </summary>
        [Obsolete("Since we are associating user with person, use 'PersonAggregate.UserAssociatedEvent' instead.")]
        public class UserAssociatedEvent
            : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAssociatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant.</param>
            /// <param name="customerId">The ID of the customer aggregate being initialized.</param>
            /// <param name="userId">The user ID of the customer.</param>
            /// <param name="performingUserId">The userId who associates user.</param>
            /// <param name="hasBeenInvited">The invitation status of the user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserAssociatedEvent(
                Guid tenantId,
                Guid customerId,
                Guid userId,
                Guid? performingUserId,
                bool hasBeenInvited,
                Instant createdTimestamp)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.UserId = userId;
                this.UserHasBeenInvitedToActivate = hasBeenInvited;
            }

            [JsonConstructor]
            private UserAssociatedEvent()
            {
            }

            /// <summary>
            /// Gets the user ID of the person who is the new owner.
            /// </summary>
            [JsonProperty]
            public Guid UserId { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the user has been invited to activate.
            /// </summary>
            [JsonProperty]
            public bool UserHasBeenInvitedToActivate { get; private set; }
        }
    }
}
