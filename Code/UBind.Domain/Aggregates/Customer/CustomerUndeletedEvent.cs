// <copyright file="CustomerUndeletedEvent.cs" company="uBind">
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
        /// Event raised when the deleted customer was restored.
        /// </summary>
        public class CustomerUndeletedEvent : Event<CustomerAggregate, Guid>
        {
            public CustomerUndeletedEvent(Guid tenantId, Guid customerId, Guid? performingUserId, Instant createdTimestamp)
                   : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
            }

            [JsonConstructor]
            private CustomerUndeletedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the user ID of the customer.
            /// </summary>
            [JsonProperty]
            public Guid CustomerId { get; private set; }
        }
    }
}
