// <copyright file="Relationship.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Stores the relationship association of two entities.
    /// </summary>
    public class Relationship : Entity<Guid>, IEquatable<Relationship>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="fromEntityType">The entity type this Relationship is from.</param>
        /// <param name="fromEntityId">The entity id of this Relationship is from.</param>
        /// <param name="type">The relationship type.</param>
        /// <param name="toEntityType">The entity type this Relationship is to.</param>
        /// <param name="toEntityId">The entity id of this Relationship is to.</param>
        /// <param name="timestamp">The time the quote email was created.</param>
        public Relationship(
            Guid tenantId,
            EntityType fromEntityType,
            Guid fromEntityId,
            RelationshipType type,
            EntityType toEntityType,
            Guid toEntityId,
            Instant timestamp)
            : base(Guid.NewGuid(), timestamp)
        {
            this.TenantId = tenantId;
            this.FromEntityType = fromEntityType;
            this.FromEntityId = fromEntityId;
            this.Type = type;
            this.ToEntityType = toEntityType;
            this.ToEntityId = toEntityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        private Relationship()
            : base(default(Guid), default(Instant))
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the tenant id of the relationship.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the entity type Relationship from with this record.
        /// </summary>
        public EntityType FromEntityType { get; private set; }

        /// <summary>
        /// Gets the entity identifier of the entity type Relationship from with this record.
        /// </summary>
        public Guid FromEntityId { get; private set; }

        /// <summary>
        /// Gets the type of the relationship.
        /// </summary>
        public RelationshipType Type { get; private set; }

        /// <summary>
        /// Gets the entity type Relationship to with this record.
        /// </summary>
        public EntityType ToEntityType { get; private set; }

        /// <summary>
        /// Gets the entity identifier of the entity type Relationship to with this record.
        /// </summary>
        public Guid ToEntityId { get; private set; }

        /// <summary>
        /// Updates the ToEntity property.
        /// </summary>
        /// <param name="entityId">The Id of the ToEntity.</param>
        /// <remarks>
        /// Is currently used when trying to associate quote or policy emails with a new customer. Property `ToEntityId` is still
        /// marked as private setter.
        /// </remarks>
        public void UpdateToEntityId(Guid entityId)
        {
            this.ToEntityId = entityId;
        }

        /// <summary>
        /// Updates the FromEntity property.
        /// </summary>
        /// <param name="entityId">The Id of the FromEntity.</param>
        /// <remarks>
        /// Is currently used when trying to associate quote or policy emails with a new customer. Property `ToEntityId` is still
        /// marked as private setter.
        /// </remarks>
        public void UpdateFromEntityId(Guid entityId)
        {
            this.FromEntityId = entityId;
        }

        /// <summary>
        /// Retrieve the entity id of an entity type.
        /// </summary>
        /// <param name="entityType">The entity id.</param>
        /// <returns>A Guid.</returns>
        public Guid? GetEntityId(EntityType entityType)
        {
            return
                this.FromEntityType == entityType ? this.FromEntityId :
                this.ToEntityType == entityType ? this.ToEntityId :
                (Guid?)null;
        }

        /// <summary>
        /// Check if the relationship has the entity type from whichever direction.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>A boolean.</returns>
        public bool HasEntity(EntityType entityType)
        {
            return this.FromEntityType == entityType || this.ToEntityType == entityType;
        }

        public bool Equals(Relationship? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.FromEntityId == this.FromEntityId
                && other.ToEntityId == this.ToEntityId
                && other.Type == this.Type;
        }
    }
}
