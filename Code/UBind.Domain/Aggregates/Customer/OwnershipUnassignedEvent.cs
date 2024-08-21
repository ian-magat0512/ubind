// <copyright file="OwnershipUnassignedEvent.cs" company="uBind">
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

    public partial class CustomerAggregate
    {
        public class OwnershipUnassignedEvent : Event<CustomerAggregate, Guid>
        {
            public OwnershipUnassignedEvent(
                Guid tenantId, Guid customerId, Guid performingUserId, Instant createdTimestamp)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private OwnershipUnassignedEvent()
            {
            }
        }
    }
}
