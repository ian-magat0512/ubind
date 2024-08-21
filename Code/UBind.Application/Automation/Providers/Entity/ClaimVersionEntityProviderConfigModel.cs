// <copyright file="ClaimVersionEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Model for building an instance of <see cref="ClaimVersionEntityProvider"/>.
    /// </summary>
    public class ClaimVersionEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the claim version id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ClaimVersionId { get; set; }

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
        /// Gets or sets the claim version number.
        /// </summary>
        public IBuilder<IProvider<Data<long>>>? VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the claim environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Environment { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var claimVersionReadModelRepository = dependencyProvider.GetRequiredService<IClaimVersionReadModelRepository>();
            return new ClaimVersionEntityProvider(
                this.ClaimVersionId?.Build(dependencyProvider),
                this.ClaimId?.Build(dependencyProvider),
                this.ClaimReference?.Build(dependencyProvider),
                this.ClaimNumber?.Build(dependencyProvider),
                this.VersionNumber?.Build(dependencyProvider),
                this.Environment?.Build(dependencyProvider),
                claimVersionReadModelRepository,
                serialisedEntityFactory);
        }
    }
}
