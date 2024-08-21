// <copyright file="EnvironmentTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for per-environment text provider.
    /// </summary>
    public class EnvironmentTextProviderModel : IExporterModel<ITextProvider>
    {
        /// <summary>
        /// Gets or sets the text provider to use when no provider is specified for the current environment.
        /// </summary>
        public IExporterModel<ITextProvider> Default { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the development environment.
        /// </summary>
        public IExporterModel<ITextProvider> Development { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the stagingt environment.
        /// </summary>
        public IExporterModel<ITextProvider> Staging { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the production environment.
        /// </summary>
        public IExporterModel<ITextProvider> Production { get; set; }

        /// <inheritdoc/>
        public ITextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            try
            {
                return new EnvironmentTextProvider(
                    this.Default?.Build(dependencyProvider, productConfiguration),
                    this.Development?.Build(dependencyProvider, productConfiguration),
                    this.Staging?.Build(dependencyProvider, productConfiguration),
                    this.Production?.Build(dependencyProvider, productConfiguration));
            }
            catch (ArgumentException ex)
            {
                throw new IntegrationConfigurationException(
                    $"Integration configuration error: {ex.Message}",
                    ex);
            }
        }
    }
}
