// <copyright file="IAdditionalPropertyDefinitionDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyDefinition
{
    using System;
    using UBind.Domain.Enums;

    /// <summary>
    /// Base contracts for additional property definition.
    /// </summary>
    public interface IAdditionalPropertyDefinitionDetails
    {
        /// <summary>
        /// Gets or sets the latest value assigned to this model.
        /// Please take note that this value will come from a different table associated with addtional property definition table.
        /// </summary>
        string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is required or not. True if required, otherwise false.
        /// </summary>
        bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is unique or not. True if unique, otherwise false.
        /// </summary>
        bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets an alias defined for this property.
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity type <see cref="AdditionalPropertyEntityType"/> where the model is associated with.
        /// </summary>
        AdditionalPropertyEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets a parent context which this context is under.
        /// </summary>
        Guid? ParentContextId { get; set; }

        /// <summary>
        /// Gets or sets the type of the data persisted. <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        AdditionalPropertyDefinitionType Type { get; set; }

        /// <summary>
        /// Gets or sets the schema type of the data persisted. <see cref="AdditionalPropertyDefinitionSchemaType"/>.
        /// </summary>
        AdditionalPropertyDefinitionSchemaType? SchemaType { get; set; }

        /// <summary>
        /// Gets or sets the custom schema of the data persisted, if it is a custom structured data property type.
        /// </summary>
        string? CustomSchema { get; set; }
    }
}
