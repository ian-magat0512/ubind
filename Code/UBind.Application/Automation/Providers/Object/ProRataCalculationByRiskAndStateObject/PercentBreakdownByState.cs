// <copyright file="PercentBreakdownByState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using Newtonsoft.Json;

    public class PercentBreakdownByState
    {
        [JsonProperty(PropertyName = "premiumPercentACT")]
        public decimal PremiumPercentACT { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentNSW")]
        public decimal PremiumPercentNSW { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentNT")]
        public decimal PremiumPercentNT { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentQLD")]
        public decimal PremiumPercentQLD { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentSA")]
        public decimal PremiumPercentSA { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentTAS")]
        public decimal PremiumPercentTAS { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentVIC")]
        public decimal PremiumPercentVIC { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentWA")]
        public decimal PremiumPercentWA { get; private set; }

        [JsonProperty(PropertyName = "premiumPercentOS")]
        public decimal PremiumPercentOS { get; private set; }
    }
}
