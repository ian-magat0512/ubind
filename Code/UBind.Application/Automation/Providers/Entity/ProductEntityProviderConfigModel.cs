// <copyright file="ProductEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Model for building an instance of <see cref="ProductEntityProvider"/>.
    /// </summary>
    public class ProductEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the product id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product alias.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ProductAlias { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var productRepository = dependencyProvider.GetRequiredService<IProductRepository>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new ProductEntityProvider(
                this.ProductId?.Build(dependencyProvider),
                this.ProductAlias?.Build(dependencyProvider),
                productRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
