// <copyright file="FundingAcceptanceResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding
{
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Represents the result of an attempt to accept a funding proposal.
    /// </summary>
    public class FundingAcceptanceResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingAcceptanceResult"/> class
        /// representing the result of a successful attempt to create a funding proposal.
        /// </summary>
        /// <param name="proposal">The accepted funding proposal.</param>
        private FundingAcceptanceResult(FundingProposal proposal)
        {
            this.FundingProposal = proposal;
            this.Succeeded = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingAcceptanceResult"/> class
        /// representing the result of a failed attempt to accept a funding proundingosal.
        /// </summary>
        /// <param name="proposal">The proposal that was to be accepted.</param>
        /// <param name="errors">The errors encountered.</param>
        private FundingAcceptanceResult(FundingProposal proposal, IEnumerable<string> errors)
        {
            this.FundingProposal = proposal;
            this.Errors = errors;
            this.Succeeded = false;
        }

        /// <summary>
        /// Gets a value indicating whether the acceptance succeeded.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the proposal to be accepted.
        /// </summary>
        public FundingProposal FundingProposal { get; }

        /// <summary>
        /// Gets any errors encountered.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FundingAcceptanceResult"/> representing a successful acceptance of a funding proposal.
        /// </summary>
        /// <param name="fundingProposal">The funding proposal accepted.</param>
        /// <returns>A new instance of <see cref="FundingProposalResult"/> representing a successful acceptance of a funding proposal.</returns>
        public static FundingAcceptanceResult Success(FundingProposal fundingProposal)
        {
            return new FundingAcceptanceResult(fundingProposal);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FundingAcceptanceResult"/> representing a failure to accept a funding proposal.
        /// </summary>
        /// <param name="fundingProposal">The funding proposal that was to be accepted.</param>
        /// <param name="errors">The errors encountered.</param>
        /// <returns>A new instance of <see cref="FundingAcceptanceResult"/> representing a failure to accept a funding proposal.</returns>
        public static FundingAcceptanceResult Failure(FundingProposal fundingProposal, IEnumerable<string> errors)
        {
            return new FundingAcceptanceResult(fundingProposal, errors);
        }
    }
}
