// <copyright file="PremiumFundingProposalResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;

    /// <summary>
    /// Model for payments breakdown when paying in installments.
    /// </summary>
    public class PremiumFundingProposalResponseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingProposalResponseModel"/> class.
        /// </summary>
        /// <param name="fundingProposal">The premium funding proposal to model.</param>
        public PremiumFundingProposalResponseModel(FundingProposal fundingProposal)
        {
            var fundingBreakdown = fundingProposal.PaymentBreakdown;
            var instalmentMerchantFeeRate = fundingBreakdown.InstalmentMerchantFeeMultiplier;
            var interestRate = fundingBreakdown.InterestRate;

            var fundingProposalAmounts = new PremiumFundingCalculation(
              fundingBreakdown.NumberOfInstalments,
              fundingProposal.PaymentBreakdown.AmountFunded,
              fundingBreakdown.InitialInstalmentAmount,
              fundingBreakdown.RegularInstalmentAmount,
              instalmentMerchantFeeRate,
              interestRate);

            this.ProposalId = fundingProposal.InternalId;
            this.ExternalProposalId = fundingProposal.ExternalId;
            this.NumberOfInstalments = fundingBreakdown.NumberOfInstalments;
            this.PaymentFrequency = fundingBreakdown.PaymentFrequency.ToString();

            this.InitialInstalmentAmount = Math.Round(fundingProposalAmounts.InitialInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();
            this.RegularInstalmentAmount = Math.Round(fundingProposalAmounts.RegularInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();
            this.InstalmentMerchantFeeMultiplier = fundingBreakdown.InstalmentMerchantFeeMultiplier;
            this.Total = fundingProposal.PaymentBreakdown.AmountFunded.ToDollarsAndCents();
            this.TotalInstalmentAmount = Math.Round(fundingProposalAmounts.TotalInstalment, 2, MidpointRounding.AwayFromZero).ToDollarsAndCents();
            this.InterestRate = fundingBreakdown.InterestRate;
            this.Interest = fundingBreakdown.Interest.ToDollarsAndCents();

            this.AcceptanceUrl = fundingProposal.AcceptanceUrl;
            this.ContractUrl = fundingProposal.ContractUrl;
            if (fundingProposal.ProposalResponse != null)
            {
                var proposalResponse = JObject.Parse(fundingProposal.ProposalResponse);
                var pdfUrl = proposalResponse?.Value<JObject>("data")?.Value<JObject>("links")?.GetValue("PdfURL");
                var uri = pdfUrl?.ToString().Split('/');
                var contractId = uri != null && uri.Length >= 2 ? uri[2] : null;
                var pdfKey = uri != null && uri.Length >= 4 ? uri[4] : null;
                this.Data = new JObject();
                this.Data.Add(new JProperty("pdfKey", pdfKey));
                this.Data.Add(new JProperty("contractId", contractId));
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
        public JObject Data { get; }

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
        [JsonProperty]
        public int NumberOfInstalments { get; }

        /// <summary>
        /// Gets the initial installment amount.
        /// </summary>
        [JsonProperty]
        public string InitialInstalmentAmount { get; }

        /// <summary>
        /// Gets the regular installment amount.
        /// </summary>
        [JsonProperty]
        public string TotalInitialInstalmentAmount { get; }

        /// <summary>
        /// Gets the regular installment amount.
        /// </summary>
        [JsonProperty]
        public string RegularInstalmentAmount { get; }

        /// <summary>
        /// Gets the total installment amount.
        /// </summary>
        [JsonProperty]
        public string TotalInstalmentAmount { get; }

        /// <summary>
        /// Gets the installment merchant fee multiplier (eg. 0.0056)
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
        /// Gets the acceptance url for the funding proposal, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public string? AcceptanceUrl { get; }

        /// <summary>
        /// Gets the contract url for the funding proposal contract file, if any, otherwise null.
        /// This contract url should not need any authentication for the file to be downloaded.
        /// </summary>
        [JsonProperty]
        public string? ContractUrl { get; }
    }
}
