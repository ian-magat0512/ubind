// <copyright file="DBSetExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Caching;
    using LinqKit;
    using MoreLinq;
    using StackExchange.Profiling;
    using UBind.Domain;

    /// <summary>
    /// DB Set Extension usually used for an entry level 2nd level cache.
    /// </summary>
    public static class DbSetExtensions
    {
        private static MemoryCache memoryCache = MemoryCache.Default;

        /// <summary>
        /// retrieve record from cache first, if doesnt have it, retrieve on database.
        /// </summary>
        /// <typeparam name="T">type on the dbset.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="predicate">the predicate.</param>
        /// <param name="time">the time.</param>
        /// <returns>returned object.</returns>
        public static T GetFromCache<T>(this DbSet<T> dbset, Expression<Func<T, bool>> predicate, DateTimeOffset? time = null)
            where T : class, new()
        {
            List<T> tmpList = GetList<T>();
            var tmpRecord = tmpList.AsQueryable().FirstOrDefault(predicate);
            if (tmpRecord != null)
            {
                return tmpRecord;
            }

            var record = dbset.FirstOrDefault(predicate);
            if (record != null)
            {
                tmpList.Add(record);
                AddOrUpdateCache(tmpList, time);
            }

            return record;
        }

        /// <summary>
        /// does a where clause on the cache, if doesnt exist query on the database.
        /// </summary>
        /// <typeparam name="T">The dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="predicate">the predicate.</param>
        /// <param name="time">the time.</param>
        /// <returns>returned ienumerable of T.</returns>
        public static IEnumerable<T> GetAllFromCache<T>(this DbSet<T> dbset, Expression<Func<T, bool>> predicate, DateTimeOffset? time = null)
            where T : class, new()
        {
            List<T> tmpList = GetList<T>();
            var tmpRecords = tmpList.AsQueryable().Where(predicate);

            // if records are the same. get the data from the cache. if there is discrepancy, get data from the db.
            if (dbset.Count(predicate) == tmpRecords.Count())
            {
                return tmpRecords;
            }

            var records = dbset.Where(predicate);
            tmpList.AddRange(records);
            AddOrUpdateCache(tmpList, time);
            return records;
        }

        /// <summary>
        /// updates the object from cache.
        /// </summary>
        /// <typeparam name="T">the dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="predicate">the predicate.</param>
        /// <param name="propertyPredicate">the property predicate to select a property.</param>
        /// <param name="updatedVal">the udated value.</param>
        public static void UpdateCacheOnly<T>(
            this DbSet<T> dbset,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> propertyPredicate,
            object updatedVal)
            where T : class, new()
        {
            List<T> tmpList = GetList<T>();
            var propertyName = propertyPredicate.Body.ToString().Replace("Convert(", string.Empty).Replace(")", string.Empty).Split('.')[1];

            tmpList.AsQueryable().Where(predicate).ToList().ForEach(x =>
            {
                foreach (var prop in x.GetType().GetProperties().ToList())
                {
                    if (prop.Name == propertyName)
                    {
                        prop.SetValue(x, updatedVal);
                    }
                }
            });

            AddOrUpdateCache(tmpList);
        }

        /// <summary>
        /// add from cache as well as do the operation in the dbset.
        /// </summary>
        /// <typeparam name="T">The dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="obj">the value to add to cache.</param>
        public static void AddToCache<T>(this DbSet<T> dbset, T obj)
        where T : class, new()
        {
            if (obj != null)
            {
                List<T> tmpList = GetList<T>();
                var record = dbset.Add(obj);
                tmpList.Add(record);
                AddOrUpdateCache(tmpList);
            }
        }

        /// <summary>
        /// Removes from cache as well as do the operation in the dbset.
        /// </summary>
        /// <typeparam name="T">the dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="obj">the objct to remove from the db and cache.</param>
        public static void RemoveFromCache<T>(this DbSet<T> dbset, T obj)
            where T : class, new()
        {
            List<T> tmpList = GetList<T>();
            T record;
            using (MiniProfiler.Current.Step($"{nameof(DbSetExtensions)}.{nameof(RemoveFromCache)} dbSet.Remove"))
            {
                record = dbset.Remove(obj);
            }

            using (MiniProfiler.Current.Step($"{nameof(DbSetExtensions)}.{nameof(RemoveFromCache)} tmpList.Remove"))
            {
                tmpList.Remove(record);
            }

            using (MiniProfiler.Current.Step($"{nameof(DbSetExtensions)}.{nameof(RemoveFromCache)} AddOrUpdateCache(tmpList)"))
            {
                AddOrUpdateCache(tmpList);
            }
        }

        /// <summary>
        /// Returns the list of sessions currently in the memory cache.
        /// </summary>
        /// <typeparam name="T">dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        public static List<T> GetCachedSessions<T>(this DbSet<T> dbset)
            where T : class, new()
        {
            return GetList<T>();
        }

        /// <summary>
        /// Removes a query from cache.
        /// </summary>
        /// <typeparam name="T">dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="predicate">the predicate.</param>
        public static void RemoveFromCacheOnly<T>(this DbSet<T> dbset, Expression<Func<T, bool>> predicate)
            where T : class, new()
        {
            List<T> tmpList = GetList<T>();
            var removeList = tmpList.AsQueryable().Where(predicate).ToList();
            foreach (var item in removeList)
            {
                tmpList.Remove(item);
            }

            AddOrUpdateCache(tmpList);
        }

        /// <summary>
        /// Overwrites the memory cache with the list of objects.
        /// </summary>
        /// <typeparam name="T">dbset type.</typeparam>
        /// <param name="dbset">the dbset.</param>
        /// <param name="list">the list of objects to add to the memory cache.</param>
        public static void AddOrUpdateCache<T>(this DbSet<T> dbset, List<T> list)
            where T : class, new()
        {
            AddOrUpdateCache(list);
        }

        public static IQueryable<TEntity> WhereNotDeleted<TEntity>(
        this DbSet<TEntity> dbSet)
        where TEntity : class, IDeletable
        {
            return dbSet.Where(entity => !entity.IsDeleted);
        }

        public static IQueryable<TEntity> WhereNotDeleted<TEntity>(
            this DbSet<TEntity> dbSet,
            Expression<Func<TEntity, bool>> predicate)
            where TEntity : class, IDeletable
        {
            var notDeletedPredicate = PredicateBuilder.And(
                predicate,
                entity => !entity.IsDeleted);

            return dbSet.Where(notDeletedPredicate);
        }

        private static List<T> GetList<T>()
        {
            var typeName = typeof(T).Name;
            List<T> tmpList = (List<T>)memoryCache.Get(typeName, null) ?? new List<T>();
            return tmpList;
        }

        private static void AddOrUpdateCache<T>(List<T> obj, DateTimeOffset? time = null)
        {
            var typeName = typeof(T).Name;
            memoryCache.Set(typeName, obj, new CacheItemPolicy
            {
                AbsoluteExpiration = time ?? DateTimeOffset.Now.AddMinutes(5),
            });
        }
    }
}
