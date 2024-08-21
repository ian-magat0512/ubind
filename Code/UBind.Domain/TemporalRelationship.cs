// <copyright file="TemporalRelationship.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents the relationship between an application (quote) and owner.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity the relationship is for.</typeparam>
    public class TemporalRelationship<TEntity> : Entity<Guid>
        where TEntity : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporalRelationship{TEntity}"/> class.
        /// </summary>
        /// <remarks>Paramterless constructor for EF.</remarks>
        public TemporalRelationship()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporalRelationship{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The entity the relationship is with.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public TemporalRelationship(TEntity entity, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets or sets the entity being referenced in the relationship.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the application being referenced in the relationship.
        /// </summary>
        public Guid ApplicationId { get; set; }
    }
}
