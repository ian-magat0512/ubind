// <copyright file="ClaimFormDataUpdatedEvent.cs" company="uBind">
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
    /// Aggregate for quotes.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event raised when a claim's form data has been updated.
        /// </summary>
        public class ClaimFormDataUpdatedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimFormDataUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the claim aggregate.</param>
            /// <param name="claimId">The ID of the quote entity.</param>
            /// <param name="formData">Form data as JSON.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public ClaimFormDataUpdatedEvent(Guid tenantId, Guid aggregateId, Guid claimId, string formData, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.ClaimId = claimId;
                this.FormUpdateId = Guid.NewGuid();
                this.FormData = formData;
            }

            [JsonConstructor]
            private ClaimFormDataUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the claim.
            /// </summary>
            [JsonProperty]
            public Guid ClaimId { get; private set; }

            /// <summary>
            /// Gets an Id uniquely identifying the form update.
            /// </summary>
            [JsonProperty]
            public Guid FormUpdateId { get; private set; }

            /// <summary>
            /// Gets the updated form data.
            /// </summary>
            [JsonProperty]
            public string FormData { get; private set; }
        }
    }
}
