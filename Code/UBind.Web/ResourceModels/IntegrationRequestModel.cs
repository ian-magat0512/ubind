// <copyright file="IntegrationRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Resource model for integration request.
    /// </summary>
    public class IntegrationRequestModel
    {
        /// <summary>
        /// Gets or sets the web integration Id.
        /// </summary>
        [JsonProperty("webIntegrationId")]
        public string WebIntegrationId { get; set; }

        /// <summary>
        /// Gets or sets the quote aggregate Id.
        /// </summary>
        [JsonProperty("quoteId")]
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary json payload.
        /// </summary>
        [JsonProperty("formModel")]
        public dynamic FormData { get; set; }
    }
}
