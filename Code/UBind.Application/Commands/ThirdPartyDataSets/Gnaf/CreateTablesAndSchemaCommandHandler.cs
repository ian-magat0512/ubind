// <copyright file="CreateTablesAndSchemaCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Helpers;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to create Gnaf tables and schema.
    /// </summary>
    public class CreateTablesAndSchemaCommandHandler : ICommandHandler<CreateTablesAndSchemaCommand, Unit>
    {
        private const string ExtractedFolder = "Extracted";
        private const string CreateTablesScriptFilename = "create_tables_sqlserver.sql";
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;

        private readonly IGnafRepository gnafRepository;
        private readonly IFileSystemService fileSystemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommandHandler "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="gnafRepository">The Gnaf repository.</param>
        /// <param name="fileSystemService">The filesystem service.</param>
        public CreateTablesAndSchemaCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IGnafRepository gnafRepository,
            IFileSystemService fileSystemService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.gnafRepository = gnafRepository;
            this.fileSystemService = fileSystemService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreateTablesAndSchemaCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), ExtractedFolder);
            var sqlCreationScriptFilePath = this.fileSystemService.Directory.GetFiles(extractPath, CreateTablesScriptFilename, SearchOption.AllDirectories).FirstOrDefault();

            if (sqlCreationScriptFilePath == null)
            {
                return await Task.FromResult(Unit.Value);
            }

            var sqlCreationScript = this.fileSystemService.File.ReadAllText(sqlCreationScriptFilePath);

            async Task CreateNewTablesAndUpdateViews()
            {
                string currentSuffix = await this.gnafRepository.GetExistingTableIndex() ?? "00";
                var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
                string nextIndex = rollingNumber.GetNext();
                await this.gnafRepository.CreateTablesAndSchemaFromScript(sqlCreationScript, nextIndex);
            }

            await RetryPolicyHelper.ExecuteAsync<Exception>(() => CreateNewTablesAndUpdateViews(), retryCount: 3, minJitter: 2000, maxJitter: 2500);

            return await Task.FromResult(Unit.Value);
        }
    }
}
