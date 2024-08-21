// <copyright file="PerEnvironmentConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Factory for generating payment gateway configuration for a given environment.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration being generated.</typeparam>
    /// <typeparam name="TConfigurationInterface">They type of the interface the configuration implements and is accessed via.</typeparam>
    public class PerEnvironmentConfigurationModel<TConfiguration, TConfigurationInterface> : IConfigurationModel<TConfigurationInterface>
        where TConfiguration : class, TConfigurationInterface
    {
        private readonly Func<TConfiguration, TConfiguration, TConfiguration> overrideFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerEnvironmentConfigurationModel{TConfiguration, TConfigurationInterface}"/> class.
        /// </summary>
        /// <param name="overrideFunction">A function for generating an overridden configuration from a default and overrides.</param>
        protected PerEnvironmentConfigurationModel(Func<TConfiguration, TConfiguration, TConfiguration> overrideFunction)
        {
            this.overrideFunction = overrideFunction;
        }

        /// <summary>
        /// Gets the default account settings.
        /// </summary>
        [JsonProperty]
        public TConfiguration Default { get; private set; }

        /// <summary>
        /// Gets account settings overrides for dev environment.
        /// </summary>
        [JsonProperty]
        public TConfiguration Dev { get; private set; }

        /// <summary>
        /// Gets account settings overrides for staging environment.
        /// </summary>
        [JsonProperty]
        public TConfiguration Staging { get; private set; }

        /// <summary>
        /// Gets account settings overrides for production environment.
        /// </summary>
        [JsonProperty]
        public TConfiguration Production { get; private set; }

        /// <summary>
        /// Generate configuration for a given environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>Configuration.</returns>
        public virtual TConfigurationInterface Generate(DeploymentEnvironment environment)
        {
            var defaultConfig = this.Default;
            var overrides =
                environment == DeploymentEnvironment.Development
                ? this.Dev
                : environment == DeploymentEnvironment.Staging
                    ? this.Staging
                    : environment == DeploymentEnvironment.Production
                        ? this.Production
                        : null;
            return overrides == null
                ? this.Default
                : this.overrideFunction(this.Default, overrides);
        }
    }
}
