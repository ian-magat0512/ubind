// <copyright file="IAdditionalProperties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System.Collections.Generic;

    /// <summary>
    /// Contract that contains generic list of <see cref="AdditionalPropertyValue"/>.
    /// </summary>
    public interface IAdditionalProperties
    {
        /// <summary>
        /// Gets the value of a list of additional property values added to an entity.
        /// </summary>
        List<AdditionalPropertyValue> AdditionalPropertyValues { get; }
    }
}
