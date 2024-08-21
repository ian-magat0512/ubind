// <copyright file="ClaimPeriodicSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Dashboard.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model for periodic summary of claims on the given dates.
    /// </summary>
    public class ClaimPeriodicSummaryModel : IPeriodicSummaryModel
    {
        /// <summary>
        /// Lists the allowed includeProperties of the quote periodic summary.
        /// </summary>
        public static readonly HashSet<string> IncludeProperties = new HashSet<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.SettledCount),
            nameof(ClaimPeriodicSummaryModel.DeclinedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };

        /// <summary>
        /// Gets or sets the label of each periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the fromDateTime of the periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromDateTime { get; set; }

        /// <summary>
        /// Gets or sets the toDateTime of the periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ToDateTime { get; set; }

        /// <summary>
        /// Gets or sets the count of claims created per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ProcessedCount { get; set; }

        /// <summary>
        /// Gets or sets the average of claims processed per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? AverageProcessingTime { get; set; }

        /// <summary>
        /// Gets or sets the total of claims settled per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SettledCount { get; set; }

        /// <summary>
        /// Gets or sets the total of claims declined per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DeclinedCount { get; set; }

        /// <summary>
        /// Gets or sets the avarage of claims settled per periodic summary.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? AverageSettlementAmount { get; set; }
    }
}
