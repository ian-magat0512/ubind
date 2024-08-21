// <copyright file="EnvironmentTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Text provider that returns fixed text.
    /// </summary>
    public class EnvironmentTextProvider : ITextProvider
    {
        private readonly Dictionary<DeploymentEnvironment, ITextProvider> textProviders =
            new Dictionary<DeploymentEnvironment, ITextProvider>();

        private readonly ITextProvider defaultTextProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentTextProvider"/> class.
        /// </summary>
        /// <param name="defaultTextProvider">Text provider to use when none specified for current environment.</param>
        /// <param name="developmentTextProvider">Text provider to use in development environment.</param>
        /// <param name="stagingTextProvider">Text provider to use in staging environment.</param>
        /// <param name="productionTextProvider">Text provider to use in production environment.</param>
        public EnvironmentTextProvider(
            ITextProvider defaultTextProvider,
            ITextProvider developmentTextProvider,
            ITextProvider stagingTextProvider,
            ITextProvider productionTextProvider)
        {
            var defaultProviderRequired =
                stagingTextProvider == null ||
                developmentTextProvider == null ||
                productionTextProvider == null;
            if (defaultProviderRequired && defaultTextProvider == null)
            {
                throw new ArgumentException("If a text provider for any environment is missing, a default must be provided.");
            }

            this.defaultTextProvider = defaultTextProvider;
            this.textProviders[DeploymentEnvironment.Development] = developmentTextProvider;
            this.textProviders[DeploymentEnvironment.Staging] = stagingTextProvider;
            this.textProviders[DeploymentEnvironment.Production] = productionTextProvider;
        }

        /// <inheritdoc />
        public async Task<string> Invoke(Domain.ApplicationEvent applicationEvent)
        {
            var environment = applicationEvent.Aggregate.Environment;
            var textProvider = this.textProviders[environment] ?? this.defaultTextProvider;
            return await textProvider.Invoke(applicationEvent);
        }
    }
}
