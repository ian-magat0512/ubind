// <copyright file="UserEntityProviderConfigModel.cs" company="uBind">
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
    /// Model for building an instance of <see cref="CustomerEntityProvider"/>.
    /// </summary>
    public class UserEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? UserId { get; set; }

        /// <summary>
        /// Gets or sets the user account email.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? UserAccountEmail { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var userReadModelRepository = dependencyProvider.GetRequiredService<IUserReadModelRepository>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new UserEntityProvider(
                this.UserId?.Build(dependencyProvider),
                this.UserAccountEmail?.Build(dependencyProvider),
                userReadModelRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
