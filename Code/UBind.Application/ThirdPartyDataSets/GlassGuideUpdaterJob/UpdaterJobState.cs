// <copyright file="UpdaterJobState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.ComponentModel;

/// <summary>
/// Provides applicable status values for the Glass's Guide updater job.
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
    [Description("Creating tables and schema")]
    CreatingTablesAndSchema,

    /// <summary>
    /// The importing fixed width values state.
    /// </summary>
    [Description("Importing data segments to Glass's Guide database")]
    ImportingFixedWidthValuesToGlassGuideDatabase,

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
