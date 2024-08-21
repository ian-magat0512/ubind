// <copyright file="PaymentConfigurationParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Payment.Zai;

    /// <summary>
    /// For parsing payment configuration json.
    /// </summary>
    public class PaymentConfigurationParser
    {
        private static JsonConverter[] converters = new JsonConverter[]
                {
                    new GenericConverter<IConfigurationModel<IPaymentConfiguration>>(
                        new TypeMap
                        {
                            { "eway", typeof(EwayConfigurationModel) },
                            { "deft", typeof(DeftConfigurationModel) },
                            { "stripe", typeof(StripeConfigurationModel) },
                            { "zai", typeof(ZaiConfigurationModel) },
                        }),
                };

        /// <summary>
        /// Parse payment configuration json.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <returns>A factory for creating configurations of a given type for a given environment.</returns>
        public static IConfigurationModel<IPaymentConfiguration> Parse(string json) =>
            JsonConvert.DeserializeObject<IConfigurationModel<IPaymentConfiguration>>(json, converters);
    }
}
