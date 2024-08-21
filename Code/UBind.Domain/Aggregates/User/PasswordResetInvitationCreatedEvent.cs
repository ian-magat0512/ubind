// <copyright file="PasswordResetInvitationCreatedEvent.cs" company="uBind">
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
        /// An password reset invitation has been created for a user.
        /// </summary>
        public class PasswordResetInvitationCreatedEvent : InvitationEvent<PasswordResetInvitationCreatedEvent, IUserEventObserver>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PasswordResetInvitationCreatedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="performingUserId">The userId who sends reset invitation.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PasswordResetInvitationCreatedEvent(Guid tenantId, Guid userId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private PasswordResetInvitationCreatedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }
        }
    }
}
