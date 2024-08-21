// <copyright file="PerformQuoteCalculationActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class PerformQuoteCalculationActionData : ActionData
    {
        public PerformQuoteCalculationActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.PerformQuoteCalculationAction, clock)
        {
        }

        [JsonConstructor]
        public PerformQuoteCalculationActionData()
            : base(ActionType.PerformQuoteCalculationAction)
        {
        }

        /// <summary>
        /// Gets or sets the reference to the quote, which we'll store just as the stringified Quote ID.
        /// </summary>
        [JsonProperty("quote", NullValueHandling = NullValueHandling.Ignore)]
        public string? Quote { get; set; }

        /// <summary>
        /// Gets or sets the reference to the policy, which we'll store just as the stringified Policy ID.
        /// </summary>
        [JsonProperty("policy", NullValueHandling = NullValueHandling.Ignore)]
        public string? Policy { get; set; }

        /// <summary>
        /// Gets or sets the reference to the product, which we'll store just as the stringified Product ID.
        /// </summary>
        [JsonProperty("product", NullValueHandling = NullValueHandling.Ignore)]
        public string? Product { get; set; }

        /// <summary>
        /// Gets or sets the Deployment Environment for the quote calcuation.
        /// </summary>
        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction type used to perform the quote calculation.
        /// </summary>
        [JsonProperty("policyTransactionType", NullValueHandling = NullValueHandling.Ignore)]
        public string? PolicyTransactionType { get; set; }

        [JsonProperty("inputData")]
        public JObject? InputData { get; set; }

        [JsonProperty("calculationResult", NullValueHandling = NullValueHandling.Ignore)]
        public JObject? CalculationResult { get; set; }
    }
}
