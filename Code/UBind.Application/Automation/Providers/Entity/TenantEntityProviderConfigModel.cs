// <copyright file="TenantEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for building an instance of <see cref="TenantEntityProvider"/>.
    /// </summary>
    public class TenantEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant alias.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? TenantAlias { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var tenantRepository = dependencyProvider.GetRequiredService<ITenantRepository>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new TenantEntityProvider(
                this.TenantId?.Build(dependencyProvider),
                this.TenantAlias?.Build(dependencyProvider),
                tenantRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
