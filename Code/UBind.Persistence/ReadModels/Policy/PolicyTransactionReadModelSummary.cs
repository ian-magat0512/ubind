// <copyright file="PolicyTransactionReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Policy transaction summary data.
    /// </summary>
    public class PolicyTransactionReadModelSummary : EntityReadModel<Guid>, IPolicyTransactionReadModelSummary
    {
        /// <summary>
        /// Gets or sets the business transaction.
        /// </summary>
        public PolicyTransaction Transaction { get; set; }

        /// <summary>
        /// Gets the form data.
        /// </summary>
        public string QuoteNumber => this.Transaction.QuoteNumber;

        /// <summary>
        /// Gets or sets the effective cancellation time in ticks, if cancelled, otherwise default.
        /// </summary>
        public long CancellationTicksSinceEpoch { get; set; }

        /// <inheritdoc/>
        public string TransactionType => this.Transaction is NewBusinessTransaction ? UBind.Domain.TransactionType.NewBusiness.ToString()
            : this.Transaction is RenewalTransaction ? UBind.Domain.TransactionType.Renewal.ToString()
                : this.Transaction is AdjustmentTransaction ? UBind.Domain.TransactionType.Adjustment.ToString()
                    : this.Transaction is CancellationTransaction ? UBind.Domain.TransactionType.Cancellation.ToString() : string.Empty;

        /// <inheritdoc/>
        public string ProductName { get; set; }

        /// <inheritdoc/>
        public LocalDate CreatedDate => this.CreatedTimestamp.ToLocalDateInAet();

        public LocalDateTime EffectiveDateTime => this.Transaction.EffectiveDateTime;

        public LocalDateTime? InceptionDateTime => this.Transaction is NewBusinessTransaction ? this.Transaction.EffectiveDateTime : (LocalDateTime?)null;

        public LocalDateTime? AdjustmentEffectiveDateTime => this.Transaction is AdjustmentTransaction ? this.Transaction.EffectiveDateTime : (LocalDateTime?)null;

        public LocalDateTime? CancellationEffectiveDateTime => this.Transaction is CancellationTransaction ? this.Transaction.EffectiveDateTime : (LocalDateTime?)null;

        public LocalDateTime? ExpiryDateTime => this.Transaction is CancellationTransaction ? this.Transaction.ExpiryDateTime : (LocalDateTime?)null;

        /// <inheritdoc/>
        public string PolicyNumber { get; set; }

        /// <inheritdoc/>
        public string QuoteReference => this.QuoteNumber;

        /// <inheritdoc/>
        public string InvoiceNumber { get; set; }

        /// <inheritdoc/>
        public string CreditNoteNumber { get; set; }

        /// <inheritdoc/>
        public string CustomerFullName { get; set; }

        /// <inheritdoc/>
        public string CustomerEmail { get; set; }

        /// <inheritdoc/>
        public string FormData => this.Transaction.PolicyData.FormData;

        /// <inheritdoc/>
        public string CalculationResult => this.Transaction.PolicyData.SerializedCalculationResult;

        /// <inheritdoc/>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets the latest calculation result.
        /// </summary>
        public CalculationResultReadModel LatestCalculationResult => new CalculationResultReadModel(this.SerializedLatestCalculationResult);

        /// <inheritdoc/>
        public string SerializedLatestCalculationResult { get; set; }

        /// <inheritdoc/>
        public string PaymentGateway { get; set; }

        /// <inheritdoc/>
        public string PaymentResponseJson { get; set; }

        /// <inheritdoc/>
        public string OrganisationName { get; set; }

        /// <inheritdoc/>
        public string OrganisationAlias { get; set; }

        /// <inheritdoc/>
        public string AgentName { get; set; }
    }
}
