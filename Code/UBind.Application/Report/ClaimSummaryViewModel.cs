// <copyright file="ClaimSummaryViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Claim Summary view model providing data for use in liquid report templates.
    /// </summary>
    public class ClaimSummaryViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSummaryViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the claim summary.</param>
        /// <param name="description">The description of the claim summary.</param>
        /// <param name="totalIncompleteClaims">The total number of incomplete claims.</param>
        /// <param name="totalCompletedClaims">The total number completed claims.</param>
        /// <param name="totalClaims">The total number of claims.</param>
        /// <param name="totalAmountOfCompleteClaims">The total 'Amount' of the completed claims.</param>
        /// <param name="totalAmountOfClaims">The total 'Amount' of the both completed and incompleted claims.</param>
        public ClaimSummaryViewModel(
            string name,
            string description,
            int totalIncompleteClaims,
            int totalCompletedClaims,
            int totalClaims,
            string totalAmountOfCompleteClaims,
            string totalAmountOfClaims)
        {
            this.Name = name;
            this.Description = description;
            this.TotalIncompleteClaims = totalIncompleteClaims;
            this.TotalCompleteClaims = totalCompletedClaims;
            this.TotalClaims = totalClaims;
            this.TotalAmountOfCompleteClaims = totalAmountOfCompleteClaims;
            this.TotalAmountOfClaims = totalAmountOfClaims;
        }

        /// <summary>
        /// Gets the name of the claim summary.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the claim summary.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the total number of claims.
        /// </summary>
        public int TotalClaims { get; }

        /// <summary>
        /// Gets the total number of incomplete claims.
        /// </summary>
        public int TotalIncompleteClaims { get; }

        /// <summary>
        /// Gets the total number of completed claims.
        /// </summary>
        public int TotalCompleteClaims { get; }

        /// <summary>
        /// Gets the total of 'Amount' of the completed claims.
        /// </summary>
        public string TotalAmountOfCompleteClaims { get; }

        /// <summary>
        /// Gets the total of 'Amount' of the completed claims.
        /// </summary>
        public string TotalAmountOfClaims { get; }
    }
}
