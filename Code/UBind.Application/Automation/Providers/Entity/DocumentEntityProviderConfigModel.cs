// <copyright file="DocumentEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="DocumentEntityProvider"/>.
    /// </summary>
    public class DocumentEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the document id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? DocumentId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var documentRepository = dependencyProvider.GetRequiredService<IQuoteDocumentReadModelRepository>();
            return new DocumentEntityProvider(
                this.DocumentId?.Build(dependencyProvider),
                documentRepository,
                serialisedEntityFactory);
        }
    }
}
