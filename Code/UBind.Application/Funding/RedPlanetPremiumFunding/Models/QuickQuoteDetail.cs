// <copyright file="QuickQuoteDetail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System.Text.Json.Serialization;

public class QuickQuoteDetail
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("amountFinanced")]
    public decimal? AmountFinanced { get; set; }

    [JsonPropertyName("amountCancellable")]
    public decimal? AmountCancellable { get; set; }

    [JsonPropertyName("amountNonCancellable")]
    public decimal? AmountNonCancellable { get; set; }

    [JsonPropertyName("numberOfInstalments")]
    public int NumberOfInstalments { get; set; }

    [JsonPropertyName("applicationFee")]
    public decimal? ApplicationFee { get; set; }

    [JsonPropertyName("instalmentAmount")]
    public decimal? InstalmentAmount { get; set; }

    [JsonPropertyName("initialAmountDue")]
    public decimal? InitialAmountDue { get; set; }

    [JsonPropertyName("inceptionDate")]
    public string? InceptionDate { get; set; }

    [JsonPropertyName("flatRate")]
    public decimal? FlatRate { get; set; }

    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("totalAmountRepayable")]
    public decimal? TotalAmountRepayable { get; set; }

    [JsonPropertyName("annualPercentageRate")]
    public decimal? AnnualPercentageRate { get; set; }

    [JsonPropertyName("ratePlan")]
    public string? RatePlan { get; set; }

    [JsonPropertyName("settlementDays")]
    public int? SettlementDays { get; set; }

    [JsonPropertyName("commissionRate")]
    public decimal? CommissionRate { get; set; }

    [JsonPropertyName("ongoingRepayment")]
    public decimal? OngoingRepayment { get; set; }

    [JsonPropertyName("depositType")]
    public string? DepositType { get; set; }

    [JsonPropertyName("depositRate")]
    public decimal? DepositRate { get; set; }

    [JsonPropertyName("depositAmount")]
    public decimal? DepositAmount { get; set; }

    [JsonPropertyName("firstPaymentDate")]
    public string? FirstPaymentDate { get; set; }

    [JsonPropertyName("interest")]
    public decimal? Interest { get; set; }

    [JsonPropertyName("disclosedCommission")]
    public decimal? DisclosedCommission { get; set; }

    [JsonPropertyName("lastInstalmentDate")]
    public string? LastInstalmentDate { get; set; }

    [JsonPropertyName("instalmentMonthDay")]
    public string? InstalmentMonthDay { get; set; }

    [JsonPropertyName("brokerIdentifier")]
    public string? BrokerIdentifier { get; set; }
}
