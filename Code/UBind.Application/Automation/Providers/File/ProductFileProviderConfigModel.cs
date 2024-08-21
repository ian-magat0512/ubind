// <copyright file="ProductFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Model for creating an instance of <see cref="ProductFileProviderConfigModel"/>.
    /// </summary>
    public class ProductFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the root folder (e.g. Quote, Claim, Shared).
        /// </summary>
        public string Repository { get; set; }

        /// <summary>
        /// Gets or sets the file visibility to the user(e.g. Public or Private).
        /// </summary>
        public string Visibility { get; set; }

        /// <summary>
        /// Gets or sets the path within the file assets of the product where the file should be loaded from, defined by a text provider.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> FilePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the alias of the product from which the file should be loaded.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ProductAlias { get; set; }

        /// <summary>
        /// Gets or sets the product environment for the product from which the file should be loaded.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            var releaseQueryService = dependencyProvider.GetRequiredService<IReleaseQueryService>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            var cqrsMediator = dependencyProvider.GetRequiredService<ICqrsMediator>();
            return new ProductFileProvider(
                this.Repository,
                this.Visibility,
                this.FilePath.Build(dependencyProvider),
                this.OutputFileName?.Build(dependencyProvider),
                this.ProductAlias?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                cqrsMediator,
                releaseQueryService,
                cachingResolver);
        }
    }
}
