// <copyright file="SystemEventEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for building an instance of <see cref="SystemEventEntityProvider"/>.
    /// </summary>
    public class SystemEventEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? EventId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var systemEventRepository = dependencyProvider.GetRequiredService<ISystemEventRepository>();
            return new SystemEventEntityProvider(
                this.EventId?.Build(dependencyProvider),
                systemEventRepository,
                serialisedEntityFactory);
        }
    }
}
