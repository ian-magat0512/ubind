// <copyright file="ContextEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Gets the context entities section for the current product.json configuration.
    /// </summary>
    public class ContextEntities : IContextEntities
    {
        /// <inheritdoc/>
        [JsonProperty("quotes")]
        public ContextEntitySettings? Quotes { get; set; }

        /// <inheritdoc/>
        [JsonProperty("newBusinessQuotes")]
        public ContextEntitySettings? NewBusinessQuotes { get; set; }

        /// <inheritdoc/>
        [JsonProperty("adjustmentQuotes")]
        public ContextEntitySettings? AdjustmentQuotes { get; set; }

        /// <inheritdoc/>
        [JsonProperty("renewalQuotes")]
        public ContextEntitySettings? RenewalQuotes { get; set; }

        /// <inheritdoc/>
        [JsonProperty("cancellationQuotes")]
        public ContextEntitySettings? CancellationQuotes { get; set; }

        /// <inheritdoc/>
        [JsonProperty("claims")]
        public ContextEntitySettings? Claims { get; set; }

        public static implicit operator ContextEntities(JToken contextEntities)
        {
            return contextEntities?.ToObject<ContextEntities>();
        }
    }
}
