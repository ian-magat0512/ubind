// <copyright file="FundingConfigurationParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding
{
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using UBind.Application.Funding.EFundExpress;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Application.Funding.RedPlanetPremiumFunding;

    /// <summary>
    /// Parser for funding configurations.
    /// </summary>
    public static class FundingConfigurationParser
    {
        private static JsonConverter[] converters = new JsonConverter[]
            {
                new GenericConverter<IConfigurationModel<IFundingConfiguration>>(
                    new TypeMap
                    {
                        { "PremiumFunding", typeof(PremiumFundingConfigurationModel) },
                        { "EFundExpress", typeof(EFundExpressFundingConfigurationModel) },
                        { "IQumulate", typeof(IqumulateConfigurationModel) },
                        { "RedPlanet", typeof(RedPlanetPremiumFundingPerEnvironmentConfigurationModel) },
                    }),
            };

        /// <summary>
        /// Parse funding configuration json.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <returns>A factory for creating funding configuration for a given environment.</returns>
        public static IConfigurationModel<IFundingConfiguration>? Parse(string json) =>
            JsonConvert.DeserializeObject<IConfigurationModel<IFundingConfiguration>>(json, converters);
    }
}
