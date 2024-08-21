// <copyright file="ClaimDescriptionUpdatedEvent.cs" company="uBind">
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
    /// Aggregate for people.
    /// </summary>
    public partial class ClaimAggregate
    {
        private void Apply(ClaimDescriptionUpdatedEvent claimDescriptionUpdatedEvent, int sequenceNumber)
        {
            this.Claim.Apply(claimDescriptionUpdatedEvent, sequenceNumber);
        }

        /// <summary>
        /// A claim's description has been updated.
        /// </summary>
        public class ClaimDescriptionUpdatedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimDescriptionUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the claim.</param>
            /// <param name="description">The new description of the claim.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimDescriptionUpdatedEvent(Guid tenantId, Guid claimId, string description, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.Description = description;
            }

            [JsonConstructor]
            private ClaimDescriptionUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new description of the claim.
            /// </summary>
            [JsonProperty]
            public string Description { get; private set; }
        }
    }
}
