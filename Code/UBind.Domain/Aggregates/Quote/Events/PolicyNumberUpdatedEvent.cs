// <copyright file="PolicyNumberUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for policy.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Policy number event update.
        /// </summary>
        public class PolicyNumberUpdatedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid? quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyNumberUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="policyId">The ID of the policy.</param>
            /// <param name="policyNumber">The new number of the policy.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PolicyNumberUpdatedEvent(Guid tenantId, Guid policyId, Guid? quoteId, string policyNumber, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, policyId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.PolicyNumber = policyNumber;
            }

            [JsonConstructor]
            private PolicyNumberUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new number of the policy.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid? QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.quoteId.GetValueOrDefault() == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }
        }
    }
}
