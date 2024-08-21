// <copyright file="ProductFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    public static class ProductFactory
    {
        public const string DefaultProductAlias = "fake-product";

        public const string DefaultProductName = "Fake Product";

        public static readonly Guid DefaultId = new Guid("ccae2079-2ebc-4200-879d-866fc82e6afa");

        public static IClock Clock { get; set; } = SystemClock.Instance;

        public static Product Create(Guid tenantId, Guid productId, string alias = null)
        {
            return new Product(tenantId, productId, DefaultProductName, alias ?? productId.ToString(), Clock.Now());
        }

        public static Product Create(Guid? tenantId = null, Guid? productId = null)
        {
            productId = productId ?? ProductFactory.DefaultId;
            tenantId = tenantId ?? TenantFactory.DefaultId;

            return new Product(tenantId.Value, productId.Value, DefaultProductName, productId.ToString(), Clock.Now());
        }

        public static Product Create(Guid tenantId)
        {
            var productId = Guid.NewGuid();
            return new Product(tenantId, productId, DefaultProductName, productId.ToString(), Clock.Now());
        }

        public static Product WithExpirySettings(this Product product)
        {
            var newDetails = new ProductDetails(
                product.Details.Name,
                product.Details.Alias,
                product.Details.Disabled,
                product.Details.Deleted,
                Clock.Now(),
                productQuoteExpirySetting: new QuoteExpirySettings(30, true));
            product.Update(newDetails);
            return product;
        }
    }
}
