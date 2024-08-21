// <copyright file="InvitationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Base abstract class for events that can occur for an invitation.
    /// </summary>
    public abstract class InvitationEvent : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationEvent"/> class.
        /// </summary>
        /// <param name="invitationId">The generated invitation UUID.</param>
        /// <param name="createdTimestamp">The time the invitation was created.</param>
        public InvitationEvent(Guid invitationId, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp) => this.InvitationId = invitationId;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationEvent"/> class.
        /// </summary>
        protected InvitationEvent()
            : base(default(Guid), default(Instant))
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the invitation event type.
        /// </summary>
        public abstract string InvitationEventType { get; }

        /// <summary>
        /// Gets the ID of the invitation.
        /// </summary>
        public Guid InvitationId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the specific invitation event if verified or not.
        /// </summary>
        public bool Verified { get; private set; }

        /// <summary>
        /// Verify the invitation event.
        /// </summary>
        public void Verify()
        {
            this.Verified = true;
        }
    }
}
