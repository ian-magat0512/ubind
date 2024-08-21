// <copyright file="EnvironmentTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Generates a text value based on the current product environment.
    /// </summary>
    public class EnvironmentTextProvider : IProvider<Data<string>>
    {
        private readonly Dictionary<DeploymentEnvironment, IProvider<Data<string>>> textProviders =
            new Dictionary<DeploymentEnvironment, IProvider<Data<string>>>();

        private readonly IProvider<Data<string>> defaultEnvironmentTextProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentTextProvider"/> class.
        /// </summary>
        /// <param name="defaultTextProvider">"The default output of this text provider, if the current product environment does not have an entry.</param>
        /// <param name="developmentTextProvider">The output of this text provider if the current product environment is development.</param>
        /// <param name="stagingTextProvider">The output of this text provider if the current product environment is staging.</param>
        /// <param name="productionTextProvider">The output of this text provider if the current product environment is production.</param>
        public EnvironmentTextProvider(
            IProvider<Data<string>> defaultTextProvider,
            IProvider<Data<string>> developmentTextProvider,
            IProvider<Data<string>> stagingTextProvider,
            IProvider<Data<string>> productionTextProvider)
        {
            var defaultProviderRequred =
                developmentTextProvider == null ||
                stagingTextProvider == null ||
                productionTextProvider == null;
            if (defaultProviderRequred && defaultTextProvider == null)
            {
                var errorData = new JObject() { { ErrorDataKey.ErrorMessage, "Default text provider must be provided if a text provider for any environment is not configured." }, };
                throw new ErrorException(UBind.Domain.Errors.Automation.InvalidAutomationConfiguration(errorData));
            }

            this.defaultEnvironmentTextProvider = defaultTextProvider;
            this.textProviders[DeploymentEnvironment.Development] = developmentTextProvider;
            this.textProviders[DeploymentEnvironment.Staging] = stagingTextProvider;
            this.textProviders[DeploymentEnvironment.Production] = productionTextProvider;
        }

        public string SchemaReferenceKey => "environmentText";

        /// <summary>
        /// Provides a text value based on the current environment configuration.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A text string.</returns>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var environment = providerContext.AutomationData.System.Environment;
            var isProviderConfigured = this.textProviders.TryGetValue(environment, out IProvider<Data<string>> textProvider);
            if (isProviderConfigured)
            {
                return await textProvider.Resolve(providerContext);
            }

            if (this.defaultEnvironmentTextProvider == null)
            {
                throw new ErrorException(
                    Errors.Automation.ProviderParameterMissing(
                        "default",
                        this.SchemaReferenceKey));
            }

            return await this.defaultEnvironmentTextProvider.Resolve(providerContext);
        }
    }
}
