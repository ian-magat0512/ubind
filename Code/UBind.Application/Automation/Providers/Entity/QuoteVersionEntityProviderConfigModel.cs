// <copyright file="QuoteVersionEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="QuoteVersionEntityProvider"/>.
    /// </summary>
    public class QuoteVersionEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the quote version id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? QuoteVersionId { get; set; }

        /// <summary>
        /// Gets or sets the quote id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the quote reference.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? QuoteReference { get; set; }

        /// <summary>
        /// Gets or sets the quote version number.
        /// </summary>
        public IBuilder<IProvider<Data<long>>>? VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the quote environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var quoteVersionReadModelRepository = dependencyProvider.GetRequiredService<IQuoteVersionReadModelRepository>();
            return new QuoteVersionEntityProvider(
                this.QuoteVersionId?.Build(dependencyProvider),
                this.QuoteId?.Build(dependencyProvider),
                this.QuoteReference?.Build(dependencyProvider),
                this.VersionNumber?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                quoteVersionReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
