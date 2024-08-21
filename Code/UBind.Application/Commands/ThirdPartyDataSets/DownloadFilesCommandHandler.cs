// <copyright file="DownloadFilesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Application.DataDownloader;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to download the files using the given updater job manifest.
    /// </summary>
    public class DownloadFilesCommandHandler : ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string Filename, string FileHash)>>
    {
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IFileSystemService fileSystemService;
        private readonly IDataDownloaderService dataDownloaderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadFilesCommandHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        /// <param name="dataDownloaderService">The data downloader service.</param>
        public DownloadFilesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IFileSystemService fileSystemService,
            IDataDownloaderService dataDownloaderService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.fileSystemService = fileSystemService;
            this.dataDownloaderService = dataDownloaderService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<(string Filename, string FileHash)>> Handle(DownloadFilesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updaterJobsPath = Path.Combine(
                this.thirdPartyDataSetsConfiguration.UpdaterJobsPath,
                request.UpdaterJobId.ToString(),
                this.thirdPartyDataSetsConfiguration.DownloadedFolder);

            if (!this.fileSystemService.Directory.Exists(updaterJobsPath))
            {
                this.fileSystemService.Directory.CreateDirectory(updaterJobsPath);
            }

            if (!this.fileSystemService.Directory.Exists(this.thirdPartyDataSetsConfiguration.FileHashesPath))
            {
                this.fileSystemService.Directory.CreateDirectory(this.thirdPartyDataSetsConfiguration.FileHashesPath);
            }

            return await this.dataDownloaderService.DownloadFilesAsync(
                request.UpdaterJobType.Humanize(),
                request.UpdaterJobManifest.DataDownloaderProtocol,
                updaterJobsPath,
                request.UpdaterJobManifest.DownloadUrls,
                this.thirdPartyDataSetsConfiguration.DownloadBufferSize,
                cancellationToken);
        }
    }
}
