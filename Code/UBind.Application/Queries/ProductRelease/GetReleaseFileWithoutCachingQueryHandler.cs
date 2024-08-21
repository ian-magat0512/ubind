// <copyright file="GetReleaseFileWithoutCachingQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Models;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Repositories;
using UBind.Domain.Services;

public class GetReleaseFileWithoutCachingQueryHandler : IQueryHandler<GetReleaseFileWithoutCachingQuery, FileContentsDto>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IProductReleaseService productReleaseService;
    private readonly IFileContentRepository fileContentRepository;
    private GetReleaseFileWithoutCachingQuery query;
    private ReleaseBase? release;
    private ReleaseDetails? releaseDetails;

    public GetReleaseFileWithoutCachingQueryHandler(
        IProductReleaseService productReleaseService,
        ICachingResolver cachingResolver,
        IFileContentRepository fileContentRepository)
    {
        this.cachingResolver = cachingResolver;
        this.productReleaseService = productReleaseService;
        this.fileContentRepository = fileContentRepository;
    }

    public async Task<FileContentsDto> Handle(GetReleaseFileWithoutCachingQuery query, CancellationToken cancellationToken)
    {
        this.query = query;
        cancellationToken.ThrowIfCancellationRequested();
        this.release = this.productReleaseService.GetReleaseFromDatabaseWithoutAssetFileContents(query.TenantId, query.ProductReleaseId);
        EntityHelper.ThrowIfNotFound(this.release, query.ProductReleaseId, "release");
        this.releaseDetails = query.WebformAppType == WebFormAppType.Quote
            ? this.release.QuoteDetails
            : this.release.ClaimDetails;
        if (this.releaseDetails == null)
        {
            await this.ThrowNotFound();
        }

        var directoryName = Path.GetDirectoryName(query.Path);
        bool isInRoot = string.IsNullOrEmpty(directoryName);
        if (isInRoot)
        {
            return await this.GetRootFile();
        }

        FileVisibility visibility = FileVisibility.Public;
        if (directoryName == "files")
        {
            visibility = FileVisibility.Private;
        }
        else if (directoryName == "assets")
        {
            visibility = FileVisibility.Public;
        }
        else
        {
            await this.ThrowNotFound();
        }

        var fileName = Path.GetFileName(query.Path);
        var file = visibility == FileVisibility.Private
            ? this.releaseDetails.Files.SingleOrDefault(af => af.Name == fileName)
            : this.releaseDetails.Assets.SingleOrDefault(af => af.Name == fileName);
        if (file == null)
        {
            await this.ThrowNotFound();
        }

        this.LoadFileContents(query.TenantId, file);
        return new FileContentsDto(file, query.Path);
    }

    private void LoadFileContents(Guid tenantId, Asset file)
    {
        if (file.FileContent == null)
        {
            // load the file content into memory
            file.FileContent = this.fileContentRepository.GetFileContent(tenantId, file.FileContentId);
        }
    }

    private async Task<FileContentsDto> GetRootFile()
    {
        switch (this.query.Path)
        {
            case "form-configuration.json":
                if (this.releaseDetails.ConfigurationJson != null
                    && this.releaseDetails.FormConfigurationJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.ConfigurationJson),
                        this.releaseDetails.FormConfigurationJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "workflow.json":
                if (this.releaseDetails.WorkflowJson != null
                    && this.releaseDetails.WorkflowJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.WorkflowJson),
                        this.releaseDetails.WorkflowJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "integrations.json":
                if (this.releaseDetails.ExportsJson != null
                    && this.releaseDetails.IntegrationsJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.ExportsJson),
                        this.releaseDetails.IntegrationsJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "automations.json":
                if (this.releaseDetails.AutomationsJson != null
                    && this.releaseDetails.AutomationsJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.AutomationsJson),
                        this.releaseDetails.AutomationsJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "payment.json":
                if (this.releaseDetails.PaymentJson != null
                    && this.releaseDetails.PaymentJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.PaymentJson),
                        this.releaseDetails.PaymentJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "payment.form.json":
                if (this.releaseDetails.PaymentFormJson != null
                    && this.releaseDetails.PaymentFormJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.PaymentFormJson),
                        this.releaseDetails.PaymentFormJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "funding.json":
                if (this.releaseDetails.FundingJson != null
                    && this.releaseDetails.FundingJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.FundingJson),
                        this.releaseDetails.FundingJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
            case "product.json":
                if (this.releaseDetails.ProductJson != null
                    && this.releaseDetails.ProductJsonLastModifiedTimestamp != null)
                {
                    return new FileContentsDto(
                        this.query.Path,
                        Encoding.UTF8.GetBytes(this.releaseDetails.ProductJson),
                        this.releaseDetails.ProductJsonLastModifiedTimestamp.Value);
                }
                await this.ThrowNotFound();
                break;
        }

        if (this.query.Path.EndsWith("Workbook.xlsx"))
        {
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.query.TenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
                this.query.TenantId,
                this.release.ProductId);
            var workbookFilename = $"{tenantAlias}-{productAlias}-Workbook.xlsx";
            if (this.query.Path == workbookFilename)
            {
                var spreadsheetBytes = this.releaseDetails.FlexCelWorkbook;
                if (spreadsheetBytes == null)
                {
                    await this.ThrowNotFound();
                }
                return new FileContentsDto(
                    this.query.Path,
                    spreadsheetBytes,
                    this.releaseDetails.SpreadsheetLastModifiedTimestamp.Value);
            }
        }

        await this.ThrowNotFound();

        // this line will never be reached, but the compiler doesn't know that.
        return null;
    }

    private async Task ThrowNotFound()
    {
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.query.TenantId);
        var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
            this.query.TenantId,
            this.release.ProductId);

        throw new ErrorException(Errors.Product.File.NotFound(
            tenantAlias,
            productAlias,
            this.query.ProductReleaseId,
            this.query.WebformAppType,
            this.query.Path));
    }
}
