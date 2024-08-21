// <copyright file="PortalLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Portal
{
    using Newtonsoft.Json;

    public class PortalLocations
    {
        [JsonProperty(PropertyName = "development")]
        public Location Development { get; set; }

        [JsonProperty(PropertyName = "staging")]
        public Location Staging { get; set; }

        [JsonProperty(PropertyName = "production")]
        public Location Production { get; set; }

        public class Location
        {
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "isEmbedded")]
            public bool IsEmbedded { get; set; }
        }
    }
}
