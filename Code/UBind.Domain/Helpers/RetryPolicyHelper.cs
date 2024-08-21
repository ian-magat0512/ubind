// <copyright file="RetryPolicyHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Polly;

    /// <summary>
    /// Static helper for handling retry attempts.
    /// </summary>
    public static class RetryPolicyHelper
    {
        private const int DefaultRetryLimit = 5;

        /// <summary>
        /// Execute a synchronous action, retrying if specified exception occur.
        /// </summary>
        /// <typeparam name="TException">The exception type to be handled.</typeparam>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <param name="minJitter">Apply minimum time to start again when retry happens.</param>
        /// <param name="maxJitter">Apply maximum time to start again when retry happens.</param>
        public static void Execute<TException>(
            Action action,
            int retryCount = DefaultRetryLimit,
            int minJitter = 200,
            int maxJitter = 1500)
            where TException : Exception
        {
            Random jitterer = new Random();
            Policy
                .Handle<TException>()
                .WaitAndRetry(
                retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(minJitter, maxJitter)))
                .Execute(action);
        }

        /// <summary>
        /// Execute an asynchronous action, retrying if specified exception occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <param name="minJitter">Apply minimum time to start again when retry happens.</param>
        /// <param name="maxJitter">Apply maximum time to start again when retry happens.</param>
        public static async Task ExecuteAsync<TException>(
            Func<Task> action,
            int retryCount = DefaultRetryLimit,
            int minJitter = 200,
            int maxJitter = 1500)
            where TException : Exception
        {
            Random jitterer = new Random();
            await Policy
                  .Handle<TException>()
                  .WaitAndRetryAsync(
                  retryCount,
                  retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(minJitter, maxJitter)))
                  .ExecuteAsync(action);
        }

        /// <summary>
        /// Execute an asynchronous action with cancellation, retrying if specified exception occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <param name="minJitter">Apply minimum time to start again when retry happens.</param>
        /// <param name="maxJitter">Apply maximum time to start again when retry happens.</param>
        public static async Task ExecuteAsync<TException>(
            Func<CancellationToken, Task> action,
            int retryCount = DefaultRetryLimit,
            int minJitter = 200,
            int maxJitter = 1500,
            CancellationToken cancellationToken = default)
            where TException : Exception
        {
            Random jitterer = new Random();
            await Policy
                  .Handle<TException>()
                  .WaitAndRetryAsync(
                  retryCount,
                  retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(minJitter, maxJitter)))
                  .ExecuteAsync(action, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Execute a synchronous function, retrying if specified exception occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <param name="minJitter">Apply minimum time to start again when retry happens.</param>
        /// <param name="maxJitter">Apply maximum time to start again when retry happens.</param>
        public static TOutout Execute<TException, TOutout>(
            Func<TOutout> action,
            int retryCount = DefaultRetryLimit,
            int minJitter = 200,
            int maxJitter = 1500)
            where TException : Exception
        {
            Random jitterer = new Random();
            return
               Policy
                   .Handle<TException>()
                   .WaitAndRetry(
                   retryCount,
                   retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(minJitter, maxJitter)))
                   .Execute(action);
        }

        /// <summary>
        /// Execute an asynchronous function, retrying if specified exception occur.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryCount">The max number of times to retry.</param>
        /// <param name="minJitter">Apply minimum time to start again when retry happens.</param>
        /// <param name="maxJitter">Apply maximum time to start again when retry happens.</param>
        public static async Task<TOutout> ExecuteAsync<TException, TOutout>(
            Func<Task<TOutout>> action,
            int retryCount = DefaultRetryLimit,
            int minJitter = 200,
            int maxJitter = 1500)
            where TException : Exception
        {
            Random jitterer = new Random();
            return await Policy
                   .Handle<TException>()
                   .WaitAndRetryAsync(
                   retryCount,
                   retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(minJitter, maxJitter)))
                   .ExecuteAsync(action);
        }

        /// <summary>
        /// Execute a synchronous function, retrying if the specified exception occur.
        /// </summary>
        /// <typeparam name="TException">The exception type to be handled.</typeparam>
        /// <param name="action">The function to execute.</param>
        /// <param name="retryAttemptDelays">Number of retries and how long each retry would delay to execute.</param>
        public static void Execute<TException>(
            Action action,
            TimeSpan[] retryAttemptDelays)
            where TException : Exception
        {
            Policy
                .Handle<TException>()
                .WaitAndRetry(retryAttemptDelays)
                .Execute(action);
        }
    }
}
