// <copyright file="UserActivatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// An activation invitation has been verified for a user.
        /// </summary>
        public class UserActivatedEvent : InvitationEvent<UserActivatedEvent, IUserEventObserver>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserActivatedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="resetInvitationId">The ID of the invitation being used.</param>
            /// <param name="newSaltedHashedPassword">The new salted, hashed password.</param>
            /// <param name="customerId">The ID of the customer associated with the user, if any.</param>
            /// <param name="personId">The ID of the person associated with the user, if any.</param>
            /// <param name="performingUserId">The userId who activate the activation invitation.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserActivatedEvent(
                Guid tenantId,
                Guid userId,
                Guid resetInvitationId,
                string? newSaltedHashedPassword,
                Guid? customerId,
                Guid personId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.ResetInvitationId = resetInvitationId;
                this.NewSaltedHashedPassword = newSaltedHashedPassword;
                this.CustomerId = customerId;
                this.PersonId = personId;
            }

            [JsonConstructor]
            private UserActivatedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the password reset invitation being used.
            /// </summary>
            [JsonProperty]
            public Guid ResetInvitationId { get; private set; }

            /// <summary>
            /// Gets the new salted, hashed password that has been set.
            /// </summary>
            [JsonProperty]
            public string? NewSaltedHashedPassword { get; private set; }

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
