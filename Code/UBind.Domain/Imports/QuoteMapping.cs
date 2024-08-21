// <copyright file="QuoteMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class QuoteMapping
    {
        /// <summary>
        /// Gets the customer email of the quote.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CustomerEmail { get; private set; }

        /// <summary>
        /// Gets the customer name of the quote.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CustomerName { get; private set; }

        /// <summary>
        /// Gets the form data of the quote.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JToken FormData { get; private set; }

        /// <summary>
        /// Gets the calculation result of the quote.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JToken CalculationResult { get; private set; }

        /// <summary>
        /// Gets the state the quote should be in after importation.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string State { get; private set; }

        /// <summary>
        /// Gets the reference number for the quote.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string QuoteNumber { get; private set; }

        public static QuoteMapping Default(JToken customFormDataMapping = null, JToken customCalcResultMapping = null)
        {
            var defaultMapping = new QuoteMapping
            {
                CustomerEmail = "CustomerEmail",
                CustomerName = "CustomerName",
                QuoteNumber = "QuoteNumber",
                State = "QuoteState",
                FormData = customFormDataMapping ?? DataMappingHelper.DefaultFormDataMap,
                CalculationResult = customCalcResultMapping ?? DataMappingHelper.DefaultCalculationResultMap,
            };

            return defaultMapping;
        }
    }
}
