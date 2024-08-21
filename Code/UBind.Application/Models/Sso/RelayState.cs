// <copyright file="RelayState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.Sso
{
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Represents state to send to a SAML request that is returned in the reponse.
    /// The information here is used to maintain information that can be used after the sign in is completed,
    /// for example, to redirect the user to a specific page.
    /// </summary>
    public class RelayState
    {
        /// <summary>
        /// Gets or sets the portal ID which the request was initiated from.
        /// </summary>
        [JsonProperty("portalId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? PortalId { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string? Path { get; set; }

        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
        public DeploymentEnvironment? Environment { get; set; }
    }
}
