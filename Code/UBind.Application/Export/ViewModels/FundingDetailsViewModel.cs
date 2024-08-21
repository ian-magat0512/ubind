// <copyright file="FundingDetailsViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// A view model for representing payment details.
    /// </summary>
    public class FundingDetailsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingDetailsViewModel"/> class.
        /// </summary>
        /// <param name="fundingProposal">The payment to expose.</param>
        public FundingDetailsViewModel(FundingProposal fundingProposal)
        {
            this.Id = fundingProposal.ExternalId;
            this.PaymentBreakdown = new FundingPaymentBreakdownViewModel(fundingProposal.PaymentBreakdown);
            var dataObject = JObject.Parse(fundingProposal.ProposalData);
            this.Data = new JsonViewModel(dataObject);
            var responseObject = JObject.Parse(fundingProposal.ProposalResponse);
            this.Response = new JsonViewModel(responseObject);
        }

        /// <summary>
        /// Gets the ID used by the funding provider to identify the proposal.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the funding proposal payment breakdown.
        /// </summary>
        public FundingPaymentBreakdownViewModel PaymentBreakdown { get; }

        /// <summary>
        /// Gets a view model representing the data used to generate the accepted premium funding proposal, if they exist.
        /// </summary>
        public JsonViewModel Data { get; }

        /// <summary>
        /// Gets a view model representing the response from the funding service for the creation of the funding proposal that was accepted, if it exists.
        /// </summary>
        public JsonViewModel Response { get; }
    }
}
