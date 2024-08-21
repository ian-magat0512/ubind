// <copyright file="IssuePolicyActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;

    public class IssuePolicyActionData : ActionData
    {
        public IssuePolicyActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.IssuePolicyAction, clock)
        {
        }

        [JsonConstructor]
        public IssuePolicyActionData()
            : base(ActionType.IssuePolicyAction)
        {
        }

        [JsonProperty("policyId")]
        public Guid PolicyId { get; set; }

        [JsonProperty(PropertyName = "quoteId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? QuoteId { get; set; }

        [JsonProperty(PropertyName = "organisationId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? OrganisationId { get; set; }

        [JsonProperty(PropertyName = "productId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ProductId { get; set; }

        [JsonProperty(PropertyName = "customerId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? CustomerId { get; set; }

        [JsonProperty(PropertyName = "isTestData", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTestData { get; set; }

        [JsonProperty(PropertyName = "environment", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public DeploymentEnvironment? Environment { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyDictionary<string, object>? AdditionalProperties { get; set; }
    }
}
