// <copyright file="CustomerAssignedEvent.cs" company="uBind">
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
        /// Event raised when a quote has been assigned to a customer.
        /// </summary>
        public class CustomerAssignedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerAssignedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="customerId">The Id of the customer.</param>
            /// <param name="personId">The Id of the person the customer relates to.</param>
            /// <param name="customerDetails">The updated customer details.</param>
            /// <param name="performingUserId">The userId who assigned quote to customer.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public CustomerAssignedEvent(
                Guid tenantId, Guid aggregateId, Guid customerId, Guid personId, IPersonalDetails customerDetails, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
                this.PersonId = personId;
                this.CustomerDetails = new PersonalDetails(customerDetails);
            }

            [JsonConstructor]
            private CustomerAssignedEvent()
            {
            }

            /// <summary>
            /// Gets the user ID of the customer.
            /// </summary>
            [JsonProperty]
            public Guid CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the person the customer refers to.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

            /// <summary>
            /// Gets the updated customer details.
            /// </summary>
            [JsonProperty]
            public PersonalDetails CustomerDetails { get; private set; }
        }
    }
}
