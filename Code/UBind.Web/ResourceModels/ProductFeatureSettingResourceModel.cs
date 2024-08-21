// <copyright file="ProductFeatureSettingResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Resource model for product feature.
    /// </summary>
    public class ProductFeatureSettingResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeatureSettingResourceModel"/> class.
        /// </summary>
        /// <param name="productFeature">The product feature persistence model.</param>
        public ProductFeatureSettingResourceModel(ProductFeatureSetting productFeature)
        {
            this.TenantId = productFeature.TenantId;
            this.ProductId = productFeature.ProductId;
            this.AllowQuotesForNewOrganisations = productFeature.AllowQuotesForNewOrganisations;
            this.AreNewBusinessQuotesEnabled = productFeature.AreNewBusinessQuotesEnabled;
            this.AreRenewalQuotesEnabled = productFeature.AreRenewalQuotesEnabled;
            this.AreAdjustmentQuotesEnabled = productFeature.AreAdjustmentQuotesEnabled;
            this.AreCancellationQuotesEnabled = productFeature.AreCancellationQuotesEnabled;
            this.IsClaimsEnabled = productFeature.IsClaimsEnabled;
            this.MustCreateClaimAgainstPolicy = productFeature.MustCreateClaimAgainstPolicy;
            this.AllowRenewalAfterExpiry = productFeature.IsRenewalAllowedAfterExpiry;
            this.ExpiredPolicyRenewalPeriodSeconds = (long)productFeature.ExpiredPolicyRenewalDuration.TotalSeconds;
            this.RefundPolicy = productFeature.RefundRule;
            this.PeriodWhichNoClaimsMade = productFeature.PeriodOfNoClaimsToQualifyForRefund;
            this.LastNumberOfYearsWhichNoClaimsMade = productFeature.LastNumberOfYearsOfNoClaimsToQualifyForRefund;
            this.AreNewBusinessPolicyTransactionsEnabled = productFeature.AreNewBusinessPolicyTransactionsEnabled;
            this.AreAdjustmentPolicyTransactionsEnabled = productFeature.AreAdjustmentPolicyTransactionsEnabled;
            this.AreRenewalPolicyTransactionsEnabled = productFeature.AreRenewalPolicyTransactionsEnabled;
            this.AreCancellationPolicyTransactionsEnabled = productFeature.AreCancellationPolicyTransactionsEnabled;
        }

        /// <summary>
        /// Gets the Tenant id of product feature.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Product id of product feature.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy purchase feature is enabled.
        /// </summary>
        public bool AreNewBusinessQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy Adjustment feature is enabled.
        /// </summary>
        public bool AreAdjustmentQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy cancellation feature is enabled.
        /// </summary>
        public bool AreCancellationQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy renewal feature is enabled.
        /// </summary>
        public bool AreRenewalQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the feature allowing new organizations to create quotes is enabled by default for the product.
        /// </summary>
        public bool AllowQuotesForNewOrganisations { get; private set; }

        /// <summary>
        /// Gets a value indicating whether claims are enabled for this product.
        /// </summary>
        public bool IsClaimsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether users can only create claims against a policy.
        /// </summary>
        public bool MustCreateClaimAgainstPolicy { get; private set; }

        /// <summary>
        /// Gets the allowable expired policy renewal duration in seconds.
        /// </summary>
        public long ExpiredPolicyRenewalPeriodSeconds { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the renewal is allowed after expiry.
        /// </summary>
        public bool AllowRenewalAfterExpiry { get; private set; }

        /// <summary>
        /// Gets the refund policy.
        /// </summary>
        public RefundRule RefundPolicy { get; private set; }

        /// <summary>
        /// Gets the required period for which no claims has been made.
        /// </summary>
        public PolicyPeriodCategory? PeriodWhichNoClaimsMade { get; private set; }

        /// <summary>
        /// Gets the number of years for which no claims were made.
        /// </summary>
        public int? LastNumberOfYearsWhichNoClaimsMade { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote purchase feature is enabled.
        /// </summary>
        public bool AreNewBusinessPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote Adjustment feature is enabled.
        /// </summary>
        public bool AreAdjustmentPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote cancellation feature is enabled.
        /// </summary>
        public bool AreCancellationPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote renewal feature is enabled.
        /// </summary>
        public bool AreRenewalPolicyTransactionsEnabled { get; private set; }
    }
}
