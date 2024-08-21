// <copyright file="PaymentAttemptResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// Represents the response from a payment gateway.
    /// </summary>
    public class PaymentAttemptResult : CorolloryResult<PaymentDetails, string>
    {
        private PaymentAttemptResult(PaymentDetails paymentDetails, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(paymentDetails, dataSnapshotIds, createdTimestamp)
        {
        }

        private PaymentAttemptResult(Outcome outcome, IEnumerable<string> errors, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
             : base(outcome, errors, dataSnapshotIds, createdTimestamp)
        {
        }

        /// <summary>
        /// Gets any reference returned for a successful payment, or null.
        /// </summary>
        public PaymentDetails PaymentDetails => this.Output;

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a successful payment.
        /// </summary>
        /// <param name="paymentDetails">The details of the payment.</param>
        /// <param name="dataSnapshotids">A snapshot of the data that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateSuccessResponse(
            PaymentDetails paymentDetails,
            QuoteDataSnapshotIds dataSnapshotids,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(paymentDetails, dataSnapshotids, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for an unsuccessful payment attempt.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="dataSnapshotIds">A snapshot of the data that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateFailureResponse(
            IEnumerable<string> errors,
            QuoteDataSnapshotIds dataSnapshotIds,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(Outcome.Failed, errors, dataSnapshotIds, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a payment attempt that erred.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="dataSnapshotIds">A snapshot of the data that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateErrorResponse(
            IEnumerable<string> errors,
            QuoteDataSnapshotIds dataSnapshotIds,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(Outcome.Error, errors, dataSnapshotIds, createdTimestamp);
        }
    }
}
