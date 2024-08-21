// <copyright file="PolicyHistorySetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Resource model for serving policy history sets to portal.
    /// </summary>
    public class PolicyHistorySetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyHistorySetModel"/> class.
        /// </summary>
        /// <param name="policyTransaction">The policy transaction.</param>
        /// <param name="areTimestampsAuthoritative">A value indicating whether to consider timestamps over datetimes.</param>
        /// <param name="timeZone">The time zone for this policy.</param>
        /// <param name="policyNumber">The policy number related to this transaction.</param>
        public PolicyHistorySetModel(
            PolicyTransaction policyTransaction,
            bool areTimestampsAuthoritative,
            DateTimeZone timeZone,
            string policyNumber = "")
        {
            this.Premium = new PremiumResult((policyTransaction as PolicyTransaction).PolicyData.CalculationResult.PayablePrice);
            this.TransactionId = policyTransaction.Id;
            this.PolicyId = policyTransaction.PolicyId;
            this.QuoteId = (policyTransaction as PolicyTransaction)?.QuoteId ?? default;
            this.ProductId = policyTransaction.ProductId;
            this.CreatedDateTime = policyTransaction.CreatedTimestamp.ToString();
            this.PolicyNumber = policyNumber;
            this.TransactionStatus = policyTransaction.GetTransactionStatus(areTimestampsAuthoritative, timeZone);
            this.EventTypeSummary = policyTransaction.GetEventTypeSummary();
            this.EffectiveDateTime = policyTransaction.EffectiveTimestamp.ToExtendedIso8601String();
            this.CancellationEffectiveDateTime = policyTransaction is CancellationTransaction
                ? policyTransaction.EffectiveTimestamp.ToExtendedIso8601String()
                : null;
            this.ExpiryDateTime = policyTransaction.ExpiryTimestamp?.ToExtendedIso8601String();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyHistorySetModel"/> class.
        /// </summary>
        public PolicyHistorySetModel()
        {
        }

        /// <summary>
        /// Gets the ID of the transaction.
        /// </summary>
        [JsonProperty]
        public Guid TransactionId { get; private set; }

        /// <summary>
        /// Gets the ID of the policy.
        /// </summary>
        [JsonProperty]
        public Guid PolicyId { get; private set; }

        [JsonProperty]
        public Guid ProductId { get; }

        /// <summary>
        /// Gets the premium of the policy.
        /// </summary>
        [JsonProperty]
        public PremiumResult Premium { get; private set; }

        /// <summary>
        /// Gets the policy number of the policy.
        /// </summary>
        [JsonProperty]
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the ID of the quote related to this policy transaction.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the event type of this policy transaction, e.g. Purchased, Renewed, Adjusted, Cancelled.
        /// </summary>
        [JsonProperty]
        public string EventTypeSummary { get; private set; }

        /// <summary>
        /// Gets the status of the transaction.
        /// </summary>
        [JsonProperty]
        public string TransactionStatus { get; private set; }

        /// <summary>
        /// Gets the date the policy is to take effect.
        /// </summary>
        [JsonProperty]
        public string EffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the expiry date the policy.
        /// </summary>
        [JsonProperty]
        public string ExpiryDateTime { get; private set; }

        /// <summary>
        /// Gets the Cancellation time.
        /// </summary>
        [JsonProperty]
        public string CancellationEffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the date the policy is to take effect.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }
    }
}
