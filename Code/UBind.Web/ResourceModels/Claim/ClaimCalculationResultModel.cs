// <copyright file="ClaimCalculationResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for claims calculation operation response.
    /// </summary>
    public class ClaimCalculationResultModel : IClaimResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimCalculationResultModel"/> class.
        /// </summary>
        /// <param name="claimAggregate">The claim aggregate.</param>
        public ClaimCalculationResultModel(ClaimAggregate claimAggregate)
        {
            var claim = claimAggregate.Claim;
            if (claim.LatestCalculationResult == null)
            {
                throw new ArgumentException("Claim must include calculation results for output");
            }

            var calculationResult = claim.LatestCalculationResult;
            this.CalculationResultId = calculationResult.Id;
            this.FormDataId = calculationResult.Data.FormDataId;
            if (calculationResult?.Data != null)
            {
                this.CalculationResult = calculationResult.Data.JObject;
            }

            this.ClaimState = claimAggregate.Claim.ClaimStatus.ToLower();
            this.ClaimId = claimAggregate.Id;
        }

        /// <summary>
        /// Gets the ID of the claim.
        /// </summary>
        public Guid ClaimId { get; }

        /// <summary>
        /// Gets the ID of the latest form data.
        /// </summary>
        public Guid FormDataId { get; }

        /// <summary>
        /// Gets the ID of the latest calculation result.
        /// </summary>
        public Guid CalculationResultId { get; }

        /// <summary>
        /// Gets the calculation result as a JObject.
        /// </summary>
        public JObject CalculationResult { get; }

        /// <summary>
        /// Gets the current claim state.
        /// </summary>
        public string ClaimState { get; }
    }
}
