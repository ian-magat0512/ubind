// <copyright file="FundingProposal.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The output from a funding proposal creation.
    /// </summary>
    public class FundingProposal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposal"/> class.
        /// </summary>
        /// <param name="externalId">The value used by the external funding service to identify this proposal.</param>
        /// <param name="paymentBreakdown">Payment breakdown for the funding repayments.</param>
        /// <param name="proposalData">The serialized proposal data.</param>
        /// <param name="proposalResponse">The serialized proposal response.</param>
        /// <param name="arePlaceholdersUsed">A value indicating whether placeholder data are used for the proposal.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        [JsonConstructor]
        public FundingProposal(
            Guid internalId,
            string externalId,
            FundingProposalPaymentBreakdown paymentBreakdown,
            string proposalData,
            string proposalResponse,
            bool arePlaceholdersUsed,
            string acceptanceUrl)
        {
            this.InternalId = internalId;
            this.ExternalId = externalId;
            this.PaymentBreakdown = paymentBreakdown;
            this.ProposalData = proposalData;
            this.ArePlaceholdersUsed = arePlaceholdersUsed;
            this.ProposalResponse = proposalResponse;
            this.AcceptanceUrl = acceptanceUrl;
        }

        public FundingProposal(
            string? externalId,
            FundingProposalPaymentBreakdown paymentBreakdown,
            string proposalData,
            string proposalResponse,
            bool arePlaceholdersUsed)
        {
            this.InternalId = Guid.NewGuid();
            this.ExternalId = externalId;
            this.PaymentBreakdown = paymentBreakdown;
            this.ProposalData = proposalData;
            this.ArePlaceholdersUsed = arePlaceholdersUsed;
            this.ProposalResponse = proposalResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposal"/> class.
        /// </summary>
        /// <param name="externalId">The value used by the external funding service to identify this proposal.</param>
        /// <param name="paymentBreakdown">Payment breakdown for the funding repayments.</param>
        /// <param name="acceptanceUrl">An external URL that can be used to accept the proposal.</param>
        /// <param name="proposalData">The serialized proposal data.</param>
        /// <param name="proposalResponse">The serialized proposal response.</param>
        /// <param name="arePlaceholdersUsed">Indicates whether placeholder data was used in the proposal.</param>
        public FundingProposal(
            string? externalId,
            FundingProposalPaymentBreakdown paymentBreakdown,
            string? acceptanceUrl,
            string proposalData,
            string proposalResponse,
            bool arePlaceholdersUsed)
            : this(externalId, paymentBreakdown, proposalData, proposalResponse, arePlaceholdersUsed)
        {
            this.AcceptanceUrl = acceptanceUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposal"/> class.
        /// </summary>
        /// <param name="externalId">The value used by the external funding service to identify this proposal.</param>
        /// <param name="paymentBreakdown">Payment breakdown for the funding repayments.</param>
        /// <param name="acceptanceUrl">An external URL that can be used to accept the proposal.</param>
        /// <param name="proposalData">The serialized proposal data.</param>
        /// <param name="proposalResponse">The serialized proposal response.</param>
        /// <param name="arePlaceholdersUsed">Indicates whether placeholder data was used in the proposal.</param>
        public FundingProposal(
            string? externalId,
            FundingProposalPaymentBreakdown paymentBreakdown,
            string? acceptanceUrl,
            string? contractUrl,
            string proposalData,
            string proposalResponse,
            bool arePlaceholdersUsed)
            : this(externalId, paymentBreakdown, acceptanceUrl, proposalData, proposalResponse, arePlaceholdersUsed)
        {
            this.ContractUrl = contractUrl;
        }

        /// <summary>
        /// Gets the proposal ID.
        /// </summary>
        [JsonProperty]
        public Guid InternalId { get; private set; }

        /// <summary>
        /// Gets the ID used by the external funding service.
        /// </summary>
        [JsonProperty]
        public string? ExternalId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether placeholder data are used for the proposal.
        /// </summary>
        [JsonProperty]
        public bool ArePlaceholdersUsed { get; private set; }

        /// <summary>
        /// Gets the payment breakdown for the funding repayments.
        /// </summary>
        [JsonProperty]
        public FundingProposalPaymentBreakdown PaymentBreakdown { get; private set; }

        /// <summary>
        /// Gets the external URL that can be used to accept the proposal, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public string? AcceptanceUrl { get; private set; }

        /// <summary>
        /// Gets the contract url for the funding proposal contract file, if any, otherwise null.
        /// This contract url should not need any authentication for the file to be downloaded.
        /// </summary>
        [JsonProperty]
        public string? ContractUrl { get; }

        /// <summary>
        /// Gets the serialized proposal data.
        /// </summary>
        [JsonProperty]
        public string ProposalData { get; private set; }

        /// <summary>
        /// Gets the serialized proposal response.
        /// </summary>
        [JsonProperty]
        public string ProposalResponse { get; private set; }
    }
}
