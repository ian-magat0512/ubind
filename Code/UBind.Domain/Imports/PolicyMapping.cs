// <copyright file="PolicyMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Container that represents the mapped policy properties.
    /// </summary>
    public class PolicyMapping
    {
        /// <summary>
        /// Gets the customer email of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
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
        public JToken FormData { get; private set; }

        /// <summary>
        /// Gets the calculation result of the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JToken CalculationResult { get; private set; }

        /// <summary>
        /// Retrieves the default policy mapping to be used.
        /// </summary>
        /// <param name="customFormDataMapping">The custom form data mapping token , if available.</param>
        /// <param name="customCalculationMapping">The custom calculation mapping token, if available.</param>
        /// <returns>An instance of <see cref="PolicyMapping"/> with default values.</returns>
        public static PolicyMapping Default(JToken customFormDataMapping = null, JToken customCalculationMapping = null)
        {
            var defaultFormDataMappingToken = DataMappingHelper.DefaultFormDataMap;
            var defaultCalculationMappingToken = DataMappingHelper.DefaultCalculationResultMap;

            var defaultMapping = new PolicyMapping
            {
                CustomerEmail = "CustomerEmail",
                CustomerName = "CustomerName",
                PolicyNumber = "PolicyNumber",
                InceptionDate = "InceptionDate",
                ExpiryDate = "ExpiryDate",
                AgentEmail = "AgentEmail",
                FormData = customFormDataMapping ?? defaultFormDataMappingToken,
                CalculationResult = customCalculationMapping ?? defaultCalculationMappingToken,
            };

            return defaultMapping;
        }
    }
}
