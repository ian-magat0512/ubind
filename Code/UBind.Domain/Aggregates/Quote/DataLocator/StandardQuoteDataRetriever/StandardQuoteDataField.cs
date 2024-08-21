// <copyright file="StandardQuoteDataField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    /// <summary>
    /// Enumeration for all quote data.
    /// </summary>
    public enum StandardQuoteDataField
    {
        /// <summary>
        /// The insured name.
        /// </summary>
        InsuredName,

        /// <summary>
        /// The customer name.
        /// </summary>
        CustomerName,

        /// <summary>
        /// The customer email.
        /// </summary>
        CustomerEmail,

        /// <summary>
        /// The customer mobile.
        /// </summary>
        CustomerMobile,

        /// <summary>
        /// The customer phone.
        /// </summary>
        CustomerPhone,

        /// <summary>
        /// The inception date.
        /// </summary>
        InceptionDate,

        /// <summary>
        /// The inception time.
        /// </summary>
        InceptionTime,

        /// <summary>
        /// The effective date.
        /// </summary>
        EffectiveDate,

        /// <summary>
        /// The cancellation date.
        /// </summary>
        CancellationEffectiveDate,

        /// <summary>
        /// The is refund approved.
        /// </summary>
        IsRefundApproved,

        /// <summary>
        /// The expiry date.
        /// </summary>
        ExpiryDate,

        /// <summary>
        /// The expiry time.
        /// </summary>
        ExpiryTime,

        /// <summary>
        /// The currency code.
        /// </summary>
        CurrencyCode,

        /// <summary>
        /// The address.
        /// </summary>
        Address,

        /// <summary>
        /// The trading name.
        /// </summary>
        TradingName,

        /// <summary>
        /// The abn.
        /// </summary>
        Abn,

        /// <summary>
        /// The number of installments.
        /// </summary>
        NumberOfInstallments,

        /// <summary>
        /// The flag if the policy is run off.
        /// </summary>
        IsRunOffPolicy,

        /// <summary>
        /// The business end date.
        /// </summary>
        BusinessEndDate,

        /// <summary>
        /// The total premium.
        /// </summary>
        TotalPremium,

        /// <summary>
        /// The quote title.
        /// </summary>
        QuoteTitle,

        /// <summary>
        /// The save customer payment detail.
        /// </summary>
        SaveCustomerPaymentDetails,

        /// <summary>
        /// The use saved payment method.
        /// </summary>
        UseSavedPaymentMethod,

        /// <summary>
        /// The payment method.
        /// </summary>
        PaymentMethod,

        /// <summary>
        /// The debit option
        /// </summary>
        DebitOption,
    }
}
