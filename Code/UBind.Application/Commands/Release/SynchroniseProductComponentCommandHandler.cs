// <copyright file="SynchroniseProductComponentCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Release;

using Newtonsoft.Json.Linq;
using NodaTime;
using StackExchange.Profiling;
using System;
using System.Threading;
using UBind.Application.Automation;
using UBind.Application.FlexCel;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Models;
using UBind.Domain.Models.Release;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Repositories;
using UBind.Domain.Repositories.Redis;

public class SynchroniseProductComponentCommandHandler : ICommandHandler<SynchroniseProductComponentCommand, DevRelease>
{
    private readonly IDevReleaseRepository devReleaseRepository;
    private readonly IFilesystemFileRepository fileRepository;
    private readonly IFilesystemStoragePathService pathService;
    private readonly IFormConfigurationGenerator formConfigurationGenerator;
    private readonly IClock clock;
    private readonly IReleaseValidator releaseValidator;
    private readonly ICachingResolver cachingResolver;
    private readonly IGlobalReleaseCache globalReleaseCache;
    private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;
    private readonly ISpreadsheetPoolService spreadsheetPoolService;
    private readonly IProductReleaseSynchroniseRepository productReleaseSync;
    private readonly IFileContentRepository fileContentRepository;

    /// <summary>
    /// This is used to limit the number of asset files we will load from disk at once,
    /// to ensure we don't use too many threads or saturate disk IO.
    /// </summary>
    private readonly SemaphoreSlim semaphore;

    private string tenantAlias;
    private string productAlias;
    private string fileRepositoryToken;
    private string productComponentFolder;
    private WebFormAppType componentType;
    private Guid tenantId;
    private Guid productId;
    private IFlexCelWorkbook? quoteWorkbook;
    private IFlexCelWorkbook? claimWorkbook;
    private CancellationToken cancellationToken;
    private ReleaseDetailsChangeTracker changeTracker;

    public SynchroniseProductComponentCommandHandler(
        IDevReleaseRepository devReleaseRepository,
        IFilesystemFileRepository fileRepository,
        IFilesystemStoragePathService pathService,
        IFormConfigurationGenerator formConfigurationGenerator,
        IReleaseValidator releaseValidator,
        IGlobalReleaseCache globalReleaseCache,
        ICachingResolver cachingResolver,
        IClock clock,
        IAutomationPeriodicTriggerScheduler periodicTriggerScheduler,
        ISpreadsheetPoolService spreadsheetPoolService,
        IProductReleaseSynchroniseRepository productReleaseSync,
        IFileContentRepository fileContentRepository)
    {
        this.cachingResolver = cachingResolver;
        this.devReleaseRepository = devReleaseRepository;
        this.fileRepository = fileRepository;
        this.pathService = pathService;
        this.formConfigurationGenerator = formConfigurationGenerator;
        this.releaseValidator = releaseValidator;
        this.globalReleaseCache = globalReleaseCache;
        this.clock = clock;
        this.periodicTriggerScheduler = periodicTriggerScheduler;
        this.spreadsheetPoolService = spreadsheetPoolService;
        this.productReleaseSync = productReleaseSync;
        this.semaphore = new SemaphoreSlim(10);
        this.fileContentRepository = fileContentRepository;
    }

