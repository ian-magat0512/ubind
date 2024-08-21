// <copyright file="AssociateClaimWithPolicyEvent.cs" company="uBind">
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
    /// Aggregate for associating claim with policy.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Associate Claim with Policy.
        /// </summary>
        public class AssociateClaimWithPolicyEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AssociateClaimWithPolicyEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="claimId">The claim Id.</param>
            /// <param name="quoteId">The quote Id.</param>
            /// <param name="policyId">The policy Id.</param>
            /// <param name="policyNumber">The policy number.</param>
            /// <param name="customerPersonId">The customer person Id.</param>
            /// <param name="customerId">The customer Id.</param>
            /// <param name="customerPreferredName">The customer preferred name.</param>
            /// <param name="customerFullName">The customer full name.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="timestamp">The time stamp when the event created.</param>
            public AssociateClaimWithPolicyEvent(
                Guid tenantId,
                Guid claimId,
                Guid? quoteId,
                Guid policyId,
                string policyNumber,
                Guid? customerPersonId,
                Guid? customerId,
                string customerPreferredName,
                string customerFullName,
                Guid? performingUserId,
                Instant timestamp)
                : base(tenantId, claimId, performingUserId, timestamp)
            {
                this.QuoteId = quoteId;
                this.PolicyNumber = policyNumber;
                this.PolicyId = policyId;
                this.CustomerPersonId = customerPersonId;
                this.CustomerId = customerId;
                this.CustomerPreferredName = customerPreferredName;
                this.CustomerFullName = customerFullName;
            }

            [JsonConstructor]
            private AssociateClaimWithPolicyEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the policy number of the claim.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            /// <summary>
            /// Gets the ID of the policy.
            /// </summary>
            [JsonProperty]
            public Guid PolicyId { get; private set; }

            /// <summary>
            /// Gets the ID of the quote of the pollcy.
            /// </summary>
            [JsonProperty]
            public Guid? QuoteId { get; private set; }

            /// <summary>
            /// Gets the Customer Id.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the Person Id.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerPersonId { get; private set; }

            /// <summary>
            /// Gets the customer preferred name.
            /// </summary>
            [JsonProperty]
            public string CustomerPreferredName { get; private set; }

            /// <summary>
            /// Gets the customer full name.
            /// </summary>
            [JsonProperty]
            public string CustomerFullName { get; private set; }
        }
    }
}
