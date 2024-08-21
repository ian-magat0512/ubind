// <copyright file="PortalEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// Model for building an instance of <see cref="PortalEntityProvider"/>.
    /// </summary>
    public class PortalEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the portal id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PortalId { get; set; }

        /// <summary>
        /// Gets or sets the portal alias.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PortalAlias { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var portalRepository = dependencyProvider.GetRequiredService<IPortalReadModelRepository>();
            return new PortalEntityProvider(
                this.PortalId?.Build(dependencyProvider),
                this.PortalAlias?.Build(dependencyProvider),
                portalRepository,
                serialisedEntityFactory);
        }
    }
}
