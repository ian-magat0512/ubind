// <copyright file="ClaimIncidentDateUpdatedEvent.cs" company="uBind">
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
        private void Apply(ClaimIncidentDateUpdatedEvent claimIncidentDateUpdatedEvent, int sequenceNumber)
        {
            this.Claim.Apply(claimIncidentDateUpdatedEvent, sequenceNumber);
        }

        /// <summary>
        /// A claim's incident date has been updated.
        /// </summary>
        public class ClaimIncidentDateUpdatedEvent : Event<ClaimAggregate, Guid>
        {
            private Instant? incidentTimestamp;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimIncidentDateUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the claim.</param>
            /// <param name="incidentTimestamp">The new incident timestamp.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimIncidentDateUpdatedEvent(Guid tenantId, Guid claimId, Instant? incidentTimestamp, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.incidentTimestamp = incidentTimestamp;
            }

            [JsonConstructor]
            private ClaimIncidentDateUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new incident date of the claim.
            /// </summary>
            [JsonProperty]
            [Obsolete("Please use IncidentTimestamp instead")]
            public LocalDate? IncidentDate { get; private set; }

#pragma warning disable CS0618 // Type or member is obsolete
            [JsonProperty]
            public Instant? IncidentTimestamp
            {
                get => this.incidentTimestamp.HasValue
                    ? this.incidentTimestamp.Value
                    : this.IncidentDate?.At(new LocalTime(12, 0)).InZoneLeniently(Timezones.AET).ToInstant();
                set => this.incidentTimestamp = value;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
