// <copyright file="CorolloryResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// Represents a result the relates to a given application, form data set and calculation result.
    /// </summary>
    /// <typeparam name="TOuput">The type used to represent output of the operation that resulted in this corollary.</typeparam>
    /// <typeparam name="TError">The type used to represent any errors.</typeparam>
    public class CorolloryResult<TOuput, TError> : QuoteCorollary
    {
        [JsonConstructor]
        protected CorolloryResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorolloryResult{TOutput, TError}"/> class for a successful result.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="dataSnapshotIds">The IDs of the data that form a snapshot of the quote data used for this corollary.</param>
        /// <param name="createdTimestamp">The created time.</param>
        protected CorolloryResult(TOuput output, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(dataSnapshotIds, createdTimestamp)
        {
            this.Output = output;
            this.Outcome = Outcome.Success;
            this.Errors = Enumerable.Empty<TError>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorolloryResult{TOuput, TError}"/> class for an unsuccessful result.
        /// </summary>
        /// <param name="outcome">The outcome (must be either <see cref="Outcome.Failed"/>, or <see cref="Outcome.Error"/>).</param>
        /// <param name="errors">The set of errors encountered.</param>
        /// <param name="dataSnapshotIds">The IDs of the data that form a snapshot of the quote data used for this corollary.</param>
        /// <param name="createdTimestamp">The created time.</param>
        protected CorolloryResult(Outcome outcome, IEnumerable<TError> errors, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(dataSnapshotIds, createdTimestamp)
        {
            if (outcome == Outcome.Success)
            {
                throw new ArgumentException("Outcome can only be failure or error for results with error.");
            }

            this.Outcome = outcome;
            this.Errors = errors;
        }

        /// <summary>
        /// Gets the outcome of the payment attempt.
        /// </summary>
        public Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets the errors returned for an unsuccessful payment.
        /// </summary>
        public IEnumerable<TError> Errors { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the payment attempt succeeded.
        /// </summary>
        public bool IsSuccess => this.Outcome == Outcome.Success;

        /// <summary>
        /// Gets the corollary output, if result was sucessful, otherwise null.
        /// </summary>
        protected TOuput Output { get; set; }
    }
}
