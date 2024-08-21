// <copyright file="PremiumFundingErrors.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    ///  For parsing Premium Funding errors, since generated client does not capture all information from response in the exceptions in throws.
    /// </summary>
    public class PremiumFundingErrors
    {
        /// <summary>
        /// Gets the individual premium funding errors.
        /// </summary>
        [JsonProperty]
        public List<PremiumFundingError> Errors { get; private set; }
    }
}
