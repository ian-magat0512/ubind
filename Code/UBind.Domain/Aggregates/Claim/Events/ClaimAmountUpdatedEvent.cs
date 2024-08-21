// <copyright file="ClaimAmountUpdatedEvent.cs" company="uBind">
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
    /// Aggregate for claim.
    /// </summary>
    public partial class ClaimAggregate
    {
        private void Apply(ClaimAmountUpdatedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        /// <summary>
        /// An event for updating claim amount.
        /// </summary>
        public class ClaimAmountUpdatedEvent
            : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimAmountUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The ID of the claim.</param>
            /// <param name="amount">The new amount of the claim.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimAmountUpdatedEvent(Guid tenantId, Guid claimId, decimal? amount, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.Amount = amount;
            }

            [JsonConstructor]
            private ClaimAmountUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new status of the claim.
            /// </summary>
            [JsonProperty]
            public decimal? Amount { get; private set; }
        }
    }
}
