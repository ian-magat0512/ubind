// <copyright file="DocumentEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Base class for document entity.
    /// </summary>
    public abstract class DocumentEntity : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEntity"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the entity.</param>
        /// <param name="createdTimestamp">The created time.</param>
        public DocumentEntity(Guid id, Instant createdTimestamp)
        {
            this.Id = id;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEntity"/> class.
        /// </summary>
        protected DocumentEntity()
        {
        }
    }
}
