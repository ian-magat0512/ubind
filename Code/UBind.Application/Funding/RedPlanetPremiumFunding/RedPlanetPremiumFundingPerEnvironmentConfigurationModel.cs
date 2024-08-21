// <copyright file="RedPlanetPremiumFundingPerEnvironmentConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding;

using Newtonsoft.Json;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.DataLocator;
using UBind.Domain.Exceptions;

public class RedPlanetPremiumFundingPerEnvironmentConfigurationModel : PerEnvironmentConfigurationModel<RedPlanetPremiumFundingConfiguration, IFundingConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedPlanetPremiumFundingPerEnvironmentConfigurationModel"/> class.
    /// </summary>
    public RedPlanetPremiumFundingPerEnvironmentConfigurationModel()
        : base((defaults, overrides) => new RedPlanetPremiumFundingConfiguration(defaults, overrides))
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

    public override IFundingConfiguration Generate(DeploymentEnvironment environment)
    {
        var fundingConfiguration = base.Generate(environment) as RedPlanetPremiumFundingConfiguration;
        if (fundingConfiguration == null)
        {
            throw new ErrorException(Errors.Payment.Funding.ProviderError(
                new string[] { "Unable to generate Arteva Premium Funding configuration" },
                false));
        }
        fundingConfiguration.SetDataLocators(this.QuoteDatumLocations, this.DataLocators);
        return fundingConfiguration;
    }
}
