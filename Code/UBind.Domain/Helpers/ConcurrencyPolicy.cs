// <copyright file="ConcurrencyPolicy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Polly;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Static helper for handling concurrency issues.
    /// </summary>
    public static class ConcurrencyPolicy
    {
        private const int DefaultConcurrencyRetryLimit = 5;

        /// <summary>
        /// Execute a function, retrying if concurrency exceptions occur.
        /// </summary>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <returns>A task from which the result can be obtained.</returns>
        public static async Task<TResult> ExecuteWithRetriesAsync<TResult>(
            Func<Task<TResult>> function, int retryCount = DefaultConcurrencyRetryLimit)
        {
            return await Policy
                .Handle<ConcurrencyException>()
                .RetryAsync(retryCount)
                .ExecuteAsync(function);
        }

        /// <summary>
        /// Execute a function, retrying if concurrency exceptions occur.
        /// </summary>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <returns>A task from which the result can be obtained.</returns>
        public static async Task<TResult> ExecuteWithRetriesAsync<TResult>(
            Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken, int retryCount = DefaultConcurrencyRetryLimit)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Policy
                .Handle<ConcurrencyException>()
                .RetryAsync(retryCount)
                .ExecuteAsync(function, cancellationToken);
        }

        /// <summary>
        /// Execute an action, retrying if concurrency exceptions occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <returns>A task from which the result can be obtained.</returns>
        public static async Task ExecuteWithRetriesAsync(
            Func<Task> action, int retryCount = DefaultConcurrencyRetryLimit)
        {
            await Policy
                .Handle<ConcurrencyException>()
                .RetryAsync(retryCount)
                .ExecuteAsync(action);
        }

        /// <summary>
        /// Execute a function, retrying if concurrency exceptions occur.
        /// </summary>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="onRetry">Action to perform before each retry.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <returns>A task from which the result can be obtained.</returns>
        public static async Task<TResult> ExecuteWithRetriesAsync<TResult>(
            Func<Task<TResult>> function, Action onRetry, int retryCount = DefaultConcurrencyRetryLimit)
        {
            return await Policy
                .Handle<ConcurrencyException>()
                .RetryAsync(retryCount, (count, ex) => onRetry())
                .ExecuteAsync(function);
        }

        /// <summary>
        /// Execute an action, retrying if concurrency exceptions occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="onRetry">Action to perform before each retry.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <returns>A task from which the result can be obtained.</returns>
        public static async Task ExecuteWithRetriesAsync(
            Func<Task> action, Action onRetry, int retryCount = DefaultConcurrencyRetryLimit)
        {
            await Policy
                .Handle<ConcurrencyException>()
                .RetryAsync(retryCount, (count, ex) => onRetry())
                .ExecuteAsync(action);
        }
    }
}
