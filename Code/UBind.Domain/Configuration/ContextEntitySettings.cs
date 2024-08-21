// <copyright file="ContextEntitySettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    using Newtonsoft.Json;

    public class ContextEntitySettings : IContextEntitySettings
    {
        /// <inheritdoc/>
        [JsonProperty("includeContextEntities")]
        public string[] IncludeContextEntities { get; set; }

        /// <inheritdoc/>
        [JsonProperty("reloadIntervalSeconds")]
        public int ReloadIntervalSeconds { get; set; }

        /// <inheritdoc/>
        [JsonProperty("reloadWithOperations")]
        public string[] ReloadWithOperations { get; set; }
    }
}
