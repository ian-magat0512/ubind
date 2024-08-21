// <copyright file="ResourcePool.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ResourcePool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Services.Email;
    using UBind.Domain.Exceptions;
    using Errors = UBind.Domain.Errors;

    /// <summary>
    /// Base implementation of a resource pool.
    /// </summary>
    public abstract class ResourcePool : IResourcePool
    {
        /// <summary>
        /// This lock is used to ensure that transactions against the available pool is locked in a single thread.
        /// </summary>
        private readonly object poolUsageLock = new object();
        private readonly IErrorNotificationService errorNotificationService;

        private ConcurrentQueue<IResourcePoolMember>? availablePool = new ConcurrentQueue<IResourcePoolMember>();
        private int instanceCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePool"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="logger">The error notification service.</param>
        public ResourcePool(
            IClock clock,
            ILogger<IResourcePool> logger,
            IErrorNotificationService errorNotificationService)
        {
            this.Logger = logger;
            this.Clock = clock;
            this.errorNotificationService = errorNotificationService;
        }

        /// <summary>
        /// An event triggered when a resource is acquired.
        /// </summary>
        public event EventHandler? ResourceAquired;

        /// <summary>
        /// An event triggered when a resource is released.
        /// </summary>
        public event EventHandler? ResourceReleased;

        /// <summary>
        /// An event triggered when the pool is exhausted (all items are used and someone wants another).
        /// </summary>
        public event EventHandler? PoolExhausted;

        /// <summary>
        /// An event triggered when a resource is added to the pool.
        /// </summary>
        public event EventHandler? ResourceAdded;

        /// <summary>
        /// An event triggered when a resource is removed from the pool.
        /// </summary>
        public event EventHandler? ResourceRemoved;

        /// <summary>
        /// Gets or sets a collector of events for generating statistics.
        /// </summary>
        public ResourcePoolStatsCollector? StatsCollector { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger<IResourcePool> Logger { get; private set; }

        /// <summary>
        /// Gets the clock.
        /// </summary>
        public IClock Clock { get; private set; }

        /// <summary>
        /// Gets or sets the pool size manager, or null if not set.
        /// </summary>
        public ResourcePoolSizeManager? PoolSizeManager { get; set; }

        /// <inheritdoc/>
        public IResourcePoolMember AcquireResource()
        {
            using (MiniProfiler.Current.Step($"{this.GetType()}.{nameof(this.AcquireResource)}"))
            {
                IResourcePoolMember? instance = null;
                int retries = 0;
                const int maxRetries = 10;

                while (retries < maxRetries)
                {
                    lock (this.poolUsageLock)
                    {
                        if (this.availablePool == null)
                        {
                            throw new ErrorException(Domain.Errors.ResourcePool.ObjectDisposed());
                        }

                        if (this.TryDequeueResource(out instance))
                        {
                            // Resource acquired successfully
                            break;
                        }
                    }

                    // No resource acquired, add a new one
                    try
                    {
                        this.AddResource();
                    }
                    catch (ErrorException ex)
                    {
                        this.Logger.LogError(ex, "Failed to add resource to pool.");
                    }

                    if (this.PoolSizeManager != null)
                    {
                        this.PoolSizeManager.OnPoolExhausted();
                        this.PoolExhausted?.Invoke(this, EventArgs.Empty);
                    }

                    retries++;
                }

                if (instance == null)
                {
                    throw new ErrorException(Domain.Errors.ResourcePool.MaxRetriesReached(retries));
                }

                if (this.PoolSizeManager != null)
                {
                    this.PoolSizeManager.OnResourceConsumed();
                }

                this.ResourceAquired?.Invoke(this, EventArgs.Empty);
                return instance;
            }
        }

        /// <inheritdoc/>
        public void ReleaseResource(IResourcePoolMember instance)
        {
            lock (this.poolUsageLock)
            {
                if (this.availablePool != null)
                {
                    this.availablePool.Enqueue(instance);
                }
            }

            this.ResourceReleased?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc/>
        public int GetResourceCount()
        {
            return this.instanceCount;
        }

        /// <inheritdoc/>
        public int GetUsageCount()
        {
            int availablePoolCount = 0;
            lock (this.poolUsageLock)
            {
                if (this.availablePool == null)
                {
                    return 0;
                }

                availablePoolCount = this.availablePool.Count;
            }

            return this.instanceCount - availablePoolCount;
        }

        /// <inheritdoc/>
        public void AddResource()
        {
            using (MiniProfiler.Current.Step(this.GetType() + "." + nameof(this.AddResource)))
            {
                var instance = this.CreateResource();
                if (instance != null)
                {
                    this.InsertResource(instance);
                }
                else
                {
                    var availableMemory = Helpers.MemoryHelper.FormatMemorySize(Helpers.MemoryHelper.GetAvailablePhysicalMemory());
                    var resourceDetails = new List<string>
                    {
                        $"Pool: {this.GetDebugIdentifier()}",
                        $"Resource Count: {this.GetResourceCount()}",
                        $"Usage Count: {this.GetUsageCount()}",
                        $"Available Memory: {availableMemory}",
                    };
                    this.Logger.LogInformation(string.Join(Environment.NewLine, resourceDetails));
                    this.Logger.LogError(
                        $"Failed to add resource in {this.GetType().Name}.{nameof(this.AddResource)}: "
                        + "Resource creation returned null.");
                    var exception = new ErrorException(Errors.ResourcePool.FailedToCreateResource(resourceDetails));
                    this.errorNotificationService.CaptureSentryException(
                        exception,
                        null,
                        resourceDetails);
                    throw exception;
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveResource()
        {
            bool succeeded = false;

            lock (this.poolUsageLock)
            {
                if (this.availablePool == null)
                {
                    return;
                }

                succeeded = this.availablePool.TryDequeue(out var workbookInstance);
            }

            if (succeeded)
            {
                Interlocked.Decrement(ref this.instanceCount);
                this.Logger.LogInformation(
                    "ResourcePoolSizeManager: {DebugIdentifier} - Resource removed. "
                    + "There are now {ResourceCount} resources in the pool.",
                    this.GetDebugIdentifier(),
                    this.GetResourceCount());
                this.ResourceRemoved?.Invoke(this, new EventArgs());
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IResourcePoolMember> GetAvailableResources()
        {
            return this.availablePool?.ToArray() ?? Enumerable.Empty<IResourcePoolMember>().ToArray();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // dispose managed objects
            if (this.StatsCollector != null)
            {
                // dispose if not yet disposed
                this.StatsCollector.Stop();
                this.StatsCollector = null;
            }

            lock (this.poolUsageLock)
            {
                if (this.availablePool != null)
                {
                    // dispose if not already disposed.
                    while (this.availablePool.TryDequeue(out var member))
                    {
                        // NOP.
                    }

                    this.availablePool = null;
                }
            }

            if (this.PoolSizeManager != null)
            {
                this.PoolSizeManager.Dispose();
                this.PoolSizeManager = null;
            }
        }

        /// <summary>
        /// Gets a string that identifies this resource pool for debugging purposes.
        /// </summary>
        /// <returns>A string that identifies this resource pool for debugging purposes.</returns>
        public abstract string GetDebugIdentifier();

        public virtual bool TryDequeueResource(out IResourcePoolMember? instance)
        {
            if (this.availablePool?.TryDequeue(out instance) != true)
            {
                instance = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Insert a resource into the pool.
        /// </summary>
        /// <param name="resource">The resource to insert.</param>
        protected void InsertResource(IResourcePoolMember resource)
        {
            using (MiniProfiler.Current.Step(this.GetType() + "." + nameof(this.InsertResource)))
            {
                bool enqueued = false;

                lock (this.poolUsageLock)
                {
                    if (this.availablePool != null)
                    {
                        this.availablePool.Enqueue(resource);
                        enqueued = true;
                    }
                }

                if (enqueued)
                {
                    Interlocked.Increment(ref this.instanceCount);
                    this.Logger.LogInformation(
                        "ResourcePoolSizeManager: {DebugIdentifier} - Resource added. "
                        + "There are now {ResourceCount} resources in the pool.",
                        this.GetDebugIdentifier(),
                        this.GetResourceCount());
                    this.ResourceAdded?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Instantiates an instance for adding to the pool. Must be implemented by subclasses.
        /// </summary>
        /// <returns>An instance which can be added to the pool.</returns>
        protected abstract IResourcePoolMember CreateResource();
    }
}
