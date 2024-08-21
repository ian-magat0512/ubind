// <copyright file="Invitation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    using System;
    using NodaTime;

    /// <summary>
    /// A time-limited invitation to permit an activity.
    /// </summary>
    public class Invitation
    {
        private const int InvitationValidityDurationInDays = 30;
        private readonly Instant createdTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Invitation"/> class.
        /// </summary>
        /// <param name="id">An ID uniquely identifying this invitation.</param>
        /// <param name="createdTimestamp">The time the invitation was created.</param>
        public Invitation(Guid id, Instant createdTimestamp)
        {
            this.Id = id;
            this.createdTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the ID of the invitation.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Check to see if the invitation has expired and time.
        /// </summary>
        /// <param name="verificationTime">The time the expiry is being checked.</param>
        /// <returns><c>true</c> if the request is within the valid period, otherwise <c>false</c>.</returns>
        public bool IsExpired(Instant verificationTime)
        {
            var elapsedTime = verificationTime - this.createdTimestamp;
            return elapsedTime > Duration.FromDays(InvitationValidityDurationInDays);
        }
    }
}
