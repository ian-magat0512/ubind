// <copyright file="AdjustmentTransaction.cs" company="uBind">
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
    /// Policy transaction for a mid-term adjustment.
    /// </summary>
    public class AdjustmentTransaction : PolicyTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustmentTransaction"/> class.
        /// </summary>
        /// <param name="adjustmentTransactionid">A unique identifier for the adjustment transaction.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="quoteId">The ID of the quote related to the policy transaction.</param>
        /// <param name="quoteNumber">The quote number of the quote related to the policy transaction.</param>
        /// <param name="adjustmentEffectiveDateTime">The effective time that the adjustment should take effect.</param>
        /// <param name="expiryDateTime">The new date time that the policy should expire.</param>
        /// <param name="policyData">The adjusted policy data.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        public AdjustmentTransaction(
            Guid tenantId,
            Guid adjustmentTransactionid,
            Guid policyId,
            Guid? quoteId,
            string quoteNumber,
            int eventSequenceNumber,
            LocalDateTime adjustmentEffectiveDateTime,
            Instant adjustmentEffectiveTimestamp,
            LocalDateTime? expiryDateTime,
            Instant? expiryTimestamp,
            Instant createdTimestamp,
            PolicyTransactionData policyData,
            Guid? productReleaseId)
            : base(
                  tenantId,
                  adjustmentTransactionid,
                  policyId,
                  eventSequenceNumber,
                  adjustmentEffectiveDateTime,
                  adjustmentEffectiveTimestamp,
                  expiryDateTime,
                  expiryTimestamp,
                  createdTimestamp,
                  quoteId,
                  quoteNumber,
                  policyData,
                  productReleaseId,
                  TransactionType.Adjustment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustmentTransaction"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        protected AdjustmentTransaction()
            : base(default, default, default, default, default, default, default, default, default, default, default, default, default, TransactionType.Adjustment)
        {
        }

        /// <summary>
        /// Gets the adjustment date.
        /// </summary>
        [NotMapped]
        public LocalDateTime? AdjustmentEffectiveDateTime => this.EffectiveDateTime;

        /// <inheritdoc/>
        public override string GetEventTypeSummary()
        {
            return "Adjusted";
        }
    }
}
