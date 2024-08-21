// <copyright file="AdditionalPropertyValueUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Contract model for additional property value with id, value and property type.
    /// </summary>
    public class AdditionalPropertyValueUpsertModel
    {
        /// <summary>
        /// Gets or sets the value when this was created.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the data persisted. <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        public AdditionalPropertyDefinitionType Type { get; set; }

        /// <summary>
        /// Gets or sets the primary ID of the <see cref="AdditionalPropertyDefinitionReadModel"/>.
        /// </summary>
        public Guid DefinitionId { get; set; }
    }
}
