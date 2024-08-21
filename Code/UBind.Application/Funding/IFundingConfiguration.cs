// <copyright file="IFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding
{
    using UBind.Domain.Funding;
    using UBind.Domain.Product;

    /// <summary>
    /// Configuration for premium funding service.
    /// </summary>
    public interface IFundingConfiguration
    {
        /// <summary>
        /// Gets the name of the gateway to use.
        /// </summary>
        FundingServiceName ServiceName { get; }

        ReleaseContext ReleaseContext { get; }

        /// <summary>
        /// Create a funding service using this configuration.
        /// </summary>
        /// <param name="factory">A factory for creating funding services.</param>
        /// <returns>A funding service using this configuration.</returns>
        IFundingService Create(FundingServiceFactory factory);

        void SetReleaseContext(ReleaseContext releaseContext);
    }
}
