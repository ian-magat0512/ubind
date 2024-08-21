// <copyright file="UpdaterJobState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.GnafUpdaterJob
{
    using System.ComponentModel;

    /// <summary>
    /// Provide enum list for Gnaf updater job state.
    /// </summary>
    public enum UpdaterJobState
    {
        /// <summary>
        /// The GNAF updater job is in queued state.
        /// </summary>
        [Description("Queued")]
        Queued,

        /// <summary>
        /// The GNAF updater job is in downloading state.
        /// </summary>
        [Description("Downloading")]
        Downloading,

        /// <summary>
        /// The GNAF updater job is in extracting state.
        /// </summary>
        [Description("Extracting")]
        Extracting,

        /// <summary>
        /// The GNAF updater job is in creating tables and schema state.
        /// </summary>
        [Description("Creating Tables and Schema")]
        CreatingTablesAndSchema,

        /// <summary>
        /// The GNAF updater job is in importing delimiter separated values state.
        /// </summary>
        [Description("Importing DSV to Gnaf database")]
        ImportingDelimiterSeparatedValuesToGnafDatabase,

        /// <summary>
        /// The GNAF updater job is in building of foreign keys and indexes state.
        /// </summary>
        [Description("Building foreign keys and indexes")]
        BuildingForeignKeysAndIndexes,

        /// <summary>
        /// The GNAF updater job is in building of address search index state.
        /// </summary>
        [Description("Building address search indexes")]
        BuildingAddressSearchIndex,

        /// <summary>
        /// The GNAF updater job is in completed state.
        /// </summary>
        [Description("Completed")]
        Completed,

        /// <summary>
        /// The GNAF updater job is in cancelled state.
        /// </summary>
        [Description("Cancelled")]
        Cancelled,

        /// <summary>
        /// The GNAF updater job is in aborted state.
        /// </summary>
        [Description("Aborted")]
        Aborted,
    }
}
