// <copyright file="CalculatedPaymentInstallments.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Total premium information from calculation result.
    /// </summary>
    public class CalculatedPaymentInstallments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPaymentInstallments"/> class.
        /// </summary>
        /// <param name="installmentsPerYear">The number of installments per year.</param>
        /// <param name="installmentAmount">The amount of each installment.</param>
        public CalculatedPaymentInstallments(int installmentsPerYear, decimal installmentAmount)
        {
            this.InstallmentsPerYear = installmentsPerYear;
            this.InstallmentAmount = installmentAmount;
        }

        [JsonConstructor]
        private CalculatedPaymentInstallments()
        {
        }

        /// <summary>
        /// Gets the number of installments per year.
        /// </summary>
        [JsonProperty(PropertyName = "instalmentsPerYear")]
        public int InstallmentsPerYear { get; private set; }

        /// <summary>
        /// Gets the amount of each installment.
        /// </summary>
        [JsonProperty(PropertyName = "instalmentAmount")]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal InstallmentAmount { get; private set; }
    }
}
