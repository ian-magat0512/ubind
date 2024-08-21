// <copyright file="CalculatedPaymentTotal.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Total premium information from calculation result.
    /// </summary>
    public class CalculatedPaymentTotal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPaymentTotal"/> class.
        /// </summary>
        /// <param name="premium">The total premium.</param>
        /// <param name="esl">The total Emergency Services Levy.</param>
        /// <param name="gst">The total Government Sales Tax.</param>
        /// <param name="stampDuty">The total stamp duty.</param>
        /// <param name="serviceFees">The total service fees.</param>
        /// <param name="interest">The total interest.</param>
        /// <param name="merchantFees">The total merchant fees.</param>
        /// <param name="transactionCosts">The total transaction costs.</param>
        /// <param name="payable">The total payable.</param>
        /// <param name="currencyCode">Currency Code.</param>
        public CalculatedPaymentTotal(
            decimal premium,
            decimal esl,
            decimal gst,
            decimal stampDuty,
            decimal serviceFees,
            decimal interest,
            decimal merchantFees,
            decimal transactionCosts,
            decimal payable,
            string currencyCode)
        {
            if (currencyCode == null)
            {
                throw new NullReferenceException("When instantiating CalculatedPaymentTotal, the passed parameter for currencyCode was null.");
            }

            this.Premium = premium;
            this.Esl = esl;
            this.Gst = gst;
            this.StampDuty = stampDuty;
            this.ServiceFees = serviceFees;
            this.Interest = interest;
            this.MerchantFees = merchantFees;
            this.TransactionCosts = transactionCosts;
            this.Payable = payable;
            this.CurrencyCode = currencyCode;
        }

        [JsonConstructor]
        private CalculatedPaymentTotal()
        {
        }

        /// <summary>
        /// Gets the currency code, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode { get; private set; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the total premium.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Premium { get; private set; }

        /// <summary>
        /// Gets the total ESL.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Esl { get; private set; }

        /// <summary>
        /// Gets the total GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Gst { get; private set; }

        /// <summary>
        /// Gets the total stamp duty.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDuty { get; private set; }

        /// <summary>
        /// Gets the total service fees.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal ServiceFees { get; private set; }

        /// <summary>
        /// Gets the total interest.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Interest { get; private set; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal MerchantFees { get; private set; }

        /// <summary>
        /// Gets the total transaction costs.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TransactionCosts { get; private set; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Payable { get; private set; }
    }
}
