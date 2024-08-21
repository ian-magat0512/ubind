// <copyright file="OneDriveRefactorService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Flurl.Http;
    using Hangfire;
    using Microsoft.Extensions.Logging.Abstractions;
    using UBind.Domain.Upgrade;

    /// <summary>
    /// For re-organizing One Drive folder structure for multi-tenancy.
    /// </summary>
    public class OneDriveRefactorService
    {
        private readonly IMicrosoftGraphConfiguration microsoftGraphConfiguration;
        private readonly IFilesystemStorageConfiguration filesystemStorageConfig;
        private ICachingAuthenticationTokenProvider cachingAuthenticationTokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveRefactorService"/> class.
        /// </summary>
        /// <param name="graphConfiguration">Configuration specifying which graph instance to use, and the required credentials etc.</param>
        /// <param name="filesystemStorageConfig">Config for filesystem storage.</param>
        /// <param name="cachingAuthenticationTokenProvider">A caching provider of the ms graph authentication token.</param>
        public OneDriveRefactorService(
            IMicrosoftGraphConfiguration graphConfiguration,
            IFilesystemStorageConfiguration filesystemStorageConfig,
            ICachingAuthenticationTokenProvider cachingAuthenticationTokenProvider)
        {
            this.microsoftGraphConfiguration = graphConfiguration;
            this.filesystemStorageConfig = filesystemStorageConfig;
            this.cachingAuthenticationTokenProvider = cachingAuthenticationTokenProvider;
        }

        /// <summary>
        /// Update One Drive to match the new layout for Release 2.0.
        /// </summary>
        /// <param name="productMappings">Mappings of existing products to new tenants and products.</param>
        /// <returns>An awaitable task.</returns>
        [AutomaticRetry(Attempts = 0)]
        public async Task UpdateOneDrive(IEnumerable<ProductMigrationMapping> productMappings)
        {
            using (var logFileWriter = new StreamWriter("OneDriveRefactorLog.txt"))
            {
                var urlProvider = new GraphUrlProvider();
                var graphClient = new GraphClient(urlProvider, this.microsoftGraphConfiguration, this.cachingAuthenticationTokenProvider, NullLogger<GraphClient>.Instance);
                var paths = new FilesystemStoragePathService(this.filesystemStorageConfig);
                var token = await graphClient.GetAuthenticationToken();

                const string archiveFolderSuffix = "-archive";
                var archivedDevelopmentFolderName = paths.DevelopmentFolderName + archiveFolderSuffix;
                var archivedReleaseFolderName = paths.ReleasesFolderName + archiveFolderSuffix;

                // Move existing folders to new location.
                await this.TryIgnoringTimeoutsAndWaiting(
                    () => graphClient.RenameItem(
                        paths.DevelopmentFolderPath,
                        archivedDevelopmentFolderName,
                        token),
                    logFileWriter);
                logFileWriter.WriteLine($"Archived development folder.");
                await this.TryIgnoringTimeoutsAndWaiting(
                    () => graphClient.RenameItem(
                        paths.ReleasesFolderPath,
                        archivedReleaseFolderName,
                        token),
                    logFileWriter);
                logFileWriter.WriteLine($"Archived development folder.");

                // Recreate dev and release folders
                await graphClient.CreateFolder(paths.UBindFolderName, paths.DevelopmentFolderName, token);
                logFileWriter.WriteLine($"Recreated development folder.");
                await graphClient.CreateFolder(paths.UBindFolderName, paths.ReleasesFolderName, token);
                logFileWriter.WriteLine($"Recreated releases folder.");

                // Create tenant folders.
                foreach (var tenantId in productMappings.Select(m => m.NewTenantAbbreviation).Distinct())
                {
                    await graphClient.CreateFolder(paths.DevelopmentFolderPath, tenantId, token);
                    logFileWriter.WriteLine($"Created dev folder for tenant {tenantId}");
                    await graphClient.CreateFolder(paths.ReleasesFolderPath, tenantId, token);
                    logFileWriter.WriteLine($"Created releases folder for tenant {tenantId}");
                }

                foreach (var mapping in productMappings)
                {
                    await this.ConvertProduct(
                        graphClient,
                        token,
                        paths,
                        mapping,
                        archivedDevelopmentFolderName,
                        archivedReleaseFolderName,
                        logFileWriter);
                }
            }
        }

        private async Task ConvertProduct(
            GraphClient graphClient,
            string token,
            FilesystemStoragePathService paths,
            ProductMigrationMapping mapping,
            string archivedDevelopmentFolderName,
            string archivedReleaseFolderName,
            StreamWriter logFileWriter)
        {
            // Copy product dev folders
            var productOldDevFolder = Path.Combine(
                paths.UBindFolderName,
                archivedDevelopmentFolderName,
                mapping.ExistingProductId);
            var tenantNewDevFolder = Path.Combine(paths.DevelopmentFolderPath, mapping.NewTenantAbbreviation);
            await graphClient.CopyItem(
                productOldDevFolder, tenantNewDevFolder, mapping.NewProductAbbreviation, token);
            logFileWriter.WriteLine($"Copied dev folder for product {mapping.ExistingProductId} in tenant {mapping.NewTenantAbbreviation} from {productOldDevFolder} to {mapping.NewProductAbbreviation} in {tenantNewDevFolder}");

            // TODO: Rename workbook
            var oldWorkbookName = $"{mapping.ExistingProductId}-Workbook.xlsx";
            var existingWorkbookPath = Path.Combine(
                tenantNewDevFolder,
                mapping.NewProductAbbreviation,
                oldWorkbookName);
            var newWorkbookName = $"{mapping.NewTenantAbbreviation}-{mapping.NewProductAbbreviation}-Workbook.xlsx";
            await graphClient.RenameItem(
                existingWorkbookPath,
                newWorkbookName,
                token);
            logFileWriter.WriteLine($"Renamed workbook {existingWorkbookPath} to {newWorkbookName}");

            // Copy product release folders
            var archivedReleaseFolderPath = Path.Combine(paths.UBindFolderName, archivedReleaseFolderName);
            var productOldReleaseFolder = Path.Combine(archivedReleaseFolderPath, mapping.ExistingProductId);
            var tenantNewReleaseFolder = Path.Combine(paths.ReleasesFolderPath, mapping.NewTenantAbbreviation);
            var archivedProductReleaseFolders = await graphClient.ListSubfoldersInFolder(archivedReleaseFolderPath, token);
            if (archivedProductReleaseFolders.Contains(mapping.ExistingProductId))
            {
                await graphClient.CopyItem(
                    productOldReleaseFolder, tenantNewReleaseFolder, mapping.NewProductAbbreviation, token);
                logFileWriter.WriteLine($"Copied releases folder for product {mapping.ExistingProductId} in tenant {mapping.NewTenantAbbreviation} from {productOldReleaseFolder} to {mapping.NewProductAbbreviation} in {tenantNewReleaseFolder}");

                // TODO: Rename each workbook in releases
                var productNewReleasesFolder = Path.Combine(
                    tenantNewReleaseFolder, mapping.NewProductAbbreviation);
                var releaseFolderNames = await graphClient.ListSubfoldersInFolder(productOldReleaseFolder, token);
                var releaseCount = releaseFolderNames.Count();
                logFileWriter.WriteLine($"Found {releaseCount} releases for {mapping.ExistingProductId} in {productOldReleaseFolder}");
                if (releaseCount > 0)
                {
                    logFileWriter.WriteLine(string.Join(", ", releaseFolderNames));
                }

                foreach (var releaseFolderName in releaseFolderNames)
                {
                    await this.RenameReleaseWorkbook(
                        graphClient,
                        token,
                        productNewReleasesFolder,
                        releaseFolderName,
                        oldWorkbookName,
                        newWorkbookName,
                        logFileWriter);
                }
            }
        }

        private async Task RenameReleaseWorkbook(
            GraphClient graphClient,
            string token,
            string productNewReleasesFolder,
            string releaseFolderName,
            string oldWorkbookName,
            string newWorkbookName,
            StreamWriter logFileWriter)
        {
            var releaseWorkbookPath = Path.Combine(
                productNewReleasesFolder,
                releaseFolderName,
                oldWorkbookName);
            await graphClient.RenameItem(releaseWorkbookPath, newWorkbookName, token);
            logFileWriter.WriteLine($"Renamed {releaseWorkbookPath} to {newWorkbookName}");
        }

        private async Task TryIgnoringTimeoutsAndWaiting(Func<Task> action, StreamWriter logFileWriter)
        {
            try
            {
                await action();
            }
            catch (FlurlHttpException ex)
            {
                logFileWriter.WriteLine(ex.Message);
                Thread.Sleep(10 * 1000);
            }
        }
    }
}
