// <copyright file="ConfiguredQuoteDatumLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies the location of quote data in quote form data and calculation result json.
    /// </summary>
    public class ConfiguredQuoteDatumLocations : IQuoteDatumLocations
    {
        // Prevent instantiation.
        [JsonConstructor]
        private ConfiguredQuoteDatumLocations()
        {
        }

        /// <summary>
        /// Gets the locator for the insured name.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? InsuredName { get; private set; }

        /// <summary>
        /// Gets the locator for the inception date.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? InceptionDate { get; private set; }

        /// <summary>
        /// Gets the locator for the expiry date.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ExpiryDate { get; private set; }

        /// <summary>
        /// Gets the locator for the effective date.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? EffectiveDate { get; private set; }

        /// <summary>
        /// Gets the locator for the cancellation date.
        /// </summary>
        [JsonProperty("cancellationDate")]
        public QuoteDatumLocation? CancellationEffectiveDate { get; private set; }

        /// <summary>
        /// Gets the locator for the total premium.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? TotalPremium { get; private set; }

        /// <summary>
        /// Gets the locator for the contact name.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactName { get; private set; }

        /// <summary>
        /// Gets the locator for the contact email.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactEmail { get; private set; }

        /// <summary>
        /// Gets the locator for the contact mobile.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactMobile { get; private set; }

        /// <summary>
        /// Gets the locator for the contact mobile.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactPhone { get; private set; }

        /// <summary>
        /// Gets the locator for the currency code.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? CurrencyCode { get; private set; }

        /// <summary>
        /// Gets the locator for the contract address first line.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactAddressLine1 { get; private set; }

        /// <summary>
        /// Gets the locator for the contract address suburb.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactAddressSuburb { get; private set; }

        /// <summary>
        /// Gets the locator for the contract address postcode.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactAddressPostcode { get; private set; }

        /// <summary>
        /// Gets the locator for the contract address state.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? ContactAddressState { get; private set; }

        /// <summary>
        /// Gets the locator for the trading name.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? TradingName { get; private set; }

        /// <summary>
        /// Gets the locator for the ABN.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? Abn { get; private set; }

        /// <summary>
        /// Gets the locator for number of installments.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? NumberOfInstallments { get; private set; }

        /// <summary>
        /// Gets the locator for question specifying whether quote is for a run-off policy.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? IsRunOffPolicy { get; private set; }

        /// <summary>
        /// Gets the locator for the business end date (for run-off policies)..
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? BusinessEndDate { get; private set; }

        /// <summary>
        /// Gets the locator for the quote title.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? QuoteTitle { get; private set; }

        /// <summary>
        /// Gets the locator for the is refund approved.
        /// </summary>
        [JsonProperty]
        public QuoteDatumLocation? IsRefundApproved { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation? SaveCustomerPaymentDetails { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation? UseSavedPaymentMethod { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation? PaymentMethod { get; private set; }

        [JsonProperty]
        public QuoteDatumLocation? DebitOption { get; private set; }
    }
}
