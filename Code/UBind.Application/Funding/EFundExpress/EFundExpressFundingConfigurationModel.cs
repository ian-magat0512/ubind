// <copyright file="EFundExpressFundingConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.EFundExpress
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Model for Principal Finance/EFundExpress funding configuration parsing.
    /// </summary>
    public class EFundExpressFundingConfigurationModel : PerEnvironmentConfigurationModel<EFundExpressProductConfiguration, IFundingConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EFundExpressFundingConfigurationModel"/> class.
        /// </summary>
        public EFundExpressFundingConfigurationModel()
            : base((defaults, overrides) => new EFundExpressProductConfiguration(defaults, overrides))
        {
        }

        /// <summary>
        /// Gets the configured quote data locations.
        /// </summary>
        [JsonProperty]
        public ConfiguredQuoteDatumLocations? QuoteDatumLocations { get; private set; }

        /// <summary>
        /// Gets the configured quote data locations.
        /// </summary>
        [JsonProperty]
        public DataLocators? DataLocators { get; private set; }

        /// <summary>
        /// Sets the quote data location to use in premium funding proposal creation.
        /// </summary>
        [Obsolete("Deprecated. Instead we will use QuoteDatumLocations")]
        [JsonProperty]
        public ConfiguredQuoteDatumLocations? QuoteDataLocator
        {
            set { this.QuoteDatumLocations = value; }
        }

        /// <inheritdoc/>
        public override IFundingConfiguration Generate(DeploymentEnvironment environment)
        {
            var efundExpressConfiguration = base.Generate(environment) as EFundExpressProductConfiguration;
            efundExpressConfiguration = EntityHelper.ThrowIfNotFound(efundExpressConfiguration, "efundExpressConfiguration");
            efundExpressConfiguration.SetDataLocators(this.QuoteDatumLocations, this.DataLocators);
            return efundExpressConfiguration;
        }
    }
}
