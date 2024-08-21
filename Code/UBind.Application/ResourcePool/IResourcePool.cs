// <copyright file="IResourcePool.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ResourcePool
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a pool of resources that can be acquired and released, and managed.
    /// </summary>
    public interface IResourcePool : IDisposable
    {
        /// <summary>
        /// An event triggered when a resource is acquired.
        /// </summary>
        event EventHandler ResourceAquired;

        /// <summary>
        /// An event triggered when a resource is released.
        /// </summary>
        event EventHandler ResourceReleased;

        /// <summary>
        /// An event triggered when the pool is exhausted (all items are used and someone wants another).
        /// </summary>
        event EventHandler PoolExhausted;

        /// <summary>
        /// An event triggered when a resource is added to the pool.
        /// </summary>
        event EventHandler ResourceAdded;

        /// <summary>
        /// An event triggered when a resource is removed from the pool.
        /// </summary>
        event EventHandler ResourceRemoved;

        /// <summary>
        /// Gets or sets the pool size manager, or null if not set.
        /// </summary>
        ResourcePoolSizeManager PoolSizeManager { get; set; }

        /// <summary>
        /// Aquire a resource instance for exclusive use.
        /// </summary>
        /// <returns>An active workbook session.</returns>
        IResourcePoolMember AcquireResource();

        /// <summary>
        /// Release a resource instance for others to use.
        /// </summary>
        /// <param name="instance">The instance of a release workbook.</param>
        void ReleaseResource(IResourcePoolMember instance);

        /// <summary>
        /// Get the resource instances count.
        /// </summary>
        /// <returns>returns the pool size.</returns>
        int GetResourceCount();

        /// <summary>
        /// Gets the number of resources currently in use.
        /// </summary>
        /// <returns>The number of resources currently in use.</returns>
        int GetUsageCount();

        /// <summary>
        /// Creates a resource instance and adds it to the pool so it can be used.
        /// </summary>
        void AddResource();

        /// <summary>
        /// Deletes a resource instance from the pool to reduce memory usage.
        /// </summary>
        void RemoveResource();

        /// <summary>
        /// Retreives a collection of the resource instance which are not currently being used.
        /// </summary>
        /// <returns>The unused resources.</returns>
        IReadOnlyCollection<IResourcePoolMember> GetAvailableResources();

        /// <summary>
        /// Gets a string that identifies this resource pool for debugging purposes.
        /// </summary>
        /// <returns>A string that identifies this resource pool for debugging purposes.</returns>
        string GetDebugIdentifier();
    }
}
