// <copyright file="EnumerableExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;

    /// <summary>
    /// Defines the <see cref="EnumerableExtensions" />.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// A replacement of the System.Link.Enumerable LINQ project method "Select" which returns a CSharpFunctionalExtension.Result and works objects functions which project to Result types.
        /// During the projection, if any of the functions return a failure, it will stop short the projection and return a failure.
        /// </summary>
        /// <typeparam name="TSource">The source type of the IEnumerable object.</typeparam>
        /// <typeparam name="TResult">The resulting type which the projection function returns, if it were unwrapped from the Result instance (e.g. result.Value type).</typeparam>
        /// <typeparam name="TError">The error type associated with the Results.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="selector">The projection function which projects the source enumerable.</param>
        /// <returns>A result instance containing an IEnumerable of the unwrapped return values, or the first error.</returns>
        public static Result<IEnumerable<TResult>, TError> ResultSelect<TSource, TResult, TError>(
            this IEnumerable<TSource> source,
            Func<TSource, Result<TResult, TError>> selector)
        {
            List<TResult> output = new List<TResult>();
            foreach (TSource item in source)
            {
                Result<TResult, TError> result = selector(item);
                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<TResult>, TError>(result.Error);
                }

                output.Add(result.Value);
            }

            return Result.Success<IEnumerable<TResult>, TError>(output);
        }

        /// <summary>
        /// Filters a list using a function which instead of just returning a boolean, returns a Result object which could be a boolean or an Error.
        /// If an error is discovered during the filtering of the list, processing immediately stops and an Result is returned with the Error.
        /// </summary>
        /// <typeparam name="TSource">The type of object being filtered.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="source">the source list.</param>
        /// <param name="include">the filtering function which returns a bool or Error wrapped in a Result.</param>
        /// <returns>The filtered list.</returns>
        public static Result<IEnumerable<TSource>, TError> ResultWhere<TSource, TError>(
            this IEnumerable<TSource> source,
            Func<TSource, Result<bool, TError>> include)
        {
            List<TSource> output = new List<TSource>();
            foreach (TSource item in source)
            {
                var includeResult = include(item);
                if (includeResult.IsFailure)
                {
                    return Result.Failure<IEnumerable<TSource>, TError>(includeResult.Error);
                }

                if (includeResult.Value)
                {
                    output.Add(item);
                }
            }

            return Result.Success<IEnumerable<TSource>, TError>(output);
        }

        /// <summary>
        /// For readabilty, provides an alternative to negating the Any() method on an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements in the Enumerable.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>true if there are no items in the enumerable.</returns>
        public static bool None<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        /// <summary>
        /// A simple version of the Where Linq function which is asynchronous, because it allows
        /// an asynchronouse predicate. Note that this is one possible way of support it and it may not
        /// be the most efficient.
        /// </summary>
        /// <typeparam name="T">The type of item in the enumerable.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="predicate">The predicate which after awaiting, returns bool.</param>
        /// <returns>An enumerable with the items which matched the predicate.</returns>
        public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            var results = await Task.WhenAll(source.Select(async x => (x, await predicate(x))));
            return results.Where(x => x.Item2).Select(x => x.Item1);
        }

        /// <summary>
        /// An async version of LINQ select which waits for all results.
        /// NOTE: don't use this when accessing a database with a large dataset as it would
        /// fire off up to N concurrent db request at a time, where N is the size of the list
        /// and this could exceed the limits of the database concurrency support and also
        /// cause deadlocks.
        /// </summary>
        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
        {
            return await Task.WhenAll(source.Select(async s => await method(s)));
        }

        /// <summary>
        /// Determines whether a list has duplicates, and if it does, it returns the first duplicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">The list.</param>
        /// <param name="firstDuplicate">The first duplicate.</param>
        /// <returns>True if there is a duplicate, or false if not.</returns>
        public static bool HasDuplicate<T>(this IEnumerable<T> source, out T firstDuplicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var checkBuffer = new HashSet<T>();
            foreach (var t in source)
            {
                if (checkBuffer.Add(t))
                {
                    continue;
                }

                firstDuplicate = t;
                return true;
            }

            firstDuplicate = default(T);
            return false;
        }
    }
}
