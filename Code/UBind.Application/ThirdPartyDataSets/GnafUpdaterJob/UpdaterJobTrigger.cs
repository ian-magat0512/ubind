// <copyright file="UpdaterJobTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.GnafUpdaterJob
{
    /// <summary>
    /// Provide enum list for Gnaf updater job state machine triggers.
    /// </summary>
    public enum UpdaterJobTrigger
    {
        /// <summary>
        /// The GNAF updater job queued trigger.
        /// </summary>
        GnafUpdaterJobQueued,

        /// <summary>
        /// The GNAF updater job cancelled trigger.
        /// </summary>
        GnafUpdaterJobCancelled,

        /// <summary>
        /// The GNAF updater job download completed trigger.
        /// </summary>
        GnafUpdaterDownloadCompleted,

        /// <summary>
        /// The GNAF updater job extract archive completed trigger.
        /// </summary>
        GnafUpdaterExtractArchiveCompleted,

        /// <summary>
        /// The GNAF updater job create table and staging schema completed trigger.
        /// </summary>
        GnafUpdaterCreateTablesAndSchemaInStagingCompleted,

        /// <summary>
        /// The GNAF updater job import delimiter separated values to GNAF database completed trigger.
        /// </summary>
        GnafUpdaterImportDelimiterSeparatedValuesToGnafDatabaseCompleted,

        /// <summary>
        /// The GNAF updater job building the index completed trigger.
        /// </summary>
        GnafBuildingForeignKeysAndIndexesCompleted,

        /// <summary>
        /// The GNAF updater job building the search index completed trigger.
        /// </summary>
        GnafBuildingSearchIndexCompleted,

        /// <summary>
        /// The GNAF updater job aborted updater job trigger.
        /// </summary>
        GnafUpdaterAborted,

        /// <summary>
        /// The GNAF updater job download aborted trigger.
        /// </summary>
        GnafUpdaterDownloadAborted,
    }
}
