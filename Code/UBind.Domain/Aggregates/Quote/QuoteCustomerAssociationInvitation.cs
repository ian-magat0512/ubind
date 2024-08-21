// <copyright file="QuoteCustomerAssociationInvitation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using NodaTime;
    using UBind.Domain.Aggregates.Common;

    /// <summary>
    /// A time-limited quote association invitation to permit an activity.
    /// </summary>
    public class QuoteCustomerAssociationInvitation : Invitation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCustomerAssociationInvitation"/> class.
        /// </summary>
        /// <param name="invitationId">An ID uniquely identifying this invitation.</param>
        /// <param name="ownerId">The owner of this invitation.</param>
        /// <param name="createdTimestamp">The time the invitation was created.</param>
        public QuoteCustomerAssociationInvitation(Guid invitationId, Guid ownerId, Instant createdTimestamp)
            : base(invitationId, createdTimestamp)
        {
            this.OwnerId = ownerId;
        }

        /// <summary>
        /// Gets the ID of the invitation.
        /// </summary>
        public Guid OwnerId { get; }
    }
}
