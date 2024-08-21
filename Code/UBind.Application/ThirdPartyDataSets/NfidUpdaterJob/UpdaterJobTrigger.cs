// <copyright file="UpdaterJobTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.NfidUpdaterJob
{
    /// <summary>
    /// Provides the list of triggers for the NFID updater job.
    /// </summary>
    public enum UpdaterJobTrigger
    {
        /// <summary>
        /// The NFID updater job queued trigger.
        /// </summary>
        NfidUpdaterJobQueued,

        /// <summary>
        /// The NFID updater job cancelled trigger.
        /// </summary>
        NfidUpdaterJobCancelled,

        /// <summary>
        /// The NFID updater job download completed trigger.
        /// </summary>
        NfidUpdaterDownloadCompleted,

        /// <summary>
        /// The NFID updater job extract archive completed trigger.
        /// </summary>
        NfidUpdaterExtractArchiveCompleted,

        /// <summary>
        /// The NFID updater job create table and staging schema completed trigger.
        /// </summary>
        NfidUpdaterCreateTablesAndSchemaInStagingCompleted,

        /// <summary>
        /// The NFID updater job import delimiter separated values to GNAF database completed trigger.
        /// </summary>
        NfidUpdaterImportDelimiterSeparatedValuesToNfidDatabaseCompleted,

        /// <summary>
        /// The NFID updater job archive downloaded files completed trigger..
        /// </summary>
        NfidArchiveDownloadedFilesCompleted,

        /// <summary>
        /// The NFID updater job building search index completed trigger..
        /// </summary>
        NfidBuildingSearchIndexCompleted,

        /// <summary>
        /// The NFID updater job clean up updater job completed trigger.
        /// </summary>
        NfidCleanUpUpdaterJobCompleted,

        /// <summary>
        /// The NFID updater job aborted updater job trigger.
        /// </summary>
        NfidUpdaterAborted,

        /// <summary>
        /// The NFID updater job download aborted trigger.
        /// </summary>
        NfidUpdaterDownloadAborted,
    }
}
