// <copyright file="IEntitySearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;

    /// <summary>
    /// Base model for writing into search indexes/documents.
    /// for a specific product.
    /// </summary>
    public interface IEntitySearchIndexWriteModel
    {
        /// <summary>
        /// Gets the Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the last updated date in unix timestamp.
        /// </summary>
        long LastModifiedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the last updated date by User in unix timestamp.
        /// </summary>
        long? LastModifiedByUserTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the created time in unix timestamp.
        /// </summary>
        long CreatedTicksSinceEpoch { get; }
    }
}
