// <copyright file="OrganisationEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="OrganisationEntityProvider"/>.
    /// </summary>
    public class OrganisationEntityProviderConfigModel : IBuilder<OrganisationEntityProvider>
    {
        /// <summary>
        /// Gets or sets the organisation id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organisation alias.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? OrganisationAlias { get; set; }

        /// <inheritdoc/>
        public OrganisationEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var organisationRepository = dependencyProvider.GetRequiredService<IOrganisationReadModelRepository>();
            return new OrganisationEntityProvider(
                this.OrganisationId?.Build(dependencyProvider),
                this.OrganisationAlias?.Build(dependencyProvider),
                organisationRepository,
                serialisedEntityFactory);
        }
    }
}
