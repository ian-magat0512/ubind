// <copyright file="FundingProposalAcceptanceResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// Represents the result of a premium funding acceptance attempt.
    /// </summary>
    public class FundingProposalAcceptanceResult
    {
        private FundingProposalAcceptanceResult(FundingProposal fundingProposal, Instant createdTimestamp)
        {
            this.FundingProposal = fundingProposal;
            this.CreatedTimestamp = createdTimestamp;
            this.Success = true;
        }

        private FundingProposalAcceptanceResult(
            FundingProposal fundingProposal,
            IEnumerable<string> errors,
            Instant createdTimestamp)
            : this(fundingProposal, createdTimestamp)
        {
            this.Errors = errors;
            this.Success = false;
        }

        /// <summary>
        /// Gets the identifier for the funding proposal being accepted.
        /// </summary>
        public FundingProposal FundingProposal { get; }

        /// <summary>
        /// Gets the time the result was recorded.
        /// </summary>
        public Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets a value indicating whether the funding proposal acceptance succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the errors returned for an unsuccessful acceptance attempt.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FundingProposalAcceptanceResult"/> class for a successful acceptance.
        /// </summary>
        /// <param name="fundingProposal">The proposal accepted.</param>
        /// <param name="createdTimestamp">The time the funding proposal result was recorded.</param>
        /// <returns>The new instance of the <see cref="FundingProposalAcceptanceResult "/> class.</returns>
        public static FundingProposalAcceptanceResult CreateSuccessResponse(
            FundingProposal fundingProposal,
            Instant createdTimestamp)
        {
            return new FundingProposalAcceptanceResult(fundingProposal, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FundingProposalAcceptanceResult"/> class for an unsuccessful acceptance attempt.
        /// </summary>
        /// <param name="fundingProposal">The proposal the acceptance request was for.</param>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="createdTimestamp">The time the funding proposal result was recorded.</param>
        /// <returns>The new instance of the <see cref="FundingProposalAcceptanceResult"/> class.</returns>
        public static FundingProposalAcceptanceResult CreateFailureResponse(
            FundingProposal fundingProposal,
            IEnumerable<string> errors,
            Instant createdTimestamp)
        {
            return new FundingProposalAcceptanceResult(fundingProposal, errors, createdTimestamp);
        }
    }
}
