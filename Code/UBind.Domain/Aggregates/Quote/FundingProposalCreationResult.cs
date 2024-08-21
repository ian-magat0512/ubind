// <copyright file="FundingProposalCreationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// Represents the response from a premium funding service.
    /// </summary>
    public class FundingProposalCreationResult : CorolloryResult<FundingProposal, string>
    {
        [JsonConstructor]
        public FundingProposalCreationResult(FundingProposal proposal, Outcome outcome, IEnumerable<string> errors, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
        {
            this.Outcome = outcome;
            this.Output = proposal;
            this.Errors = errors;
            this.DataSnapshotIds = dataSnapshotIds;
            this.CreatedTimestamp = createdTimestamp;
        }

        private FundingProposalCreationResult(FundingProposal fundingProposal, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(fundingProposal, dataSnapshotIds, createdTimestamp)
        {
        }

        private FundingProposalCreationResult(Outcome outcome, IEnumerable<string> errors, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(outcome, errors, dataSnapshotIds, createdTimestamp)
        {
        }

        /// <summary>
        /// Gets the proposal created, if successful, otherwise null.
        /// </summary>
        public FundingProposal Proposal => this.Output;

        /// <summary>
        /// Creates a new instance of the <see cref="FundingProposalCreationResult"/> class for a successful payment.
        /// </summary>
        /// <param name="fundingProposal">The funding proposal.</param>
        /// <param name="dataSnapshotIds">A snapshot of the quote data used for the funding proposal.</param>
        /// <param name="createdTimestamp">The time the funding proposal result was recorded.</param>
        /// <returns>The new instance of the <see cref="FundingProposalCreationResult "/> class.</returns>
        public static FundingProposalCreationResult CreateSuccessResponse(
            FundingProposal fundingProposal,
            QuoteDataSnapshotIds dataSnapshotIds,
            Instant createdTimestamp)
        {
            return new FundingProposalCreationResult(fundingProposal, dataSnapshotIds, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FundingProposalCreationResult"/> class for an unsuccessful payment attempt.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="dataSnapshotIds">A snapshot of the quote data used for the funding proposal.</param>
        /// <param name="createdTimestamp">The time the funding proposal result was recorded.</param>
        /// <returns>The new instance of the <see cref="FundingProposalCreationResult"/> class.</returns>
        public static FundingProposalCreationResult CreateFailureResponse(
            IEnumerable<string> errors,
            QuoteDataSnapshotIds dataSnapshotIds,
            Instant createdTimestamp)
        {
            return new FundingProposalCreationResult(Outcome.Failed, errors, dataSnapshotIds, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FundingProposalCreationResult "/> class for a payment attempt that erred.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="dataSnapshotIds">A snapshot of the quote data used for the funding proposal.</param>
        /// <param name="createdTimestamp">The time the funding proposal result was recorded.</param>
        /// <returns>The new instance of the <see cref="FundingProposalCreationResult"/> class.</returns>
        public static FundingProposalCreationResult CreateErrorResponse(
            IEnumerable<string> errors,
            QuoteDataSnapshotIds dataSnapshotIds,
            Instant createdTimestamp)
        {
            return new FundingProposalCreationResult(Outcome.Error, errors, dataSnapshotIds, createdTimestamp);
        }
    }
}
