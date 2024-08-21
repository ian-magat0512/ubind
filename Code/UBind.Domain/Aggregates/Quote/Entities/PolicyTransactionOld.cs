// <copyright file="PolicyTransactionOld.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Entities
{
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// This class is required for backwards compatibility.
    /// The FixNullQuoteAggregateForPolicyCommandHandler creates a special event called SetPolicyTransactionsEvent,
    /// where it sets a list of PolicyTransaction, which is serialised to the EventJson.
    /// We've since refactored PolicyTransaction to be an abstract class, so it can no longer be deserialised.
    /// Instead, we created have this class to allow the deserialisation.
    /// </summary>
    public class PolicyTransactionOld : PolicyTransaction
    {
        /// <summary>
        /// Gets the value of the time stamp it was created.
        /// </summary>
        [JsonProperty]
        public Instant CreationTime
        {
            get => this.CreatedTimestamp;
            private set => this.CreatedTimestamp = value;
        }

        /// <summary>
        /// Gets the policy data for the transaction.
        /// </summary>
        [JsonProperty]
        public PolicyData PolicyData { get; private set; }

        [System.Obsolete]
        public new LocalDateTime EffectiveDateTime => this.PolicyData.EffectiveDate.AtStartOfDayInZone(Timezones.AET).LocalDateTime;

        /// <summary>
        /// Gets the effective timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        [System.Obsolete]
        public new Instant EffectiveTimestamp => this.PolicyData.EffectiveTime;

        /// <summary>
        /// Gets or the expiry date time of the policy.
        /// Only valid for new business, renewal and adjustment transactions.
        /// For policies which are not date based, this would be null.
        /// </summary>
        [System.Obsolete]
        public new LocalDateTime? ExpiryDateTime => this.PolicyData.ExpiryDate?.AtStartOfDayInZone(Timezones.AET).LocalDateTime;

        /// <summary>
        /// Gets the expiry timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        [System.Obsolete]
        public new Instant? ExpiryTimestamp => this.PolicyData.ExpiryTime;

        public override TransactionType Type => this.Type;
    }
}
