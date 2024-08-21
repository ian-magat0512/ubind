// <copyright file="IResourceFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Filters
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for resource filters.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource being filtered.</typeparam>
    public interface IResourceFilter<TResource>
    {
        /// <summary>
        /// Apply the filter to a source of resources.
        /// </summary>
        /// <param name="resources">The resources to filter.</param>
        /// <returns>A filtered collection of resources.</returns>
        IEnumerable<TResource> Apply(IEnumerable<TResource> resources);
    }
}
