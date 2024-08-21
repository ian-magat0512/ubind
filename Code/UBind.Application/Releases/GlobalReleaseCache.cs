// <copyright file="GlobalReleaseCache.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Profiling;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class GlobalReleaseCache : IGlobalReleaseCache
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ConcurrentDictionary<ReleaseContext, ActiveDeployedRelease> cache
            = new ConcurrentDictionary<ReleaseContext, ActiveDeployedRelease>();

        private readonly ConcurrentDictionary<ReleaseContext, SemaphoreSlim> releaseLocks
            = new ConcurrentDictionary<ReleaseContext, SemaphoreSlim>();

        private IFieldSerializationBinder fieldSerializationBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalReleaseCache"/> class.
        /// </summary>
        /// <param name="fieldSerializationBinder">The json serialization binder for field types.</param>
        public GlobalReleaseCache(
            IFieldSerializationBinder fieldSerializationBinder,
            IServiceProvider serviceProvider)
        {
            this.fieldSerializationBinder = fieldSerializationBinder;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public event EventHandler<ReleaseCachingArgs> ReleaseCached;

        /// <inheritdoc/>
        public ActiveDeployedRelease CacheNewDevRelease(DevReleaseInitializationArtefacts artefacts)
        {
            using (MiniProfiler.Current.Step($"{nameof(GlobalReleaseCache)}.{nameof(this.CacheNewDevRelease)}"))
            {
                var releaseContext = new ReleaseContext(
                    artefacts.DevRelease.TenantId,
                    artefacts.DevRelease.ProductId,
                    DeploymentEnvironment.Development,
                    artefacts.DevRelease.Id);
                var cachedRelease = new ActiveDeployedRelease(
                    artefacts.DevRelease,
                    DeploymentEnvironment.Development,
                    this.fieldSerializationBinder);
                this.cache[releaseContext] = cachedRelease;
                var eventArgs = ReleaseCachingArgs.CreateWithSeedWorkbooks(
                    cachedRelease, artefacts.QuoteWorkbook, artefacts.ClaimWorkbook);
                this.ReleaseCached?.Invoke(this, eventArgs);
                return cachedRelease;
            }
        }

        /// <inheritdoc/>
        public ActiveDeployedRelease GetRelease(
            ReleaseContext context,
            IProductReleaseService productReleaseService)
        {
            using (MiniProfiler.Current.Step($"{nameof(GlobalReleaseCache)}.{nameof(this.GetRelease)}"))
            {
                if (this.cache.TryGetValue(context, out var cachedRelease) && !this.IsStaleOrNonExistent(cachedRelease, context, productReleaseService))
                {
                    return cachedRelease;
                }

                // Here we get a lock for the release context so that only one thread can get the release from the database
                // and cache it. This is to prevent multiple threads from getting the same release from the database at the same time
                // and overwhelming the database.
                var semaphore = this.releaseLocks.GetOrAdd(context, ctx => new SemaphoreSlim(1, 1));
                semaphore.Wait(); // Acquire the lock

                try
                {
                    // Double-check the cache in case it was populated while waiting for the lock.
                    cachedRelease = this.cache.TryGetValue(context, out var existingRelease)
                        ? existingRelease
                        : null;

                    if (this.IsStaleOrNonExistent(cachedRelease, context, productReleaseService))
                    {
                        var release = productReleaseService.GetReleaseFromDatabaseWithoutAssetFileContents(context.TenantId, context.ProductReleaseId);
                        if (release == null)
                        {
                            // this should never happen unless someone just made up a fake release ID
                            throw new InvalidOperationException($"The release with ID {context.ProductReleaseId} for tenant "
                                + $"{context.TenantId} and product {context.ProductId} was not found.");
                        }

                        // Clear resource pool
                        var spreadsheetPoolService = this.serviceProvider.GetService<ISpreadsheetPoolService>();
                        spreadsheetPoolService?.RemoveSpreadsheetPool(context, WebFormAppType.Quote);
                        spreadsheetPoolService?.RemoveSpreadsheetPool(context, WebFormAppType.Claim);

                        cachedRelease = new ActiveDeployedRelease(
                            release,
                            context.Environment,
                            this.fieldSerializationBinder);
                        this.cache[context] = cachedRelease;
                        this.ReleaseCached?.Invoke(
                            this,
                            ReleaseCachingArgs.CreateWithoutSeedWorkbooks(cachedRelease));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("semaphore worked");
                    }
                }
                finally
                {
                    semaphore.Release();
                }

                return cachedRelease;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<ReleaseContext, Guid>> GetCacheState() =>

            // Need to use ToArray for thread-safety as Linq operations are not thread-safe on concurrent dictionary.
            this.cache.ToArray().Select(kvp => new KeyValuePair<ReleaseContext, Guid>(kvp.Key, kvp.Value.ReleaseId)).ToList();

        public int InvalidateCache(
            IProductReleaseService productReleaseService,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
            Guid? productReleaseId = null)
        {
            // Need to use ToArray for thread-safety as Linq operations are not thread-safe on concurrent dictionary.
            int numberCleared = 0;
            var releaseContexts = this.cache.ToArray()
                .Where(kvp => (tenantId == null || kvp.Key.TenantId == tenantId)
                    && (productId == null || kvp.Key.ProductId == productId)
                    && (environment == null || kvp.Key.Environment == environment)
                    && (productReleaseId == null || kvp.Key.ProductReleaseId == productReleaseId))
                .Select(kvp => kvp.Key);
            foreach (var releaseContext in releaseContexts)
            {
                if (this.cache.TryGetValue(releaseContext, out ActiveDeployedRelease cachedRelease))
                {
                    if (this.IsStaleOrNonExistent(cachedRelease, releaseContext, productReleaseService))
                    {
                        this.cache.Remove(releaseContext, out _);
                        numberCleared++;
                    }
                }
            }

            return numberCleared;
        }

        private bool IsStaleOrNonExistent(
            ActiveDeployedRelease? cachedRelease, ReleaseContext context, IProductReleaseService productReleaseService)
        {
            if (cachedRelease == null || cachedRelease?.LastModifiedTimestamp == null)
            {
                return true;
            }

            // For non-development environment releases, we don't need to check the last modified timestamp
            // since only DevReleases can actually be changed. So we can return early.
            if (context.Environment != DeploymentEnvironment.Development)
            {
                return false;
            }

            using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.IsStaleOrNonExistent)}"))
            {
                // Check if the cached release exists and if its last modified timestamp is older than the one in the database.
                var lastModifiedTimestamp = productReleaseService.GetLastModifiedTimestamp(
                    context.TenantId, context.ProductReleaseId);
                return cachedRelease.LastModifiedTimestamp < lastModifiedTimestamp;
            }
        }
    }
}
