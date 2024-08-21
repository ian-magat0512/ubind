// <copyright file="AdditionalPropertyDefinitionDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    using System;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Dto class for <see cref="AdditionalPropertyDefinitionReadModel"/>.
    /// </summary>
    public class AdditionalPropertyDefinitionDto
    {
        /// <summary>
        /// Gets or sets the primary ID of <see cref="AdditionalPropertyDefinitionReadModel"/>.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Gets or sets property type.
        /// </summary>
        public AdditionalPropertyDefinitionType PropertyType { get; set; }

        /// <summary>
        /// Gets or sets a value of the parent context which this model falls under.
        /// </summary>
        public Guid? ParentContextId { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property was deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

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
