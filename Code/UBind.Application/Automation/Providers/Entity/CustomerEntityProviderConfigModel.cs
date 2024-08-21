// <copyright file="CustomerEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="TenantEntityProvider"/>.
    /// </summary>
    public class CustomerEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the customer id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer account email.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? CustomerAccountEmail { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var customerReadModelRepository = dependencyProvider.GetRequiredService<ICustomerReadModelRepository>();
            return new CustomerEntityProvider(
                this.CustomerId?.Build(dependencyProvider),
                this.CustomerAccountEmail?.Build(dependencyProvider),
                customerReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
