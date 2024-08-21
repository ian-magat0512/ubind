// <copyright file="IThirdPartyDataSetsConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides the contract to be used by updater job state machine to access third party data sets configuration like csv/dsv working directory path.
    /// </summary>
    public interface IThirdPartyDataSetsConfiguration
    {
        /// <summary>
        /// Gets the updater jobs path.
        /// </summary>
        string UpdaterJobsPath { get; }

        /// <summary>
        /// Gets the file hashes path.
        /// </summary>
        string FileHashesPath { get; }

        /// <summary>
        /// Gets the download buffer size.
        /// </summary>
        int DownloadBufferSize { get; }

        /// <summary>
        /// Gets the downloaded folder.
        /// </summary>
        string DownloadedFolder { get; }

        /// <summary>
        /// Gets the extracted folder.
        /// </summary>
        string ExtractedFolder { get; }

        /// <summary>
        /// Gets the index base path.
        /// </summary>
        string IndexBasePath { get; }

        /// <summary>
        /// Gets the index temporary path.
        /// </summary>
        string IndexTemporaryPath { get; }

        /// <summary>
        /// Gets the list of available index configuration.
        /// </summary>
        Dictionary<string, ThirdPartyDataSetsIndexName> IndexNames { get; }
    }
}
