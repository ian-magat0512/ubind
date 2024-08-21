// <copyright file="ClaimNumberUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Claim number event update.
        /// </summary>
        public class ClaimNumberUpdatedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimNumberUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the claim.</param>
            /// <param name="claimNumber">The new status of the claim.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimNumberUpdatedEvent(Guid tenantId, Guid claimId, string claimNumber, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.ClaimNumber = claimNumber;
            }

            [JsonConstructor]
            private ClaimNumberUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new status of the claim.
            /// </summary>
            [JsonProperty]
            public string ClaimNumber { get; private set; }
        }
    }
}
