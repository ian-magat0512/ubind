// <copyright file="CalculationStateTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Calculation state template JSON object provider.
    /// </summary>
    public class CalculationStateTemplateJObjectProvider : IJObjectProvider
    {
        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var malformedJson = quote.LatestCalculationResult?.Data?.Json;
            if (malformedJson == null)
            {
                return Task.CompletedTask;
            }

            var verifiedCalculationJson = malformedJson.Replace(",}", "}");

            try
            {
                JObject calculationDataObj = JObject.Parse(verifiedCalculationJson);
                calculationDataObj.Remove("questions");

                IJsonObjectParser parser = new GenericJObjectParser(
                    "Calculation", calculationDataObj.SelectToken("state"));
                if (parser.JsonObject != null)
                {
                    this.JsonObject = parser.JsonObject;
                }
            }
            catch (JsonReaderException)
            {
                // Nothing to do
            }

            return Task.CompletedTask;
        }
    }
}
