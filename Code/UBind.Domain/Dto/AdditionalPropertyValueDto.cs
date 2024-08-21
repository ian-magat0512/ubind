// <copyright file="AdditionalPropertyValueDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    using System;

    /// <summary>
    /// Data transfer object for additional property definitions.
    /// </summary>
    public class AdditionalPropertyValueDto
    {
        /// <summary>
        /// Gets or sets the value when this was created.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the primary ID of an eav table.
        /// The ID can be null if no value has been set, and we're using the default value.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the ID where this model is associated to.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        ///  Gets or sets the dto object for its parent definition.
        /// </summary>
        public AdditionalPropertyDefinitionDto AdditionalPropertyDefinition { get; set; }
    }
}
