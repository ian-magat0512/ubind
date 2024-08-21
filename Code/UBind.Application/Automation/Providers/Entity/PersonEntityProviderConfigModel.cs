// <copyright file="PersonEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="PersonEntityProvider"/>.
    /// </summary>
    public class PersonEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PersonId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var personReadModelRepository = dependencyProvider.GetRequiredService<IPersonReadModelRepository>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new PersonEntityProvider(
                this.PersonId?.Build(dependencyProvider),
                personReadModelRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
