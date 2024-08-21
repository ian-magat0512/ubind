// <copyright file="IAdditionalPropertyValueReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// Table entity that contains all the historical values assigned to an entity with reference to its parent additional property definition.
    /// </summary>
    public interface IAdditionalPropertyValueReadModel
    {
        /// <summary>
        /// Gets the foreign key of additional property definition.
        /// </summary>
        public Guid AdditionalPropertyDefinitionId { get; }

        /// <summary>
        /// Gets the additional property definition.
        /// </summary>
        public AdditionalPropertyDefinitionReadModel AdditionalPropertyDefinition { get; }

        /// <summary>
        /// Gets or sets the persisted value for the mapping between additional property definition and entity.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the entity id that owns this value.
        /// </summary>
        public Guid EntityId { get; }

        /// <summary>
        /// Gets or sets the new tenant ID in GUID type.
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
