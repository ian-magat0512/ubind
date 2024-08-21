// <copyright file="ImportConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports.MappingObjects
{
    using Newtonsoft.Json;
    using UBind.Domain.Imports;

    /// <summary>
    /// Renders the configuration based from import.
    /// </summary>
    public class ImportConfiguration
    {
        /// <summary>
        /// Gets the customer mapping configuration.
        /// </summary>
        [JsonProperty]
        public CustomerMapping CustomerMapping { get; private set; }

        /// <summary>
        /// Gets the policy mapping configuration.
        /// </summary>
        [JsonProperty]
        public PolicyMapping PolicyMapping { get; private set; }

        /// <summary>
        /// Gets the claim mapping configuration.
        /// </summary>
        [JsonProperty]
        public ClaimMapping ClaimMapping { get; private set; }

        [JsonProperty]
        public QuoteMapping QuoteMapping { get; private set; }
    }
}
