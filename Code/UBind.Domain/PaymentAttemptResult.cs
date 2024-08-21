// <copyright file="PaymentAttemptResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Represents the response from a payment gateway.
    /// </summary>
    public class PaymentAttemptResult : CorolloryResult
    {
        /// <summary>
        /// Gets an expression mapping private field requiring persistence for EF.
        /// </summary>
        public static readonly Expression<Func<PaymentAttemptResult, string>> ErrorsExpression =
            rf => rf.ErrorsJson;

        private PaymentAttemptResult(string paymentReference, Guid formUpdateId, Guid calculationResultId, Instant createdTimestamp)
             : base(formUpdateId, calculationResultId, createdTimestamp)
        {
            this.Reference = paymentReference;
        }

        private PaymentAttemptResult(PaymentAttemptOutcome outcome, IEnumerable<string> errors, Guid formUpdateId, Guid calculationResultId, Instant createdTimestamp)
             : base(outcome, errors, formUpdateId, calculationResultId, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAttemptResult"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private PaymentAttemptResult()
            : base(default(Guid), default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets any reference returned for a successful payment, or null.
        /// </summary>
        public string Reference { get; private set; }

        /////// <summary>
        /////// Gets or sets a string containing serialized error messages for EF.
        /////// </summary>
        ////protected override string ErrorsJson
        ////{
        ////    get
        ////    {
        ////        return base.ErrorsJson;
        ////    }

        ////    set
        ////    {
        ////        base.ErrorsJson = value;
        ////    }
        ////}

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a successful payment.
        /// </summary>
        /// <param name="reference">The reference returned by the payment gateway.</param>
        /// <param name="formUpdateId">The ID of the form update that was used.</param>
        /// <param name="calculationResultId">The ID of the calculation result that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateSuccessResponse(
            string reference,
            Guid formUpdateId,
            Guid calculationResultId,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(reference, formUpdateId, calculationResultId, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for an unsuccessful payment attempt.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="formUpdateId">The ID of the form update that was used.</param>
        /// <param name="calculationResultId">The ID of the calculation result that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateFailureResponse(
            IEnumerable<string> errors,
            Guid formUpdateId,
            Guid calculationResultId,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(PaymentAttemptOutcome.Failed, errors, formUpdateId, calculationResultId, createdTimestamp);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaymentAttemptResult"/> class for a payment attempt that erred.
        /// </summary>
        /// <param name="errors">The errors returned by the payment gateway.</param>
        /// <param name="formUpdateId">The ID of the form update that was used.</param>
        /// <param name="calculationResultId">The ID of the calculation result that was used.</param>
        /// <param name="createdTimestamp">The time the payment attempt result was recorded.</param>
        /// <returns>The new instance of the <see cref="PaymentAttemptResult"/> class.</returns>
        public static PaymentAttemptResult CreateErrorResponse(
            IEnumerable<string> errors,
            Guid formUpdateId,
            Guid calculationResultId,
            Instant createdTimestamp)
        {
            return new PaymentAttemptResult(PaymentAttemptOutcome.Error, errors, formUpdateId, calculationResultId, createdTimestamp);
        }
    }
}
