// <copyright file="QuickQuoteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System.Text.Json.Serialization;

public class QuickQuoteModel
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("numberOfInstalments")]
    public int NumberOfInstalments { get; set; }

    [JsonPropertyName("paymentFrequency")]
    public string? PaymentFrequency { get; set; }

    [JsonPropertyName("commissionRate")]
    public decimal CommissionRate { get; set; }
}
