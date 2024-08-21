// <copyright file="DataDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Maintenance
{
    using System.Diagnostics.Contracts;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.Server;
    using NodaTime;
    using UBind.Domain.Loggers;
    using UBind.Domain.Reduction;
    using UBind.Domain.Services;

    /// <summary>
    /// Service for uBind database record deletion.
    /// </summary>
    public class DataDeletionService : IDeletionService
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IProgressLoggerFactory progressLoggerFactory;
        private readonly IQuoteDeletionManager quoteDeletionManager;
        private readonly IMiniProfilerDeletionManager miniProfilerDeletionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDeletionService"/> class.
        /// </summary>
        /// <param name="jobClient">Client for queuing background jobs.</param>
        /// <param name="recurringJobManager">Job manager that handles recurring jobs.</param>
        /// <param name="progressLoggerFactory">The progress logger factory.</param>
        /// <param name="quoteDeletionManager">The instance of quote deletion manager.</param>
        /// <param name="miniProfilerDeletionManager">The instance of mini-profiler deletion manager.</param>
        public DataDeletionService(
            IBackgroundJobClient jobClient,
            IRecurringJobManager recurringJobManager,
            IProgressLoggerFactory progressLoggerFactory,
            IQuoteDeletionManager quoteDeletionManager,
            IMiniProfilerDeletionManager miniProfilerDeletionManager)
        {
            Contract.Assert(jobClient != null);
            Contract.Assert(recurringJobManager != null);
            Contract.Assert(progressLoggerFactory != null);
            Contract.Assert(quoteDeletionManager != null);
            Contract.Assert(miniProfilerDeletionManager != null);

            this.jobClient = jobClient;
            this.recurringJobManager = recurringJobManager;
            this.progressLoggerFactory = progressLoggerFactory;
            this.quoteDeletionManager = quoteDeletionManager;
            this.miniProfilerDeletionManager = miniProfilerDeletionManager;
        }

        /// <inheritdoc/>
        public void QueueBackgroundDeletionNascentQuotes(int batchSize, int deletionLimit)
        {
            this.jobClient.Enqueue<IDeletionService>(
                service => service.DeleteNascentQuotes(batchSize, deletionLimit, null));
        }

        /// <inheritdoc/>
        public void DeleteNascentQuotes(int batchSize, int deletionLimit, PerformContext performContext)
        {
            var progressLogger = this.progressLoggerFactory.Invoke(performContext);
            this.quoteDeletionManager.DeleteNascent(progressLogger, batchSize, deletionLimit, Duration.FromDays(30));
        }

        /// <inheritdoc/>
        public void QueueBackgroundTruncateMiniProfilerData()
        {
            this.jobClient.Enqueue<IDeletionService>(service => service.TruncateMiniProfilerData(null));
        }

        /// <inheritdoc/>
        public void TruncateMiniProfilerData(PerformContext performContext)
        {
            var progressLogger = this.progressLoggerFactory.Invoke(performContext);
            this.miniProfilerDeletionManager.Truncate(progressLogger);
        }

        /// <inheritdoc/>
        public void ScheduleNascentDeletion(
            string recurringJobId, int batchSize, int deletionLimit, string cronExpression)
        {
            this.recurringJobManager.AddOrUpdate(
                recurringJobId,
                Job.FromExpression<IDeletionService>(
                    service => service.DeleteNascentQuotes(batchSize, deletionLimit, null)),
                cronExpression);
        }

        /// <inheritdoc/>
        public void ScheduleTruncateMiniProfiler(string recurringJobId, string cronExpression)
        {
            this.recurringJobManager.AddOrUpdate(
                recurringJobId,
                Job.FromExpression<IDeletionService>(service => service.TruncateMiniProfilerData(null)),
                cronExpression);
        }

        /// <inheritdoc/>
        public void RemoveAnyRecurringJob(string recurringJobId)
        {
            this.recurringJobManager.RemoveIfExists(recurringJobId);
        }
    }
}
