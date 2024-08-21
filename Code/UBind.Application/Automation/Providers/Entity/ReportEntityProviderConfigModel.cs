// <copyright file="ReportEntityProviderConfigModel.cs" company="uBind">
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
    public class ReportEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ReportId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var reportReadModelRepository = dependencyProvider.GetRequiredService<IReportReadModelRepository>();
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
            return new ReportEntityProvider(
                this.ReportId?.Build(dependencyProvider),
                reportReadModelRepository,
                serialisedEntityFactory,
                cachingResolver);
        }
    }
}
