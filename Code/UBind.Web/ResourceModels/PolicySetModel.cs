// <copyright file="PolicySetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    /// <summary>
    /// Resource model for serving policy sets to portal.
    /// </summary>
    public class PolicySetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicySetModel"/> class.
        /// </summary>
        /// <param name="policySummary">The policy to be passed back.</param>
        /// <param name="time">The current time.</param>
        public PolicySetModel(IPolicyReadModelSummary policySummary, Instant time)
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

            this.IsTestData = policySummary.IsTestData;
            this.CreatedDateTime = policySummary.CreatedTimestamp.ToExtendedIso8601String();
            this.CancellationEffectiveDateTime = policySummary.CancellationEffectiveTimestamp?.ToExtendedIso8601String();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicySetModel"/> class.
        /// </summary>
        /// <param name="policySearchResult">The policy to be passed back.</param>
        public PolicySetModel(IPolicySearchResultItemReadModel policySearchResult)
        {
            this.Id = policySearchResult.Id;
            this.PolicyTitle = policySearchResult.PolicyTitle;
            this.QuoteId = policySearchResult.QuoteId;
            this.PolicyNumber = policySearchResult.PolicyNumber;
            this.ProductId = policySearchResult.ProductId;
            this.ProductName = policySearchResult.ProductName;
            this.CreatedDateTime = policySearchResult.CreatedTimestamp.ToExtendedIso8601String();
            this.IssuedDateTime = policySearchResult.IssuedTimestamp.ToExtendedIso8601String();
            this.InceptionDateTime = policySearchResult.InceptionTimestamp.ToExtendedIso8601String();
            this.ExpiryDateTime = policySearchResult.ExpiryTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = policySearchResult.LastModifiedTimestamp.ToExtendedIso8601String();
            this.LatestRenewalEffectiveDateTime = policySearchResult.LatestRenewalEffectiveTimestamp?.ToExtendedIso8601String();

            if (policySearchResult.CustomerId.HasValue)
            {
                this.Customer = new CustomerSimpleModel(policySearchResult.CustomerId.Value, policySearchResult.CustomerFullName);
            }

            this.Status = policySearchResult.PolicyState.ToUpperFirstChar();
            this.IsTestData = policySearchResult.IsTestData;
            this.CreatedDateTime = policySearchResult.CreatedTimestamp.ToExtendedIso8601String();
            this.CancellationEffectiveDateTime = policySearchResult.CancellationEffectiveTimestamp?.ToExtendedIso8601String();
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
}
