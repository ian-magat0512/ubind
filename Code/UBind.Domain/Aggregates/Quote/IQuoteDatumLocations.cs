// <copyright file="IQuoteDatumLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote
{
    /// <summary>
    /// Represents the full list of locations of quote data that may be used by the system,
    /// whereby each location is in the form of a IQuoteDataLocator which can be resolved to
    /// retrieve the value.
    /// </summary>
    public interface IQuoteDatumLocations
    {
        QuoteDatumLocation Abn { get; }

        QuoteDatumLocation BusinessEndDate { get; }

        QuoteDatumLocation CancellationEffectiveDate { get; }

        QuoteDatumLocation ContactAddressLine1 { get; }

        QuoteDatumLocation ContactAddressPostcode { get; }

        QuoteDatumLocation ContactAddressState { get; }

        QuoteDatumLocation ContactAddressSuburb { get; }

        QuoteDatumLocation ContactEmail { get; }

        QuoteDatumLocation ContactMobile { get; }

        QuoteDatumLocation ContactName { get; }

        QuoteDatumLocation ContactPhone { get; }

        QuoteDatumLocation CurrencyCode { get; }

        QuoteDatumLocation EffectiveDate { get; }

        QuoteDatumLocation ExpiryDate { get; }

        QuoteDatumLocation InceptionDate { get; }

        QuoteDatumLocation InsuredName { get; }

        QuoteDatumLocation IsRunOffPolicy { get; }

        QuoteDatumLocation NumberOfInstallments { get; }

        QuoteDatumLocation TotalPremium { get; }

        QuoteDatumLocation TradingName { get; }

        QuoteDatumLocation QuoteTitle { get; }

        QuoteDatumLocation IsRefundApproved { get; }

        QuoteDatumLocation SaveCustomerPaymentDetails { get; }

        QuoteDatumLocation UseSavedPaymentMethod { get; }

        QuoteDatumLocation PaymentMethod { get; }

        QuoteDatumLocation DebitOption { get; }
    }
}
