// <copyright file="IWebServiceTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For providing text in response to a webhook integration.
    /// </summary>
    public interface IWebServiceTextProvider
    {
        /// <summary>
        /// Retrieves the text in response to a web integration request.
        /// </summary>
        /// <param name="payloadJson">The payload.</param>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="productConfiguration">The product configuration to be used.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>The retrieved text.</returns>
        string Invoke(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId);
    }
}
