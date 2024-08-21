// <copyright file="EntityTypeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;

    /// <summary>
    /// An attribute to specify the entity type which the field, class or interface relates to.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface,
        AllowMultiple = true)]
    public class EntityTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeAttribute"/> class.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        public EntityTypeAttribute(EntityType entityType)
        {
            this.EntityType = entityType;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType EntityType { get; }
    }
}