    public async Task<DevRelease> Handle(SynchroniseProductComponentCommand command, CancellationToken cancellationToken)
    {
        using (MiniProfiler.Current.Step($"{nameof(SynchroniseProductComponentCommandHandler)}.{nameof(this.Handle)}"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.cancellationToken = cancellationToken;
            this.fileRepositoryToken = await this.fileRepository.GetAuthenticationToken();
            this.tenantId = command.TenantId;
            this.productId = command.ProductId;
            this.tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(command.TenantId);
            this.productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(command.TenantId, command.ProductId);
            this.componentType = command.ComponentType;

            if (await this.productReleaseSync.Exists(command.TenantId, command.ProductId))
            {
                throw new ErrorException(Errors.Product.ProductSynchronisationInProgress(this.tenantAlias, this.productAlias));
            }

            try
            {
                // mark the product as being sync in progress. This will prevent other syncs from starting until this one is complete.
                await this.productReleaseSync.Upsert(command.TenantId, command.ProductId);
                this.productComponentFolder = this.pathService.GetProductDevelopmentAppFolder(
                    this.tenantAlias, this.productAlias, command.ComponentType);
                await this.ThrowIfProductComponentFolderDoesNotExist(command);
                await this.ThrowIfProductWorkbookDoesNotExist();

                // since we are not using entity framework to track changes, we need to track them ourselves
                this.changeTracker = new ReleaseDetailsChangeTracker(command.ComponentType);

                // get the dev release
                var devRelease = this.devReleaseRepository.GetDevReleaseForProductWithoutAssetFileContents(command.TenantId, command.ProductId);
                if (devRelease == null)
                {
                    // this is the first time somebody has tried to sync this product, so let's create the dev release
                    devRelease = new DevRelease(command.TenantId, command.ProductId, this.clock.Now());
                    this.changeTracker.IsDevReleaseNew = true;
                }

                this.changeTracker.DevRelease = devRelease;
                ReleaseDetails? existingReleaseDetails = command.ComponentType == WebFormAppType.Quote
                    ? devRelease.QuoteDetails
                    : devRelease.ClaimDetails;

                var currentTimestamp = this.clock.Now();
                devRelease.LastModifiedTimestamp = currentTimestamp;

                var updatedReleaseDetails = await this.UpdateReleaseDetails(existingReleaseDetails, command);
                if (command.ComponentType == WebFormAppType.Quote)
                {
                    this.releaseValidator.ValidateQuoteDetails(updatedReleaseDetails);
                    devRelease.QuoteDetails = updatedReleaseDetails;
                    devRelease.QuoteDetails.LastSynchronisedTimestamp = currentTimestamp;
                }
                else
                {
                    this.releaseValidator.ValidateClaimDetails(updatedReleaseDetails);
                    devRelease.ClaimDetails = updatedReleaseDetails;
                    devRelease.ClaimDetails.LastSynchronisedTimestamp = currentTimestamp;
                }

                await this.devReleaseRepository.SaveChanges(this.changeTracker);

                var productContext = new ProductContext(this.tenantId, this.productId, DeploymentEnvironment.Development);
                this.spreadsheetPoolService.RemoveSpreadsheetPools(productContext, command.ComponentType);

                this.quoteWorkbook = this.quoteWorkbook ?? (devRelease.QuoteDetails?.FlexCelWorkbook != null
                    ? new FlexCelWorkbook(devRelease.QuoteDetails.FlexCelWorkbook, WebFormAppType.Quote, this.clock)
                    : null);
                this.claimWorkbook = this.claimWorkbook ?? (devRelease.ClaimDetails?.FlexCelWorkbook != null
                    ? new FlexCelWorkbook(devRelease.ClaimDetails.FlexCelWorkbook, WebFormAppType.Claim, this.clock)
                    : null);
                var artefacts = new DevReleaseInitializationArtefacts(
                    devRelease,
                    this.quoteWorkbook,
                    this.claimWorkbook);

                // TODO: A Possible performance optimisation would be to update the cache with only the new quote or claim workbook,
                // instead of having to load both workbooks into memory when one hasn't changed.
                var activeDeployedRelease = this.globalReleaseCache.CacheNewDevRelease(artefacts);
                await this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(
                    this.tenantId, this.productId, DeploymentEnvironment.Development, activeDeployedRelease);
                return devRelease;
            }
            finally
            {
                // mark the product as no longer being sync in progress. This will allow other syncs to start.
                await this.productReleaseSync.Delete(command.TenantId, command.ProductId);
            }
        }
    }

    private async Task ThrowIfProductComponentFolderDoesNotExist(SynchroniseProductComponentCommand command)
    {
        bool exists = await this.fileRepository.FolderExists(this.productComponentFolder, this.fileRepositoryToken);
        if (!exists)
        {
            var errorData = new JObject
            {
                { "tenantId", command.TenantId },
                { "productId", command.ProductId },
            };
            throw new ErrorException(Errors.Product.ProductComponentFolderDoesNotExist(
                this.productAlias, command.ComponentType, errorData));
        }
    }

    private async Task ThrowIfProductWorkbookDoesNotExist()
    {
        bool exists = await this.fileRepository.FileExists(
                this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, this.componentType),
                this.fileRepositoryToken);
        if (!exists)
        {
            throw new ErrorException(Errors.Product.WorkbookNotFound(
                this.tenantAlias, this.productAlias, this.componentType.ToString().ToLower()));
        }
    }

