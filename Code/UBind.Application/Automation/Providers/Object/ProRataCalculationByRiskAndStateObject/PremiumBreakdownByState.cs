// <copyright file="PremiumBreakdownByState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using Newtonsoft.Json;

    public class PremiumBreakdownByState
    {
        public PremiumBreakdownByState(PercentBreakdownByState statePercentBreakdown, decimal premium)
        {
            this.Act = statePercentBreakdown.PremiumPercentACT * premium;
            this.Nsw = statePercentBreakdown.PremiumPercentNSW * premium;
            this.Nt = statePercentBreakdown.PremiumPercentNT * premium;
            this.Qld = statePercentBreakdown.PremiumPercentQLD * premium;
            this.Sa = statePercentBreakdown.PremiumPercentSA * premium;
            this.Tas = statePercentBreakdown.PremiumPercentTAS * premium;
            this.Wa = statePercentBreakdown.PremiumPercentWA * premium;
            this.Vic = statePercentBreakdown.PremiumPercentVIC * premium;
            this.Os = statePercentBreakdown.PremiumPercentOS * premium;
        }

        [JsonProperty(PropertyName = "ACT")]
        public decimal Act { get; private set; }

        [JsonProperty(PropertyName = "NSW")]
        public decimal Nsw { get; private set; }

        [JsonProperty(PropertyName = "NT")]
        public decimal Nt { get; private set; }

        [JsonProperty(PropertyName = "QLD")]
        public decimal Qld { get; private set; }

        [JsonProperty(PropertyName = "SA")]
        public decimal Sa { get; private set; }

        [JsonProperty(PropertyName = "TAS")]
        public decimal Tas { get; private set; }

        [JsonProperty(PropertyName = "VIC")]
        public decimal Vic { get; private set; }

        [JsonProperty(PropertyName = "WA")]
        public decimal Wa { get; private set; }

        [JsonProperty(PropertyName = "OS")]
        public decimal Os { get; private set; }
    }
}
