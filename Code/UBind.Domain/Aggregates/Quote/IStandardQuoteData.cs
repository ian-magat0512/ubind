// <copyright file="IStandardQuoteData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using NodaTime;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Interface for accessing a set of standard quote data used in common places throughout.
    /// </summary>
    public interface IStandardQuoteData
    {
        /// <summary>
        /// Gets the name of the insured person.
        /// </summary>
        string InsuredName { get; }

        /// <summary>
        /// Gets the inception date.
        /// </summary>
        LocalDate? InceptionDate { get; }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        LocalDate? ExpiryDate { get; }

        /// <summary>
        /// Gets the effective date (for adjustments).
        /// </summary>
        LocalDate? EffectiveDate { get; }

        /// <summary>
        /// Gets the cancellation date (for cancellation).
        /// </summary>
        LocalDate? CancellationEffectiveDate { get; }

        /// <summary>
        /// Gets the inception time.
        /// </summary>
        Instant? InceptionTimestamp { get; }

        /// <summary>
        /// Gets the expiry time.
        /// </summary>
        Instant? ExpiryTimestamp { get; }

        /// <summary>
        /// Gets the total premium including all taxes and fees (excluding payment or funding fees).
        /// </summary>
        decimal? TotalPremium { get; }

        /// <summary>
        /// Gets the currency code, e.g. "AUD", "PGK".
        /// </summary>
        string CurrencyCode { get; }

        /// <summary>
        /// Gets the contact address to use for this quote (for payment etc.).
        /// </summary>
        Address ContactAddress { get; }

        /// <summary>
        /// Gets the trading name of the insured company, if applicable, otherwise null.
        /// </summary>
        string TradingName { get; }

        /// <summary>
        /// Gets the Australian Business Number (ABN) of the insured company, if applicable, otherwise null.
        /// </summary>
        string Abn { get; }

        /// <summary>
        /// Gets the number of installments, otherwise null.
        /// </summary>
        int? NumberOfInstallments { get; }

        /// <summary>
        /// Gets a value indicating whether the quote is for a run-off policy.
        /// </summary>
        bool IsRunOffPolicy { get; }

        /// <summary>
        /// Gets the business end date for run off policies, if known, otherwise null.
        /// </summary>
        LocalDate? BusinessEndDate { get; }

        /// <summary>
        /// Gets the quote title, if known, otherwise null.
        /// </summary>
        string QuoteTitle { get; }
    }
}
