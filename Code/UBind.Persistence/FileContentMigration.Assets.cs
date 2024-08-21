// <copyright file="FileContentMigration.Assets.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services.Migration;

    public partial class FileContentMigration : IFileContentMigration
    {
        private readonly string tmpAssetFileContentsTable = "TmpAssetFileContents";

        public void PopulateAssetFileContents()
        {
            if (typeof(Asset).GetProperty("Content") == null)
            {
                // Contents already copied over to file contents table.
                this.logger.LogInformation($"Migration for asset file contents DONE.");
                return;
            }

            this.backgroundJobClient.Enqueue(() => this.UpdateFileContentsTenantId());

            // Release 1. Populate file contents from assets.
            this.logger.LogInformation($"Started {DateTime.Now}");

            var dbTimeout = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = NoTimeout;

            this.logger.LogInformation($"Populating temp table with asset file contents to process...");
            string selectColumns =
                "SELECT [Id], 0 AS [IsProcessed], CAST(null as uniqueidentifier) AS FileContentId, CAST(null as BIGINT) AS FileSize ";
            string tmpTableSql;

            if (!this.TableExists(this.tmpAssetFileContentsTable))
            {
                tmpTableSql =
                    $"{selectColumns} INTO {this.tmpAssetFileContentsTable} " +
                    "FROM Assets WITH (NOLOCK) ";
            }
            else
            {
                tmpTableSql = $"INSERT INTO {this.tmpAssetFileContentsTable} " +
                    $"{selectColumns} FROM Assets a WITH(NOLOCK) " +
                    $"WHERE NOT EXISTS(SELECT 1 FROM {this.tmpAssetFileContentsTable} t WHERE t.Id = a.Id)";
            }

            this.dbContext.Database.ExecuteSqlCommand(tmpTableSql);
            this.dbContext.Database.CommandTimeout = dbTimeout;

            this.backgroundJobClient.Enqueue(() => this.ProcessAssetBatch(1));
        }

        [JobDisplayName("Startup Job: PopulateAssetFileContents Update FileContents TenantId")]
        public void UpdateFileContentsTenantId()
        {
            // Update FileContents.TenantId
            string[] tables = { "QuoteFileAttachments", "ClaimFileAttachments", "QuoteDocumentReadModels" };
            foreach (var table in tables)
            {
                var sql = $@"UPDATE fc 
SET fc.TenantId = fa.TenantId
FROM Filecontents fc
INNER JOIN {table} fa ON fc.Id = fa.FileContentId
WHERE fc.TenantId IS NULL OR fc.TenantId='{Guid.Empty}'";
                this.logger.LogInformation($"Update {table} TenantId");
                this.dbContext.Database.ExecuteSqlCommand(sql);
            }
        }

        [JobDisplayName("Startup Job: PopulateAssetFileContents Process Batch {0}")]
        public void ProcessAssetBatch(int batch)
        {
            const int batchSize = 5000;

            var sqlCount = $"SELECT COUNT(*) FROM {this.tmpAssetFileContentsTable} WITH (NOLOCK) WHERE IsProcessed=0";
            var totalRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");

            var sqlBatch = $"SELECT TOP {batchSize} Id FROM {this.tmpAssetFileContentsTable} WITH (NOLOCK) WHERE IsProcessed=0";

            var assetFileContents = this.dbContext.Database.SqlQuery<Guid>(sqlBatch).ToList();

            this.ProcessAssetFileContents(assetFileContents);

            if (assetFileContents.Count == batchSize)
            {
                batch++;
                this.backgroundJobClient.Enqueue(() => this.ProcessAssetBatch(batch));
            }
        }

        private static Guid GetTenantId(UBindDbContext jobContext, Guid assetId)
        {
            string baseSql = @"SELECT dr.TenantId FROM Assets a
INNER JOIN ReleaseEvents re ON a.ReleaseDetails_Id IN(re.QuoteDetails_Id, re.ClaimDetails_Id)
    OR a.ReleaseDetails_Id1 IN(re.QuoteDetails_Id, re.ClaimDetails_Id)
{0} WHERE a.Id = '{1}'";

            string[] releases =
                {
                    "INNER JOIN DevReleases dr ON dr.Id = re.DevRelease_Id",
                    "INNER JOIN Releases dr ON dr.Id=re.Release_Id",
                    "INNER JOIN Deployments dr ON dr.Id=re.Release_Id",
                };

            foreach (var join in releases)
            {
                var sql = string.Format(baseSql, join, assetId);
                Guid tenantId = jobContext.Database.SqlQuery<Guid>(sql).FirstOrDefault();
                if (tenantId != Guid.Empty)
                {
                    return tenantId;
                }
            }

            return Guid.Empty;
        }

        private void ProcessAssetFileContents(List<Guid> assetFileContents)
        {
            int total = assetFileContents.Count;
            this.logger.LogInformation($"Retrieved {total} assets {DateTime.Now}");

            int ctr = 1;
            foreach (var id in assetFileContents)
            {
                var jobContext = new UBindDbContext(this.connectionString);
                jobContext.Configuration.AutoDetectChangesEnabled = false;

                this.logger.LogInformation(
                    $"PROCESSING ({ctr++}/{total}) Id='{id}'");
                try
                {
                    var asset = jobContext.Assets.AsNoTracking()
                        .FirstOrDefault(a => a.Id == id);
                    if (asset == null)
                    {
                        continue;
                    }

                    var tenantId = GetTenantId(jobContext, asset.Id);

                    var fileContent = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), asset.FileContent.Content);
                    var fileContentId = this.UpsertFileContent(fileContent, jobContext);
                    this.logger.LogInformation($"FileContent updated: {fileContentId}");

                    // Update Assets.FileContentId in relation to FileContents table
                    jobContext.Database.ExecuteSqlCommand(
                        $"UPDATE Assets SET FileContentId='{fileContentId}' WHERE Id='{id}'");

                    var updateFileInfo =
                        $"UPDATE {this.tmpAssetFileContentsTable} SET IsProcessed=1, FileContentId='{fileContentId}' " +
                        $"WHERE Id='{id}'";
                    jobContext.Database.ExecuteSqlCommand(updateFileInfo);

                    jobContext.SaveChanges();

                    Thread.Sleep(1000);
                    if (ctr % 100 == 0)
                    {
                        this.MemoryCleanup();
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    Thread.Sleep(1000);
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                    jobContext.Database.ExecuteSqlCommand(
                        $"UPDATE {this.tmpAssetFileContentsTable} SET IsProcessed=-1 WHERE Id='{id}'");
                    this.logger.LogError(ex, ex.Message);
                }
                finally
                {
                    jobContext.Configuration.AutoDetectChangesEnabled = true;
                }
            }
        }
    }
}
