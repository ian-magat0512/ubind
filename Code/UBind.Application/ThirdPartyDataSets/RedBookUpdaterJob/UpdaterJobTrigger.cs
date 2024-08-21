// <copyright file="UpdaterJobTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob
{
    /// <summary>
    /// Provides the list of triggers for the RedBook updater job.
    /// </summary>
    public enum UpdaterJobTrigger
    {
        /// <summary>
        /// The job queued trigger.
        /// </summary>
        RedBookUpdaterJobQueued,

        /// <summary>
        /// The job cancelled trigger.
        /// </summary>
        RedBookUpdaterJobCancelled,

        /// <summary>
        /// The download completed trigger.
        /// </summary>
        RedBookUpdaterDownloadCompleted,

        /// <summary>
        /// The download aborted trigger.
        /// </summary>
        RedBookUpdaterDownloadAborted,

        /// <summary>
        /// The extract archive completed trigger.
        /// </summary>
        RedBookUpdaterExtractArchiveCompleted,

        /// <summary>
        /// The create table and staging schema completed trigger.
        /// </summary>
        RedBookUpdaterCreateTablesAndSchemaInStagingCompleted,

        /// <summary>
        /// The import delimiter separated values to RedBook database completed trigger.
        /// </summary>
        RedBookUpdaterImportDelimiterSeparatedValuesToRedBookDatabaseCompleted,

        /// <summary>
        /// The archive downloaded files completed trigger.
        /// </summary>
        RedBookArchiveDownloadedFilesCompleted,

        /// <summary>
        /// The clean up updater job completed trigger.
        /// </summary>
        RedBookCleanUpUpdaterJobCompleted,

        /// <summary>
        /// The aborted updater job trigger.
        /// </summary>
        RedBookUpdaterAborted,
    }
}
