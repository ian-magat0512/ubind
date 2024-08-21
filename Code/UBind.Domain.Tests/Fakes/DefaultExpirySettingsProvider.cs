// <copyright file="DefaultExpirySettingsProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;
    using UBind.Domain.Services.QuoteExpiry;

    /// <summary>
    /// For providing default expiry settings.
    /// </summary>
    public class DefaultExpirySettingsProvider : IQuoteExpirySettingsProvider
    {
        /// <inheritdoc/>
        public Task<IQuoteExpirySettings> Retrieve(QuoteAggregate quoteAggregate)
        {
            IQuoteExpirySettings settings = new QuoteExpirySettings(30, true);
            return Task.FromResult(settings);
        }

        public Task<IQuoteExpirySettings> Retrieve(Guid tenantId, Guid productId)
        {
            IQuoteExpirySettings settings = new QuoteExpirySettings(30);
            return Task.FromResult(settings);
        }

        public IQuoteExpirySettings Retrieve(Product product)
        {
            return new QuoteExpirySettings(30);
        }
    }
}
