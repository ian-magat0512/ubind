// <copyright file="PolicyEntityProviderConfigModel.cs" company="uBind">
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
    public class PolicyEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the policy id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the  policy number.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the policy environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var policyReadModelRepository = dependencyProvider.GetRequiredService<IPolicyReadModelRepository>();
            return new PolicyEntityProvider(
                this.PolicyId?.Build(dependencyProvider),
                this.PolicyNumber?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                policyReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
