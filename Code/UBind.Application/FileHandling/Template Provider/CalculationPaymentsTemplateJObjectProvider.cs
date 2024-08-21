// <copyright file="CalculationPaymentsTemplateJObjectProvider.cs" company="uBind">
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
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Export.ViewModels;
    using UBind.Domain;

    /// <summary>
    /// Calculation payments template JSON object provider.
    /// </summary>
    public class CalculationPaymentsTemplateJObjectProvider : IJObjectProvider
    {
        private readonly ILogger<CalculationPaymentsTemplateJObjectProvider> logger = new Logger<CalculationPaymentsTemplateJObjectProvider>(new NullLoggerFactory());

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
                if (quote.LatestCalculationResult != null)
                {
                    var calculationResultData = quote.LatestCalculationResult.Data;
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    };

                    calculationDataObj["payment"]["payableComponents"] = PriceBreakdownViewModelHelper.GenerateViewModelAsJObject(calculationResultData.PayablePrice);
                    calculationDataObj["payment"]["refundComponents"] = PriceBreakdownViewModelHelper.GenerateViewModelAsJObject(calculationResultData.RefundBreakdown);
                }

                IJsonObjectParser parser = new GenericJObjectParser(
                    string.Empty, calculationDataObj.SelectToken("payment"));
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
