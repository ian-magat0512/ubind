﻿// <copyright file="PolicyTransactionPeriodicSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Dashboard.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// This model represents a summary of policy transactions
    /// between two dates or daily, and holds the total count of transactions
    /// and total premium for the transactions in that period.
    /// </summary>
    public class PolicyTransactionPeriodicSummaryModel : IPeriodicSummaryModel
    {
        /// <summary>
        /// Lists the allowed includeProperties of the quote periodic summary.
        /// </summary>
        public static readonly HashSet<string> IncludeProperties = new HashSet<string>()
        {
            nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount),
            nameof(PolicyTransactionPeriodicSummaryModel.CreatedTotalPremium),
        };

        /// <summary>
        /// Gets or sets the label of each periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the fromDate of the periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromDateTime { get; set; }

        /// <summary>
        /// Gets or sets the toDate of the periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ToDateTime { get; set; }

        /// <summary>
        /// Gets or sets the count of policy transactions created per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CreatedCount { get; set; }

        /// <summary>
        /// Gets or sets the total of policy transactions created per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? CreatedTotalPremium { get; set; }
    }
}
