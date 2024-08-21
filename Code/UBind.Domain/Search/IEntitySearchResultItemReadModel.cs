// <copyright file="IEntitySearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;

    /// <summary>
    /// Base class for search index read models.
    /// </summary>
    public interface IEntitySearchResultItemReadModel : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets the last updated date(in unix ticks).
        /// </summary>
        long LastModifiedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the created date(in unix ticks).
        /// </summary>
        long CreatedTicksSinceEpoch { get; }
    }
}
