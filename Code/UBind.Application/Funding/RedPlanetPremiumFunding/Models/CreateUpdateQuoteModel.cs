// <copyright file="CreateUpdateQuoteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System.Text.Json.Serialization;

public class CreateUpdateQuoteModel
{
    [JsonPropertyName("contractDetails")]
    public ContractDetails? ContractDetails { get; set; }

    [JsonPropertyName("clientDetails")]
    public ClientDetails? ClientDetails { get; set; }

    [JsonPropertyName("paymentDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaymentDetails? PaymentDetails { get; set; }

    [JsonPropertyName("policies")]
    public IList<Policy>? Policies { get; set; }
}
