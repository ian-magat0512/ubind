// <copyright file="CustomerDeletedEvent.cs" company="uBind">
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
    /// Class extension for customer aggregate class.
    /// </summary>
    public partial class CustomerAggregate
    {
        /// <summary>
        /// An event that represents the customer that has been deleted or orphaned.
        /// </summary>
        public class CustomerDeletedEvent
            : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerDeletedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="customerId">The ID of the persisted customer read model.</param>
            /// <param name="performingUserId">The userId who delete.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            /// <param name="isPermanentDelete">(Optional) Determines whether to delete the customer permanently with its records.</param>
            public CustomerDeletedEvent(
                Guid tenantId,
                Guid customerId,
                Guid? performingUserId,
                Instant createdTimestamp,
                bool isPermanentDelete = false)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
                this.IsPermanentDelete = isPermanentDelete;
            }

            [JsonConstructor]
            private CustomerDeletedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the user ID of the customer.
            /// </summary>
            [JsonProperty]
            public Guid CustomerId { get; private set; }

            /// <summary>
            /// Determines whether to delete the customer permanently with its records.
            /// </summary>
            public bool IsPermanentDelete { get; private set; }
        }
    }
}
