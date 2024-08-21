// <copyright file="ZaiUserOperationResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the response received from a user account-related request with Zai.
    /// </summary>
    public class ZaiUserOperationResponse
    {
        /// <summary>
        /// Gets or sets the user account retrieved.
        /// </summary>
        [JsonProperty("users")]
        public ZaiEntities.User User { get; set; }

        /// <summary>
        /// Gets or sets the list of items for the user, if any.
        /// </summary>
        [JsonProperty("items")]
        public List<ZaiEntities.Item> Items { get; set; }
    }
}
