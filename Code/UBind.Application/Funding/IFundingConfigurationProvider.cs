// <copyright file="IFundingConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding
{
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Product;

    /// <summary>
    /// For obtaining premium funding configuration for a given product context.
    /// </summary>
    public interface IFundingConfigurationProvider
    {
        /// <summary>
        /// Gets the payment configuration for a given product environment.
        /// </summary>
        /// <param name="releaseContext">The release context.</param>
        /// <returns>The payment configuration.</returns>
        Task<Maybe<IFundingConfiguration>> GetConfigurationAsync(ReleaseContext releaseContext);
    }
}
