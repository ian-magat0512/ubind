// <copyright file="IEntitySupportingAdditionalProperties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System.Collections.Generic;

    /// <summary>
    /// This is an interface for all entitiy schema objects that do support additional properties.
    /// </summary>
    public interface IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the additional properties of the entity, where the definition alias is the property key used.
        /// </summary>/
        Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
