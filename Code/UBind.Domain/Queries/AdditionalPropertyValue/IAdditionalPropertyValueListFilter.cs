// <copyright file="IAdditionalPropertyValueListFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyValue
{
    using System;
    using UBind.Domain.Enums;

    /// <summary>
    /// Contract model which is used as query filter in getting the list of additional property value.
    /// </summary>
    public interface IAdditionalPropertyValueListFilter
    {
        /// <summary>
        /// Gets or sets the ID of additional property definition.
        /// </summary>
        Guid? AdditionalPropertyDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the value of additional property value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets or sets the Additional property entity type.
        /// </summary>
        AdditionalPropertyEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the property type <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        AdditionalPropertyDefinitionType PropertyType { get; set; }
    }
}
