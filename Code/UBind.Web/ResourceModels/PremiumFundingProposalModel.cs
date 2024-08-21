// <copyright file="PremiumFundingProposalModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for payments breakdown when paying in installments.
    /// </summary>
    public class PremiumFundingProposalModel
    {
        public PremiumFundingProposalModel(NewQuoteReadModel quote)
        {
            var instalmentMerchantFeeRate = quote.FundingInstalmentMerchantFeeMultiplier ?? 0;
            var interestRate = quote.FundingInstalmentInterestRate ?? 0;

            var fundingProposalAmounts = new PremiumFundingCalculation(
                quote.FundingNumberOfInstallments.Value,
                quote.AmountFunded ?? 0,
                quote.FundingRegularInstalmentAmount ?? 0,
                quote.FundingInitialInstalmentAmount ?? 0,
                instalmentMerchantFeeRate,
                interestRate);

            this.ProposalId = quote.FundingInternalId.Value;
            this.ExternalProposalId = quote.FundingId;
            this.NumberOfInstalments = quote.FundingNumberOfInstallments.Value;
            this.PaymentFrequency = quote.FundingPaymentFrequency.ToString();

            this.InitialInstalmentAmount = Math.Round(fundingProposalAmounts.InitialInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();
            this.RegularInstalmentAmount = Math.Round(fundingProposalAmounts.RegularInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();
            this.InstalmentMerchantFeeMultiplier = instalmentMerchantFeeRate;

            this.Total = quote.AmountFunded.ToDollarsAndCents();
            this.TotalInstalmentAmount = Math.Round(fundingProposalAmounts.TotalInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();

            this.InterestRate = interestRate;
            this.Interest = fundingProposalAmounts.Interest.ToDollarsAndCents();

            this.AcceptanceUrl = quote.FundingAcceptanceUrl;
            this.ContractUrl = quote.FundingContractUrl;
            if (quote.FundingProposalResponseJson != null)
            {
                this.UpdateDataFromProposalResponse(quote.FundingProposalResponseJson);
            }
        }

        /// <summary>
        /// Gets the ID used to represent this funding proposal version internally.
        /// </summary>
        [JsonProperty]
        public Guid ProposalId { get; }

        /// <summary>
        /// Gets the external proposal ID used by the funding provider.
        /// </summary>
        [JsonProperty]
        public string? ExternalProposalId { get; }

        /// <summary>
        /// Gets the premium funding Data.
        /// </summary>
        [JsonProperty]
        public JObject Data { get; private set; }

        /// <summary>
        /// Gets the amount funded.
        /// </summary>
        [JsonProperty]
        public string Total { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        [JsonProperty]
        public string PaymentFrequency { get; }

        /// <summary>
        /// Gets the number of installments.
        /// </summary>
        [Obsolete("Misspelled")]
        [JsonProperty]
        public int NumberOfInstallments => this.NumberOfInstalments;

        /// <summary>
        /// Gets the number of installments.
        /// </summary>
        [JsonProperty]
        public int NumberOfInstalments { get; }

        /// <summary>
        /// Gets the initial installment amount.
        /// </summary>
        [Obsolete("Misspelled")]
        [JsonProperty]
        public string InitialInstallmentAmount => this.InitialInstalmentAmount;

        /// <summary>
        /// Gets the initial installment amount.
        /// </summary>
        [JsonProperty]
        public string InitialInstalmentAmount { get; }

        /// <summary>
        /// Gets the regular installment amount.
        /// </summary>
        [Obsolete("Misspelled")]
        [JsonProperty]
        public string RegularInstallmentAmount => this.RegularInstalmentAmount;

        /// <summary>
        /// Gets the regular installment amount.
        /// </summary>
        [JsonProperty]
        public string RegularInstalmentAmount { get; }

        /// <summary>
        /// Gets the acceptance url for the funding proposal, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public string AcceptanceUrl { get; }

        /// <summary>
        /// Gets the contract url for the funding proposal contract file, if any, otherwise null.
        /// This contract url should not need any authentication for the file to be downloaded.
        /// </summary>
        [JsonProperty]
        public string? ContractUrl { get; }

        /// <summary>
        /// Gets the instalment merchant fee multiplier (eg. 0.005).
        /// </summary>
        [JsonProperty]
        public decimal InstalmentMerchantFeeMultiplier { get; }

        /// <summary>
        /// This is the interest amount applied by the funding provider
        /// </summary>
        [JsonProperty]
        public string Interest { get; }

        /// <summary>
        /// This is the interest rate used by the funding provider (0.154803)
        /// </summary>
        [JsonProperty]
        public decimal InterestRate { get; }

        /// <summary>
        /// This is the total cost the customer is going to incur during the whole instalment period.
        /// It includes the instalment amount multiplied by the number of instalment,
        /// any additional amount added in the initial instalment and the merchant fee.
        /// </summary>
        [JsonProperty]
        public string TotalInstalmentAmount { get; }

        private void UpdateDataFromProposalResponse(string proposalResponseJson)
        {
            var proposalResponse = JObject.Parse(proposalResponseJson);
            var pdfUrl = proposalResponse?.Value<JObject>("data")?.Value<JObject>("links")?.GetValue("PdfURL");
            if (pdfUrl == null)
            {
                return;
            }

            var uri = pdfUrl?.ToString().Split('/');
            var contractId = uri != null && uri.Length >= 2 ? uri[2] : null;
            var pdfKey = uri != null && uri.Length >= 4 ? uri[4] : null;
            this.Data = new JObject
            {
                new JProperty("pdfKey", pdfKey),
                new JProperty("contractId", contractId),
            };
        }
    }
}
