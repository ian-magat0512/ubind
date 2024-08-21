// <copyright file="CreateQuoteActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class CreateQuoteActionData : ActionData
    {
        public CreateQuoteActionData(
           string name,
           string alias,
           IClock clock)
           : base(name, alias, ActionType.CreateQuoteAction, clock)
        {
        }

        public CreateQuoteActionData(
            string name,
            string alias,
            ActionType type,
            IClock clock)
            : base(name, alias, type, clock)
        {
        }

        [JsonProperty("policyTransactionType", NullValueHandling = NullValueHandling.Ignore)]
        public string? PolicyTransactionType { get; set; }

        [JsonProperty("policyId", NullValueHandling = NullValueHandling.Ignore)]
        public string? PolicyId { get; set; }

        [JsonProperty("customerId", NullValueHandling = NullValueHandling.Ignore)]
        public string? CustomerId { get; set; }

        [JsonProperty("quoteId")]
        public Guid QuoteId { get; set; }

        [JsonProperty("organisationId")]
        public Guid OrganisationId { get; set; }

        [JsonProperty("productId")]
        public Guid ProductId { get; set; }

        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public string? Environment { get; set; }

        [JsonProperty("initialQuoteState", NullValueHandling = NullValueHandling.Ignore)]
        public string? InitialQuoteState { get; set; }

        [JsonProperty("formData", NullValueHandling = NullValueHandling.Ignore)]
        public object? FormData { get; set; }
    }
}
