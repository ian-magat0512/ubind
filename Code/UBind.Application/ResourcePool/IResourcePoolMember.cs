// <copyright file="IResourcePoolMember.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ResourcePool
{
    using NodaTime;

    /// <summary>
    /// Represents a member of the pool, which has key properties required for
    /// managing the pool size.
    /// </summary>
    public interface IResourcePoolMember
    {
        /// <summary>
        /// Gets or sets the Instant this workbook instance was created.
        /// </summary>
        Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Instant this workbook was last used.
        /// </summary>
        Instant LastUsedTimestamp { get; set; }
    }
}
