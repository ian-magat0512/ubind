// <copyright file="UpdaterJobManifest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using UBind.Application.DataDownloader;

    /// <inheritdoc/>
    public class UpdaterJobManifest : IUpdaterJobManifest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobManifest"/> class.
        /// </summary>
        /// <param name="dataDownloaderProtocol">The data downloader protocol.</param>
        /// <param name="downloadUrls">The download URL.</param>
        /// <param name="isForceUpdate">The flag if the force update is enabled.</param>
        public UpdaterJobManifest(
            DataDownloaderProtocol dataDownloaderProtocol,
            IReadOnlyList<(string Url, string FileHash, string Filename)> downloadUrls,
            bool isForceUpdate)
        {
            this.DataDownloaderProtocol = dataDownloaderProtocol;
            this.DownloadUrls = downloadUrls;
            this.IsForceUpdate = isForceUpdate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobManifest"/> class.
        /// UpdaterJobManifest.
        /// </summary>
        /// <param name="dataDownloaderProtocol">The data downloader protocol.</param>
        /// <param name="isForceUpdate">The flag if the force update is enabled.</param>
        public UpdaterJobManifest(DataDownloaderProtocol dataDownloaderProtocol, bool isForceUpdate)
        {
            this.DataDownloaderProtocol = dataDownloaderProtocol;
            this.IsForceUpdate = isForceUpdate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobManifest"/> class.
        /// </summary>
        public UpdaterJobManifest()
        {
        }

        /// <inheritdoc/>
        public DataDownloaderProtocol DataDownloaderProtocol { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<(string Url, string FileHash, string Filename)> DownloadUrls { get; set; }

        /// <inheritdoc/>
        public bool IsForceUpdate { get; set; }
    }
}
