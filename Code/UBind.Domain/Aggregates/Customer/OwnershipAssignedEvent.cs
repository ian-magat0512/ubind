// <copyright file="OwnershipAssignedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class CustomerAggregate
    {
        /// <summary>
        /// A customer has been assigned to a new owner.
        /// </summary>
        public class OwnershipAssignedEvent : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OwnershipAssignedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant.</param>
            /// <param name="customerId">The ID of the customer aggregate being initialized.</param>
            /// <param name="userId">The user ID of the new owner.</param>
            /// <param name="ownerPersonId">The ID of the person who is the new owner.</param>
            /// <param name="ownerFullName">The full name of the new owner.</param>
            /// <param name="performingUserId">The userId who assigned the ownership to new owner.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public OwnershipAssignedEvent(
                Guid tenantId, Guid customerId, Guid userId, Guid ownerPersonId, string ownerFullName, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.UserId = userId;
                this.PersonId = ownerPersonId;
                this.FullName = ownerFullName;
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
            /// Gets the ID of the person who is the new owner.
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
