// <copyright file="ApplyNewIdEvent.cs" company="uBind">
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
        /// An event that represents updating of the customer tenant new Id.
        /// </summary>
        public class ApplyNewIdEvent : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ApplyNewIdEvent"/> class.
            /// </summary>
            /// <param name="tenantNewId">The tenants new Id.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="personId">The person id.</param>
            /// <param name="performingUserId">The performing user id.</param>
            /// <param name="createdTimestamp">The created time.</param>
            public ApplyNewIdEvent(
                Guid tenantNewId,
                Guid aggregateId,
                Guid personId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantNewId, aggregateId, performingUserId, createdTimestamp)
            {
                this.PersonId = personId;
            }

            [JsonConstructor]
            private ApplyNewIdEvent()
            {
            }

            /// <summary>
            /// Gets the person id.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }
        }
    }
}
