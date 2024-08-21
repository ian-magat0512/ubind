// <copyright file="CalculatedPaymentInfoViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain;

    /// <summary>
    /// View model for calculated payments.
    /// </summary>
    public class CalculatedPaymentInfoViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPaymentInfoViewModel"/> class.
        /// </summary>
        /// <param name="payment">The calculated payment object.</param>
        public CalculatedPaymentInfoViewModel(CalculatedPayment payment)
        {
            this.Total = new CalculatedPaymentTotalInfoViewModel(payment.ComponentsV1);
            this.Installments = new CalculatedPaymentInstallmentsInfoViewModel(payment.Installments);
        }

        /// <summary>
        /// Gets the total premium information.
        /// </summary>
        public CalculatedPaymentTotalInfoViewModel Total { get; private set; }

        /// <summary>
        /// Gets the installments information.
        /// </summary>
        public CalculatedPaymentInstallmentsInfoViewModel Installments { get; private set; }
    }
}
