// <copyright file="CreateForeignKeysAndIndexesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to create Gnaf tables and schema.
    /// </summary>
    public class CreateForeignKeysAndIndexesCommandHandler : ICommandHandler<CreateForeignKeysAndIndexesCommand, Unit>
    {
        private const string ExtractedFolder = "Extracted";
        private const string AddGnafFkConstraintsScript = "add_fk_constraints.sql";
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;

        private readonly IGnafRepository gnafRepository;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForeignKeysAndIndexesCommandHandler "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="gnafRepository">The Gnaf repository.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        public CreateForeignKeysAndIndexesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IGnafRepository gnafRepository,
            IFileSystemService fileSystemService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.gnafRepository = gnafRepository;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreateForeignKeysAndIndexesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), ExtractedFolder);
            var sqlIndexCreationScriptFilePath = this.fileSystemService.Directory.GetFiles(extractPath, AddGnafFkConstraintsScript, SearchOption.AllDirectories).FirstOrDefault();

            if (sqlIndexCreationScriptFilePath == null)
            {
                return await Task.FromResult(Unit.Value);
            }

            var sqlIndexCreationScript = this.fileSystemService.File.ReadAllText(sqlIndexCreationScriptFilePath);
            var currentSuffix = await this.gnafRepository.GetExistingTableIndex();
            await this.gnafRepository.CreateForeignKeyAndIndexes(sqlIndexCreationScript, currentSuffix);
            return await Task.FromResult(Unit.Value);
        }
    }
}
