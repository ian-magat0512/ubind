// <copyright file="GetProductReleaseSourceFilesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Configuration;
using UBind.Domain.Dto;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GetProductReleaseSourceFilesQueryHandler : IQueryHandler<GetProductReleaseSourceFilesQuery, List<ConfigurationFileDto>>
{
    private static string[] nonBrowserViewableExtensions = new string[]
    {
        ".docx",
        ".doc",
        ".xlsx",
        ".xls",
        ".pptx",
        ".ppt",
    };

    private readonly IDevReleaseRepository devReleaseRepository;
    private readonly IReleaseRepository releaseRepository;
    private readonly ICachingResolver cachingResolver;
    private readonly IEmailInvitationConfiguration hostConfiguration;
    private string? tenantAlias;
    private string? productAlias;
    private Guid releaseId;

    public GetProductReleaseSourceFilesQueryHandler(
        IDevReleaseRepository devReleaseRepository,
        IReleaseRepository releaseRepository,
        ICachingResolver cachingResolver,
        IEmailInvitationConfiguration hostConfiguration)
    {
        this.devReleaseRepository = devReleaseRepository;
        this.releaseRepository = releaseRepository;
        this.cachingResolver = cachingResolver;
        this.hostConfiguration = hostConfiguration;
    }

    public Task<List<ConfigurationFileDto>> Handle(GetProductReleaseSourceFilesQuery query, CancellationToken cancellationToken)
    {
        ReleaseBase? release;

        if (query.ReleaseId.HasValue)
        {
            this.releaseId = query.ReleaseId.Value;
            release = this.releaseRepository.GetReleaseWithoutAssetFileContents(query.TenantId, this.releaseId);
            if (release == null)
            {
                throw new ErrorException(Errors.Release.NotFound(this.tenantAlias, this.productAlias, query.ReleaseId.Value));
            }
        }
        else
        {
            if (query.ProductId == null)
            {
                throw new InvalidOperationException("You must specify either a product ID or a release ID to list release files.");
            }

            release = this.devReleaseRepository.GetDevReleaseForProductWithoutAssetFileContents(query.TenantId, query.ProductId.Value);
            if (release == null)
            {
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.TenantId);
                var productAlias = this.cachingResolver.GetProductAliasOrThrow(query.TenantId, query.ProductId.Value);
                throw new ErrorException(Errors.Release.AssetsNotSynchronised(tenantAlias, productAlias));
            }

            this.releaseId = release.Id;
        }

        this.tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.TenantId);
        this.productAlias = this.cachingResolver.GetProductAliasOrThrow(query.TenantId, release.ProductId);

        var fileList = new List<ConfigurationFileDto>();
        if (release.QuoteDetails != null)
        {
            fileList.AddRange(this.GetFiles(release.QuoteDetails, release.Id));
        }

        if (release.ClaimDetails != null)
        {
            fileList.AddRange(this.GetFiles(release.ClaimDetails, release.Id));
        }

        return Task.FromResult(fileList);
    }

    private List<ConfigurationFileDto> GetFiles(ReleaseDetails releaseDetails, Guid productReleaseId)
    {
        var fileList = new List<ConfigurationFileDto>();
        if (releaseDetails.ConfigurationJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "form-configuration.json",
                Path = "form-configuration.json",
                LastModifiedTimestamp = releaseDetails.FormConfigurationJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("form-configuration.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.WorkflowJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "workflow.json",
                Path = "workflow.json",
                LastModifiedTimestamp = releaseDetails.WorkflowJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("workflow.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.ExportsJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "integrations.json",
                Path = "integrations.json",
                LastModifiedTimestamp = releaseDetails.IntegrationsJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("integrations.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.AutomationsJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "automations.json",
                Path = "automations.json",
                LastModifiedTimestamp = releaseDetails.AutomationsJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("automations.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.PaymentJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "payment.json",
                Path = "payment.json",
                LastModifiedTimestamp = releaseDetails.PaymentJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("payment.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.PaymentFormJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "payment.form.json",
                Path = "payment.form.json",
                LastModifiedTimestamp = releaseDetails.PaymentFormJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("payment.form.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.FundingJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "funding.json",
                Path = "funding.json",
                LastModifiedTimestamp = releaseDetails.FundingJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("funding.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.ProductJson != null)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = "product.json",
                Path = "product.json",
                LastModifiedTimestamp = releaseDetails.ProductJsonLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl("product.json", productReleaseId, releaseDetails.AppType),
            });
        }

        if (releaseDetails.FlexCelWorkbook != null)
        {
            var workbookFilename = $"{this.tenantAlias}-{this.productAlias}-Workbook.xlsx";
            fileList.Add(new ConfigurationFileDto
            {
                Id = workbookFilename,
                Path = workbookFilename,
                LastModifiedTimestamp = releaseDetails.SpreadsheetLastModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl(workbookFilename, productReleaseId, releaseDetails.AppType),
                IsBrowserViewable = false,
            });
        }

        foreach (var file in releaseDetails.Files)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = file.Id.ToString(),
                Path = $"files/{file.Name}",
                LastModifiedTimestamp = file.FileModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl($"files/{file.Name}", productReleaseId, releaseDetails.AppType),
                IsBrowserViewable = this.IsFileBrowserViewable(file.Name),
            });
        }

        foreach (var asset in releaseDetails.Assets)
        {
            fileList.Add(new ConfigurationFileDto
            {
                Id = asset.Id.ToString(),
                Path = $"assets/{asset.Name}",
                LastModifiedTimestamp = asset.FileModifiedTimestamp,
                WebFormAppType = releaseDetails.AppType,
                ResourceUrl = this.GenerateResourceUrl($"assets/{asset.Name}", productReleaseId, releaseDetails.AppType),
                IsBrowserViewable = this.IsFileBrowserViewable(asset.Name),
            });
        }

        return fileList;
    }

    private string GenerateResourceUrl(string path, Guid releaseId, WebFormAppType appType)
    {
        return this.hostConfiguration.InvitationLinkHost
            + $"api/v1/release/{releaseId}/{appType.ToString().ToLowerInvariant()}"
            + $"/file/{path}";
    }

    private bool IsFileBrowserViewable(string fileName)
    {
        foreach (var extension in nonBrowserViewableExtensions)
        {
            if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
