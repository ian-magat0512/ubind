// <copyright file="PasswordChangedEvent.cs" company="uBind">
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
        /// An password reset invitation has been verified for a user.
        /// </summary>
        public class PasswordChangedEvent : InvitationEvent<PasswordChangedEvent, IUserEventObserver>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PasswordChangedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="resetInvitationId">The ID of the invitation being used.</param>
            /// <param name="newSaltedHashedPassword">The new salted, hashed password.</param>
            /// <param name="performingUserId">The userId who changed the password.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PasswordChangedEvent(Guid tenantId, Guid userId, Guid resetInvitationId, string newSaltedHashedPassword, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.ResetInvitationId = resetInvitationId;
                this.NewSaltedHashedPassword = newSaltedHashedPassword;
            }

            [JsonConstructor]
            private PasswordChangedEvent()
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
            public string NewSaltedHashedPassword { get; private set; }
        }
    }
}
