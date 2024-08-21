// <copyright file="OwnershipAssignedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote has been assigned to a new owner.
        /// </summary>
        public class OwnershipAssignedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OwnershipAssignedEvent"/> class.
            /// </summary>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="userId">The user Id of the new owner.</param>
            /// <param name="personId">The person Id of the new owner.</param>
            /// <param name="fullName">The full name of the new owner.</param>
            /// <param name="performingUserId">The userId who assigned the ownership.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public OwnershipAssignedEvent(Guid tenantId, Guid quoteId, Guid userId, Guid personId, string fullName, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, performingUserId, createdTimestamp)
            {
                this.UserId = userId;
                this.PersonId = personId;
                this.FullName = fullName;
            }

            [JsonConstructor]
            private OwnershipAssignedEvent()
            {
            }

            /// <summary>
            /// Gets the user ID of the new owner.
            /// </summary>
            [JsonProperty]
            public Guid UserId { get; private set; }

            /// <summary>
            /// Gets the person ID of the new owner.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

            /// <summary>
            /// Gets the full name of the new owner.
            /// </summary>
            [JsonProperty]
            public string FullName { get; private set; }
        }
    }
}
