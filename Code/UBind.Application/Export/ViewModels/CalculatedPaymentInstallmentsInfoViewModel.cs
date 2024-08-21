// <copyright file="CalculatedPaymentInstallmentsInfoViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain;

    /// <summary>
    /// View model for calculated payment installments.
    /// </summary>
    public class CalculatedPaymentInstallmentsInfoViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPaymentInstallmentsInfoViewModel"/> class.
        /// </summary>
        /// <param name="installments">The calculated payment installments.</param>
        public CalculatedPaymentInstallmentsInfoViewModel(CalculatedPaymentInstallments installments)
        {
            this.InstallmentsPerYear = installments.InstallmentsPerYear;
            this.InstallmentAmount = installments.InstallmentAmount;
        }

        /// <summary>
        /// Gets the number of installments per year.
        /// </summary>
        public int InstallmentsPerYear { get; private set; }

        /// <summary>
        /// Gets the amount of each installment.
        /// </summary>
        public decimal InstallmentAmount { get; private set; }
    }
}
