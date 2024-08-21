// <copyright file="MemoryCachingHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Helpers
{
    using System.Runtime.Caching;

    /// <summary>
    /// helper to modify properties in the memory cache.
    /// </summary>
    public static class MemoryCachingHelper
    {
        /// <summary>
        /// if the cache doesn't exist, retrieve the value lazily. and put to cache.
        /// </summary>
        /// <typeparam name="T">type.</typeparam>
        /// <param name="key">identifier of the value .</param>
        /// <param name="function">value to store.</param>
        /// <param name="expiration">the expiration.</param>
        /// <returns>returns the object.</returns>
        public static T? AddOrGet<T>(string key, Func<T?> function, DateTimeOffset expiration)
            where T : class
        {
            var memoryCache = MemoryCache.Default;
            var cacheValue = memoryCache.Get(key, null);
            if (cacheValue != null)
            {
                return (T)cacheValue;
            }

            var value = function();
            if (value != null)
            {
                memoryCache.Add(key, value, expiration);
            }

            return value;
        }

        /// <summary>
        /// Updates the cache if it exists, add new record if it doesn't.
        /// </summary>
        /// <param name="key">identifier of the value .</param>
        public static void Upsert(string key, object value, DateTimeOffset expiration)
        {
            var memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Set(key, value, expiration);
            }
            else
            {
                memoryCache.Add(key, value, expiration);
            }
        }

        /// <summary>
        /// if the cache doesn't exist, retrieve the value lazily, but this is for async. then put to cache.
        /// </summary>
        /// <typeparam name="T">type.</typeparam>
        /// <param name="key">identifier of the value .</param>
        /// <param name="function">function that returns the value to store.</param>
        /// <param name="expiration">the expiration.</param>
        /// <returns>returns the object, or null if not found and the function returned null.</returns>
        public static async Task<T?> AddOrGetAsync<T>(string key, Func<Task<T?>> function, DateTimeOffset expiration)
        {
            var memoryCache = MemoryCache.Default;
            var cacheValue = memoryCache.Get(key, null);
            if (cacheValue != null)
            {
                return (T)cacheValue;
            }
            var value = await function();

            if (value != null)
            {
                memoryCache.Add(key, value, expiration);
            }

            return value;
        }

        /// <summary>
        /// removes the cache.
        /// </summary>
        /// <param name="cacheKey">the cache key.</param>
        public static void Remove(string cacheKey)
        {
            if (!string.IsNullOrEmpty(cacheKey))
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }

        public static T? Get<T>(string key)
        {
            var memoryCache = MemoryCache.Default;
            var cacheValue = memoryCache.Get(key, null);
            return (T)cacheValue;
        }
    }
}
