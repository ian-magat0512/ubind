// <copyright file="PolicyModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels;

using NodaTime;
using UBind.Domain.ReadModel;
using UBind.Domain;
using UBind.Domain.ReadModel.Policy;
using UBind.Domain.Extensions;
using Newtonsoft.Json;
using Humanizer;

/// <summary>
/// Resource model for a policy.
/// </summary>
public class PolicyModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyModel"/> class.
    /// </summary>
    /// <param name="policy">The policy.</param>
    public PolicyModel(PolicyReadModel policy)
    {
        this.PolicyNumber = policy.PolicyNumber;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyModel"/> class.
    /// </summary>
    /// <param name="policySummary">The policy to be passed back.</param>
    /// <param name="time">The current time.</param>
    public PolicyModel(IPolicyReadModelSummary policySummary, Instant time)
    {
        this.Id = policySummary.PolicyId;
        this.PolicyTitle = policySummary.PolicyTitle;
        this.QuoteId = policySummary.QuoteId;
        this.PolicyNumber = policySummary.PolicyNumber;
        this.ProductId = policySummary.ProductId;
        this.ProductName = policySummary.ProductName;
        this.CreatedDateTime = policySummary.CreatedTimestamp.ToExtendedIso8601String();
        this.IssuedDateTime = policySummary.IssuedTimestamp.ToExtendedIso8601String();
        this.InceptionDateTime = policySummary.InceptionTimestamp.ToExtendedIso8601String();
        this.ExpiryDateTime = policySummary.ExpiryTimestamp?.ToExtendedIso8601String();
        this.LastModifiedDateTime = policySummary.LastModifiedTimestamp.ToExtendedIso8601String();
        this.LatestRenewalEffectiveDateTime = policySummary.LatestRenewalEffectiveTimestamp?.ToExtendedIso8601String();
        if (policySummary.CustomerId.HasValue)
        {
            this.Customer = new CustomerSimpleModel(policySummary.CustomerId.Value, policySummary.CustomerFullName);
        }

        PolicyStatus rawStatus = policySummary.GetPolicyStatus(time);
        this.Status = rawStatus.Humanize();
        if (policySummary.ProductFeatureSetting != null && policySummary.IsTermBased)
        {
            this.IsForRenewal = policySummary.CheckPolicyRenewalEligibilityAtTime(policySummary.ProductFeatureSetting, time);
        }

        this.TotalPayable = policySummary.CalculationResult.PayablePrice.TotalPayable.ToString();
        this.IsTestData = policySummary.IsTestData;
        this.CreatedDateTime = policySummary.CreatedTimestamp.ToExtendedIso8601String();
        this.CancellationEffectiveDateTime = policySummary.CancellationEffectiveTimestamp?.ToExtendedIso8601String();
    }

    public PolicyModel(IPolicyReadModelDetails policyReadModel)
    {
        this.Id = policyReadModel.Id;
        this.PolicyTitle = policyReadModel.PolicyTitle;
        this.QuoteId = policyReadModel.QuoteId;
        this.PolicyNumber = policyReadModel.PolicyNumber;
        this.ProductId = policyReadModel.ProductId;
        this.ProductName = policyReadModel.ProductName;
        this.CreatedDateTime = policyReadModel.CreatedTimestamp.ToExtendedIso8601String();
        this.IssuedDateTime = policyReadModel.IssuedTimestamp.ToExtendedIso8601String();
        this.InceptionDateTime = policyReadModel.InceptionTimestamp.ToExtendedIso8601String();
        this.ExpiryDateTime = policyReadModel.ExpiryTimestamp?.ToExtendedIso8601String();
        this.LastModifiedDateTime = policyReadModel.LastModifiedTimestamp.ToExtendedIso8601String();
        this.LatestRenewalEffectiveDateTime = policyReadModel.LatestRenewalEffectiveTimestamp?.ToExtendedIso8601String();
        if (policyReadModel.CustomerId.HasValue)
        {
            this.Customer = new CustomerSimpleModel(policyReadModel.CustomerId.Value, policyReadModel.CustomerFullName);
        }

        this.Status = policyReadModel.PolicyState;
        this.TotalPayable = policyReadModel.CalculationResult.PayablePrice.TotalPayable.ToString();
        this.IsTestData = policyReadModel.IsTestData;
        this.CreatedDateTime = policyReadModel.CreatedTimestamp.ToExtendedIso8601String();
        this.CancellationEffectiveDateTime = policyReadModel.CancellationEffectiveTimestamp?.ToExtendedIso8601String();
    }

    /// <summary>
    /// Gets the ID of the policy.
    /// </summary>
    [JsonProperty]
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the quote/application that was used to create this policy.
    /// </summary>
    [JsonProperty]
    public Guid? QuoteId { get; private set; }

    /// <summary>
    /// Gets the policy title.
    /// </summary>
    [JsonProperty]
    public string PolicyTitle { get; private set; }

    /// <summary>
    /// Gets the policy number.
    /// </summary>
    [JsonProperty]
    public string PolicyNumber { get; private set; }

    /// <summary>
    /// Gets the name of the product the quote is for.
    /// </summary>
    [JsonProperty]
    public string ProductName { get; private set; }

    /// <summary>
    /// Gets the id of the product the quote is for.
    /// </summary>
    [JsonProperty]
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the total amount payable for the policy.
    /// </summary>
    [JsonProperty]
    public string TotalPayable { get; private set; }

    /// <summary>
    /// Gets the current status of the policy.
    /// </summary>
    [JsonProperty]
    public string Status { get; private set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    [JsonProperty]
    public string CreatedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date the policy was last modified.
    /// </summary>
    [JsonProperty]
    public string LastModifiedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the cancellation date.
    /// </summary>
    [JsonProperty]
    public string CancellationEffectiveDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date the policy is issued/created.
    /// </summary>
    [JsonProperty]
    public string IssuedDateTime { get; set; }

    /// <summary>
    /// Gets the date the policy is to take effect.
    /// </summary>
    [JsonProperty]
    public string InceptionDateTime { get; private set; }

    /// <summary>
    /// Gets the date the policy is set to expire.
    /// </summary>
    [JsonProperty]
    public string ExpiryDateTime { get; private set; }

    /// <summary>
    /// Gets the date the policy is set when latest renewal date.
    /// </summary>
    [JsonProperty]
    public string LatestRenewalEffectiveDateTime { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the Policy is for Renewal.
    /// </summary>
    [JsonProperty]
    public bool IsForRenewal { get; private set; }

    /// <summary>
    /// Gets the current customer for the quote holding the policy.
    /// </summary>
    [JsonProperty]
    public CustomerSimpleModel Customer { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether data is for test.
    /// </summary>
    [JsonProperty]
    public bool IsTestData { get; set; }
}
