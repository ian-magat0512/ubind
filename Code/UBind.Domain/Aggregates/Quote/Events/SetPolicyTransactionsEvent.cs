// <copyright file="SetPolicyTransactionsEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Entities;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised When setting policy transaction to the aggregate.
        /// </summary>
        public class SetPolicyTransactionsEvent : Event<QuoteAggregate, Guid>
        {
            public SetPolicyTransactionsEvent(
                Guid tenantId,
                Guid aggregateId,
                List<PolicyTransactionOld> policyTransactions,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.PolicyTransactions = policyTransactions;
            }

            [JsonConstructor]
            private SetPolicyTransactionsEvent()
            {
            }

            /// <summary>
            /// Gets the policy transactions.
            /// </summary>
            [JsonProperty]
            public List<PolicyTransactionOld> PolicyTransactions { get; private set; }
        }
    }
}
