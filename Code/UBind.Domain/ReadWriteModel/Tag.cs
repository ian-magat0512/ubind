// <copyright file="Tag.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Stores the tag data of the association.
    /// </summary>
    public class Tag : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="entityType">The entity type this tag is associated with.</param>
        /// <param name="tagType">The tag type this tag.</param>
        /// <param name="entityId">The entity id this tag is associated with.</param>
        /// <param name="tagValue">The tag value of the entity so its easily searchable. ( any 64 character string ).</param>
        /// <param name="timestamp">The time the tag was created.</param>
        public Tag(EntityType entityType, Guid entityId, TagType tagType, string tagValue, Instant timestamp)
            : base(Guid.NewGuid(), timestamp)
        {
            this.EntityType = entityType;
            this.EntityId = entityId;
            this.TagType = tagType;
            this.Value = tagValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteModel.Tag"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        private Tag()
            : base(default(Guid), default(Instant))
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the ID of the entity this tag is for.
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Gets the entity type associated with this record.
        /// </summary>
        public EntityType EntityType { get; private set; }

        /// <summary>
        /// Gets the entity tag so its easily searchable associated with this record.
        /// </summary>
        public TagType TagType { get; private set; }

        /// <summary>
        /// Gets the entity tag value so its easily searchable associated with this record.
        /// </summary>
        public string Value { get; private set; }
    }
}
