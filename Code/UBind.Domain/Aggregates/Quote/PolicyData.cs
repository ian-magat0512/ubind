// <copyright file="PolicyData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// For persisting policy transaction data.
    /// </summary>
    public class PolicyData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyData"/> class.
        /// </summary>
        /// <param name="quoteDataSnapshot">The data associated with the policy.</param>
        public PolicyData(
            QuoteDataSnapshot quoteDataSnapshot)
        {
            this.QuoteDataSnapshot = quoteDataSnapshot;
        }

        [JsonConstructor]
        private PolicyData()
        {
        }

        /// <summary>
        /// Gets the date the policy is set to take effect.
        /// Only applicable for new business transactions.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public LocalDate InceptionDate { get; private set; }

        /// <summary>
        /// Gets the date the policy is set to expire.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public LocalDate? ExpiryDate { get; private set; }

        /// <summary>
        /// Gets the date the policy coverage is set to start.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public LocalDate EffectiveDate { get; private set; }

        /// <summary>
        /// Gets the date the policy coverage is set to end.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public LocalDate EffectiveEndDate { get; private set; }

        /// <summary>
        /// Gets the precise time the policy is set to take effect.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public Instant InceptionTime { get; private set; }

        /// <summary>
        /// Gets the precise time the policy is set to expire.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public Instant? ExpiryTime { get; private set; }

        /// <summary>
        /// Gets the precise time the policy coverage is set to start.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public Instant EffectiveTime { get; private set; }

        /// <summary>
        /// Gets the precise time the policy coverage is set to end.
        /// </summary>
        [JsonProperty]
        [Obsolete("LocalDateTimes from the events are to be used directly")]
        public Instant EffectiveEndTime { get; private set; }

        /// <summary>
        /// Gets the data that is applicable for the policy transaction.
        /// </summary>
        [JsonProperty]
        public QuoteDataSnapshot QuoteDataSnapshot { get; private set; }
    }
}
