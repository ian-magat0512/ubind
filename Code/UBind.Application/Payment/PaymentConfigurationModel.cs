// <copyright file="PaymentConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


////namespace UBind.Application.Payment
////{
////    using System;
////    using Newtonsoft.Json;
////    using UBind.Domain;

////    /// <summary>
////    /// Factory for generating payment gateway configuration for a given environment.
////    /// </summary>
////    /// <typeparam name="TPaymentConfiguration">The type of configuration being generated.</typeparam>
////    public class PaymentConfigurationModel<TPaymentConfiguration> : IConfigurationModel<IPaymentConfiguration>
////        where TPaymentConfiguration : class, IPaymentConfiguration
////    {
////        private readonly Func<TPaymentConfiguration, TPaymentConfiguration, TPaymentConfiguration> overrideFunction;

////        /// <summary>
////        /// Initializes a new instance of the <see cref="PaymentConfigurationModel{TPaymentConfiguration}"/> class.
////        /// </summary>
////        /// <param name="overrideFunction">A function for generating an overridden configuration from a default and overrides.</param>
////        protected PaymentConfigurationModel(Func<TPaymentConfiguration, TPaymentConfiguration, TPaymentConfiguration> overrideFunction)
////        {
////            this.overrideFunction = overrideFunction;
////        }

////        /// <summary>
////        /// Gets the default account settings.
////        /// </summary>
////        [JsonProperty]
////        public TPaymentConfiguration Default { get; private set; }

////        /// <summary>
////        /// Gets account settings overrides for dev environment.
////        /// </summary>
////        [JsonProperty]
////        public TPaymentConfiguration Dev { get; private set; }

////        /// <summary>
////        /// Gets account settings overrides for staging environment.
////        /// </summary>
////        [JsonProperty]
////        public TPaymentConfiguration Staging { get; private set; }

////        /// <summary>
////        /// Gets account settings overrides for production environment.
////        /// </summary>
////        [JsonProperty]
////        public TPaymentConfiguration Production { get; private set; }

////        /// <summary>
////        /// Generate configuration for a given environment.
////        /// </summary>
////        /// <param name="environment">The environment.</param>
////        /// <returns>Configuration.</returns>
////        public IPaymentConfiguration Generate(DeploymentEnvironment environment)
////        {
////            var defaultConfig = this.Default;
////            var overrides =
////                environment == DeploymentEnvironment.Development
////                ? this.Dev
////                : environment == DeploymentEnvironment.Staging
////                    ? this.Staging
////                    : environment == DeploymentEnvironment.Production
////                        ? this.Production
////                        : null;
////            return overrides == null
////                ? this.Default
////                : this.overrideFunction(this.Default, overrides);
////        }
////    }
////}
