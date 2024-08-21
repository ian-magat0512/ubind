// <copyright file="AdditionalPropertyValueQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Enums;

    /// <summary>
    /// Filter model in getting the list of additional property value in eav table.
    /// </summary>
    public class AdditionalPropertyValueQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets the ID of additional property definition.
        /// </summary>
        [JsonProperty]
        public Guid? AdditionalPropertyDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the value of additional property value.
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        [JsonProperty]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the property type <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyDefinitionType PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the additional property entity type.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyEntityType EntityType { get; set; }
    }
}
