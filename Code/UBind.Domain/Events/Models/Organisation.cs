﻿// <copyright file="Organisation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Short representation of an organisation for the event payload.
    /// </summary>
    public class Organisation
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Alias.
        /// </summary>
        [JsonProperty(PropertyName = "alias")]
        public string Alias { get; set; }
    }
}