    private async Task<ReleaseDetails> UpdateReleaseDetails(
        ReleaseDetails? releaseDetails,
        SynchroniseProductComponentCommand command)
    {
        using (MiniProfiler.Current.Step($"{nameof(SynchroniseProductComponentCommandHandler)}.{nameof(this.UpdateReleaseDetails)}"))
        {
            ReleaseDetails updatedReleaseDetails = releaseDetails != null
                ? releaseDetails
                : new ReleaseDetails(command.ComponentType, this.clock.Now());
            this.changeTracker.ReleaseDetails = updatedReleaseDetails;
            this.changeTracker.IsReleaseDetailsNew = releaseDetails == null;
            List<Task> tasks = new List<Task>();

            var configItemProxies = new List<IReleaseConfigItemProxy>
            {
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.WorkflowJson,
                    (rd, value) => rd.WorkflowJson = value,
                    rd => rd.WorkflowJsonLastModifiedTimestamp,
                    (rd, value) => rd.WorkflowJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.WorkflowFileName),
                    () =>
                    {
                        this.changeTracker.HasFormConfigurationJsonChanged = true;
                        this.changeTracker.HasWorkflowJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.ExportsJson,
                    (rd, value) => rd.ExportsJson = value,
                    rd => rd.IntegrationsJsonLastModifiedTimestamp,
                    (rd, value) => rd.IntegrationsJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.IntegrationsFileName),
                    () =>
                    {
                        this.changeTracker.HasIntegrationsJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.AutomationsJson,
                    (rd, value) => rd.AutomationsJson = value,
                    rd => rd.AutomationsJsonLastModifiedTimestamp,
                    (rd, value) => rd.AutomationsJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.AutomationsFileName),
                    () =>
                    {
                        this.changeTracker.HasAutomationsJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.PaymentJson,
                    (rd, value) => rd.PaymentJson = value,
                    rd => rd.PaymentJsonLastModifiedTimestamp,
                    (rd, value) => rd.PaymentJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.PaymentFileName),
                    () =>
                    {
                        this.changeTracker.HasPaymentJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.PaymentFormJson,
                    (rd, value) => rd.PaymentFormJson = value,
                    rd => rd.PaymentFormJsonLastModifiedTimestamp,
                    (rd, value) => rd.PaymentFormJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.PaymentFormFileName),
                    () =>
                    {
                        this.changeTracker.HasFormConfigurationJsonChanged = true;
                        this.changeTracker.HasPaymentFormJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.FundingJson,
                    (rd, value) => rd.FundingJson = value,
                    rd => rd.FundingJsonLastModifiedTimestamp,
                    (rd, value) => rd.FundingJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.FundingFileName),
                    () =>
                    {
                        this.changeTracker.HasFundingJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<string>(
                    updatedReleaseDetails,
                    rd => rd.ProductJson,
                    (rd, value) => rd.ProductJson = value,
                    rd => rd.ProductJsonLastModifiedTimestamp,
                    (rd, value) => rd.ProductJsonLastModifiedTimestamp = value,
                    this.GetFullPath(this.pathService.ProductConfigFileName),
                    () =>
                    {
                        this.changeTracker.HasFormConfigurationJsonChanged = true;
                        this.changeTracker.HasProductJsonChanged = true;
                        return Task.CompletedTask;
                    }),
                new ReleaseConfigItemProxy<byte[]?>(
                    updatedReleaseDetails,
                    rd => rd.FlexCelWorkbook,
                    (rd, value) => rd.FlexCelWorkbook = value,
                    rd => rd.SpreadsheetLastModifiedTimestamp,
                    (rd, value) => rd.SpreadsheetLastModifiedTimestamp = value,
                    this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, this.componentType),
                    () =>
                    {
                        this.changeTracker.HasFormConfigurationJsonChanged = true;
                        this.changeTracker.HasSpreadsheetChanged = true;
                        return Task.CompletedTask;
                    }),
            };

            foreach (var proxy in configItemProxies)
            {
                tasks.Add(proxy.CheckAndUpdate(this.fileRepository, this.fileRepositoryToken, this.clock));
            }

            tasks.Add(this.UpdatePrivateFiles(updatedReleaseDetails));
            tasks.Add(this.UpdatePublicAssets(updatedReleaseDetails));
            await Task.WhenAll(tasks);

            if (this.changeTracker.HasFormConfigurationJsonChanged)
            {
                await this.UpdateFormConfiguration(updatedReleaseDetails);
            }

            return updatedReleaseDetails;
        }
    }

    private async Task UpdateFormConfiguration(ReleaseDetails releaseDetails)
    {
        if (releaseDetails.FlexCelWorkbook == null || releaseDetails.WorkflowJson == null)
        {
            releaseDetails.ConfigurationJson = null;
            releaseDetails.FormConfigurationJsonLastModifiedTimestamp = null;
            return;
        }

        var flexCelWorkbook = new FlexCelWorkbook(releaseDetails.FlexCelWorkbook, this.componentType, this.clock);
        var formConfiguration = await this.formConfigurationGenerator.Generate(
            this.tenantId,
            this.productId,
            this.componentType,
            flexCelWorkbook,
            releaseDetails.WorkflowJson,
            releaseDetails.ProductJson,
            releaseDetails.PaymentFormJson);
        releaseDetails.ConfigurationJson = formConfiguration;
        releaseDetails.FormConfigurationJsonLastModifiedTimestamp = releaseDetails.SpreadsheetLastModifiedTimestamp;

        // store a reference to the workbook so we can use it to initialise the cache later
        if (this.componentType == WebFormAppType.Quote)
        {
            this.quoteWorkbook = flexCelWorkbook;
        }
        else
        {
            this.claimWorkbook = flexCelWorkbook;
        }
    }

    private async Task UpdatePrivateFiles(ReleaseDetails releaseDetails)
    {
        var privateFilesFolder = this.pathService.GetPrivateFilesFolder(this.tenantAlias, this.productAlias, this.componentType);
        var existingPrivateFiles = releaseDetails.Files;
        var assetChanges = await this.UpdateFiles(existingPrivateFiles, privateFilesFolder);
        this.changeTracker.AddedPrivateFiles = assetChanges.Added;
        this.changeTracker.RemovedPrivateFiles = assetChanges.Removed;
    }

    private async Task UpdatePublicAssets(ReleaseDetails releaseDetails)
    {
        var publicAssetsFolder = this.pathService.GetPublicAssetFolder(this.tenantAlias, this.productAlias, this.componentType);
        var existingPublicAssets = releaseDetails.Assets;
        var assetChanges = await this.UpdateFiles(existingPublicAssets, publicAssetsFolder);
        this.changeTracker.AddedPublicFiles = assetChanges.Added;
        this.changeTracker.RemovedPublicFiles = assetChanges.Removed;
    }

    private async Task<AssetChanges> UpdateFiles(ICollection<Asset> existingFiles, string folder)
    {
        var fileInfoList = await this.fileRepository.ListFilesInfoInFolder(folder, this.fileRepositoryToken);
        var tasks = new List<Task>();
        var assetChanges = new AssetChanges();
        var assetsToRemove = new List<Asset>();
        var assetsToAdd = new List<FileInfo>();
        var addTasks = new List<Task<Asset>>();
        foreach (var existingFile in existingFiles)
        {
            var fileInfo = fileInfoList.FirstOrDefault(f => Path.GetFileName(f.Path) == existingFile.Name);
            if (fileInfo == null)
            {
                assetsToRemove.Add(existingFile);
            }
        }
        foreach (var fileInfo in fileInfoList)
        {
            var filename = Path.GetFileName(fileInfo.Path);
            var existingFile = existingFiles.FirstOrDefault(f => f.Name == filename);
            if (existingFile == null)
            {
                assetsToAdd.Add(fileInfo);
            }
            else if (fileInfo.LastModifiedTimestamp > existingFile.FileModifiedTimestamp)
            {
                assetsToRemove.Add(existingFile);
                assetsToAdd.Add(fileInfo);
            }
        }
        foreach (var asset in assetsToRemove)
        {
            tasks.Add(this.RemoveAsset(asset, existingFiles));
        }
        foreach (var fileInfo in assetsToAdd)
        {
            var addTask = this.AddAsset(fileInfo, existingFiles);
            tasks.Add(addTask);
            addTasks.Add(addTask);
        }

        await Task.WhenAll(tasks);
        assetChanges.Removed = assetsToRemove;
        foreach (var addTask in addTasks)
        {
            var asset = addTask.Result;
            assetChanges.Added.Add(asset);
        }

        return assetChanges;
    }

    private async Task<Asset> LoadAsset(FileInfo fileInfo)
    {
        await this.semaphore.WaitAsync(this.cancellationToken);

        try
        {
            var fullPath = Path.Combine(this.productComponentFolder, fileInfo.Path);
            var filename = Path.GetFileName(fileInfo.Path);
            byte[] content = await this.fileRepository.GetFileContents(fullPath, this.fileRepositoryToken);
            var fileContent = FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), content);
            var existingFileContent = this.fileContentRepository.GetFileContentByHashCode(this.tenantId, fileContent.HashCode);
            fileContent = existingFileContent ?? fileContent;
            return new Asset(this.tenantId, filename, fileInfo.LastModifiedTimestamp, fileContent, this.clock.Now());
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private async Task<Asset> AddAsset(FileInfo fileInfo, ICollection<Asset> fileCollection)
    {
        var asset = await this.LoadAsset(fileInfo);
        fileCollection.Add(asset);
        return asset;
    }

    private Task RemoveAsset(Asset asset, ICollection<Asset> fileCollection)
    {
        fileCollection.Remove(asset);
        return Task.CompletedTask;
    }

    private string GetFullPath(string productFileName)
    {
        return Path.Combine(
            this.pathService.GetProductDevelopmentAppFolder(this.tenantAlias, this.productAlias, this.componentType),
            productFileName);
    }

    private class AssetChanges
    {
        public List<Asset> Added { get; set; } = new List<Asset>();

        public List<Asset> Removed { get; set; } = new List<Asset>();
    }
}
