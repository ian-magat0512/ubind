// <copyright file="CustomerOrganisationMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class CustomerAggregate
    {
        /// <summary>
        /// Event raised when a customer has been modified due to the added organisation Id property.
        /// </summary>
        public class CustomerOrganisationMigratedEvent
            : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerOrganisationMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="organisationId">The ID of the organisation the customer belongs to.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="customerId">The ID of the customer.</param>
            /// <param name="performingUserId">The userId who performed the migration for the customer.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public CustomerOrganisationMigratedEvent(
                Guid tenantId, Guid organisationId, Guid personId, Guid customerId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.PersonId = personId;
                this.CustomerId = customerId;
            }

            [JsonConstructor]
            private CustomerOrganisationMigratedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the customer.
            /// </summary>
            [JsonProperty]
            public Guid CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the customer person.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the customer is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
