// <copyright file="EntitySearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Search;

    /// <inheritdoc/>
    public abstract class EntitySearchResultItemReadModel : EntityReadModel<Guid>, IEntitySearchResultItemReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySearchResultItemReadModel"/> class.
        /// </summary>
        public EntitySearchResultItemReadModel(
            Guid id,
            long createdTimestamp,
            long lastModifiedTimestamp)
        {
            this.Id = id;
            this.CreatedTicksSinceEpoch = createdTimestamp;
            this.LastModifiedTicksSinceEpoch = lastModifiedTimestamp;
        }
    }
}
