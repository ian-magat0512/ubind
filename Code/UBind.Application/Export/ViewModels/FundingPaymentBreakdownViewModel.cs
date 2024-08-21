// <copyright file="FundingPaymentBreakdownViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;

    /// <summary>
    /// A view model for representing payment details.
    /// </summary>
    public class FundingPaymentBreakdownViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingPaymentBreakdownViewModel"/> class.
        /// </summary>
        /// <param name="paymentBreakdown">The payment breakdown to expose.</param>
        public FundingPaymentBreakdownViewModel(FundingProposalPaymentBreakdown paymentBreakdown)
        {
            this.AmountFunded = paymentBreakdown.AmountFunded.ToDollarsAndCents();
            this.PaymentFrequency = paymentBreakdown.PaymentFrequency.ToString();
            this.NumberOfInstalments = paymentBreakdown.NumberOfInstalments;
            this.InitialInstalmentAmount = paymentBreakdown.InitialInstalmentAmount.ToDollarsAndCents();
            this.RegularInstalmentAmount = paymentBreakdown.RegularInstalmentAmount.ToDollarsAndCents();
        }

        /// <summary>
        /// Gets the amount funded.
        /// </summary>
        public string AmountFunded { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        public string PaymentFrequency { get; }

        /// <summary>
        /// Gets the number of instalments.
        /// </summary>
        public int NumberOfInstalments { get; }

        /// <summary>
        /// Gets the initial instalment amount.
        /// </summary>
        public string InitialInstalmentAmount { get; }

        /// <summary>
        /// Gets the regular instalment amount.
        /// </summary>
        public string RegularInstalmentAmount { get; }
    }
}
