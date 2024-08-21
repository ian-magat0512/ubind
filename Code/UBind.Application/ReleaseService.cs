// <copyright file="ReleaseService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class ReleaseService : IReleaseService
    {
        private readonly IReleaseRepository releaseRepository;
        private readonly IDevReleaseRepository devReleaseRepository;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService pathService;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;
        private readonly IUBindDbContext dbContext;

        public ReleaseService(
            IReleaseRepository releaseRepository,
            IDevReleaseRepository devReleaseRepository,
            IProductRepository productRepository,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService pathService,
            ICachingResolver cachingResolver,
            IClock clock,
            IUBindDbContext dbContext)
        {
            Contract.Assert(productRepository != null);
            Contract.Assert(fileRepository != null);

            this.cachingResolver = cachingResolver;
            this.releaseRepository = releaseRepository;
            this.devReleaseRepository = devReleaseRepository;
            this.fileRepository = fileRepository;
            this.pathService = pathService;
            this.clock = clock;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Release> CreateReleaseAsync(Guid tenantId, Guid productId, string description, ReleaseType type)
        {
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);

            if (product.GetStatus() != "Initialised")
            {
                throw new ErrorException(
                    Errors.Release.CannotCreateBecauseProductInitialising(
                        product.Details.Alias));
            }

            DevRelease? latestInitializedDevRelease = this.devReleaseRepository.GetDevReleaseForProductWithoutAssetFileContents(tenantId, productId);
            if (latestInitializedDevRelease == null)
            {
                throw new ErrorException(Errors.Release.AssetsNotSynchronisedWhenCreatingRelease(tenantAlias, product.Details.Alias));
            }

            ReleaseNumberModel releaseNumberModel = this.releaseRepository.GetHighestReleaseNumberForProduct(tenantId, productId);
            releaseNumberModel.IncrementForRelease(type);
            Instant now = this.clock.GetCurrentInstant();

            var release = new Release(
                latestInitializedDevRelease.TenantId,
                latestInitializedDevRelease.ProductId,
                releaseNumberModel.MajorReleaseNumber,
                releaseNumberModel.MinorReleaseNumber,
                description,
                type,
                now);
            release.QuoteDetails = latestInitializedDevRelease.QuoteDetails != null
                ? new ReleaseDetails(latestInitializedDevRelease.QuoteDetails, now)
                : null;
            release.ClaimDetails = latestInitializedDevRelease.ClaimDetails != null
                ? new ReleaseDetails(latestInitializedDevRelease.ClaimDetails, now)
                : null;

            this.releaseRepository.Insert(release);
            this.releaseRepository.SaveChanges();
            return await Task.FromResult<Release>(release);
        }

        /// <inheritdoc/>
        public Release UpdateRelease(Guid tenantId, Guid id, Guid productId, int number, int minorNumber, string description, ReleaseType type)
        {
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, id);

            // we need to attach the release to the context so that EF6 can track changes
            this.dbContext.Releases.Attach(release);

            release.Description = description;
            release.Number = number;
            release.MinorNumber = minorNumber;
            release.Type = type;

            // we need to mark the release as modified so that EF6 will update the record
            this.dbContext.Entry(release).State = EntityState.Modified;

            this.releaseRepository.SaveChanges();
            return release;
        }

        /// <inheritdoc/>
        public void DeleteRelease(Guid tenantId, Guid releaseId)
        {
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
            if (release.TenantId != tenantId && release.TenantId != Domain.Tenant.MasterTenantId)
            {
                throw new UnauthorizedException($"Unauthorized deletion of Release {releaseId}");
            }

            this.releaseRepository.Delete(release);
            this.releaseRepository.SaveChanges();
        }

        /// <inheritdoc/>
        public IEnumerable<Release> GetReleases(Guid tenantId, EntityListFilters filters)
        {
            return this.releaseRepository.GetReleases(tenantId, filters);
        }

        /// <inheritdoc/>
        public async Task RestoreReleaseToDevelopmentEnvironment(Guid tenantId, Guid releaseId)
        {
            Release? release = this.releaseRepository.GetReleaseByIdWithFileContents(tenantId, releaseId);
            EntityHelper.ThrowIfNotFound(release, releaseId, "release");
            if (release.TenantId != tenantId && tenantId != Domain.Tenant.MasterTenantId)
            {
                throw new UnauthorizedException();
            }

            await this.UploadReleaseFilesBackToDevelopmentFolder(release);
        }

        private async Task UploadReleaseFilesBackToDevelopmentFolder(Release release)
        {
            var token = await this.fileRepository.GetAuthenticationToken();
            var releaseDetailsItems = new Dictionary<WebFormAppType, ReleaseDetails>
            {
                { WebFormAppType.Quote, release.QuoteDetails },
                { WebFormAppType.Claim, release.ClaimDetails },
            };

            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(release.TenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(release.TenantId, release.ProductId);
            foreach (var detailsItem in releaseDetailsItems)
            {
                if (detailsItem.Value != null)
                {
                    var releaseDetails = detailsItem.Value;
                    var tenantFolder = this.pathService.GetTenantDevelopmentFolder(tenantAlias);
                    var productFolder = this.pathService.GetProductDevelopmentAppFolder(tenantAlias, productAlias, detailsItem.Key);

                    // let's create it in case it doesn't exist.
                    await this.fileRepository.CreateFolder(productFolder, token);

                    if (releaseDetails.FlexCelWorkbook != null)
                    {
                        var workbookFilename = this.pathService.GetProductWorkbookName(tenantAlias, productAlias);
                        await this.fileRepository.WriteFileContents(releaseDetails.FlexCelWorkbook, System.IO.Path.Combine(productFolder, workbookFilename), token);
                    }

                    if (releaseDetails.WorkflowJson != null)
                    {
                        var workflowJsonFilename = this.pathService.WorkflowFileName;
                        var workflowJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.WorkflowJson);
                        await this.fileRepository.WriteFileContents(workflowJsonFileContent, System.IO.Path.Combine(productFolder, workflowJsonFilename), token);
                    }

                    if (releaseDetails.FundingJson != null)
                    {
                        var fundingJsonFilename = this.pathService.FundingFileName;
                        var fundingJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.FundingJson);
                        await this.fileRepository.WriteFileContents(fundingJsonFileContent, System.IO.Path.Combine(productFolder, fundingJsonFilename), token);
                    }

                    if (releaseDetails.ExportsJson != null)
                    {
                        var exportsJsonFilename = this.pathService.IntegrationsFileName;
                        var exportsJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.ExportsJson);
                        await this.fileRepository.WriteFileContents(exportsJsonFileContent, System.IO.Path.Combine(productFolder, exportsJsonFilename), token);
                    }

                    if (releaseDetails.AutomationsJson != null)
                    {
                        var automationsJsonFilename = this.pathService.AutomationsFileName;
                        var automationsJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.AutomationsJson);
                        await this.fileRepository.WriteFileContents(automationsJsonFileContent, System.IO.Path.Combine(productFolder, automationsJsonFilename), token);
                    }

                    if (releaseDetails.PaymentFormJson != null)
                    {
                        var paymentJsonFilename = this.pathService.PaymentFormFileName;
                        var paymentJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.PaymentFormJson);
                        await this.fileRepository.WriteFileContents(paymentJsonFileContent, System.IO.Path.Combine(productFolder, paymentJsonFilename), token);
                    }

                    if (releaseDetails.PaymentJson != null)
                    {
                        var paymentJsonFilename = this.pathService.PaymentFileName;
                        var paymentJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.PaymentJson);
                        await this.fileRepository.WriteFileContents(paymentJsonFileContent, System.IO.Path.Combine(productFolder, paymentJsonFilename), token);
                    }

                    if (releaseDetails.ProductJson != null)
                    {
                        var productJsonFileName = this.pathService.ProductConfigFileName;
                        var productJsonFileContent = Encoding.UTF8.GetBytes(releaseDetails.ProductJson);
                        await this.fileRepository.WriteFileContents(productJsonFileContent, System.IO.Path.Combine(productFolder, productJsonFileName), token);
                    }

                    await this.fileRepository.CreateFolder(productFolder, this.pathService.MiscFilesFolderName, token);
                    await this.fileRepository.CreateFolder(productFolder, this.pathService.AssetFilesFolderName, token);

                    var semaphore = new SemaphoreSlim(initialCount: 20);
                    var writeFileTasks = releaseDetails.Files.Select(async (file) =>
                    {
                        await semaphore.WaitAsync(); // Acquire the semaphore
                        try
                        {
                            await this.WriteFileContents(file, productFolder, this.pathService.MiscFilesFolderName, token);
                        }
                        finally
                        {
                            semaphore.Release(); // Release the semaphore
                        }
                    }).ToList();

                    writeFileTasks.AddRange(releaseDetails.Assets.Select(async (file) =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await this.WriteFileContents(file, productFolder, this.pathService.AssetFilesFolderName, token);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));

                    await Task.WhenAll(writeFileTasks);
                }
            }
        }

        private async Task WriteFileContents(Asset file, string productFolder, string assetsFolder, string token)
        {
            var destinationPath = Path.Combine(productFolder, assetsFolder, file.Name);
            await this.fileRepository.WriteFileContents(file.FileContent?.Content, destinationPath, token);
        }
    }
}
