// <copyright file="ProductFeatureSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;
    using UBind.Domain.Product;

    /// <summary>
    /// A product feature setting.
    /// </summary>
    public class ProductFeatureSetting : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeatureSetting"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="createdTimestamp">The created time.</param>
        public ProductFeatureSetting(Guid tenantId, Guid productId, Instant createdTimestamp)
         : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.AreNewBusinessQuotesEnabled = true;
            this.IsClaimsEnabled = true;
            this.MustCreateClaimAgainstPolicy = true;
            this.AreNewBusinessPolicyTransactionsEnabled = true;
        }

        // Parameterless constructor for EF.
        private ProductFeatureSetting()
            : base(default, default)
        {
        }

        public Dictionary<ProductFeatureSettingItem, bool> FeatureGetters
        {
            get
            {
                return new Dictionary<ProductFeatureSettingItem, bool>
                {
                    { ProductFeatureSettingItem.NewBusinessQuotes, this.AreNewBusinessQuotesEnabled },
                    { ProductFeatureSettingItem.RenewalQuotes, this.AreRenewalQuotesEnabled },
                    { ProductFeatureSettingItem.AdjustmentQuotes, this.AreAdjustmentQuotesEnabled },
                    { ProductFeatureSettingItem.CancellationQuotes, this.AreCancellationQuotesEnabled },
                    { ProductFeatureSettingItem.Claims, this.IsClaimsEnabled },
                    { ProductFeatureSettingItem.MustCreateClaimsAgainstPolicy, this.MustCreateClaimAgainstPolicy },
                    { ProductFeatureSettingItem.AllowQuotesForNewOrganisations, this.AllowQuotesForNewOrganisations },
                    { ProductFeatureSettingItem.NewBusinessPolicyTransactions, this.AreNewBusinessPolicyTransactionsEnabled },
                    { ProductFeatureSettingItem.RenewalPolicyTransactions, this.AreRenewalPolicyTransactionsEnabled },
                    { ProductFeatureSettingItem.AdjustmentPolicyTransactions, this.AreAdjustmentPolicyTransactionsEnabled },
                    { ProductFeatureSettingItem.CancellationPolicyTransactions, this.AreCancellationPolicyTransactionsEnabled },
                };
            }
        }

        public Dictionary<ProductFeatureSettingItem, Action<bool>> FeatureSetters
        {
            get
            {
                return new Dictionary<ProductFeatureSettingItem, Action<bool>>
            {
                { ProductFeatureSettingItem.NewBusinessQuotes, value => this.AreNewBusinessQuotesEnabled = value },
                { ProductFeatureSettingItem.RenewalQuotes, value => this.AreRenewalQuotesEnabled = value },
                { ProductFeatureSettingItem.AdjustmentQuotes, value => this.AreAdjustmentQuotesEnabled = value },
                { ProductFeatureSettingItem.CancellationQuotes, value => this.AreCancellationQuotesEnabled = value },
                { ProductFeatureSettingItem.Claims, value => this.IsClaimsEnabled = value },
                { ProductFeatureSettingItem.MustCreateClaimsAgainstPolicy, value => this.MustCreateClaimAgainstPolicy = value },
                { ProductFeatureSettingItem.AllowQuotesForNewOrganisations, value => this.AllowQuotesForNewOrganisations = value },
                { ProductFeatureSettingItem.NewBusinessPolicyTransactions, value => this.AreNewBusinessPolicyTransactionsEnabled = value },
                { ProductFeatureSettingItem.RenewalPolicyTransactions, value => this.AreRenewalPolicyTransactionsEnabled = value },
                { ProductFeatureSettingItem.AdjustmentPolicyTransactions, value => this.AreAdjustmentPolicyTransactionsEnabled = value },
                { ProductFeatureSettingItem.CancellationPolicyTransactions, value => this.AreCancellationPolicyTransactionsEnabled = value },
            };
            }
        }

        /// <summary>
        /// Gets the Tenant ID of product feature.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Product ID of product feature.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote purchase feature is enabled.
        /// </summary>
        public bool AreNewBusinessQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote adjustment feature is enabled.
        /// </summary>
        public bool AreAdjustmentQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote cancellation feature is enabled.
        /// </summary>
        public bool AreCancellationQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote renewal feature is enabled.
        /// </summary>
        public bool AreRenewalQuotesEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether quotes are allowed for newly created organizations
        /// </summary>
        public bool AllowQuotesForNewOrganisations { get; set; }

        /// <summary>
        /// Gets a value indicating whether claims can be created.
        /// </summary>
        public bool IsClaimsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether claims must be created against a policy.
        /// </summary>
        public bool MustCreateClaimAgainstPolicy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether renewal is allowed after expiry.
        /// </summary>
        public bool IsRenewalAllowedAfterExpiry { get; private set; }

        /// <summary>
        /// Gets expired policy renewal duration in seconds.
        /// </summary>
        public int ExpiredPolicyRenewalDurationInSeconds { get; private set; }

        /// <summary>
        /// Gets a refund policy.
        /// </summary>
        public RefundRule RefundRule { get; private set; }

        /// <summary>
        /// Gets the policy period of no claims to qualify for refund.
        /// </summary>
        public PolicyPeriodCategory? PeriodOfNoClaimsToQualifyForRefund { get; private set; }

        /// <summary>
        /// Gets the number in recent years for which no claims were made.
        /// </summary>
        public int? LastNumberOfYearsOfNoClaimsToQualifyForRefund { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy new business feature is enabled.
        /// </summary>
        public bool AreNewBusinessPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy adjustment feature is enabled.
        /// </summary>
        public bool AreAdjustmentPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy cancellation feature is enabled.
        /// </summary>
        public bool AreCancellationPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy renewal feature is enabled.
        /// </summary>
        public bool AreRenewalPolicyTransactionsEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the allowable expired policy renewal duration.
        /// </summary>
        public Duration ExpiredPolicyRenewalDuration
        {
            get
            {
                return Duration.FromSeconds(this.ExpiredPolicyRenewalDurationInSeconds);
            }

            set
            {
                this.ExpiredPolicyRenewalDurationInSeconds = (int)value.TotalSeconds;
            }
        }

        /// <summary>
        /// Disable a product feature.
        /// </summary>
        /// <param name="productFeatureType">The product feature type to disable.</param>
        public void Disable(ProductFeatureSettingItem productFeatureType)
        {
            this.SetProductFeatureStatus(productFeatureType, false);
        }

        /// <summary>
        /// Update renewal setting.
        /// </summary>
        /// <param name="isRenewalAllowedAfterExpiry">The Indicator whether the renewal is allowed after expiry.</param>
        /// <param name="expiredPolicyRenewalDuration">The expired policy renewal duration.</param>
        public void UpdateProductFeatureRenewalSetting(bool isRenewalAllowedAfterExpiry, Duration expiredPolicyRenewalDuration)
        {
            this.IsRenewalAllowedAfterExpiry = isRenewalAllowedAfterExpiry;
            this.ExpiredPolicyRenewalDuration = expiredPolicyRenewalDuration;
        }

        /// <summary>
        /// Update cancellation Setting.
        /// </summary>
        /// <param name="cancellationRefundRule">The Cancellation refund rule.</param>
        /// <param name="policyDuration">The policy duration.</param>
        /// <param name="lastNumberOfYearsWhichNoClaimsMade">The last number of years with no claims made.</param>
        public void UpdateCancellationSetting(RefundRule cancellationRefundRule, PolicyPeriodCategory? policyDuration = null, int? lastNumberOfYearsWhichNoClaimsMade = null)
        {
            this.RefundRule = cancellationRefundRule;
            this.PeriodOfNoClaimsToQualifyForRefund = policyDuration;
            this.LastNumberOfYearsOfNoClaimsToQualifyForRefund = lastNumberOfYearsWhichNoClaimsMade;
        }

        /// <summary>
        /// Enable a product feature.
        /// </summary>
        /// <param name="productFeatureType">The product feature type to enable.</param>
        public void Enable(ProductFeatureSettingItem productFeatureType)
        {
            this.SetProductFeatureStatus(productFeatureType, true);
        }

        public bool IsProductFeatureEnabled(ProductFeatureSettingItem productFeatureType)
        {
            this.FeatureGetters.TryGetValue(productFeatureType, out var isProductFeatureSettingEnabled);
            return isProductFeatureSettingEnabled;
        }

        private void SetProductFeatureStatus(ProductFeatureSettingItem productFeatureType, bool isEnable)
        {
            if (this.FeatureSetters.TryGetValue(productFeatureType, out var setter))
            {
                setter(isEnable);
            }
        }
    }
}
