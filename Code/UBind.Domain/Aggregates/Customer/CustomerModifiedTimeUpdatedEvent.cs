// <copyright file="CustomerModifiedTimeUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class CustomerAggregate
    {
        /// <summary>
        /// Event raised when a customer's modified time has been updated.
        /// </summary>
        public class CustomerModifiedTimeUpdatedEvent
            : Event<CustomerAggregate, Guid>
        {
            public CustomerModifiedTimeUpdatedEvent(
                Guid tenantId, Guid aggregateId, Guid personId, Instant modifiedTime, Guid? performingUserId, Instant timestamp)
                : base(tenantId, aggregateId, performingUserId, timestamp)
            {
                this.ModifiedTime = modifiedTime;
                this.PersonId = personId;
            }

            [JsonConstructor]
            private CustomerModifiedTimeUpdatedEvent()
            {
            }

            [JsonProperty]
            public Instant ModifiedTime { get; private set; }

            [JsonProperty]
            public Guid PersonId { get; private set; }
        }
    }
}
