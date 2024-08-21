// <copyright file="GeneralData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;

    /// <summary>
    /// General data for IQumulate requests.
    /// </summary>
    public class GeneralData
    {
        /// <summary>
        /// Gets or sets the region.
        /// string (maxlen=2)
        /// (optional) Either ‘AU’ for Australia or ‘NZ’ for New Zealand. If left blank, ‘AU’ will be used.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the first instalment date.
        /// date (format yyyy-mm-dd)
        /// (optional) Date first payment will be made on the loan. If not supplied, it will default to the current day.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstInstalmentDate { get; set; }

        /// <summary>
        /// Gets or sets the payment frequency
        /// string (maxlen=1)
        /// (optional) Frequency of the instalments.Valid options are:
        /// • M - Monthly
        /// • Q - Quarterly
        /// Note: If not supplied, default for Introducer will be used.Only options that are setup for the requesting Introducer can be used here.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentFrequency { get; set; }

        /// <summary>
        /// Gets or sets the number of instalments the client must pay.
        /// (optional) Total number of instalments the client must pay.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfInstalments { get; set; }

        /// <summary>
        /// Gets or sets the commission rate.
        /// (optional) Commission rate(percent) to be added to loan amount.
        /// Example: 0.022=2.2% commission, 2.20=220%
        /// Note: If not supplied, default for Introducer will be used.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? CommissionRate { get; set; }

        /// <summary>
        /// Gets or sets the number of days after loan approval that the premium will be settled.
        /// (optional) Number of days after loan approval that the premium will be settled to broker/underwriter.
        /// Note: If not supplied, default for Introducer will be used.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SettlementDays { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// string (maxlen=6)
        /// Determines what payment methods are available for instalments. Valid options are:
        /// • ‘CC’ allows credit card only
        /// • ‘CCDD’ allows the Client to choose credit card for both initial/ongoing instalments OR credit card for the initial instalment followed by the direct debit of a bank account for ongoing instalments.
        /// • ‘either’ allows either credit card or direct debit of a bank account for initial and ongoing instalments.
        /// </summary>
        [JsonProperty]
        public string PaymentMethod { get; set; }
    }
}
