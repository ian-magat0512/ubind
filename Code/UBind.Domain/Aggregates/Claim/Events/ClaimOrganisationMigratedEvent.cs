// <copyright file="ClaimOrganisationMigratedEvent.cs" company="uBind">
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
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event raised when a claim has been modified due to the added organisation Id property.
        /// </summary>
        public class ClaimOrganisationMigratedEvent
            : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimOrganisationMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="organisationId">The ID of the organisation the claim belongs to.</param>
            /// <param name="claimId">The ID of the claim.</param>
            /// <param name="performingUserId">The userId who performed the migration for the claim.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimOrganisationMigratedEvent(
                Guid tenantId, Guid organisationId, Guid claimId, Guid? performingUserId, Instant createdTimestamp)
                 : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.ClaimId = claimId;
            }

            [JsonConstructor]
            private ClaimOrganisationMigratedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the claim.
            /// </summary>
            [JsonProperty]
            public Guid ClaimId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the claim is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
