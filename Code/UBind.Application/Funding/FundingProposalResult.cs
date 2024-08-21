// <copyright file="FundingProposalResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Represents the result of an attempt to create a funding proposal.
    /// </summary>
    public class FundingProposalResult : ResultOld<FundingProposal, IEnumerable<string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposalResult"/> class
        /// representing the result of a successful attempt to create a funding proposal.
        /// </summary>
        /// <param name="proposal">The resulting funding proposal.</param>
        private FundingProposalResult(FundingProposal proposal)
            : base(proposal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposalResult"/> class
        /// representing the result of a failed attempt to create a funding proundingosal.
        /// </summary>
        /// <param name="error">The error(s) encountered.</param>
        private FundingProposalResult(IEnumerable<string> error)
            : base(error)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="FundingProposalResult"/> representing a successful funding proposal creation.
        /// </summary>
        /// <param name="fundingProposal">The funding proposal created.</param>
        /// <returns>A new instance of <see cref="FundingProposalResult"/> representing a successful funding proposal creation.</returns>
        public static new FundingProposalResult Success(FundingProposal fundingProposal)
        {
            return new FundingProposalResult(fundingProposal);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FundingProposalResult"/> representing a failure to create a funding proposal.
        /// </summary>
        /// <param name="errors">The errors encountered.</param>
        /// <returns>A new instance of <see cref="FundingProposalResult"/> representing a failure to create a funding proposal.</returns>
        public static new FundingProposalResult Failure(IEnumerable<string> errors)
        {
            return new FundingProposalResult(errors);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FundingProposalResult"/> representing a failure to create a funding proposal.
        /// </summary>
        /// <param name="errors">The errors encountered.</param>
        /// <returns>A new instance of <see cref="FundingProposalResult"/> representing a failure to create a funding proposal.</returns>
        public static FundingProposalResult Failure(params string[] errors)
        {
            return new FundingProposalResult(errors);
        }
    }
}
