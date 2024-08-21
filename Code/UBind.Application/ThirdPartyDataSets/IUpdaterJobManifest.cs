// <copyright file="IUpdaterJobManifest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using UBind.Application.DataDownloader;

    /// <summary>
    /// Provides the contract to be use by updater job state machine to access request configuration like protocol configs (http , ftp ) , Urls and credentials.
    /// </summary>
    public interface IUpdaterJobManifest
    {
        /// <summary>
        /// Gets the data downloader protocol.
        /// </summary>
        DataDownloaderProtocol DataDownloaderProtocol { get; }

        /// <summary>
        /// Gets the DownloadUrls.
        /// </summary>
        IReadOnlyList<(string Url, string FileHash, string Filename)> DownloadUrls { get; }

        /// <summary>
        /// Gets a value indicating whether force update is enabled.
        /// </summary>
        bool IsForceUpdate { get; }
    }
}
