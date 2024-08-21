// <copyright file="DefaultQuoteDatumLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600 // Elements should be documented

namespace UBind.Domain.Aggregates.Quote
{
    /// <summary>
    /// This represents the default locations for quote data, in the form of QuoteDataLocations.
    /// It is consumed by the StandardQuoteDataLocator.
    /// Note that further data locators can be used to resolve sets of quote data for specific purposes,
    /// and they will use a subset of the QuoteDataLocators here.
    /// </summary>
    public class DefaultQuoteDatumLocations : IQuoteDatumLocations
    {
        private static DefaultQuoteDatumLocations instance;

        private DefaultQuoteDatumLocations()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the default quote data locations.
        /// </summary>
        public static IQuoteDatumLocations Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultQuoteDatumLocations();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the InsuredName from form data.
        /// </summary>
        public QuoteDatumLocation InsuredName { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "insuredName");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the TotalPremium from form data.
        /// </summary>
        public QuoteDatumLocation TotalPremium { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.CalculationResult, "payment.total.premium");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactName from form data.
        /// </summary>
        public QuoteDatumLocation ContactName { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactName");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactEmail from form data.
        /// </summary>
        public QuoteDatumLocation ContactEmail { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactEmail");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactMobile from form data.
        /// </summary>
        public QuoteDatumLocation ContactMobile { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactMobile");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactPhone from form data.
        /// </summary>
        public QuoteDatumLocation ContactPhone { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactPhone");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the CurrencyCode from form data.
        /// </summary>
        public QuoteDatumLocation CurrencyCode { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.CalculationResult, "payment.currencyCode");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactAddressLine1 from form data.
        /// </summary>
        public QuoteDatumLocation ContactAddressLine1 { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactAddressLine1");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactAddressSuburb from form data.
        /// </summary>
        public QuoteDatumLocation ContactAddressSuburb { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactAddressSuburb");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactAddressState from form data.
        /// </summary>
        public QuoteDatumLocation ContactAddressState { get; }
        = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactAddressState");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the ContactAddressPostcode from form data.
        /// </summary>
        public QuoteDatumLocation ContactAddressPostcode { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "contactAddressPostcode");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the TradingName from form data.
        /// </summary>
        public QuoteDatumLocation TradingName { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "tradingName");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the AustralianBusinessNumber (ABN) from form data.
        /// </summary>
        public QuoteDatumLocation Abn { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "abn");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the NumberOfInstallments from form data.
        /// </summary>
        public QuoteDatumLocation NumberOfInstallments { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "numberOfInstallments");

        /// <summary>
        /// Gets a QuoteDataLocation to retrieve the RunOffQuestion from form data.
        /// </summary>
        public QuoteDatumLocation IsRunOffPolicy { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "runoffQuestion");

        public QuoteDatumLocation BusinessEndDate { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "businessEndDate");

        public QuoteDatumLocation CancellationEffectiveDate { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "cancellationDate");

        public QuoteDatumLocation EffectiveDate { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "effectiveDate");

        public QuoteDatumLocation ExpiryDate { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "expiryDate");

        public QuoteDatumLocation InceptionDate { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "inceptionDate");

        public QuoteDatumLocation QuoteTitle { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "quoteTitle");

        public QuoteDatumLocation IsRefundApproved { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.CalculationResult, "isRefundApproved");

        public QuoteDatumLocation SaveCustomerPaymentDetails { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "saveCustomerPaymentDetail");

        public QuoteDatumLocation UseSavedPaymentMethod { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "useSavedPaymentMethod");

        public QuoteDatumLocation PaymentMethod { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "paymentMethod");

        public QuoteDatumLocation DebitOption { get; }
            = new QuoteDatumLocation(QuoteDataLocationObject.FormData, "debitOption");
    }
}
