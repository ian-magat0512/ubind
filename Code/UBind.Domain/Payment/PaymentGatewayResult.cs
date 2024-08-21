// <copyright file="PaymentGatewayResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Payment
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Represents the response from a payment gateway.
    /// </summary>
    public class PaymentGatewayResult
    {
        private PaymentGatewayResult(PaymentAttemptOutcome outcome, PaymentDetails details, IEnumerable<string>? errors)
            : this(outcome, details)
        {
            this.Errors = errors?.ToList();
        }

        private PaymentGatewayResult(PaymentAttemptOutcome outcome, PaymentDetails details)
        {
            this.Outcome = outcome;
            this.Details = details;
            this.Errors = new List<string>();
        }

        /// <summary>
        /// Gets the outcomde.
        /// </summary>
        public PaymentAttemptOutcome Outcome { get; }

        /// <summary>
        /// Gets a value indicating whether the payment attempt succeeded.
        /// </summary>
        public bool Success => this.Outcome == PaymentAttemptOutcome.Success;

        /// <summary>
        /// Gets the payment request and response as json.
        /// </summary>
        public PaymentDetails Details { get; }

        /// <summary>
        /// Gets any reference returned for a successful payment, or null.
        /// </summary>
        public string? Reference { get; }

        /// <summary>
        /// Gets the errors returned for an unsuccessful payment.
        /// </summary>
        public IReadOnlyList<string>? Errors { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a successful payment.
        /// </summary>
        /// <param name="details">Details of payment request and response.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentGatewayResult CreateSuccessResponse(PaymentDetails details)
        {
            return new PaymentGatewayResult(PaymentAttemptOutcome.Success, details);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for an unsuccessful payment attempt.
        /// </summary>
        /// <param name="details">Details of payment request and response.</param>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentGatewayResult CreateFailureResponse(PaymentDetails details, params string[] errors)
        {
            return new PaymentGatewayResult(PaymentAttemptOutcome.Failed, details, errors);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a payment attempt that had errors.
        /// </summary>
        /// <param name="details">Details of payment request and response.</param>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentGatewayResult CreateErrorResponse(PaymentDetails details, params string[]? errors)
        {
            return new PaymentGatewayResult(PaymentAttemptOutcome.Error, details, errors);
        }
    }
}
