// <copyright file="IDataDownloaderService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.DataDownloader
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a contract for data downloader services to download files via FTP and HTTP.
    /// </summary>
    public interface IDataDownloaderService
    {
        /// <summary>
        /// Asynchronously downloads a file from the given URL.
        /// </summary>
        /// <param name="downloadProfile">The download profile to be used for resolving connections from the configuration.</param>
        /// <param name="dataDownloaderProtocol">The downloader protocol.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="downloadUrls">The download URLs.</param>
        /// <param name="downloadBufferSize">The download buffer size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the local path of the downloaded file.</returns>
        Task<IReadOnlyList<(string Filename, string FileHash)>> DownloadFilesAsync(
            string downloadProfile,
            DataDownloaderProtocol dataDownloaderProtocol,
            string workingFolder,
            IReadOnlyList<(string Url, string FileHash, string fileName)> downloadUrls,
            int downloadBufferSize,
            CancellationToken cancellationToken);
    }
}
