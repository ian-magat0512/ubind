// <copyright file="AssociatedWithCustomerEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person has been associated to customer.
        /// </summary>
        public class AssociatedWithCustomerEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AssociatedWithCustomerEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the person is for.</param>
            /// <param name="personId">A unique ID for the Person.</param>
            /// <param name="customerId">The ID of the customer.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public AssociatedWithCustomerEvent(Guid tenantId, Guid personId, Guid customerId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
            }

            [JsonConstructor]
            public AssociatedWithCustomerEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the customer to be assigned to person.
            /// </summary>
            [JsonProperty]
            public Guid CustomerId { get; private set; }
        }
    }
}
