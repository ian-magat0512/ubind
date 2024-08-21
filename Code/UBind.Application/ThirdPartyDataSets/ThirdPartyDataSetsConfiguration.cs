// <copyright file="ThirdPartyDataSetsConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc/>
    public class ThirdPartyDataSetsConfiguration : IThirdPartyDataSetsConfiguration
    {
        /// <inheritdoc/>
        public int DownloadBufferSize { get; set; }

        /// <inheritdoc />
        public string DownloadedFolder { get; set; }

        /// <inheritdoc />
        public string ExtractedFolder { get; set; }

        /// <inheritdoc />
        public string UpdaterJobsPath { get; set; }

        /// <inheritdoc />
        public string FileHashesPath { get; set; }

        /// <inheritdoc/>
        public string IndexBasePath { get; set; }

        /// <inheritdoc/>
        public string IndexTemporaryPath { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, ThirdPartyDataSetsIndexName> IndexNames { get; set; }
    }
}
