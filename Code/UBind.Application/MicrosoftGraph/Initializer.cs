// <copyright file="Initializer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class Initializer : IInitializer
    {
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService pathService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Initializer"/> class.
        /// </summary>
        /// <param name="authenticator">Authenticator for obtaining authentication token.</param>
        /// <param name="fileRepository">A repository for accessing source files for a release.</param>
        /// <param name="pathService">Service for generating file paths for use in One Drive.</param>
        public Initializer(
            ICachingAuthenticationTokenProvider authenticator,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService pathService)
        {
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.pathService = pathService;
        }

        /// <inheritdoc/>
        [DisplayName("Setup base folder structure")]
        public async Task SetupFilesystemStorage()
        {
            string bearerToken = await this.fileRepository.GetAuthenticationToken();

            // Create UBind folder
            await this.fileRepository.CreateFolder(string.Empty, this.pathService.UBindFolderName, bearerToken);

            // Create templates folder
            await this.fileRepository.CreateFolder(this.pathService.UBindFolderName, this.pathService.TemplatesFolderName, bearerToken);

            // Create development folder
            await this.fileRepository.CreateFolder(this.pathService.UBindFolderName, this.pathService.DevelopmentFolderName, bearerToken);

            // Create releases folder
            await this.fileRepository.CreateFolder(this.pathService.UBindFolderName, this.pathService.ReleasesFolderName, bearerToken);

            // Create default tenant folder inside development and releases folder.
            await this.fileRepository.CreateFolder(this.pathService.DevelopmentFolderPath, Tenant.MasterTenantAlias, bearerToken);
            await this.fileRepository.CreateFolder(this.pathService.ReleasesFolderPath, Tenant.MasterTenantAlias, bearerToken);

            // Upload default workbook.
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            var localDefaultWorkbookPath = Path.Combine(directory, "Templates", this.pathService.DefaultWorkbookName);
            var workbookBytes = System.IO.File.ReadAllBytes(localDefaultWorkbookPath);
            await this.fileRepository.WriteFileContents(workbookBytes, this.pathService.GetSampleWorkbookPath(), bearerToken);

            // Upload default worflow file.
            var localDefaultWorkflowPath = Path.Combine(directory, "Templates", this.pathService.WorkflowFileName);
            var workflowBytes = System.IO.File.ReadAllBytes(localDefaultWorkflowPath);
            await this.fileRepository.WriteFileContents(workflowBytes, this.pathService.GetSampleWorkflowFilePath(), bearerToken);
        }
    }
}
