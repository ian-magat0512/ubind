// <copyright file="Relationship.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is needed because we need to generate json representation of relationship that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Relationship : ISchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class.
        /// </summary>
        /// <param name="model">The relationship read model.</param>
        public Relationship(UBind.Domain.ReadWriteModel.Relationship model)
        {
            if (model == null)
            {
                return;
            }

            var relationshipType = model.Type.GetAttributeOfType<RelationshipTypeInformationAttribute>();
            if (relationshipType == null)
            {
                return;
            }

            this.Name = relationshipType.Name.Humanize(LetterCasing.Title);
            this.Alias = relationshipType.Alias.ToCamelCase();
            this.SourceEntityPropertyName = relationshipType.SourceEntityPropertyName;
            this.SourceEntityPropertyNamePlural = relationshipType.SourceEntityPropertyNamePlural;
            this.SourceEntityPropertyAlias = relationshipType.SourceEntityPropertyAlias;
            this.SourceEntityPropertyAliasPlural = relationshipType.SourceEntityPropertyAliasPlural;
            this.SourceEntityDescription = relationshipType.SourceEntityDescription;
            this.TargetEntityPropertyName = relationshipType.TargetEntityPropertyName;
            this.TargetEntityPropertyNamePlural = relationshipType.TargetEntityPropertyNamePlural;
            this.TargetEntityPropertyAlias = relationshipType.TargetEntityPropertyAlias;
            this.TargetEntityPropertyAliasPlural = relationshipType.TargetEntityPropertyAliasPlural;
            this.TargetEntityDescription = relationshipType.TargetEntityDescription;
            this.TargetEntityType = model.ToEntityType.ToString().ToCamelCase();
            this.TargetEntityId = model.ToEntityId != default ?
                model.ToEntityId.ToString() : null;
            this.SourceEntityType = model.FromEntityType.ToString().ToCamelCase();
            this.SourceEntityId = model.FromEntityId != default ?
                model.FromEntityId.ToString() : null;
            this.RelationshipType = model.Type;
        }

        public Relationship(RelationshipType relationshipType, IEntity sourceEntity, IEntity targetEntity)
        {
            if (sourceEntity == null && targetEntity == null)
            {
                throw new InvalidOperationException("When trying to generate a serialised version of a Relationship, "
                    + "for automations, both the source and target entities were found to be null. This should not "
                    + "happen during an automation as a relationship is created against an existing entity in the "
                    + "schema. This is an unexpected failure.");
            }

            var info = relationshipType.GetAttributeOfType<RelationshipTypeInformationAttribute>();
            this.Name = info.Name.Humanize(LetterCasing.Title);
            this.Alias = info.Alias.ToCamelCase();
            if (targetEntity != null)
            {
                this.TargetEntityPropertyName = info.TargetEntityPropertyName;
                this.TargetEntityPropertyNamePlural = info.TargetEntityPropertyNamePlural;
                this.TargetEntityPropertyAlias = info.TargetEntityPropertyAlias;
                this.TargetEntityPropertyAliasPlural = info.TargetEntityPropertyAliasPlural;
                this.TargetEntityDescription = info.TargetEntityDescription;
                this.TargetEntityType = targetEntity.GetType().Name.ToString().ToCamelCase();
                this.TargetEntityId = targetEntity.Id != default ?
                    targetEntity.Id.ToString() : null;
            }

            if (sourceEntity != null)
            {
                this.SourceEntityPropertyName = info.SourceEntityPropertyName;
                this.SourceEntityPropertyNamePlural = info.SourceEntityPropertyNamePlural;
                this.SourceEntityPropertyAlias = info.SourceEntityPropertyAlias;
                this.SourceEntityPropertyAliasPlural = info.SourceEntityPropertyAliasPlural;
                this.SourceEntityDescription = info.SourceEntityDescription;
                this.SourceEntityType = sourceEntity.GetType().Name.ToString().ToCamelCase();
                this.SourceEntityId = sourceEntity.Id != default ?
                    sourceEntity.Id.ToString() : null;
            }
        }

        [JsonConstructor]
        private Relationship()
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 1)]
        [Required]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "alias", Order = 2)]
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the entity type of the entity that is the source entity in this relationship.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityType", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public string SourceEntityType { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the entity that is the source entity in this relationship.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityId", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public string SourceEntityId { get; set; }

        /// <summary>
        /// Gets or sets the source entity property name.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityPropertyName", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public string SourceEntityPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the source entity property name plural.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityPropertyNamePlural", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public string SourceEntityPropertyNamePlural { get; set; }

        /// <summary>
        /// Gets or sets the source entity property alias.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityPropertyAlias", NullValueHandling = NullValueHandling.Ignore, Order = 7)]
        public string SourceEntityPropertyAlias { get; set; }

        /// <summary>
        /// Gets or sets the source entity property alias plural.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityPropertyAliasPlural", NullValueHandling = NullValueHandling.Ignore, Order = 8)]
        public string SourceEntityPropertyAliasPlural { get; set; }

        /// <summary>
        /// Gets or sets the source entity description.
        /// </summary>
        [JsonProperty(PropertyName = "sourceEntityDescription", NullValueHandling = NullValueHandling.Ignore, Order = 9)]
        public string SourceEntityDescription { get; set; }

        /// <summary>
        /// Gets or sets the entity type of the entity that is the target entity in this relationship.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityType", NullValueHandling = NullValueHandling.Ignore, Order = 10)]
        public string TargetEntityType { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the entity that is the target entity in this relationship.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityId", NullValueHandling = NullValueHandling.Ignore, Order = 11)]
        public string TargetEntityId { get; set; }

        /// <summary>
        /// Gets or sets the target entity property name.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityPropertyName", NullValueHandling = NullValueHandling.Ignore, Order = 12)]
        public string TargetEntityPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the target entity property name plural.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityPropertyNamePlural", NullValueHandling = NullValueHandling.Ignore, Order = 13)]
        public string TargetEntityPropertyNamePlural { get; set; }

        /// <summary>
        /// Gets or sets the target entity property alias.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityPropertyAlias", NullValueHandling = NullValueHandling.Ignore, Order = 14)]
        public string TargetEntityPropertyAlias { get; set; }

        /// <summary>
        /// Gets or sets the target entity property alias plural.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityPropertyAliasPlural", NullValueHandling = NullValueHandling.Ignore, Order = 15)]
        public string TargetEntityPropertyAliasPlural { get; set; }

        /// <summary>
        /// Gets or sets the target entity description.
        /// </summary>
        [JsonProperty(PropertyName = "targetEntityDescription", NullValueHandling = NullValueHandling.Ignore, Order = 16)]
        public string TargetEntityDescription { get; set; }

        /// <summary>
        /// Gets the domain entity type.
        /// </summary>
        [JsonIgnore]
        public Type DomainEntityType => this.GetType();

        [JsonIgnore]
        public RelationshipType RelationshipType { get; set; }

        [JsonIgnore]
        public bool SupportsAdditionalProperties => false;

        [JsonIgnore]
        public bool SupportsFileAttachment { get; set; } = false;

        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public string EntityReference { get; set; } = string.Empty;

        [JsonIgnore]
        public string EntityDescriptor { get; set; } = string.Empty;

        [JsonIgnore]
        public string EntityEnvironment { get; set; } = string.Empty;

        public List<string> RetrieveAdditionalDetails()
        {
            // nop
            return new List<string>();
        }
    }
}
