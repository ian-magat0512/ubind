// <copyright file="QuoteImportData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class QuoteImportData
    {
        private readonly IList<JToken> removeList = new List<JToken>();

        public QuoteImportData(JObject obj, QuoteMapping mappingData)
        {
            if (obj == null || mappingData == null)
            {
                return;
            }

            this.CustomerEmail = obj.Value<string>(mappingData.CustomerEmail);
            this.CustomerName = obj.Value<string>(mappingData.CustomerName);
            this.QuoteState = obj.Value<string>(mappingData.State);
            this.QuoteNumber = obj.Value<string>(mappingData.QuoteNumber);

            Action<JProperty> action = (property) =>
            {
                if (property.Value.Type == JTokenType.String)
                {
                    var extractedValue = obj.Value<string>(property.Value.ToString());
                    if (extractedValue != null)
                    {
                        property.Value = extractedValue;
                    }
                    else
                    {
                        this.removeList.Add(property);
                    }
                }
            };

            var newCalculationResult = mappingData.CalculationResult.DeepClone();
            this.removeList.Clear();
            DataMappingHelper.WalkNode(newCalculationResult, action);
            RemoveTokenFromList();

            // specify static values
            var calculationResultJson = (JObject)newCalculationResult;
            this.CalculationResult = DataMappingHelper.SetCalculationDefaults(calculationResultJson);

            var nodes = mappingData.FormData.DeepClone();
            this.removeList.Clear();
            DataMappingHelper.WalkNode(nodes, action);
            RemoveTokenFromList();
            this.FormData = nodes.ToString();

            void RemoveTokenFromList()
            {
                foreach (JToken token in this.removeList)
                {
                    token.Remove();
                }
            }
        }

        [JsonConstructor]
        public QuoteImportData()
        {
        }

        [JsonProperty(Required = Required.AllowNull)]
        public string CustomerEmail { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string CustomerName { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string FormData { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string CalculationResult { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string QuoteState { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the timezone with which to interpret dates, and which is
        /// the operable time zone for the quote.
        /// This field is optional, and if not provided, a timezone setting for the product
        /// or organisation may be used.
        /// </summary>
        [JsonProperty]
        public string TimeZoneId { get; private set; }
    }
}
