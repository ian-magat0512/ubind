// <copyright file="Tenant.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Short representation of a tenant for the event payload.
    /// </summary>
    public class Tenant
    {

        [JsonConstructor]
        public Tenant(Guid id, string alias)
        {
            this.Id = id;
            this.Alias = alias;
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Alias.
        /// </summary>
        [JsonProperty(PropertyName = "alias")]
        public string? Alias { get; set; }
    }
}
