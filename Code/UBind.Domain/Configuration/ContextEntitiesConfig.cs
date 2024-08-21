// <copyright file="ContextEntitiesConfig.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// Provides access to context entities configuration from product.json file.
    /// </summary>
    public class ContextEntitiesConfig : IContextEntitiesConfig
    {
        /// <inheritdoc />
        [JsonProperty("contextEntities")]
        public ContextEntities ContextEntities { get; set; }

        /// <inheritdoc />
        [JsonProperty("newBusinessContextEntities")]
        public ContextEntities NewBusinessContextEntities { get; set; }

        /// <inheritdoc />
        [JsonProperty("adjustmentContextEntities")]
        public ContextEntities AdjustmentContextEntities { get; set; }

        /// <inheritdoc />
        [JsonProperty("renewalContextEntities")]
        public ContextEntities RenewalContextEntities { get; set; }

        /// <inheritdoc />
        [JsonProperty("cancellationContextEntities")]
        public ContextEntities CancellationContextEntities { get; set; }
    }
}
