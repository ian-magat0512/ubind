// <copyright file="IQuoteExpirySettingsProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.QuoteExpiry
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;

    /// <summary>
    /// For providing product quote settings.
    /// </summary>
    public interface IQuoteExpirySettingsProvider
    {
        Task<IQuoteExpirySettings> Retrieve(QuoteAggregate quoteAggregate);

        Task<IQuoteExpirySettings> Retrieve(Guid tenantId, Guid productId);

        IQuoteExpirySettings Retrieve(Product product);
    }
}
