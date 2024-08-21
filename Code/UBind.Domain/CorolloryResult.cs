// <copyright file="CorolloryResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Represents a result the relates to a given application, form data set and calculation result.
    /// </summary>
    public class CorolloryResult : ApplicationCorollary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorolloryResult"/> class for a successful result.
        /// </summary>
        /// <param name="formUpdateId">The ID of the form update the collory uses.</param>
        /// <param name="calculationResultId">The ID of the calculation result the corollory uses.</param>
        /// <param name="createdTimestamp">The created time.</param>
        protected CorolloryResult(Guid formUpdateId, Guid calculationResultId, Instant createdTimestamp)
             : base(formUpdateId, calculationResultId, createdTimestamp)
        {
            this.Outcome = PaymentAttemptOutcome.Success;
            this.Errors = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorolloryResult"/> class for an unsuccessful result.
        /// </summary>
        /// <param name="outcome">The outcome (must be neither none, nor success).</param>
        /// <param name="errors">The set of errors encountered.</param>
        /// <param name="formUpdateId">The ID of the form update the collory uses.</param>
        /// <param name="calculationResultId">The ID of the calculation result the corollory uses.</param>
        /// <param name="createdTimestamp">The created time.</param>
        protected CorolloryResult(PaymentAttemptOutcome outcome, IEnumerable<string> errors, Guid formUpdateId, Guid calculationResultId, Instant createdTimestamp)
             : base(formUpdateId, calculationResultId, createdTimestamp)
        {
            if (outcome < PaymentAttemptOutcome.Failed)
            {
                throw new ArgumentException("Outcome can only be failure or error for results with error.");
            }

            this.Outcome = outcome;
            this.Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorolloryResult"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        protected CorolloryResult()
            : base(default(Guid), default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the outcome of the payment attempt.
        /// </summary>
        public PaymentAttemptOutcome Outcome { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the payment attempt succeeded.
        /// </summary>
        public bool Success => this.Outcome == PaymentAttemptOutcome.Success;

        /// <summary>
        /// Gets the errors returned for an unsuccessful payment.
        /// </summary>
        [NotMapped]
        public IEnumerable<string> Errors { get; private set; }

        /// <summary>
        /// Gets or sets a string containing serialized error messages for EF.
        /// </summary>
        protected string ErrorsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Errors);
            }

            set
            {
                this.Errors = JsonConvert.DeserializeObject<IEnumerable<string>>(value);
            }
        }
    }
}
