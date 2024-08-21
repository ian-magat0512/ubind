// <copyright file="ArchiveDownloadedFilesCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to archive downloaded the files using the given updater job manifest.
    /// </summary>
    public class ArchiveDownloadedFilesCommandHandler : ICommandHandler<ArchiveDownloadedFilesCommand, IReadOnlyList<string>>
    {
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDownloadedFilesCommandHandler "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        public ArchiveDownloadedFilesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IFileSystemService fileSystemService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> Handle(ArchiveDownloadedFilesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileList = new List<string>();

            var downloadedFilesPath = Path.Combine(
                this.thirdPartyDataSetsConfiguration.UpdaterJobsPath,
                request.UpdaterJobId.ToString(),
                this.thirdPartyDataSetsConfiguration.DownloadedFolder);

            var downloadedFilePaths = this.fileSystemService.Directory.GetFiles(downloadedFilesPath);

            foreach (var file in downloadedFilePaths)
            {
                var destination = Path.Combine(this.thirdPartyDataSetsConfiguration.FileHashesPath, Path.GetFileName(file));

                if (this.fileSystemService.File.Exists(destination))
                {
                    this.fileSystemService.File.Delete(destination);
                }

                this.fileSystemService.File.Move(file, destination);
                fileList.Add(destination);
            }

            return await Task.FromResult(fileList);
        }
    }
}
