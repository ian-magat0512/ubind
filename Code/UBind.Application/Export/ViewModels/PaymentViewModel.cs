// <copyright file="PaymentViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Payment view model for Razor Templates to use.
    /// </summary>
    public class PaymentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentViewModel"/> class.
        /// </summary>
        /// <param name="payment">The payment to present.</param>
        public PaymentViewModel(Domain.Aggregates.Quote.PaymentAttemptResult payment)
        {
            this.Reference = payment.PaymentDetails.Reference;
            this.Outcome = payment.Outcome;
            this.IsSuccess = payment.IsSuccess;
            this.CreationTime = payment.CreatedTimestamp.ToRfc5322DateStringInAet();
        }

        /// <summary>
        /// Gets any reference returned for a successful payment, or null.
        /// </summary>
        public string Reference { get; private set; }

        /// <summary>
        /// Gets the policy unique identifier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the outcome of the payment.
        /// </summary>
        public Domain.Aggregates.Quote.Outcome Outcome { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the payment attempt succeeded.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the policy created time.
        /// </summary>
        public string CreationTime { get; private set; }
    }
}
