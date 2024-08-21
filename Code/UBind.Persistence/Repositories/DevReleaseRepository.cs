// <copyright file="DevReleaseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Repositories;

using System;
using System.Data;
using System.Linq;
using System.Transactions;
using Dapper;
using Humanizer;
using NodaTime;
using StackExchange.Profiling;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Product;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Repositories;
using UBind.Persistence.RepositoryResourceScripts;

/// <inheritdoc/>
public class DevReleaseRepository : IDevReleaseRepository
{
    private readonly IUBindDbContext dbContext;
    private readonly IFileContentRepository fileContentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevReleaseRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public DevReleaseRepository(IUBindDbContext dbContext, IFileContentRepository fileContentRepository)
    {
        this.dbContext = dbContext;
        this.fileContentRepository = fileContentRepository;
    }

    /// <inheritdoc/>
    public DevRelease? GetDevReleaseByIdWithFileContents(Guid tenantId, Guid releaseId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetDevReleaseByIdWithFileContents)}"))
        {
            /* The EF6 Query is incredibly slow, so we switched to dapper below.
            return this.dbContext.DevReleases
                .Include(r => r.QuoteDetails.Files.Select(f => f.FileContent))
                .Include(r => r.QuoteDetails.Assets.Select(a => a.FileContent))
                .Include(r => r.ClaimDetails.Files.Select(f => f.FileContent))
                .Include(r => r.ClaimDetails.Assets.Select(a => a.FileContent))
                .Where(r => r.TenantId == tenantId && r.Id == releaseId)
                .FirstOrDefault();
            */
            var connection = this.dbContext.Database.Connection;
            using (var results = connection.QueryMultiple(
                DevReleaseRepositoryResourceScript.GetDevReleaseByIdWithFileContents,
                new { TenantId = tenantId, ReleaseId = releaseId }))
            {
                var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                    (dr, qd, cd) =>
                    {
                        dr.QuoteDetails = qd;
                        dr.ClaimDetails = cd;
                        return dr;
                    },
                    splitOn: "Id, Id").ToList();

                var assetsWithContent = results.Read<Asset, FileContent, Guid, int, Asset>(
                    (asset, fileContent, releaseDetailsId, isPublicInt) =>
                    {
                        asset.FileContent = fileContent;
                        var isPublic = isPublicInt == 1;
                        var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                            .FirstOrDefault(rd => rd != null && rd.Id == releaseDetailsId);
                        if (releaseDetails != null)
                        {
                            if (isPublic)
                            {
                                releaseDetails.Assets.Add(asset);
                            }
                            else
                            {
                                releaseDetails.Files.Add(asset);
                            }
                        }
                        return asset;
                    },
                    splitOn: "Id, ReleaseDetailsId, IsPublic").ToList();
                return devReleases.FirstOrDefault();
            }
        }
    }

    /// <inheritdoc/>
    public Guid? GetIdOfDevReleaseForProduct(Guid tenantId, Guid productId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetIdOfDevReleaseForProduct)}"))
        {
            return this.dbContext.DevReleases
                .Where(r => r.TenantId == tenantId && r.ProductId == productId)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefault();
        }
    }

    public DevRelease? GetDevReleaseForProductWithoutAssetFileContents(Guid tenantId, Guid productId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetDevReleaseForProductWithoutAssetFileContents)}"))
        {
            /* The EF6 Query is incredibly slow, so we switched to dapper below.
            var release = this.dbContext.DevReleases
                .Include(r => r.QuoteDetails.Files)
                .Include(r => r.QuoteDetails.Assets)
                .Include(r => r.ClaimDetails.Files)
                .Include(r => r.ClaimDetails.Assets)
                .Where(r => r.TenantId == tenantId && r.ProductId == productId)
                .FirstOrDefault();
            */
            var connection = this.dbContext.Database.Connection;
            using (var results = connection.QueryMultiple(
                DevReleaseRepositoryResourceScript.GetDevReleaseForProductWithoutAssetFileContents,
                new { TenantId = tenantId, ProductId = productId }))
            {
                var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                    (dr, qd, cd) =>
                    {
                        dr.QuoteDetails = qd;
                        dr.ClaimDetails = cd;
                        return dr;
                    },
                    splitOn: "Id, Id").ToList();

                results.Read<Asset, Guid, int, Asset>(
                    (asset, releaseDetailsId, isPublicInt) =>
                    {
                        var isPublic = isPublicInt == 1;
                        var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                            .FirstOrDefault(rd => rd?.Id == releaseDetailsId);
                        if (releaseDetails != null)
                        {
                            if (isPublic)
                            {
                                releaseDetails.Assets.Add(asset);
                            }
                            else
                            {
                                releaseDetails.Files.Add(asset);
                            }
                        }
                        return asset;
                    },
                    splitOn: "ReleaseDetailsId, IsPublic").ToList();

                var devRelease = devReleases.FirstOrDefault();
                return devRelease;
            }
        }
    }

    public DevRelease? GetDevReleaseForProductWithoutAssets(Guid tenantId, Guid productId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetDevReleaseForProductWithoutAssets)}"))
        {
            /* The EF6 Query is incredibly slow, so we switched to dapper below.
            var release = this.dbContext.DevReleases
                .Include(r => r.QuoteDetails)
                .Include(r => r.ClaimDetails)
                .Where(r => r.TenantId == tenantId && r.ProductId == productId)
                .FirstOrDefault();
            return release;
            */
            var connection = this.dbContext.Database.Connection;
            var sql = @"
                SELECT TOP(1) dr.*, qd.*, cd.*
                FROM DevReleases dr
                LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
                LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
                WHERE dr.TenantId = @TenantId AND dr.ProductId = @ProductId";

            var release = connection.Query<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                sql,
                (dr, qd, cd) =>
                {
                    dr.QuoteDetails = qd;
                    dr.ClaimDetails = cd;
                    return dr;
                },
                new { TenantId = tenantId, ProductId = productId },
                splitOn: "Id, Id").FirstOrDefault();
            return release;
        }
    }

    public DevRelease? GetDevReleaseWithoutAssets(Guid tenantId, Guid productReleaseId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetDevReleaseForProductWithoutAssets)}"))
        {
            /* The EF6 Query is incredibly slow, so we switched to dapper below.
            var release = this.dbContext.DevReleases
                .Include(r => r.QuoteDetails)
                .Include(r => r.ClaimDetails)
                .Where(r => r.TenantId == tenantId && r.Id == productReleaseId)
                .FirstOrDefault();
            return release;
            */
            var connection = this.dbContext.Database.Connection;
            var sql = @"
                SELECT TOP(1) dr.*, qd.*, cd.*
                FROM DevReleases dr
                LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
                LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
                WHERE dr.TenantId = @TenantId AND dr.Id = @ProductReleaseId";

            var devRelease = connection.Query<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                sql,
                (dr, qd, cd) =>
                {
                    dr.QuoteDetails = qd;
                    dr.ClaimDetails = cd;
                    return dr;
                },
                new { TenantId = tenantId, ProductReleaseId = productReleaseId },
                splitOn: "Id, Id").FirstOrDefault();

            return devRelease;
        }
    }

    public DevRelease? GetDevReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetDevReleaseWithoutAssetFileContents)}"))
        {
            /* The EF6 Query is incredibly slow, so we switched to dapper below.
            var release = this.dbContext.DevReleases
                .Include(r => r.QuoteDetails.Files)
                .Include(r => r.QuoteDetails.Assets)
                .Include(r => r.ClaimDetails.Files)
                .Include(r => r.ClaimDetails.Assets)
                .Where(r => r.TenantId == tenantId && r.Id == productReleaseId)
                .FirstOrDefault();
            */
            var connection = this.dbContext.Database.Connection;
            using (var results = connection.QueryMultiple(
                DevReleaseRepositoryResourceScript.GetDevReleaseWithoutAssetFileContents,
                new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
            {
                var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                    (dr, qd, cd) =>
                    {
                        dr.QuoteDetails = qd;
                        dr.ClaimDetails = cd;
                        return dr;
                    },
                    splitOn: "Id, Id").ToList();

                results.Read<Asset, Guid, int, Asset>(
                    (asset, releaseDetailsId, isPublicInt) =>
                    {
                        var isPublic = isPublicInt == 1;
                        var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                            .FirstOrDefault(rd => rd?.Id == releaseDetailsId);
                        if (releaseDetails != null)
                        {
                            if (isPublic)
                            {
                                releaseDetails.Assets.Add(asset);
                            }
                            else
                            {
                                releaseDetails.Files.Add(asset);
                            }
                        }
                        return asset;
                    },
                    splitOn: "ReleaseDetailsId, IsPublic").ToList();

                var devRelease = devReleases.FirstOrDefault();
                return devRelease;
            }
        }
    }

    public Instant? GetLastModifiedTimestamp(Guid tenantId, Guid productReleaseId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.GetLastModifiedTimestamp)}"))
        {
            const string query = @"
                SELECT TOP 1 LastModifiedTicksSinceEpoch
                FROM DevReleases
                WHERE TenantId = @TenantId AND Id = @ProductReleaseId
                ORDER BY LastModifiedTicksSinceEpoch DESC";
            var connection = this.dbContext.Database.Connection;
            var lastModifiedTicksSinceEpoch =
                connection.QuerySingleOrDefault<long>(query, new { TenantId = tenantId, ProductReleaseId = productReleaseId });
            return Instant.FromUnixTimeTicks(lastModifiedTicksSinceEpoch);
        }
    }

    /// <inheritdoc/>
    public void Insert(DevRelease devRelease)
    {
        this.dbContext.DevReleases.Add(devRelease);
    }

    /// <inheritdoc/>
    public void SaveChanges()
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.SaveChanges)}"))
        {
            this.dbContext.SaveChanges();
        }
    }

    public async Task SaveChanges(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.SaveChanges)}"))
        {
            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                this.dbContext.TransactionStack.Push(transaction);
                try
                {
                    if (changeTracker.IsDevReleaseNew)
                    {
                        await this.InsertReleaseDetails(changeTracker);
                        await this.InsertDevRelease(changeTracker);
                    }
                    else if (changeTracker.IsReleaseDetailsNew)
                    {
                        await this.InsertReleaseDetails(changeTracker);
                        await this.UpdateDevRelease(changeTracker);
                    }
                    else
                    {
                        await this.SavePropertyChanges(changeTracker);
                    }

                    if (changeTracker.RemovedPrivateFiles.Any() || changeTracker.RemovedPublicFiles.Any())
                    {
                        await this.RemoveFiles(changeTracker);
                    }

                    if (changeTracker.AddedPrivateFiles.Any())
                    {
                        await this.AddAssetFiles(changeTracker.AddedPrivateFiles, FileVisibility.Private, changeTracker.ReleaseDetails.Id);
                    }

                    if (changeTracker.AddedPublicFiles.Any())
                    {
                        await this.AddAssetFiles(changeTracker.AddedPublicFiles, FileVisibility.Public, changeTracker.ReleaseDetails.Id);
                    }

                    await this.UpdateLastModifiedTimestamp(changeTracker);

                    transaction.Complete();
                }
                finally
                {
                    this.dbContext.TransactionStack.Pop();
                }
            }
        }
    }

    private async Task UpdateLastModifiedTimestamp(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.UpdateLastModifiedTimestamp)}"))
        {
            var parameters = new DynamicParameters();
            string sqlUpdate = "UPDATE [dbo].[DevReleases] SET [LastModifiedTicksSinceEpoch] = @LastModifiedTicksSinceEpoch WHERE [Id] = @Id";
            parameters.Add("LastModifiedTicksSinceEpoch", changeTracker.DevRelease.LastModifiedTicksSinceEpoch);
            parameters.Add("Id", changeTracker.DevRelease.Id);
            await this.dbContext.Database.Connection.ExecuteAsync(sqlUpdate, parameters);
        }
    }

    private async Task AddAssetFiles(List<Asset> assetFiles, FileVisibility fileVisibility, Guid releaseDetailsId)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.AddAssetFiles)} ({fileVisibility.Humanize()})"))
        {
            string releaseDetailsForeignKeyColumnName = fileVisibility == FileVisibility.Private ? "ReleaseDetails_Id1" : "ReleaseDetails_Id";

            // This flag lets us know if we need to call dbContext.SaveChanges() at the end of this method.
            bool dbContextChanged = false;

            var sqlInsert = string.Empty;
            var parameters = new DynamicParameters();
            Dictionary<string, Guid> fileContentDictionary = new Dictionary<string, Guid>();
            foreach (var file in assetFiles)
            {
                Guid fileContentId;

                // add the file contents, making sure to use de-duplication
                if (fileContentDictionary.ContainsKey(file.FileContent.HashCode))
                {
                    fileContentId = fileContentDictionary[file.FileContent.HashCode];
                }
                else
                {
                    fileContentId = this.fileContentRepository.Insert(file.FileContent);
                    fileContentDictionary.Add(file.FileContent.HashCode, fileContentId);
                }

                // mark if we need to call dbContext.SaveChanges() at the end of this method
                bool fileContentsAlreadyExists = fileContentId != file.FileContent.Id;
                dbContextChanged = dbContextChanged || !fileContentsAlreadyExists;

                var fileId = file.Id.ToString().Replace("-", string.Empty);
                sqlInsert += "INSERT INTO [dbo].[Assets] ("
                    + $"[Id], [Name], [CreatedTicksSinceEpoch], [{releaseDetailsForeignKeyColumnName}], [FileContentId], [FileModifiedTicksSinceEpoch]) VALUES "
                    + $"(@Id{fileId}, @Name{fileId}, @CreatedTicksSinceEpoch{fileId}, @ReleaseDetailsId, "
                    + $"@FileContentId{fileId}, @FileModifiedTicksSinceEpoch{fileId}), ";
                parameters.Add($"Id{fileId}", file.Id);
                parameters.Add($"Name{fileId}", file.Name);
                parameters.Add($"CreatedTicksSinceEpoch{fileId}", file.CreatedTicksSinceEpoch);
                parameters.Add($"FileContentId{fileId}", fileContentId);
                parameters.Add($"FileModifiedTicksSinceEpoch{fileId}", file.FileModifiedTicksSinceEpoch);
                sqlInsert = sqlInsert.TrimEnd(',', ' ');
                sqlInsert += ";";
            }

            if (dbContextChanged)
            {
                // save the inserted FileContents
                await this.dbContext.SaveChangesAsync();
            }

            parameters.Add($"ReleaseDetailsId", releaseDetailsId);
            await this.dbContext.Database.Connection.ExecuteAsync(sqlInsert, parameters);
        }
    }

    private async Task SavePropertyChanges(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.SavePropertyChanges)}"))
        {
            var parameters = new DynamicParameters();
            var sqlUpdate = "UPDATE [dbo].[ReleaseDetails] SET [LastSynchronisedTicksSinceEpoch] = @LastSynchronisedTicksSinceEpoch, ";
            parameters.Add("LastSynchronisedTicksSinceEpoch", changeTracker.ReleaseDetails.LastSynchronisedTicksSinceEpoch);

            if (changeTracker.HasFormConfigurationJsonChanged)
            {
                sqlUpdate += "[ConfigurationJson] = @FormConfigurationJson, ";
                sqlUpdate += "[FormConfigurationJsonLastModifiedTicksSinceEpoch] = @FormConfigurationJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("FormConfigurationJson", changeTracker.ReleaseDetails.ConfigurationJson);
                parameters.Add(
                    "FormConfigurationJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.FormConfigurationJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasWorkflowJsonChanged)
            {
                sqlUpdate += "[WorkflowJson] = @WorkflowJson, ";
                sqlUpdate += "[WorkflowJsonLastModifiedTicksSinceEpoch] = @WorkflowJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("WorkflowJson", changeTracker.ReleaseDetails.WorkflowJson);
                parameters.Add(
                    "WorkflowJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.WorkflowJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasIntegrationsJsonChanged)
            {
                sqlUpdate += "[ExportsJson] = @IntegrationsJson, ";
                sqlUpdate += "[IntegrationsJsonLastModifiedTicksSinceEpoch] = @IntegrationsJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("IntegrationsJson", changeTracker.ReleaseDetails.ExportsJson);
                parameters.Add(
                    "IntegrationsJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.IntegrationsJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasAutomationsJsonChanged)
            {
                sqlUpdate += "[AutomationsJson] = @AutomationsJson, ";
                sqlUpdate += "[AutomationsJsonLastModifiedTicksSinceEpoch] = @AutomationsJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("AutomationsJson", changeTracker.ReleaseDetails.AutomationsJson);
                parameters.Add(
                    "AutomationsJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.AutomationsJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasPaymentJsonChanged)
            {
                sqlUpdate += "[PaymentJson] = @PaymentJson, ";
                sqlUpdate += "[PaymentJsonLastModifiedTicksSinceEpoch] = @PaymentJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("PaymentJson", changeTracker.ReleaseDetails.PaymentJson);
                parameters.Add(
                    "PaymentJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.PaymentJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasPaymentFormJsonChanged)
            {
                sqlUpdate += "[PaymentFormJson] = @PaymentFormJson, ";
                sqlUpdate += "[PaymentFormJsonLastModifiedTicksSinceEpoch] = @PaymentFormJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("PaymentFormJson", changeTracker.ReleaseDetails.PaymentFormJson);
                parameters.Add(
                    "PaymentFormJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.PaymentFormJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasFundingJsonChanged)
            {
                sqlUpdate += "[FundingJson] = @FundingJson, ";
                sqlUpdate += "[FundingJsonLastModifiedTicksSinceEpoch] = @FundingJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("FundingJson", changeTracker.ReleaseDetails.FundingJson);
                parameters.Add(
                    "FundingJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.FundingJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasProductJsonChanged)
            {
                sqlUpdate += "[ProductJson] = @ProductJson, ";
                sqlUpdate += "[ProductJsonLastModifiedTicksSinceEpoch] = @ProductJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("ProductJson", changeTracker.ReleaseDetails.ProductJson);
                parameters.Add(
                    "ProductJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.ProductJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.HasSpreadsheetChanged)
            {
                sqlUpdate += "[FlexCelWorkbook] = @SpreadsheetBytes, ";
                sqlUpdate += "[SpreadsheetLastModifiedTicksSinceEpoch] = @SpreadsheetLastModifiedTicksSinceEpoch, ";
                parameters.Add("SpreadsheetBytes", changeTracker.ReleaseDetails.FlexCelWorkbook, DbType.Binary);
                parameters.Add(
                    "SpreadsheetLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.SpreadsheetLastModifiedTicksSinceEpoch);
            }

            sqlUpdate = sqlUpdate.TrimEnd(',', ' ');

            // TODO: add TenantId to the ReleaseDetails table
            // sqlUpdate += " WHERE [TenantId] = @TenantId AND [Id] = @Id;";
            sqlUpdate += " WHERE [Id] = @Id;";
            parameters.Add("TenantId", changeTracker.DevRelease.TenantId);
            parameters.Add("Id", changeTracker.ReleaseDetails.Id);
            await this.dbContext.Database.Connection.ExecuteAsync(sqlUpdate, parameters);
        }
    }

    private async Task RemoveFiles(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.RemoveFiles)}"))
        {
            // TODO: decrement the reference count of FileContent (UB-10238)
            var ids = changeTracker.RemovedPrivateFiles.Select(f => f.Id).Concat(changeTracker.RemovedPublicFiles.Select(f => f.Id)).ToArray();

            // TODO: add TenantId to the Assets table
            // var sqlDelete = "DELETE FROM [dbo].[Assets] WHERE [TenantId] = @TenantId AND [Id] IN @Ids;";
            var sqlDelete = "DELETE FROM [dbo].[Assets] WHERE [Id] IN @Ids;";

            await this.dbContext.Database.Connection.ExecuteAsync(
                sqlDelete,
                new
                {
                    changeTracker.DevRelease.TenantId,
                    Ids = ids,
                });
        }
    }

    private async Task InsertReleaseDetails(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.InsertReleaseDetails)}"))
        {
            var parameters = new DynamicParameters();

            // TODO: add TenantId to the ReleaseDetails table
            var columnNamesSql = "[Id], [CreatedTicksSinceEpoch], [LastSynchronisedTicksSinceEpoch], [AppType], ";

            // TODO: add TenantId to the ReleaseDetails table
            var columnValuesSql = "@Id, @CreatedTicksSinceEpoch, @LastSynchronisedTicksSinceEpoch, @AppType, ";
            parameters.Add("Id", changeTracker.ReleaseDetails.Id);
            parameters.Add("TenantId", changeTracker.DevRelease.TenantId);
            parameters.Add("CreatedTicksSinceEpoch", changeTracker.ReleaseDetails.CreatedTicksSinceEpoch);
            parameters.Add("LastSynchronisedTicksSinceEpoch", changeTracker.ReleaseDetails.LastSynchronisedTicksSinceEpoch);
            parameters.Add("AppType", changeTracker.ReleaseDetails.AppType);

            if (changeTracker.ReleaseDetails.ConfigurationJson != null)
            {
                columnNamesSql += "[ConfigurationJson], [FormConfigurationJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@FormConfigurationJson, @FormConfigurationJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("FormConfigurationJson", changeTracker.ReleaseDetails.ConfigurationJson);
                parameters.Add(
                    "FormConfigurationJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.FormConfigurationJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.WorkflowJson != null)
            {
                columnNamesSql += "[WorkflowJson], [WorkflowJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@WorkflowJson, @WorkflowJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("WorkflowJson", changeTracker.ReleaseDetails.WorkflowJson);
                parameters.Add(
                    "WorkflowJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.WorkflowJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.ExportsJson != null)
            {
                columnNamesSql += "[ExportsJson], [IntegrationsJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@IntegrationsJson, @IntegrationsJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("IntegrationsJson", changeTracker.ReleaseDetails.ExportsJson);
                parameters.Add(
                    "IntegrationsJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.IntegrationsJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.AutomationsJson != null)
            {
                columnNamesSql += "[AutomationsJson], [AutomationsJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@AutomationsJson, @AutomationsJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("AutomationsJson", changeTracker.ReleaseDetails.AutomationsJson);
                parameters.Add(
                    "AutomationsJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.AutomationsJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.PaymentJson != null)
            {
                columnNamesSql += "[PaymentJson], [PaymentJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@PaymentJson, @PaymentJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("PaymentJson", changeTracker.ReleaseDetails.PaymentJson);
                parameters.Add(
                    "PaymentJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.PaymentJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.PaymentFormJson != null)
            {
                columnNamesSql += "[PaymentFormJson], [PaymentFormJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@PaymentFormJson, @PaymentFormJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("PaymentFormJson", changeTracker.ReleaseDetails.PaymentFormJson);
                parameters.Add(
                    "PaymentFormJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.PaymentFormJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.FundingJson != null)
            {
                columnNamesSql += "[FundingJson], [FundingJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@FundingJson, @FundingJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("FundingJson", changeTracker.ReleaseDetails.FundingJson);
                parameters.Add(
                    "FundingJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.FundingJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.ProductJson != null)
            {
                columnNamesSql += "[ProductJson], [ProductJsonLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@ProductJson, @ProductJsonLastModifiedTicksSinceEpoch, ";
                parameters.Add("ProductJson", changeTracker.ReleaseDetails.ProductJson);
                parameters.Add(
                    "ProductJsonLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.ProductJsonLastModifiedTicksSinceEpoch);
            }

            if (changeTracker.ReleaseDetails.FlexCelWorkbook != null)
            {
                columnNamesSql += "[FlexCelWorkbook], [SpreadsheetLastModifiedTicksSinceEpoch], ";
                columnValuesSql += "@SpreadsheetBytes, @SpreadsheetLastModifiedTicksSinceEpoch, ";
                parameters.Add("SpreadsheetBytes", changeTracker.ReleaseDetails.FlexCelWorkbook, DbType.Binary);
                parameters.Add(
                    "SpreadsheetLastModifiedTicksSinceEpoch",
                    changeTracker.ReleaseDetails.SpreadsheetLastModifiedTicksSinceEpoch);
            }

            columnNamesSql = columnNamesSql.TrimEnd(',', ' ');
            columnValuesSql = columnValuesSql.TrimEnd(',', ' ');
            var sqlInsert = "INSERT INTO [dbo].[ReleaseDetails] (" + columnNamesSql + ") VALUES (" + columnValuesSql + ");";
            await this.dbContext.Database.Connection.ExecuteAsync(sqlInsert, parameters);
        }
    }

    private async Task InsertDevRelease(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.InsertDevRelease)}"))
        {
            var parameters = new DynamicParameters();
            var columnNamesSql = "[Id], [TenantId], [ProductId], [CreatedTicksSinceEpoch], ";
            var columnValuesSql = "@Id, @TenantId, @ProductId, @CreatedTicksSinceEpoch, ";
            parameters.Add("Id", changeTracker.DevRelease.Id);
            parameters.Add("TenantId", changeTracker.DevRelease.TenantId);
            parameters.Add("ProductId", changeTracker.DevRelease.ProductId);
            parameters.Add("CreatedTicksSinceEpoch", changeTracker.DevRelease.CreatedTicksSinceEpoch);
            switch (changeTracker.ComponentType)
            {
                case WebFormAppType.Quote:
                    columnNamesSql += "[QuoteDetails_Id], ";
                    columnValuesSql += "@ReleaseDetailsId, ";
                    break;
                case WebFormAppType.Claim:
                    columnNamesSql += "[ClaimDetails_Id], ";
                    columnValuesSql += "@ReleaseDetailsId, ";
                    break;
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(changeTracker.ComponentType, typeof(WebFormAppType)));
            }

            columnNamesSql = columnNamesSql.TrimEnd(',', ' ');
            columnValuesSql = columnValuesSql.TrimEnd(',', ' ');
            parameters.Add("ReleaseDetailsId", changeTracker.ReleaseDetails.Id);
            var sqlInsert = "INSERT INTO [dbo].[DevReleases] (" + columnNamesSql + ") VALUES (" + columnValuesSql + ");";
            await this.dbContext.Database.Connection.ExecuteAsync(sqlInsert, parameters);
        }
    }

    private async Task UpdateDevRelease(ReleaseDetailsChangeTracker changeTracker)
    {
        using (MiniProfiler.Current.Step($"{nameof(DevReleaseRepository)}.{nameof(this.UpdateDevRelease)}"))
        {
            if (!changeTracker.IsReleaseDetailsNew)
            {
                throw new InvalidOperationException("You should not call UpdateDevRelease unless we are adding "
                    + " a new ReleaseDetails.");
            }
            var parameters = new DynamicParameters();
            string sqlUpdate = "UPDATE [dbo].[DevReleases] SET ";
            if (changeTracker.ComponentType == WebFormAppType.Quote)
            {
                sqlUpdate += "[QuoteDetails_Id] = @QuoteDetailsId ";
                parameters.Add("QuoteDetailsId", changeTracker.ReleaseDetails.Id);
            }
            else if (changeTracker.ComponentType == WebFormAppType.Claim)
            {
                sqlUpdate += "[ClaimDetails_Id] = @ClaimDetailsId ";
                parameters.Add("ClaimDetailsId", changeTracker.ReleaseDetails.Id);
            }
            else
            {
                throw new ErrorException(Errors.General.UnexpectedEnumValue(changeTracker.ComponentType, typeof(WebFormAppType)));
            }

            sqlUpdate += " WHERE [Id] = @Id;";
            parameters.Add("Id", changeTracker.DevRelease.Id);
            await this.dbContext.Database.Connection.ExecuteAsync(sqlUpdate, parameters);
        }
    }
}
