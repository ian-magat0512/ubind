// <copyright file="RoleEntityProviderConfigModel.cs" company="uBind">
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
    /// Model for building an instance of <see cref="CustomerEntityProvider"/>.
    /// </summary>
    public class RoleEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? RoleId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var roleRepository = dependencyProvider.GetRequiredService<IRoleRepository>();
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new RoleEntityProvider(
                this.RoleId?.Build(dependencyProvider),
                roleRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
