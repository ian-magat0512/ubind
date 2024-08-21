// <copyright file="PolicyTransactionEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="PolicyEntityProvider"/>.
    /// </summary>
    public class PolicyTransactionEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the policy transaction id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PolicyTransactionId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var policyTransactionReadModelRepository = dependencyProvider.GetRequiredService<IPolicyTransactionReadModelRepository>();
            return new PolicyTransactionEntityProvider(
                this.PolicyTransactionId?.Build(dependencyProvider),
                policyTransactionReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
