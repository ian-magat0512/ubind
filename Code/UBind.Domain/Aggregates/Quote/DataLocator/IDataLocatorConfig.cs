// <copyright file="IDataLocatorConfig.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator
{
    /// <summary>
    /// Interface for data locator and quote data locator configuration.
    /// </summary>
    public interface IDataLocatorConfig
    {
        /// <summary>
        /// Gets the quote data locations used to find important data from
        /// submitted form data or calculation results.
        /// </summary>
        IQuoteDatumLocations QuoteDataLocations { get; }

        /// <summary>
        /// Gets the configured data locations.
        /// </summary>
        DataLocators DataLocators { get; }
    }
}
