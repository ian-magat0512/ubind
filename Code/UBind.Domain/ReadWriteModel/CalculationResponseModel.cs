// <copyright file="CalculationResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaMoney;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// A model for the caclculation result and related information which is used when returning a response to a calculation request.
    /// </summary>
    public class CalculationResponseModel
    {
        public CalculationResponseModel(
            CalculationResult calculationResult,
            NewQuoteReadModel? quote,
            FundingProposal? fundingProposal)
        {
            this.Quote = quote;
            this.CalculationResult = calculationResult.JObject;
            if (quote != null)
            {
                this.QuoteState = quote.QuoteState;
                this.QuoteId = quote.Id;
                this.QuoteNumber = quote.QuoteNumber;
            }

            var payablePrice = calculationResult.PayablePrice;
            var refundBreakdown = calculationResult.RefundBreakdown;

            string currencyCode = payablePrice?.CurrencyCode
                ?? refundBreakdown?.CurrencyCode
                ?? ReadWriteModel.PriceBreakdown.DefaultCurrencyCode;
            this.AmountPayable = new Money(payablePrice.TotalPayable, currencyCode).ToString();
            this.PriceBreakdown = new PriceBreakdownResponseModel(payablePrice);
            this.RefundBreakdown = new PriceBreakdownResponseModel(refundBreakdown);

            if (fundingProposal != null)
            {
                this.PremiumFundingProposal = new PremiumFundingProposalResponseModel(fundingProposal);
            }
        }

        [JsonIgnore]
        public NewQuoteReadModel Quote { get; }

        /// <summary>
        /// Gets the calculation result as a JObject.
        /// </summary>
        [JsonProperty]
        public JObject CalculationResult { get; }

        /// <summary>
        /// Gets the Application ID representing the calculation result.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the current state of a quote.
        /// </summary>
        [JsonProperty]
        public string QuoteState { get; }

        /// <summary>
        /// Gets the Quote number as reference.
        /// </summary>
        [JsonProperty]
        public string QuoteNumber { get; }

        /// <summary>
        /// Gets the amount payable for the quote.
        /// </summary>
        [JsonProperty]
        public string AmountPayable { get; }

        /// <summary>
        /// Gets the price breakdown for the quote.
        /// </summary>
        [JsonProperty]
        public PriceBreakdownResponseModel PriceBreakdown { get; }

        /// <summary>
        /// Gets the refund breakdown for the quote.
        /// </summary>
        [JsonProperty]
        public PriceBreakdownResponseModel RefundBreakdown { get; }

        /// <summary>
        /// Gets the details of the premium funding proposal for this calculation result, if one exists, otherwise null.
        /// </summary>
        [JsonProperty]
        public PremiumFundingProposalResponseModel PremiumFundingProposal { get; }
    }
}
