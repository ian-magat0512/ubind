// <copyright file="CalculationRiskTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Calculation risk template JSON object provider.
    /// </summary>
    public class CalculationRiskTemplateJObjectProvider : IJObjectProvider
    {
        private readonly ILogger<CalculationRiskTemplateJObjectProvider> logger = new Logger<CalculationRiskTemplateJObjectProvider>(new NullLoggerFactory());

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
                calculationDataObj.Remove("state");
                calculationDataObj.Remove("triggers");
                calculationDataObj.Remove("payment");

                IJsonObjectParser parser = new GenericJObjectParser(
                    string.Empty, calculationDataObj);
                if (parser.JsonObject != null)
                {
                    this.JsonObject = parser.JsonObject;
                }
            }
            catch (JsonReaderException ex)
            {
                this.logger.LogError(ex, ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
