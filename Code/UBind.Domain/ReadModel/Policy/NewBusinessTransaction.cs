// <copyright file="NewBusinessTransaction.cs" company="uBind">
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
    /// Transaction for the creation of a new policy.
    /// </summary>
    public class NewBusinessTransaction : PolicyTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewBusinessTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">A unique identifier for the transaction.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="quoteId">The ID of the quote for the policy.</param>
        /// <param name="quoteNumber">The quote number of the quote for the policy.</param>
        /// <param name="policyData">The policy data.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="createdTimestamp">The timestamp of the transaction.</param>
        public NewBusinessTransaction(
            Guid tenantId,
            Guid transactionId,
            Guid policyId,
            Guid? quoteId,
            string quoteNumber,
            int eventSequenceNumber,
            LocalDateTime inceptionDateTime,
            Instant inceptionTimestamp,
            LocalDateTime? expiryDateTime,
            Instant? expiryTimestamp,
            Instant createdTimestamp,
            PolicyTransactionData policyData,
            Guid? productReleaseId)
            : base(
                  tenantId,
                  transactionId,
                  policyId,
                  eventSequenceNumber,
                  inceptionDateTime,
                  inceptionTimestamp,
                  expiryDateTime,
                  expiryTimestamp,
                  createdTimestamp,
                  quoteId,
                  quoteNumber,
                  policyData,
                  productReleaseId,
                  TransactionType.NewBusiness)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewBusinessTransaction"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        protected NewBusinessTransaction()
            : base(default, default, default, default, default, default, default, default, default, default, default, default, default, TransactionType.NewBusiness)
        {
        }

        /// <summary>
        /// Gets the adjustment date.
        /// </summary>
        [NotMapped]
        public LocalDateTime InceptionDateTime => this.EffectiveDateTime;

        [NotMapped]
        public Instant InceptionTimestamp => this.EffectiveTimestamp;

        /// <inheritdoc/>
        public override string GetEventTypeSummary()
        {
            return "Purchased";
        }
    }
}
