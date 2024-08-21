// <copyright file="QuoteCalculationResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaMoney;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Model for exposing calculation results to client application.
    /// </summary>
    public class QuoteCalculationResultModel
    {
        public QuoteCalculationResultModel(NewQuoteReadModel quote)
        {
            var calculationResult = quote.LatestCalculationResult;
            this.CalculationResultId = quote.LatestCalculationResultId;
            this.CalculationResult = calculationResult?.JObject;
            this.QuoteState = quote.QuoteState.ToLower();
            this.QuoteId = quote.Id;

            var payablePrice = calculationResult.PayablePrice;
            var refundBreakdown = calculationResult.RefundBreakdown;

            string currencyCode = payablePrice?.CurrencyCode
                ?? refundBreakdown?.CurrencyCode
                ?? Domain.ReadWriteModel.PriceBreakdown.DefaultCurrencyCode;
            this.AmountPayable = new Money(payablePrice.TotalPayable, currencyCode).ToString();
            this.PriceBreakdown = new PriceBreakdownModel(payablePrice);
            this.RefundBreakdown = new PriceBreakdownModel(refundBreakdown);
            this.QuoteNumber = quote.QuoteNumber;

            if (quote.FundingInternalId != null)
            {
                this.PremiumFundingProposal = new PremiumFundingProposalModel(quote);
            }
        }

        /// <summary>
        /// Gets an ID uniquely identifying this calculation result.
        /// </summary>
        public Guid CalculationResultId { get; }

        /// <summary>
        /// Gets the calculation result as a JObject.
        /// </summary>
        public JObject CalculationResult { get; }

        /// <summary>
        /// Gets the Application ID representing the calculation result.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the current state of a quote.
        /// </summary>
        public string QuoteState { get; }

        /// <summary>
        /// Gets the Quote number as reference.
        /// </summary>
        public string QuoteNumber { get; }

        /// <summary>
        /// Gets the amount payable for the quote.
        /// </summary>
        public string AmountPayable { get; }

        /// <summary>
        /// Gets the price breakdown for the quote.
        /// </summary>
        public PriceBreakdownModel PriceBreakdown { get; }

        /// <summary>
        /// Gets the refund breakdown for the quote.
        /// </summary>
        public PriceBreakdownModel RefundBreakdown { get; }

        /// <summary>
        /// Gets the details of the premium funding proposal for this calculation result, if one exists, otherwise null.
        /// </summary>
        [JsonProperty]
        public PremiumFundingProposalModel PremiumFundingProposal { get; }
    }
}
