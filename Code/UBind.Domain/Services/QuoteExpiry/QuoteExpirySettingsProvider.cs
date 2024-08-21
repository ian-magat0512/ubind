// <copyright file="QuoteExpirySettingsProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.QuoteExpiry
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;

    /// <summary>
    /// Provides the Quote expiry settings for a given product under a specific tenant.
    /// </summary>
    public class QuoteExpirySettingsProvider : IQuoteExpirySettingsProvider
    {
        private readonly ICachingResolver cachingResolver;

        public QuoteExpirySettingsProvider(
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task<IQuoteExpirySettings> Retrieve(QuoteAggregate quoteAggregate)
        {
            return await this.Retrieve(quoteAggregate.TenantId, quoteAggregate.ProductId);
        }

        /// <inheritdoc/>
        public async Task<IQuoteExpirySettings> Retrieve(Guid tenantId, Guid productId)
        {
            Product product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
            return this.Retrieve(product);
        }

        public IQuoteExpirySettings Retrieve(Product product)
        {
            return product?.Details?.QuoteExpirySetting ?? QuoteExpirySettings.Default;
        }
    }
}
