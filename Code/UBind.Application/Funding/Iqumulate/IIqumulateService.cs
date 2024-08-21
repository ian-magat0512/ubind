// <copyright file="IIqumulateService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.Iqumulate
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.Product;

    /// <summary>
    /// Provides the Iqumulate PremiumFunding configuration for a given application.
    /// </summary>
    public interface IIqumulateService : IFundingService
    {
        /// <summary>
        /// Gets the integration configuration for a given application.
        /// </summary>
        /// <param name="releaseContext">The context of the product the funding data is for.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="calculationResultId">The ID of the calculation result the funding is for.</param>
        /// <returns>The data required to make the funding request.</returns>
        Task<IqumulateRequestData> GetIQumulateFundingRequestData(
            ReleaseContext releaseContext,
            Guid quoteId,
            Guid calculationResultId);

        /// <summary>
        /// Validates the configuration for Iqumulate funding.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        void ValidateConfiguration(IIqumulateConfiguration configuration);
    }
}
