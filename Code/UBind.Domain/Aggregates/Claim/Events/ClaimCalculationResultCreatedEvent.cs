// <copyright file="ClaimCalculationResultCreatedEvent.cs" company="uBind">
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
        /// Event raised when a quote has been created.
        /// </summary>
        public class ClaimCalculationResultCreatedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimCalculationResultCreatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the quote.</param>
            /// <param name="calculationResult">The calculation result.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public ClaimCalculationResultCreatedEvent(Guid tenantId, Guid claimId, ClaimCalculationResult calculationResult, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.CalculationResultId = Guid.NewGuid();
                this.CalculationResult = calculationResult;
            }

            [JsonConstructor]
            private ClaimCalculationResultCreatedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the calculation result.
            /// </summary>
            [JsonProperty]
            public Guid CalculationResultId { get; private set; }

            /// <summary>
            /// Gets an Id uniquely identifying the form update used to create the calculation result.
            /// </summary>
            [JsonProperty]
            public ClaimCalculationResult CalculationResult { get; private set; }
        }
    }
}
