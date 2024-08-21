// <copyright file="RenewPolicyActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using UBind.Application.Automation.Enums;

/// <summary>
/// Represents the data associated with the "Renew Policy" action within the automation workflow.
/// This data class encapsulates information such as the quote ID, input data, policy ID, and policy transaction ID.
/// </summary>
/// <remarks>
/// The <see cref="RenewPolicyActionData"/> class inherits from the <see cref="ActionData"/> base class
/// and extends it to include specific properties relevant to the "Renew Policy" action. It provides properties
/// for storing identifiers and input data associated with the renewal process, facilitating tracking and management
/// of relevant information during execution.
/// </remarks>
public class RenewPolicyActionData : ActionData
{
    public RenewPolicyActionData(string name, string alias, IClock clock)
        : base(name, alias, ActionType.RenewPolicyAction, clock)
    {
    }

    [JsonConstructor]
    public RenewPolicyActionData()
        : base(ActionType.RenewPolicyAction)
    {
    }

    [JsonProperty("quoteId")]
    public Guid? QuoteId { get; set; }

    [JsonProperty("inputData")]
    public object? InputData { get; set; }

    [JsonProperty("policyId")]
    public Guid? PolicyId { get; set; }

    [JsonProperty("policyTransactionId")]
    public Guid? PolicyTransactionId { get; set; }
}
