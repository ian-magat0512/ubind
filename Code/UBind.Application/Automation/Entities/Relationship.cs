// <copyright file="Relationship.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Entities
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need an object that will hold the relationship properties of an email entity.
    /// </summary>
    public class Relationship
    {
        public Relationship(
            Guid tenantId,
            RelationshipType relationshipType,
            IEntity sourceEntity,
            IEntity targetEntity)
        {
            this.TenantId = tenantId;
            this.RelationshipType = relationshipType;
            this.SourceEntity = sourceEntity;
            this.TargetEntity = targetEntity;
        }

        [JsonConstructor]
        private Relationship()
        {
        }

        /// <summary>
        /// gets the tenant id of the relationship.
        /// </summary>
        [JsonProperty("tenantId")]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the relationship type.
        /// </summary>
        [JsonProperty("relationshipTypeAlias")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public RelationshipType RelationshipType { get; private set; }

        /// <summary>
        /// Gets the source entity.
        /// </summary>
        [JsonProperty("sourceEntity")]
        public IEntity SourceEntity { get; private set; }

        /// <summary>
        /// Gets the target entity.
        /// </summary>
        [JsonProperty("targetEntity")]
        public IEntity TargetEntity { get; private set; }
    }
}
