// <copyright file="QuoteCustomerAssociationInvitationCreatedEvent.cs" company="uBind">
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
    /// Aggregate for quote.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// The event that is triggered when the quote association to an existing customer user account is called.
        /// </summary>
        public class QuoteCustomerAssociationInvitationCreatedEvent
            : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteCustomerAssociationInvitationCreatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The quote aggregate ID.</param>
            /// <param name="quoteId">The quote ID.</param>
            /// <param name="customerUserId">The ID of the customer that is also a user in the system.</param>
            /// <param name="performingUserId">The userId who associates customer invitation creation.</param>
            /// <param name="createdTimestamp">The created time event was created.</param>
            public QuoteCustomerAssociationInvitationCreatedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid customerUserId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.CustomerUserId = customerUserId;
            }

            [JsonConstructor]
            private QuoteCustomerAssociationInvitationCreatedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the ID of the customer as a user.
            /// </summary>
            [JsonProperty]
            public Guid CustomerUserId { get; private set; }

            /// <summary>
            /// Gets the quote ID.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }
        }
    }
}
