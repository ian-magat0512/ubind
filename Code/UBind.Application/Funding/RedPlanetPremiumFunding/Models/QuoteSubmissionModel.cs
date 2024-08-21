// <copyright file="QuoteSubmissionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System.Text.Json.Serialization;

public partial class QuoteSubmissionModel
{
    [JsonPropertyName("paymentDetails")]
    public PaymentDetails? PaymentDetails { get; set; }

    [JsonPropertyName("billingStartDate")]
    public string? BillingStartDate { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("signedByFullName")]
    public string? SignedByFullName { get; set; }

    [JsonPropertyName("signedByPosition")]
    public string? SignedByPosition { get; set; }

    [JsonPropertyName("processRealtimePayment")]
    public bool ProcessRealtimePayment { get; set; }

    [JsonPropertyName("initialInstalmentAmount")]
    public decimal InitialInstalmentAmount { get; set; }
}