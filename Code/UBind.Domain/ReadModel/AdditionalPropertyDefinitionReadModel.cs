// <copyright file="AdditionalPropertyDefinitionReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// Model that represents the AdditionalPropertyDefinition of the table.
    /// </summary>
    public class AdditionalPropertyDefinitionReadModel : MutableEntity<Guid>, IEntityReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="id">Primary key value.</param>
        /// <param name="createdTimestamp">The created timestamp.</param>
        /// <param name="alias">Alias.</param>
        /// <param name="name">Name of the property.</param>
        /// <param name="entityType">Entity type.</param>
        /// <param name="contextType">Context Type.</param>
        /// <param name="contextId">Context id.</param>
        /// <param name="isRequired">True if required otherwise false.</param>
        /// <param name="isUnique">True if it should be unique otherwise false.</param>
        /// <param name="isDeleted">True if mark as deleted otherwise false.</param>
        /// <param name="defaultValue">If it contains a default value.</param>
        /// <param name="propertyType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="parentContextId">The parent context of the context where this model is under.</param>
        public AdditionalPropertyDefinitionReadModel(
            Guid tenantId,
            Guid id,
            Instant createdTimestamp,
            string alias,
            string name,
            AdditionalPropertyEntityType entityType,
            AdditionalPropertyDefinitionContextType contextType,
            Guid contextId,
            bool isRequired,
            bool isUnique,
            bool isDeleted,
            string defaultValue,
            AdditionalPropertyDefinitionType propertyType,
            AdditionalPropertyDefinitionSchemaType? schemaType,
            Guid? parentContextId = null,
            string? customSchema = null)
            : base(id, createdTimestamp)
        {
            this.Alias = alias;
            this.Name = name;
            this.EntityType = entityType;
            this.ContextType = contextType;
            this.ContextId = contextId;
            this.IsRequired = isRequired;
            this.IsUnique = isUnique;
            this.IsDeleted = isDeleted;
            this.DefaultValue = defaultValue;
            this.ParentContextId = parentContextId;
            this.PropertyType = propertyType;
            this.TenantId = tenantId;
            this.SchemaType = schemaType;
            this.CustomSchema = customSchema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionReadModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        private AdditionalPropertyDefinitionReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the property alias of this model.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the property name of this model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity type this model belongs to.
        /// </summary>
        public AdditionalPropertyEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the context type this model belongs to.
        /// </summary>
        public AdditionalPropertyDefinitionContextType ContextType { get; set; }

        /// <summary>
        /// Gets or sets the primary id of the context (TenantId,OrganisationId,ProductId) that uses this model.
        /// Reason why it is in string form is because there's a possibility that context id's data types varies.
        /// </summary>
        public Guid ContextId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is unique.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model has been soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value of the parent context which this model falls under.
        /// </summary>
        public Guid? ParentContextId { get; set; }

        /// <summary>
        /// Gets or sets the default value of this property.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets property type.
        /// </summary>
        public AdditionalPropertyDefinitionType PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets schema type of a structured data type property.
        /// </summary>
        public AdditionalPropertyDefinitionSchemaType? SchemaType { get; set; }

        /// <summary>
        /// Gets or sets custom schema of a structured data type property.
        /// </summary>
        public string? CustomSchema { get; set; }
    }
}
