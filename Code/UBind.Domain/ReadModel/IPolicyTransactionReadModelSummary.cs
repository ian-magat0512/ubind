// <copyright file="IPolicyTransactionReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using NodaTime;

    /// <summary>
    /// Policy transaction summary data.
    /// </summary>
    public interface IPolicyTransactionReadModelSummary : IBaseReportReadModel
    {
        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        string TransactionType { get; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        LocalDate CreatedDate { get; }

        /// <summary>
        /// Gets the created time.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the effective date time.
        /// For a new business or renewal transaction, this represents the policy period start date.
        /// </summary>
        LocalDateTime EffectiveDateTime { get; }

        /// <summary>
        /// Gets the policy inception date time
        /// This is only relevant for a new business transaction.
        /// </summary>
        LocalDateTime? InceptionDateTime { get; }

        /// <summary>
        /// Gets the adjustment effective date.
        /// </summary>
        LocalDateTime? AdjustmentEffectiveDateTime { get; }

        /// <summary>
        /// Gets the cancellation effective date.
        /// </summary>
        LocalDateTime? CancellationEffectiveDateTime { get; }

        /// <summary>
        /// Gets the epiry date time.
        /// This would only be relevant for a new business, renewal or adjustment transaction.
        /// </summary>
        LocalDateTime? ExpiryDateTime { get; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        string PolicyNumber { get; }

        /// <summary>
        /// Gets the quote reference.
        /// </summary>
        string QuoteReference { get; }

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        string InvoiceNumber { get; }

        /// <summary>
        /// Gets the credit note number.
        /// </summary>
        string CreditNoteNumber { get; }

        /// <summary>
        /// Gets the customer name.
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets the customer email.
        /// </summary>
        string CustomerEmail { get; }

        /// <summary>
        /// Gets a value indicating whether the data is for testing.
        /// </summary>
        bool IsTestData { get; }

        /// <summary>
        /// Gets the form data.
        /// </summary>
        string FormData { get; }

        /// <summary>
        /// Gets the calculation result.
        /// </summary>
        string CalculationResult { get; }

        /// <summary>
        /// Gets the Organisation name.
        /// </summary>
        string OrganisationName { get; }

        /// <summary>
        /// Gets the Organisation alias.
        /// </summary>
        string OrganisationAlias { get; }

        /// <summary>
        /// Gets the Agent name.
        /// </summary>
        string AgentName { get; }
    }
}
