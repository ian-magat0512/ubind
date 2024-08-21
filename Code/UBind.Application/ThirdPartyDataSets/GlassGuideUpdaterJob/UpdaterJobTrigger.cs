// <copyright file="UpdaterJobTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

/// <summary>
/// Provides the list of triggers for the Glass's Guide updater job.
/// </summary>
public enum UpdaterJobTrigger
{
    /// <summary>
    /// The job queued trigger.
    /// </summary>
    GlassGuideUpdaterJobQueued,

    /// <summary>
    /// The job cancelled trigger.
    /// </summary>
    GlassGuideUpdaterJobCancelled,

    /// <summary>
    /// The download completed trigger.
    /// </summary>
    GlassGuideUpdaterDownloadCompleted,

    /// <summary>
    /// The download aborted trigger.
    /// </summary>
    GlassGuideUpdaterDownloadAborted,

    /// <summary>
    /// The extract archive completed trigger.
    /// </summary>
    GlassGuideUpdaterExtractArchiveCompleted,

    /// <summary>
    /// The create table and staging schema completed trigger.
    /// </summary>
    GlassGuideUpdaterCreateTablesAndSchemaInStagingCompleted,

    /// <summary>
    /// The import fixed width values to Glass's Guide database completed trigger.
    /// </summary>
    GlassGuideUpdaterImportFixedWidthValuesToGlassGuideDatabaseCompleted,

    /// <summary>
    /// The archive downloaded files completed trigger.
    /// </summary>
    GlassGuideArchiveDownloadedFilesCompleted,

    /// <summary>
    /// The clean up updater job completed trigger.
    /// </summary>
    GlassGuideCleanUpUpdaterJobCompleted,

    /// <summary>
    /// The aborted updater job trigger.
    /// </summary>
    GlassGuideUpdaterAborted,
}