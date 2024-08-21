// <copyright file="ClaimUpdateImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;

    /// <summary>
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// A claim has been updated through import.
        /// </summary>
        public class ClaimUpdateImportedEvent
            : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimUpdateImportedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the claim to update.</param>
            /// <param name="data">The claim imported data object.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="timestamp">The time the person aggregate was notified.</param>
            public ClaimUpdateImportedEvent(Guid tenantId, Guid claimId, ClaimImportData data, Guid? performingUserId, Instant timestamp)
                : base(tenantId, claimId, performingUserId, timestamp)
            {
                this.PolicyNumber = data.PolicyNumber;
                this.ClaimNumber = data.ClaimNumber;
                this.ReferenceNumber = data.ReferenceNumber;
                this.Amount = data.Amount;
                this.Description = data.Description;
                this.IncidentDate = data.IncidentDate.ToLocalDateFromMdyy();
                this.Status = data.Status;
            }

            [JsonConstructor]
            private ClaimUpdateImportedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the number of the policy the claim relates to.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            /// <summary>
            /// Gets the reference number for the claim.
            /// </summary>
            /// <remarks>
            /// JSON property is wrongly named for backwards compatibility with original imports which mistakenly treated
            /// claim number as reference number.
            /// </remarks>
            [JsonProperty(PropertyName = "referenceNumber")]
            public string ClaimNumber { get; private set; }

            /// <summary>
            /// Gets the reference number for the claim.
            /// </summary>
            /// <remarks>
            /// JSON property misnamed for backwards compatibility cf <see cref="ClaimNumber" />.
            /// </remarks>
            [JsonProperty(PropertyName = "realReferenceNumber")]
            public string ReferenceNumber { get; private set; }

            /// <summary>
            /// Gets the amount of the claim.
            /// </summary>
            [JsonProperty]
            public decimal Amount { get; private set; }

            /// <summary>
            /// Gets the description of the claim.
            /// </summary>
            [JsonProperty]
            public string Description { get; private set; }

            /// <summary>
            /// Gets the date the incident being claimed for occurred.
            /// </summary>
            [JsonProperty]
            public LocalDate? IncidentDate { get; private set; }

            /// <summary>
            /// Gets the status to set the claim to.
            /// </summary>
            [JsonProperty]
            public string Status { get; private set; }
        }
    }
}
