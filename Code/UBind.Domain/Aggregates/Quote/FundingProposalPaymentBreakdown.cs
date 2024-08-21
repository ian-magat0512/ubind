// <copyright file="FundingProposalPaymentBreakdown.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Payment details for a funding proposal.
    /// These relate to payment amounts and schedule and are not sensitive. I.e. card details etc. are not included.
    /// </summary>
    public class FundingProposalPaymentBreakdown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingProposalPaymentBreakdown"/> class.
        /// </summary>
        /// <param name="amountFunded">The amount funded.</param>
        /// <param name="paymentFrequency">The payment frequency.</param>
        /// <param name="numberOfInstalments">The number of installments.</param>
        /// <param name="initialInstalmentAmount">The initial instalment amount.</param>
        /// <param name="regularInstalmentAmount">The regular instalment amount.</param>
        /// <param name="instalmentMerchantFeeMultiplier">The instalment merchant fee multiplier</param>
        public FundingProposalPaymentBreakdown(
            decimal amountFunded,
            Frequency paymentFrequency,
            int numberOfInstalments,
            decimal initialInstalmentAmount,
            decimal regularInstalmentAmount,
            decimal instalmentMerchantFeeMultiplier = 0,
            decimal interest = 0,
            decimal interestRate = 0)
        {
            this.AmountFunded = amountFunded;
            this.PaymentFrequency = paymentFrequency;
            this.NumberOfInstalments = numberOfInstalments;
            this.InitialInstalmentAmount = initialInstalmentAmount;
            this.RegularInstalmentAmount = regularInstalmentAmount;
            this.InstalmentMerchantFeeMultiplier = instalmentMerchantFeeMultiplier;
            this.Interest = interest;
            this.InterestRate = interestRate;
        }

        /// <summary>
        /// Gets the amount funded.
        /// </summary>
        public decimal AmountFunded { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        public Frequency PaymentFrequency { get; }

        /// <summary>
        /// Gets the number of instalments.
        /// </summary>
        public int NumberOfInstalments { get; }

        /// <summary>
        /// Gets the initial instalment amount.
        /// </summary>
        public decimal InitialInstalmentAmount { get; }

        /// <summary>
        /// Gets the regular instalment amount.
        /// </summary>
        public decimal RegularInstalmentAmount { get; }

        /// <summary>
        /// Gets the instalment merchant fee multiplier (0.00506).
        /// </summary>
        public decimal InstalmentMerchantFeeMultiplier { get; }

        /// <summary>
        /// This is the interest amount applied by the funding provider
        /// </summary>
        public decimal Interest { get; }

        /// <summary>
        /// This is the interest rate used by the funding provider (0.154803)
        /// </summary>
        public decimal InterestRate { get; }
    }
}
