// <copyright file="UpdaterJobState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob
{
    using System.ComponentModel;

    /// <summary>
    /// Provides applicable status values for the RedBook updater job.
    /// </summary>
    public enum UpdaterJobState
    {
        /// <summary>
        /// The queued state.
        /// </summary>
        [Description("Queued")]
        Queued,

        /// <summary>
        /// The completed state.
        /// </summary>
        [Description("Completed")]
        Completed,

        /// <summary>
        /// The canceled state to be used when the user cancels the job.
        /// </summary>
        [Description("Cancelled")]
        Cancelled,

        /// <summary>
        /// The aborted state to be used by state machine when the job was programmatically aborted.
        /// </summary>
        [Description("Aborted")]
        Aborted,

        /// <summary>
        /// The downloading state.
        /// </summary>
        [Description("Downloading")]
        Downloading,

        /// <summary>
        /// The extracting state.
        /// </summary>
        [Description("Extracting")]
        Extracting,

        /// <summary>
        /// The creating tables and schema state.
        /// </summary>
        [Description("Creating Tables and Schema")]
        CreatingTablesAndSchema,

        /// <summary>
        /// The importing delimiter separated values state.
        /// </summary>
        [Description("Importing DSV to RedBook database")]
        ImportingDelimiterSeparatedValuesToRedBookDatabase,

        /// <summary>
        /// The archive downloaded files state.
        /// </summary>
        [Description("Archiving downloaded files")]
        ArchiveDownloadedFiles,

        /// <summary>
        /// The clean-up updater job state to be used to free resources allocated like downloaded files.
        /// </summary>
        [Description("Clean up updater job")]
        CleanUpUpdaterJob,
    }
}
