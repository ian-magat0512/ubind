// <copyright file="RemoveDuplicateReleaseAssetsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Repositories;

/// <summary>
/// Removes duplicate release assets. We had an issue where we found some ReleaseDetails has two or more files with the same name.
/// This should not be possible, but due to an earlier bug it happened. This command removes the duplicate files.
/// </summary>
public class RemoveDuplicateReleaseAssetsCommandHandler : ICommandHandler<RemoveDuplicateReleaseAssetsCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly ITenantRepository tenantRepository;
    private readonly IProductRepository productRepository;
    private readonly ILogger<RemoveDuplicateReleaseAssetsCommandHandler> logger;
    private readonly IDevReleaseRepository devReleaseRepository;

    public RemoveDuplicateReleaseAssetsCommandHandler(
        IUBindDbContext dbContext,
        ITenantRepository tenantRepository,
        IProductRepository productRepository,
        ILogger<RemoveDuplicateReleaseAssetsCommandHandler> logger,
        IDevReleaseRepository devReleaseRepository)
    {
        this.dbContext = dbContext;
        this.tenantRepository = tenantRepository;
        this.productRepository = productRepository;
        this.logger = logger;
        this.devReleaseRepository = devReleaseRepository;
    }

    public async Task<Unit> Handle(RemoveDuplicateReleaseAssetsCommand request, CancellationToken cancellationToken)
    {
        var tenants = this.tenantRepository.GetActiveTenants();
        int totalDuplicatesRemoved = 0;
        foreach (var tenant in tenants)
        {
            this.logger.LogInformation($"Tenant \"{tenant.Details.Alias}\"");
            var products = this.productRepository.GetAllProductsForTenant(tenant.Id);
            foreach (var product in products)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.logger.LogInformation($"    Product \"{product.Details.Alias}\"");
                var devRelease = this.devReleaseRepository.GetDevReleaseForProductWithoutAssets(tenant.Id, product.Id);
                if (devRelease != null)
                {
                    this.logger.LogInformation($"        Processing dev release");
                    totalDuplicatesRemoved += await this.RemoveDuplicateDevReleaseAssets(devRelease);
                }
                cancellationToken.ThrowIfCancellationRequested();
                totalDuplicatesRemoved += await this.RemoveDuplicateReleaseAssets(tenant, product);
            }
        }

        this.logger.LogInformation($"Total duplicates removed: {totalDuplicatesRemoved}");
        return Unit.Value;
    }

    private async Task<int> RemoveDuplicateDevReleaseAssets(DevRelease release)
    {
        int duplicateQuoteAssets = 0;
        if (release.QuoteDetails != null)
        {
            duplicateQuoteAssets = await this.RemoveDuplicateAssets(release.QuoteDetails.Id, "QuoteDetails");
        }

        int duplicateClaimAssets = 0;
        if (release.ClaimDetails != null)
        {
            duplicateClaimAssets = await this.RemoveDuplicateAssets(release.ClaimDetails.Id, "QuoteDetails");
        }

        return duplicateQuoteAssets + duplicateClaimAssets;
    }

    private async Task<int> RemoveDuplicateReleaseAssets(Tenant tenant, Product product)
    {
        var sql = "SELECT Id, Number, MinorNumber, QuoteDetails_Id, ClaimDetails_Id FROM Releases WHERE TenantId = @TenantId AND ProductId = @ProductId";
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenant.Id);
        parameters.Add("ProductId", product.Id);
        var releases = this.dbContext.Database.Connection.Query<ReleaseInfo>(sql, parameters);
        int duplicateQuoteAssets = 0;
        int duplicateClaimAssets = 0;
        foreach (var release in releases)
        {
            this.logger.LogInformation($"        Processing release {release.Number}.{release.MinorNumber} with ID \"{release.Id}\"");
            if (release.QuoteDetails_Id.HasValue)
            {
                duplicateQuoteAssets += await this.RemoveDuplicateAssets(release.QuoteDetails_Id.Value, "QuoteDetails");
            }

            if (release.ClaimDetails_Id.HasValue)
            {
                duplicateClaimAssets += await this.RemoveDuplicateAssets(release.ClaimDetails_Id.Value, "ClaimDetails");
            }
        }

        return duplicateQuoteAssets + duplicateClaimAssets;
    }

    private async Task<int> RemoveDuplicateAssets(Guid releaseDetailsId, string detailsType)
    {
        int duplicatePublicFiles = await this.RemoveDuplicateFiles(releaseDetailsId, isPublic: true);
        if (duplicatePublicFiles > 0)
        {
            this.logger.LogInformation($"            Removed {duplicatePublicFiles} duplicate public files from {detailsType}");
        }
        int duplicatePrivateFiles = await this.RemoveDuplicateFiles(releaseDetailsId, isPublic: false);
        if (duplicatePrivateFiles > 0)
        {
            this.logger.LogInformation($"            Removed {duplicatePrivateFiles} duplicate private files from {detailsType}");
        }
        return duplicatePublicFiles + duplicatePrivateFiles;
    }

    private async Task<int> RemoveDuplicateFiles(Guid releaseDetailsId, bool isPublic)
    {
        string suffix = isPublic ? string.Empty : "1";
        var sql = $@"
            BEGIN TRANSACTION;
            
            WITH Duplicates AS(
                SELECT
                    [Id],
                    [Name],
                    [CreatedTicksSinceEpoch],
                    [ReleaseDetails_Id],
                    [ReleaseDetails_Id1],
                    [FileContentId],
                    [FileModifiedTicksSinceEpoch],
                    ROW_NUMBER() OVER(
                        PARTITION BY[Name]
                        ORDER BY[FileModifiedTicksSinceEpoch] DESC
                    ) AS RowNum
                FROM
                    [Assets]
                WHERE
                    [ReleaseDetails_Id{suffix}] = @ReleaseDetailsId
            )
            DELETE FROM Duplicates
            WHERE RowNum > 1;

            SELECT @@ROWCOUNT as AffectedRows;

            COMMIT TRANSACTION;";
        var parameters = new DynamicParameters();
        parameters.Add("ReleaseDetailsId", releaseDetailsId);
        int affectedRows = await this.dbContext.Database.Connection.QuerySingleAsync<int>(sql, parameters);
        return affectedRows;
    }

    private class ReleaseInfo
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public int MinorNumber { get; set; }

        public Guid? QuoteDetails_Id { get; set; }

        public Guid? ClaimDetails_Id { get; set; }
    }
}
