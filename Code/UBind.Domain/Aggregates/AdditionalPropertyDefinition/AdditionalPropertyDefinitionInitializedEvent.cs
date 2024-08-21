// <copyright file="AdditionalPropertyDefinitionInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyDefinition
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// Extended class for additional property definition's class.
    /// </summary>
    public partial class AdditionalPropertyDefinition
    {
        /// <summary>
        /// An event that creates an instance of the <see cref="AdditionalPropertyDefinition"/> aggregate.
        /// </summary>
        public class AdditionalPropertyDefinitionInitializedEvent :
            Event<AdditionalPropertyDefinition, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">Tenant's ID.</param>
            /// <param name="id">Assigned primary key value.</param>
            /// <param name="contextId">It could be primary id of tenant,organisation and product which it will be associated to. If the id is in other type than string then you should covert to string first.</param>
            /// <param name="alias">Alias of the additional property.</param>
            /// <param name="name">Name of the additional property.</param>
            /// <param name="entityType">Entity type which this will be assigned to.</param>
            /// <param name="contextType">Context which this will be assigned to.</param>
            /// <param name="isRequired">Set to true if required.</param>
            /// <param name="isUnique">Set to true if unique.</param>
            /// <param name="parentContextId">primary value of the parent context which this model falls under.</param>
            /// <param name="propertyType">Type of property of this definition.</param>
            /// <param name="defaultValue">The default value for this property.</param>
            /// <param name="performingUserId">Performing user id.</param>
            /// <param name="localtime">Local time.</param>
            /// <param name="schemaType">Schema type of the structured data property type.</param>
            /// <param name="customSchema">Custom schema of the structured data.</param>
            public AdditionalPropertyDefinitionInitializedEvent(
                Guid tenantId,
                Guid id,
                Guid contextId,
                string alias,
                string name,
                AdditionalPropertyEntityType entityType,
                AdditionalPropertyDefinitionContextType contextType,
                bool isRequired,
                bool isUnique,
                Guid? parentContextId,
                AdditionalPropertyDefinitionType propertyType,
                string defaultValue,
                Guid? performingUserId,
                Instant localtime,
                AdditionalPropertyDefinitionSchemaType? schemaType,
                string? customSchema)
                : base(tenantId, id, performingUserId, localtime)
            {
                this.ContextId = contextId;
                this.Alias = alias;
                this.Name = name;
                this.EntityType = entityType;
                this.ContextType = contextType;
                this.IsRequired = isRequired;
                this.IsUnique = isUnique;
                this.ParentContextId = parentContextId;
                this.DefaultValue = defaultValue;
                this.PropertyType = propertyType;
                this.SchemaType = schemaType;
                this.CustomSchema = customSchema;
            }

            private AdditionalPropertyDefinitionInitializedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the ID of the context where the property is under.
            /// </summary>
            [JsonProperty]
            public Guid ContextId { get; private set; }

            /// <summary>
            /// Gets the alias of the property.
            /// </summary>
            [JsonProperty]
            public string Alias { get; private set; }

            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            [JsonProperty]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the entity type.
            /// </summary>
            [JsonProperty]
            public AdditionalPropertyEntityType EntityType { get; private set; }

            /// <summary>
            /// Gets the context type <see cref="AdditionalPropertyDefinitionContextType"/> of the property.
            /// </summary>
            [JsonProperty]
            public AdditionalPropertyDefinitionContextType ContextType { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the property is required. Otherwise false.
            /// </summary>
            [JsonProperty]
            public bool IsRequired { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the property is true if unique. Otherwise false.
            /// </summary>
            [JsonProperty]
            public bool IsUnique { get; private set; }

            /// <summary>
            /// Gets the ID of the parent context which the immediate context is under.
            /// </summary>
            [JsonProperty]
            public Guid? ParentContextId { get; private set; }

            /// <summary>
            /// Gets or sets the default value of the property.
            /// </summary>
            [JsonProperty]
            public string DefaultValue { get; set; }

            /// <summary>
            /// Gets the data-type of property value.
            /// </summary>
            [JsonProperty]
            public AdditionalPropertyDefinitionType PropertyType { get; private set; }

            /// <summary>
            /// Gets the schema-type of a structured data property type.
            /// </summary>
            [JsonProperty]
            public AdditionalPropertyDefinitionSchemaType? SchemaType { get; private set; }

            /// <summary>
            /// Gets the custom schema of a structured data property type.
            /// </summary>
            [JsonProperty]
            public string? CustomSchema { get; private set; }
        }
    }
}
