// <copyright file="PremiumFundingConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;

    /// <summary>
    /// Model for premium funding configuration parsing.
    /// </summary>
    public class PremiumFundingConfigurationModel : PerEnvironmentConfigurationModel<PremiumFundingConfiguration, IFundingConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingConfigurationModel"/> class.
        /// </summary>
        public PremiumFundingConfigurationModel()
            : base((defaults, overrides) => new PremiumFundingConfiguration(defaults, overrides))
        {
        }

        /// <summary>
        /// Gets the configured quote data locations.
        /// </summary>
        [JsonProperty]
        public ConfiguredQuoteDatumLocations QuoteDatumLocations { get; private set; }

        /// <summary>
        /// Gets the configured quote data locations.
        /// </summary>
        [JsonProperty]
        public DataLocators DataLocators { get; private set; }

        /// <summary>
        /// Sets the quote data location to use in premium funding proposal creation.
        /// </summary>
        [Obsolete("Deprecated. Instead we will use QuoteDatumLocations")]
        [JsonProperty]
        public ConfiguredQuoteDatumLocations QuoteDataLocator
        {
            set { this.QuoteDatumLocations = value; }
        }

        /// <inheritdoc/>
        public override IFundingConfiguration Generate(DeploymentEnvironment environment)
        {
            var premiumFundingConfiguration = base.Generate(environment) as PremiumFundingConfiguration;
            premiumFundingConfiguration.SetDataLocators(this.QuoteDatumLocations, this.DataLocators);
            return premiumFundingConfiguration;
        }
    }
}
