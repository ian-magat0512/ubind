// <copyright file="ExtractArchivesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to extract the archive files.
    /// </summary>
    public class ExtractArchivesCommandHandler : ICommandHandler<ExtractArchivesCommand, string>
    {
        private const string DefaultArchivesSearchPattern = "*.zip";

        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractArchivesCommandHandler "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        public ExtractArchivesCommandHandler(IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration, IFileSystemService fileSystemService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<string> Handle(ExtractArchivesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var sourcePath = Path.Combine(basePath, request.UpdaterJobId.ToString(), this.thirdPartyDataSetsConfiguration.DownloadedFolder);
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), this.thirdPartyDataSetsConfiguration.ExtractedFolder);

            if (this.fileSystemService.Directory.Exists(extractPath))
            {
                this.fileSystemService.Directory.Delete(extractPath, true);
            }

            if (!this.fileSystemService.Directory.Exists(extractPath))
            {
                this.fileSystemService.Directory.CreateDirectory(extractPath);
            }

            var zipFiles = this.fileSystemService.Directory.GetFiles(sourcePath, DefaultArchivesSearchPattern);

            foreach (var file in zipFiles)
            {
                this.fileSystemService.ExtractToDirectory(file, extractPath);
            }

            return await Task.FromResult(extractPath);
        }
    }
}
