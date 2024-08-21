// <copyright file="DataLocators.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed for deserializing the data locators field in product.json.
    /// This is direct deserialization; each property in this class is mapped exactly as it is in the product.json > data locators.
    /// </summary>
    public class DataLocators
    {
        /// <summary>
        /// Gets or sets the locations of insured name.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? InsuredName { get; set; }

        /// <summary>
        /// Gets or sets the locations of customer name.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the locations of customer email.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the locations of customer mobile number.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? CustomerMobile { get; set; }

        /// <summary>
        /// Gets or sets the locations of customer phone number.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? CustomerPhone { get; set; }

        /// <summary>
        /// Gets or sets the locations of inception date.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? InceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the locations of effective date.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the locations of cancellation date.
        /// </summary>
        [JsonProperty("cancellationDate")]
        public List<DataLocation>? CancellationEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the locations of cancellation refund approved.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? IsRefundApproved { get; set; }

        /// <summary>
        /// Gets or sets the locations of expiry date.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the locations of currency code.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the locations of address line.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? ContactAddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the locations of address suburb.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? ContactAddressSuburb { get; set; }

        /// <summary>
        /// Gets or sets the locations of address state.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? ContactAddressState { get; set; }

        /// <summary>
        /// Gets or sets the locations of postal code.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? ContactAddressPostcode { get; set; }

        /// <summary>
        /// Gets or sets the locations of trading name.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? TradingName { get; set; }

        /// <summary>
        /// Gets or sets the locations of abn.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? Abn { get; set; }

        /// <summary>
        /// Gets or sets the locations of abn.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? NumberOfInstallments { get; set; }

        /// <summary>
        /// Gets or sets the locations of is run off policy.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? IsRunOffPolicy { get; set; }

        /// <summary>
        /// Gets or sets the locations of business end date.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? BusinessEndDate { get; set; }

        /// <summary>
        /// Gets or sets the locations of total premium.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? TotalPremium { get; set; }

        /// <summary>
        /// Gets or sets the locations of quote title.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? QuoteTitle { get; set; }

        /// <summary>
        /// Gets or sets the locations for retrieving answer to 'Save Customer Payment Detail?'.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? SaveCustomerPaymentDetails { get; set; }

        /// <summary>
        /// Gets or sets the locations of use saved payment method.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? UseSavedPaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the locations of payment method.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the locations of payment method.
        /// </summary>
        [JsonProperty]
        public List<DataLocation>? DebitOption { get; set; }
    }
}
