// <copyright file="CleanUpUpdaterJobCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to archive downloaded the files using the given updater job manifest.
    /// </summary>
    public class CleanUpUpdaterJobCommandHandler : ICommandHandler<CleanUpUpdaterJobCommand, Unit>
    {
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpUpdaterJobCommandHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        public CleanUpUpdaterJobCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IFileSystemService fileSystemService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CleanUpUpdaterJobCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updaterPath = Path.Combine(
                this.thirdPartyDataSetsConfiguration.UpdaterJobsPath,
                request.UpdaterJobId.ToString());

            if (this.fileSystemService.Directory.Exists(updaterPath))
            {
                this.fileSystemService.Directory.Delete(updaterPath, true);
            }

            return await Task.FromResult(default(Unit));
        }
    }
}
