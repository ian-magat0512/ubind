// <copyright file="CustomerDetailsUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote's customer details have been updated.
        /// </summary>
        public class CustomerDetailsUpdatedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerDetailsUpdatedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="customerDetails">Form data as JSON.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public CustomerDetailsUpdatedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, IPersonalDetails customerDetails, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.CustomerDetailsUpdateId = Guid.NewGuid();
                this.CustomerDetails = new PersonalDetails(customerDetails);
            }

            [JsonConstructor]
            private CustomerDetailsUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.quoteId == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets an Id uniquely identifying the customer details update.
            /// </summary>
            [JsonProperty]
            public Guid CustomerDetailsUpdateId { get; private set; }

            /// <summary>
            /// Gets the updated customer details.
            /// </summary>
            [JsonProperty]
            public PersonalDetails CustomerDetails { get; private set; }
        }
    }
}
