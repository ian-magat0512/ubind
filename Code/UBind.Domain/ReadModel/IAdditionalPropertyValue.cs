// <copyright file="IAdditionalPropertyValue.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// A contract for representing the additional property definition and its value for the given entity.
    /// </summary>
    public interface IAdditionalPropertyValue
    {
        /// <summary>
        /// Gets the Id of the additional property definition.
        /// </summary>
        Guid AdditionalPropertyDefinitionId { get; }

        /// <summary>
        /// Gets the ID of the entity.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        /// Gets or sets the value to be persisted.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets the tenant's Id.
        /// </summary>
        Guid TenantId { get; }
    }
}
