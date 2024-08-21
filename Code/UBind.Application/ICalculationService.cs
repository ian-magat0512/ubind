// <copyright file="ICalculationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for obtaining quotes.
    /// </summary>
    public interface ICalculationService
    {
        /// <summary>
        /// Gets a quote calculation for a given product in a given environments based on provided quote data.
        /// </summary>
        /// <param name="releaseContext">The context indicating which release/environment to use.</param>
        /// <param name="calculationDataModel">The calculation data model.</param>
        /// <param name="additionalRatingFactors">The addional rating factor.</param>
        /// <returns>A string containing the quote JSON.</returns>
        ReleaseCalculationOutput GetQuoteCalculation(
            ReleaseContext releaseContext,
            SpreadsheetCalculationDataModel calculationDataModel,
            IAdditionalRatingFactors additionalRatingFactors = null);

        /// <summary>
        /// Gets a claim calculation for a given product in a given environments based on provided quote data.
        /// </summary>
        /// <param name="releaseContext">The context indicating which release/environment to use.</param>
        /// <param name="formData">The form data to base the calculation on.</param>
        /// <returns>A string containing the claim JSON.</returns>
        string GetClaimCalculation(ReleaseContext releaseContext, string formData);
    }
}
