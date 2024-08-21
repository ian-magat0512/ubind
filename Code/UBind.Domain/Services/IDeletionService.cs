// <copyright file="IDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using Hangfire.Server;

    /// <summary>
    /// A service interface for deleting data.
    /// </summary>
    public interface IDeletionService
    {
        /// <summary>
        /// Begin and delete all test records by batch running in background thread.
        /// Dev note: Current implementation will only delete all monitoring tests created from PRTG.
        /// </summary>
        /// <param name="batchSize">The number of records to delete per batch. Default value is 100.</param>
        /// <param name="deletionLimit">
        ///     The maximum number of entries to delete. Setting the value to -1 will include all nascent records,
        ///     which is the default as well.
        /// </param>
        void QueueBackgroundDeletionNascentQuotes(int batchSize = 100, int deletionLimit = -1);

        /// <summary>
        /// Remove all test and per batch size.
        /// </summary>
        /// <param name="batchSize">
        ///     The size per batch to delete. Setting the value into -1 will remove all records at once.
        ///     Default value is 100.
        /// </param>
        /// <param name="deletionLimit">
        ///     The maximum number of entries to delete. Setting the value to -1 will include all nascent records,
        ///     which is the default as well.
        /// </param>
        /// <param name="performContext">The hangfire parameter context.</param>
        void DeleteNascentQuotes(int batchSize = 100, int deletionLimit = -1, PerformContext performContext = null);

        /// <summary>
        /// Delete all mini profiler data running in background thread.
        /// </summary>
        void QueueBackgroundTruncateMiniProfilerData();

        /// <summary>
        /// Delete all mini profiler data.
        /// </summary>
        /// <param name="performContext">The hangfire parameter context.</param>
        void TruncateMiniProfilerData(PerformContext performContext = null);

        /// <summary>
        /// Begin to schedule the recurrence of nascent deletion in background thread.
        /// </summary>
        /// <param name="recurringJobId">The ID of the recurring job to execute.</param>
        /// <param name="batchSize">The number of records to delete per batch (capped to 150, default 100).</param>
        /// <param name="deletionLimit">The deletion limit per recurrence.</param>
        /// <param name="cronExpression">The cron expression that specifies interval of recurrence.</param>
        void ScheduleNascentDeletion(string recurringJobId, int batchSize, int deletionLimit, string cronExpression);

        /// <summary>
        /// Begin to schedule the recurrence of mini-profiler truncation in background thread.
        /// </summary>
        /// <param name="recurringJobId">The ID of the recurring job to execute.</param>
        /// <param name="cronExpression">The cron expression that specifies interval of recurrence.</param>
        void ScheduleTruncateMiniProfiler(string recurringJobId, string cronExpression);

        /// <summary>
        /// Stop and remove any recurring deletion in background thread.
        /// </summary>
        /// <param name="recurringJobId">The ID of the recurring job to delete.</param>
        void RemoveAnyRecurringJob(string recurringJobId);
    }
}
