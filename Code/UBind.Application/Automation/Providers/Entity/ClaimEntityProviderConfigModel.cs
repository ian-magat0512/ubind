// <copyright file="ClaimEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Model for building an instance of <see cref="ClaimEntityProvider"/>.
    /// </summary>
    public class ClaimEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the claim id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the claim reference.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the claim number.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the claim environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var claimReadModelRepository = dependencyProvider.GetRequiredService<IClaimReadModelRepository>();
            return new ClaimEntityProvider(
                this.ClaimId?.Build(dependencyProvider),
                this.ClaimReference?.Build(dependencyProvider),
                this.ClaimNumber?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                claimReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
