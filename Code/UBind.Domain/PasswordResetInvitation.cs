// <copyright file="PasswordResetInvitation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Event for password reset invitations.
    /// </summary>
    public class PasswordResetInvitation : InvitationEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetInvitation"/> class.
        /// </summary>
        /// <param name="invitationId">The invitation ID.</param>
        /// <param name="createdTimestamp">The time the activation invitation occured.</param>
        public PasswordResetInvitation(Guid invitationId, Instant createdTimestamp)
            : base(invitationId, createdTimestamp)
        {
            // Nothing to do
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetInvitation"/> class.
        /// </summary>
        private PasswordResetInvitation()
            : base()
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the invitation event type as password reset.
        /// </summary>
        public override string InvitationEventType => InvitationType.PasswordReset;
    }
}
