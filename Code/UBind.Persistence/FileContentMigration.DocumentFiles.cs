// <copyright file="FileContentMigration.DocumentFiles.cs" company="uBind">
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
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services.Migration;

    /// <summary>
    /// File content migration for document files.
    /// </summary>
    public partial class FileContentMigration : IFileContentMigration
    {
        private readonly string tmpFileContentDocumentFilesTable = "tmpFileContentDocumentFilesTable";
        private string dbTableName = "DocumentFiles";

        public void PopulateFileContentsFromDocumentFiles()
        {
            if (typeof(DocumentFile).GetProperty("Content") == null)
            {
                // Contents already copied over to file contents table.
                this.logger.LogInformation($"Migration for document file contents DONE.");
                return;
            }

            // Release 1. Populate file contents from document files.
            this.logger.LogInformation($"Started {DateTime.Now}");

            var dbTimeout = this.dbContext.Database.CommandTimeout;

            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;

                this.logger.LogInformation($"Populating temp table from {this.dbTableName} contents to process...");
                string selectColumns =
                    "SELECT [Id], 0 AS [IsProcessed], CAST(null as uniqueidentifier) AS FileContentId, CAST(null as BIGINT) AS FileSize ";
                string tmpTableSql;

                if (!this.TableExists(this.tmpFileContentDocumentFilesTable))
                {
                    tmpTableSql =
                        $"{selectColumns} INTO {this.tmpFileContentDocumentFilesTable} " +
                        $"FROM {this.dbTableName} WITH (NOLOCK) ";
                }
                else
                {
                    tmpTableSql = $"INSERT INTO {this.tmpFileContentDocumentFilesTable} " +
                        $"{selectColumns} FROM {this.dbTableName} a WITH(NOLOCK) " +
                        $"WHERE NOT EXISTS(SELECT 1 FROM {this.tmpFileContentDocumentFilesTable} t WHERE t.Id = a.Id)";
                }

                this.dbContext.Database.ExecuteSqlCommand(tmpTableSql);

                this.backgroundJobClient.Enqueue(() => this.ProcessDocumentFileBatch(1, null));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = dbTimeout;
            }
        }

        public void CleanupDocumentFileContents()
        {
            if (!this.CheckIfColumnOnTableExists("DocumentFiles", "Content"))
            {
                // Contents already copied over to file contents table.
                this.logger.LogInformation($"Migration for document file contents DONE.");
                return;
            }

            // Release 1. Populate file contents from document files.
            this.logger.LogInformation($"Started {DateTime.Now}");
            var dbTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                this.logger.LogInformation($"Populating temp table from {this.dbTableName} contents to process...");
                string selectColumns =
                    "SELECT a.[Id], 0 AS [IsProcessed], CAST(null as uniqueidentifier) AS FileContentId, CAST(null as BIGINT) AS FileSize ";
                string tmpTableSql;
                if (!this.TableExists(this.tmpFileContentDocumentFilesTable))
                {
                    tmpTableSql =
                        $"{selectColumns} INTO {this.tmpFileContentDocumentFilesTable} " +
                        $"FROM {this.dbTableName} a WITH (NOLOCK) WHERE a.FileContentId = '00000000-0000-0000-0000-000000000000' ";
                }
                else
                {
                    tmpTableSql = $"INSERT INTO {this.tmpFileContentDocumentFilesTable} " +
                        $"{selectColumns} FROM {this.dbTableName} a WITH (NOLOCK) WHERE a.FileContentId = '00000000-0000-0000-0000-000000000000' " +
                        $"AND NOT EXISTS(SELECT 1 FROM {this.tmpFileContentDocumentFilesTable} t WHERE t.Id = a.Id)";
                }

                this.dbContext.Database.ExecuteSqlCommand(tmpTableSql);
                this.backgroundJobClient.Enqueue(() => this.ProcessDocumentFileBatch(1, null));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = dbTimeout;
            }
        }

        [JobDisplayName("Startup Job: PopulateFileContentsFromDocumentFiles Process Batch {0}")]
        public void ProcessDocumentFileBatch(int batch, PerformContext context)
        {
            const int batchSize = 5000;
            var sqlCount = $"SELECT COUNT(*) FROM {this.tmpFileContentDocumentFilesTable} WITH (NOLOCK) WHERE IsProcessed=0";
            var sqlBatch = $"SELECT TOP {batchSize} Id FROM {this.tmpFileContentDocumentFilesTable} WITH (NOLOCK) WHERE IsProcessed=0";

            var unprocessedRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            while (unprocessedRows > 0)
            {
                this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {unprocessedRows}, Batch: {batch}, Size: {batchSize}");

                var fileContents = this.dbContext.Database.SqlQuery<Guid>(sqlBatch).ToList();
                this.ProcessFileContentsFromDocumentFiles(fileContents);
                this.logger.LogInformation($"Batch: {batch}, {sqlCount} files migrated");
                batch++;
                Thread.Sleep(200);
                unprocessedRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();

                if (unprocessedRows > 0 && this.DbLogNeedsManualCleanup())
                {
                    // Enqueue the job for 4hrs so we have time to manually cleanup the DB logs
                    this.logger.LogInformation("DB log has reached the threshold, job enqueued for 4hrs to enable manual shrinking of DB log");
                    this.backgroundJobClient.Schedule(() => this.ProcessDocumentFileBatch(batch, null), TimeSpan.FromHours(4));
                    break;
                }
            }

            var sqlCountOrphanedRows =
                "SELECT COUNT(1) FROM EmailAttachments e INNER JOIN DocumentFiles d ON d.Id = e.DocumentFile_Id WHERE d.FileContentId='00000000-0000-0000-0000-000000000000'";
            var orphanedRows = this.dbContext.Database.SqlQuery<int>(sqlCountOrphanedRows).Single();
            if (orphanedRows == 0)
            {
                this.dbContext.Database.ExecuteSqlCommand("ALTER TABLE dbo.DocumentFiles DROP COLUMN IF EXISTS[Content];" +
                    $"DROP TABLE IF EXISTS {this.tmpFileContentDocumentFilesTable};");
                this.logger.LogInformation("DONE!");
            }
        }

        private static Guid GetTenantIdForDocumentFile(UBindDbContext jobContext, Guid documentFileId)
        {
            string sql = $@"SELECT TenantId FROM Emails e
INNER JOIN EmailAttachments a ON a.EmailId = e.Id
WHERE a.DocumentFile_Id='{documentFileId}'";

            Guid tenantId = jobContext.Database.SqlQuery<Guid>(sql).FirstOrDefault();
            return tenantId;
        }

        private void ProcessFileContentsFromDocumentFiles(List<Guid> fileContents)
        {
            int total = fileContents.Count;
            this.logger.LogInformation($"Retrieved {total} document files {DateTime.Now}");

            int ctr = 0;
            foreach (var id in fileContents)
            {
                ctr++;
                var jobContext = new UBindDbContext(this.connectionString);
                jobContext.Configuration.AutoDetectChangesEnabled = false;

                try
                {
                    var file = jobContext.DocumentFile.AsNoTracking()
                        .FirstOrDefault(a => a.Id == id);
                    if (file == null)
                    {
                        continue;
                    }

                    var tenantId = GetTenantIdForDocumentFile(jobContext, file.Id);

                    // Make sure it's backward compatible by getting the Content column directly from DB.
                    byte[] content = jobContext.Database.SqlQuery<byte[]>($"SELECT Content FROM DocumentFiles WHERE Id='{id}'").First();
                    var fileContent = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), content);
                    var fileContentId = this.UpsertFileContent(fileContent, jobContext);

                    // Update DocumentFiles.FileContentId in relation to FileContents table
                    jobContext.Database.ExecuteSqlCommand(
                        $"UPDATE {this.dbTableName} SET FileContentId='{fileContentId}' WHERE Id='{id}'");

                    var updateFileInfo =
                        $"UPDATE {this.tmpFileContentDocumentFilesTable} SET IsProcessed=1, FileContentId='{fileContentId}' " +
                        $"WHERE Id='{id}'";
                    jobContext.Database.ExecuteSqlCommand(updateFileInfo);
                    jobContext.SaveChanges();
                    Thread.Sleep(100);

                    if (ctr % 1000 == 0)
                    {
                        this.logger.LogInformation($"({ctr}/{total}) files migrated");
                    }

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
                        $"UPDATE {this.tmpFileContentDocumentFilesTable} SET IsProcessed=-1 WHERE Id='{id}'");
                    this.logger.LogError(ex, ex.Message);
                }
                finally
                {
                    jobContext.Configuration.AutoDetectChangesEnabled = true;
                }
            }
        }

        private bool DbLogNeedsManualCleanup()
        {
            // Check db log size if threshold of 35GB is reached so we can cleanup the DB log
            var logSize = this.GetLogSize();
            this.logger.LogInformation($"DB log size: {logSize}");
            const decimal logSizeThreshold = 35000m;
            return logSize > logSizeThreshold;
        }

        private bool CheckIfColumnOnTableExists(string table, string column)
        {
            var result = this.dbContext.Database.SqlQuery<int>($@"SELECT Count(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = '{table}'
            AND COLUMN_NAME = '{column}'").Single();
            return result == 1;
        }
    }
}
