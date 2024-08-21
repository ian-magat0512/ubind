// <copyright file="CalculatedPaymentTotalInfoViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain;

    /// <summary>
    /// View model for calculated payment totals.
    /// </summary>
    public class CalculatedPaymentTotalInfoViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPaymentTotalInfoViewModel"/> class.
        /// </summary>
        /// <param name="total">The calculated payment total.</param>
        public CalculatedPaymentTotalInfoViewModel(CalculatedPaymentTotal total)
        {
            this.Premium = total.Premium;
            this.Esl = total.Esl;
            this.Gst = total.Gst;
            this.StampDuty = total.StampDuty;
            this.ServiceFees = total.ServiceFees;
            this.Interest = total.Interest;
            this.MerchantFees = total.MerchantFees;
            this.TransactionCosts = total.TransactionCosts;
            this.Payable = total.Payable;
        }

        /// <summary>
        /// Gets the total premium.
        /// </summary>
        public decimal Premium { get; private set; }

        /// <summary>
        /// Gets the total ESL.
        /// </summary>
        public decimal Esl { get; private set; }

        /// <summary>
        /// Gets the total GST.
        /// </summary>
        public decimal Gst { get; private set; }

        /// <summary>
        /// Gets the total stamp duty.
        /// </summary>
        public decimal StampDuty { get; private set; }

        /// <summary>
        /// Gets the total service fees.
        /// </summary>
        public decimal ServiceFees { get; private set; }

        /// <summary>
        /// Gets the total interest.
        /// </summary>
        public decimal Interest { get; private set; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        public decimal MerchantFees { get; private set; }

        /// <summary>
        /// Gets the total transaction costs.
        /// </summary>
        public decimal TransactionCosts { get; private set; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        public decimal Payable { get; private set; }
    }
}
