// <copyright file="PolicyImportData.cs" company="uBind">
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

    /// <summary>
    /// Represents a policy import object.
    /// </summary>
    public class PolicyImportData
    {
        private readonly IList<JToken> removeList = new List<JToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyImportData"/> class.
        /// </summary>
        /// <param name="jObject">Represents a JSON object.</param>
        /// <param name="mapping">Represents the policy mapping definition.</param>
        public PolicyImportData(JObject jObject, PolicyMapping mapping)
        {
            if (jObject == null || mapping == null)
            {
                return;
            }

            this.CustomerEmail = jObject.Value<string>(mapping.CustomerEmail);
            this.CustomerName = jObject.Value<string>(mapping.CustomerName);
            this.PolicyNumber = jObject.Value<string>(mapping.PolicyNumber);
            this.InceptionDate = jObject.Value<string>(mapping.InceptionDate);
            this.ExpiryDate = jObject.Value<string>(mapping.ExpiryDate);
            this.AgentEmail = (string?)jObject.GetValue(mapping.AgentEmail);

            Action<JProperty> action = (prop) =>
            {
                if (prop.Value.Type == JTokenType.String)
                {
                    var extractedValue = jObject.Value<string>(prop.Value.ToString());
                    if (extractedValue != null)
                    {
                        prop.Value = extractedValue;
                    }
                    else
                    {
                        this.removeList.Add(prop);
                    }
                }
            };

            var newCalculationResult = mapping.CalculationResult.DeepClone();
            this.removeList.Clear();
            DataMappingHelper.WalkNode(newCalculationResult, action);
            RemoveTokenFromList();

            // specify static values
            var calculationResultJson = (JObject)newCalculationResult;
            this.CalculationResult = DataMappingHelper.SetCalculationDefaults(calculationResultJson);

            var nodes = mapping.FormData.DeepClone();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyImportData"/> class.
        /// </summary>
        [JsonConstructor]
        public PolicyImportData()
        {
        }

        /// <summary>
        /// Gets the customer email of the policy.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string CustomerEmail { get; private set; }

        /// <summary>
        /// Gets the customer name of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CustomerName { get; private set; }

        /// <summary>
        /// Gets the policy number assigned to the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the inception date of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string InceptionDate { get; private set; }

        /// <summary>
        /// Gets the expiry date of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ExpiryDate { get; private set; }

        [JsonProperty]
        public string? AgentEmail { get; private set; }

        /// <summary>
        /// Gets the form data of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string FormData { get; private set; }

        /// <summary>
        /// Gets the calculation result of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CalculationResult { get; private set; }

        /// <summary>
        /// Gets the timezone with which to interpret dates, and which is
        /// the operable time zone for the policy.
        /// This field is optional, and if not provided, a timezone setting for the product
        /// or organisation may be used.
        /// </summary>
        [JsonProperty]
        public string TimeZoneId { get; private set; }
    }
}
