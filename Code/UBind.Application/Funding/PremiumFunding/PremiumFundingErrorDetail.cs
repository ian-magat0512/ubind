// <copyright file="PremiumFundingErrorDetail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using Newtonsoft.Json;

    /// <summary>
    /// For parsing Premium Funding errors, since generated client does not capture all information from response in the exceptions in throws.
    /// </summary>
    public class PremiumFundingErrorDetail
    {
        /// <summary>
        /// Gets the error type.
        /// </summary>
        [JsonProperty]
        public string Type { get; private set; }

        /// <summary>
        /// Gets the rule that triggered the error.
        /// </summary>
        [JsonProperty]
        public string Rule { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        [JsonProperty]
        public string Message { get; private set; }
    }
}
