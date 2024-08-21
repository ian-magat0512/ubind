// <copyright file="CustomerTransferredToAnotherOrganisationEvent.cs" company="uBind">
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
        /// Event raised when a costumer is about to be transferred to another organisation.
        /// </summary>
        public class CustomerTransferredToAnotherOrganisationEvent
            : Event<CustomerAggregate, Guid>
        {
            public CustomerTransferredToAnotherOrganisationEvent(
                Guid tenantId,
                Guid organisationId,
                Guid customerId,
                Guid personId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, customerId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.PersonId = personId;
            }

            [JsonConstructor]
            private CustomerTransferredToAnotherOrganisationEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            [JsonProperty]
            public Guid PersonId { get; private set; }
        }
    }
}
