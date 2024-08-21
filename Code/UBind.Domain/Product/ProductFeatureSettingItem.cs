// <copyright file="ProductFeatureSettingItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    /// <summary>
    /// Product feature types.
    /// </summary>
    public enum ProductFeatureSettingItem
    {
        /// <summary>
        /// The Quote Purchase product feature.
        /// </summary>
        NewBusinessQuotes = 0,

        /// <summary>
        /// The Quote Update product feature.
        /// </summary>
        AdjustmentQuotes = 1,

        /// <summary>
        /// The Quote Renewal product feature.
        /// </summary>
        RenewalQuotes = 2,

        /// <summary>
        /// The Quote Cancellation product feature.
        /// </summary>
        CancellationQuotes = 3,

        /// <summary>
        /// The Claims product feature.
        /// </summary>
        Claims = 4,

        /// <summary>
        /// Whether claims must be created against policies.
        /// </summary>
        MustCreateClaimsAgainstPolicy = 5,

        /// <summary>
        /// Whether quotes are allowed for a newly created organisation
        /// </summary>
        AllowQuotesForNewOrganisations = 6,

        /// <summary>
        /// The Policy Purchase product feature.
        /// </summary>
        NewBusinessPolicyTransactions = 7,

        /// <summary>
        /// The Policy Update product feature.
        /// </summary>
        AdjustmentPolicyTransactions = 8,

        /// <summary>
        /// The Policy Renewal product feature.
        /// </summary>
        RenewalPolicyTransactions = 9,

        /// <summary>
        /// The Policy Cancellation product feature.
        /// </summary>
        CancellationPolicyTransactions = 10,
    }
}
