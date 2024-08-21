// <copyright file="ClaimActualisedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// A claim has been actualised.
        /// </summary>
        public class ClaimActualisedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimActualisedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">A unique ID for the claim.</param>
            /// <param name="performingUserId">The identifier of the performing user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimActualisedEvent(
                Guid tenantId,
                Guid claimId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private ClaimActualisedEvent()
            {
            }
        }
    }
}
