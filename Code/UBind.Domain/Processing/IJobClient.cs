// <copyright file="IJobClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Processing
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Hangfire;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for queuing background jobs.
    /// </summary>
    public interface IJobClient
    {
        /// <summary>
        /// Queues a background job and adds job parameters.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="parameters">Parameters to add to the queued job.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Action<T>> methodCall,
            params JobParameter[] parameters);

        /// <summary>
        /// Queues a background job implemented in an aysnc method, and adds job parameters.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="parameters">Parameters to add to the queued job.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            params JobParameter[] parameters);

        /// <summary>
        /// Queues a background job and adds job parameters based on product context.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="productContext">The context identifying tenant, product and environement the job belongs to.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Action<T>> methodCall,
            ProductContext productContext);

        /// <summary>
        /// Queues a background job.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="tenantAlias">The Alias of the tenant the job is for.</param>
        /// <param name="productAlias">The Alias of the product the job is for.</param>
        /// <param name="environment">The environment the job is for.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Action<T>> methodCall,
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="tenantId">The Id of the tenant the job is for.</param>
        /// <param name="productId">The Id of the product the job is for.</param>
        /// <param name="environment">The environment the job is for.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Action<T>> methodCall,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queus a background job implemented as an asyncronous method.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="tenantAlias">The Alias of the tenant the job is for.</param>
        /// <param name="productAlias">The Alias of the product the job is for.</param>
        /// <param name="environment">The environment the job is for.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from the
        /// time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queus a background job implemented as an asyncronous method.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="tenantId">The Id of the tenant the job is for.</param>
        /// <param name="productId">The Id of the product the job is for.</param>
        /// <param name="environment">The environment the job is for.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job implemented as an asyncronous method and sets parameters based on product context.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="productContext">The context identifying tenant, product and environement the job belongs to.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            ProductContext productContext,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job to be triggered by the completion of an existing job.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="jobId">Identifier of a background job to wait completion for.</param>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string ContinueJobWith<T>(
            string jobId,
            Expression<Action<T>> methodCall,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job to be triggered by the completion of an existing job.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="jobId">The ID of the job to wait for before.</param>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from the
        /// time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string ContinueJobWith<T>(
            string jobId,
            Expression<Func<T, Task>> methodCall,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job to be triggered by the completion of an existing job.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="jobId">The ID of the job to wait for before.</param>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="option">option if will continue even if parent failed or success only.</param>
        /// <param name="expireAfter">Optional time-span property indicating when to expire job from
        /// the time of enqueuing. Default is TimeSpan.Zero, indicating no expiration.</param>
        /// <returns>The ID of the queued job.</returns>
        string ContinueJobWith<T>(
            string jobId,
            Expression<Action<T>> methodCall,
            JobContinuationOptions option,
            TimeSpan expireAfter = default);

        /// <summary>
        /// Queues a background job to be triggered by schedule.
        /// </summary>
        /// <typeparam name="T">Type whose method will be invoked during job processing.</typeparam>
        /// <param name="methodCall">Instance method call expression that will be invoked during job processing.</param>
        /// <param name="timeSpan">Schedule in time span.</param>
        void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan);

        /// <summary>
        /// Sets a parameter on a given job.
        /// </summary>
        /// <param name="jobId">The ID of the job to annotate.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        void SetJobParameter(string jobId, string name, string value);

        /// <summary>
        /// Annotate a given job with a parameter.
        /// </summary>
        /// <param name="jobId">The ID of the job to annotate.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>The parameter value.</returns>
        string GetJobParameter(string jobId, string name);
    }
}
