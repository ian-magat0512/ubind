// <copyright file="General.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Iqumulate Premium Funding Response "General" - for setting A post call.
    /// </summary>
    public class General
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="General"/> class.
        /// </summary>
        [JsonConstructor]
        public General()
        {
        }

        /// <summary>
        /// Gets Iqumulate PF Reference Code.
        /// </summary>
        [JsonProperty]
        public string Number { get; private set; }

        /// <summary>
        /// Gets Region: Australia or NZ.
        /// </summary>
        [JsonProperty]
        public string Region { get; private set; }

        /// <summary>
        /// Gets the financed amount.
        /// </summary>
        [JsonProperty]
        public decimal AmountFinanced { get; private set; }

        /// <summary>
        /// Gets a value indicating whether amount is over the threshold.
        /// </summary>
        [JsonProperty]
        public bool IsOverThreshold { get; private set; }

        /// <summary>
        /// Gets the flat interest rate.
        /// </summary>
        [JsonProperty]
        public float FlatInterestRate { get; private set; }

        /// <summary>
        /// Gets the APR.
        /// </summary>
        [JsonProperty]
        public float APR { get; private set; }

        /// <summary>
        /// Gets the total interest charges.
        /// </summary>
        [JsonProperty]
        public decimal TotalInterestCharges { get; private set; }

        /// <summary>
        /// Gets the total amount repayable.
        /// </summary>
        [JsonProperty]
        public decimal TotalAmountRepayable { get; private set; }

        /// <summary>
        /// Gets the Payment Frequency.
        /// </summary>
        [JsonProperty]
        public string PaymentFrequency { get; private set; }

        /// <summary>
        /// Gets the number of instalments.
        /// </summary>
        [JsonProperty]
        public int NumberOfInstalments { get; private set; }

        /// <summary>
        /// Gets the first installment date.
        /// </summary>
        [JsonProperty]
        public DateTime FirstInstalmentDate { get; private set; }

        /// <summary>
        /// Gets the Last installment date.
        /// </summary>
        [JsonProperty]
        public DateTime LastInstalmentDate { get; private set; }

        /// <summary>
        /// Gets the initialInstalmentAmount.
        /// </summary>
        [JsonProperty]
        public decimal InitialInstalmentAmount { get; private set; }

        /// <summary>
        /// Gets the OngoingInstalmentAmount.
        /// </summary>
        [JsonProperty]
        public decimal OngoingInstalmentAmount { get; private set; }

        /// <summary>
        /// Gets the ApplicationFee.
        /// </summary>
        [JsonProperty]
        public decimal ApplicationFee { get; private set; }

        /// <summary>
        /// Gets the settlementDays.
        /// </summary>
        [JsonProperty]
        public int SettlementDays { get; private set; }

        /// <summary>
        /// Gets the CancellationFee.
        /// </summary>
        [JsonProperty]
        public decimal CancellationFee { get; private set; }

        /// <summary>
        /// Gets the dishonourFee.
        /// </summary>
        [JsonProperty]
        public decimal DishonourFee { get; private set; }

        /// <summary>
        /// Gets the CancellationFee.
        /// </summary>
        [JsonProperty]
        public int ResponseCode { get; private set; }

        /// <summary>
        /// Gets the ResponseDescription.
        /// </summary>
        [JsonProperty]
        public string ResponseDescription { get; private set; }

        /// <summary>
        /// Gets the CommissionRate.
        /// </summary>
        [JsonProperty]
        public string CommissionRate { get; private set; }

        /// <summary>
        /// Gets the PreviousLoanNumber.
        /// </summary>
        [JsonProperty]
        public string PreviousLoanNumber { get; private set; }
    }
}
