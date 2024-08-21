// <copyright file="IAdditionalPropertyValueEventDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using UBind.Domain.Enums;

    /// <summary>
    /// Contract that contains properties for additional property value event.
    /// </summary>
    public interface IAdditionalPropertyValueEventDetails
    {
        /// <summary>
        /// Gets the value of the ID of additional property definition.
        /// </summary>
        Guid AdditionalPropertyDefinitionId { get; }

        /// <summary>
        /// Gets the value of the ID where this is associated to.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        /// Gets the value to be persisted.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets the value of the property type.
        /// </summary>
        AdditionalPropertyDefinitionType AdditionalPropertyDefinitionType { get; }

        /// <summary>
        /// Gets the tenant's ID in GUID.
        /// </summary>
        Guid TenantId { get; }
    }
}
