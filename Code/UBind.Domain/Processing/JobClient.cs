// <copyright file="JobClient.cs" company="uBind">
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
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for queuing background jobs.
    /// </summary>
    /// <remarks>
    /// Just a wrapper for Hangfire's <see cref="IBackgroundJobClient"/> that let's us set job parameters.
    /// </remarks>
    public class JobClient : IJobClient
    {
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobClient"/> class.
        /// </summary>
        /// <param name="backgroundJobClient">A hangfire job client for queuing jobs.</param>
        public JobClient(
            IBackgroundJobClient backgroundJobClient,
            ICachingResolver cachingResolver)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public void SetJobParameter(string jobId, string name, string value) =>
            JobStorage.Current.GetConnection().SetJobParameter(jobId, name, value);

        /// <inheritdoc/>
        public string GetJobParameter(string jobId, string name) =>
            JobStorage.Current.GetConnection().GetJobParameter(jobId, name);

        /// <inheritdoc/>
        public string ContinueJobWith<T>(string jobId, Expression<Action<T>> methodCall, TimeSpan expireAfter = default)
        {
            var continuationJobId = this.backgroundJobClient.ContinueJobWith<T>(jobId, methodCall);
            this.SetJobExpiry(continuationJobId, expireAfter);
            return continuationJobId;
        }

        /// <inheritdoc/>
        public string ContinueJobWith<T>(string jobId, Expression<Func<T, Task>> methodCall, TimeSpan expireAfter = default)
        {
            var continuationJobId = this.backgroundJobClient.ContinueJobWith<T>(jobId, methodCall);
            this.SetJobExpiry(continuationJobId, expireAfter);
            return continuationJobId;
        }

        /// <inheritdoc/>
        public string ContinueJobWith<T>(string jobId, Expression<Action<T>> methodCall, JobContinuationOptions option, TimeSpan expireAfter = default)
        {
            var continuationJobId = this.backgroundJobClient.ContinueJobWith<T>(jobId, methodCall, option);
            this.SetJobExpiry(continuationJobId, expireAfter);
            return continuationJobId;
        }

        /// <inheritdoc/>
        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan) =>
            this.backgroundJobClient.Schedule<T>(methodCall, timeSpan);

        /// <inheritdoc/>
        public string Enqueue<T>(
            Expression<Action<T>> methodCall,
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default)
        {
            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            JobStorage.Current.GetConnection().SetJobParameters(
                jobId, tenantAlias, productAlias, environment?.ToString());
            this.SetJobExpiry(jobId, expireAfter);
            return jobId;
        }

        /// <inheritdoc/>
        public string Enqueue<T>(
           Expression<Action<T>> methodCall,
           params JobParameter[] parameters)
        {
            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            foreach (var parameter in parameters)
            {
                JobStorage.Current.GetConnection().SetJobParameter(
                    jobId, parameter.Name, parameter.Value);
            }

            return jobId;
        }

        /// <inheritdoc/>
        public string Enqueue<T>(
           Expression<Func<T, Task>> methodCall,
           params JobParameter[] parameters)
        {
            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            foreach (var parameter in parameters)
            {
                JobStorage.Current.GetConnection().SetJobParameter(
                    jobId, parameter.Name, parameter.Value);
            }

            return jobId;
        }

        /// <inheritdoc/>
        public string Enqueue<T>(Expression<Func<T, Task>> methodCall, ProductContext productContext, TimeSpan expireAfter = default) =>
            this.Enqueue(methodCall, productContext.TenantId, productContext.ProductId, productContext.Environment, expireAfter);

        /// <inheritdoc/>
        public string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default)
        {
            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            JobStorage.Current.GetConnection().SetJobParameters(
                jobId, tenantAlias, productAlias, environment?.ToString());
            this.SetJobExpiry(jobId, expireAfter);

            return jobId;
        }

        /// <inheritdoc/>
        public string Enqueue<T>(Expression<Action<T>> methodCall, ProductContext productContext)
        {
            var tenantAlias = this.GetTenantAlias(productContext.TenantId);
            var productAlias = this.GetProductAlias(productContext.TenantId, productContext.ProductId);

            return this.Enqueue(
                 methodCall,
                 JobParameter.Tenant(tenantAlias),
                 JobParameter.Product(productAlias),
                 JobParameter.Environment(productContext.Environment));
        }

        /// <inheritdoc/>
        public string Enqueue<T>(
            Expression<Func<T, Task>> methodCall,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
           TimeSpan expireAfter = default)
        {
            var tenantAlias = tenantId == null ?
                string.Empty : this.GetTenantAlias(tenantId.Value);
            var productAlias = productId == null ?
                string.Empty : this.GetProductAlias(tenantId.Value, productId.Value);

            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            JobStorage.Current.GetConnection().SetJobParameters(
                jobId, tenantAlias, productAlias, environment?.ToString());
            this.SetJobExpiry(jobId, expireAfter);

            return jobId;
        }

        /// <inheritdoc/>
        public string Enqueue<T>(
            Expression<Action<T>> methodCall,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
            TimeSpan expireAfter = default)
        {
            var tenantAlias = this.GetTenantAlias(tenantId);
            var productAlias = this.GetProductAlias(tenantId, productId);

            var jobId = this.backgroundJobClient.Enqueue<T>(methodCall);
            JobStorage.Current.GetConnection().SetJobParameters(
                jobId, tenantAlias, productAlias, environment?.ToString());
            this.SetJobExpiry(jobId, expireAfter);

            return jobId;
        }

        private string GetTenantAlias(Guid? tenantId)
            => tenantId == null ?
                string.Empty : this.cachingResolver.GetTenantAliasOrThrow(tenantId.Value);

        private string GetProductAlias(Guid? tenantId, Guid? productId)
           => tenantId == null || productId == null ?
                string.Empty : this.cachingResolver.GetProductAliasOrThrow(tenantId.Value, productId.Value);

        private void SetJobExpiry(string jobId, TimeSpan expireAfter = default)
        {
            if (expireAfter == default)
            {
                return;
            }

            using (var connection = JobStorage.Current.GetConnection())
            {
                connection.SetJobParameter(jobId, BackgroundJobParameter.ExpireAt, expireAfter.ToString());
                using (var transaction = connection.CreateWriteTransaction())
                {
                    transaction.ExpireJob(jobId, expireAfter);
                    transaction.Commit();
                }
            }
        }
    }
}
