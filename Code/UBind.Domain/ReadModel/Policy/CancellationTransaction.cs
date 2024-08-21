// <copyright file="CancellationTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Policy
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using NodaTime;

    /// <summary>
    /// Policy transaction for a cancellation.
    /// </summary>
    public class CancellationTransaction : PolicyTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">A unique identifier for the transaction.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="cancellationEffectiveDateTime">The cancellation time.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        /// <param name="quoteId">The ID of the quote related to the policy transaction.</param>
        /// <param name="quoteNumber">The quote number of the quote related to the policy transaction.</param>
        /// <param name="policyData">The policy data.</param>
        public CancellationTransaction(
            Guid tenantId,
            Guid transactionId,
            Guid policyId,
            Guid? quoteId,
            string quoteNumber,
            int eventSequenceNumber,
            LocalDateTime cancellationEffectiveDateTime,
            Instant cancellationEffectiveTimestamp,
            Instant createdTimestamp,
            PolicyTransactionData policyData,
            Guid? productReleaseId)
            : base(
                  tenantId,
                  transactionId,
                  policyId,
                  eventSequenceNumber,
                  cancellationEffectiveDateTime,
                  cancellationEffectiveTimestamp,
                  null,
                  null,
                  createdTimestamp,
                  quoteId,
                  quoteNumber,
                  policyData,
                  productReleaseId,
                  TransactionType.Cancellation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTransaction"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        protected CancellationTransaction()
            : base(default, default, default, default, default, default, default, default, default, default, default, default, default, TransactionType.Cancellation)
        {
        }

        /// <summary>
        /// Gets the policy cancellation date.
        /// </summary>
        [NotMapped]
        public LocalDateTime CancellationEffectiveDateTime => this.EffectiveDateTime;

        /// <inheritdoc/>
        public override string GetEventTypeSummary()
        {
            return "Cancelled";
        }
    }
}
