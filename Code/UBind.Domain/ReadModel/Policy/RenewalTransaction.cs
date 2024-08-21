// <copyright file="RenewalTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Policy
{
    using System;
    using NodaTime;

    /// <summary>
    /// Policy transaction for a renewal.
    /// </summary>
    public class RenewalTransaction : PolicyTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenewalTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">A unique identifier for the transaction.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="quoteId">The ID of the quote related to the policy transaction.</param>
        /// <param name="quoteNumber">The quote number of the quote for the renewal.</param>
        /// <param name="policyData">The renewal policy data.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created this transaction.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        public RenewalTransaction(
            Guid tenantId,
            Guid transactionId,
            Guid policyId,
            Guid? quoteId,
            string quoteNumber,
            int eventSequenceNumber,
            LocalDateTime effectiveDateTime,
            Instant effectiveTimestamp,
            LocalDateTime expiryDateTime,
            Instant expiryTimestamp,
            Instant createdTimestamp,
            PolicyTransactionData policyData,
            Guid? productReleaseId)
            : base(
                  tenantId,
                  transactionId,
                  policyId,
                  eventSequenceNumber,
                  effectiveDateTime,
                  effectiveTimestamp,
                  expiryDateTime,
                  expiryTimestamp,
                  createdTimestamp,
                  quoteId,
                  quoteNumber,
                  policyData,
                  productReleaseId,
                  TransactionType.Renewal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewalTransaction"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        protected RenewalTransaction()
            : base(default, default, default, default, default, default, default, default, default, default, default, default, default, TransactionType.Renewal)
        {
        }

        /// <inheritdoc/>
        public override string GetEventTypeSummary()
        {
            return "Renewed";
        }
    }
}
