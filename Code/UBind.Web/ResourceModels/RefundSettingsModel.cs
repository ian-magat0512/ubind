// <copyright file="RefundSettingsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using UBind.Domain;

    /// <summary>
    /// Resource model for refund policy.
    /// </summary>
    public class RefundSettingsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefundSettingsModel"/> class.
        /// </summary>
        /// <param name="refundPolicy">The refund policy.</param>
        /// <param name="periodWhichNoClaimsMade">The period which no claims made.</param>
        /// <param name="lastNumberOfYearsWhichNoClaimsMade">The last number of years with no claims made.</param>
        public RefundSettingsModel(
            RefundRule refundPolicy,
            PolicyPeriodCategory? periodWhichNoClaimsMade,
            int? lastNumberOfYearsWhichNoClaimsMade)
        {
            this.RefundPolicy = refundPolicy;
            this.PeriodWhichNoClaimsMade = periodWhichNoClaimsMade;
            this.LastNumberOfYearsWhichNoClaimsMade = lastNumberOfYearsWhichNoClaimsMade;
        }

        /// <summary>
        /// Gets a refund policy.
        /// </summary>
        public RefundRule RefundPolicy { get; private set; }

        /// <summary>
        /// Gets a cancellation period.
        /// </summary>
        public PolicyPeriodCategory? PeriodWhichNoClaimsMade { get; private set; }

        /// <summary>
        /// Gets a value indicating the number of years required with which no claims were made.
        /// </summary>
        public int? LastNumberOfYearsWhichNoClaimsMade { get; private set; }
    }
}
