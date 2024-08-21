// <copyright file="RemoveDuplicateFileContentsMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations;

using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Runtime;
using System.Threading;
using UBind.Domain.Helpers;
using UBind.Domain.Repositories;
using UBind.Domain.Services.Maintenance;
using UBind.Domain.Services.Migration;

/// <summary>
/// This migration will remove these duplicates and update other tables references to the correct file contents.
/// Some code introduced duplicate file contents in the database.
/// </summary>
public class RemoveDuplicateFileContentsMigration : IRemoveDuplicateFileContentsMigration
{
    private const int BatchSize = 1000;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<RemoveDuplicateFileContentsMigration> logger;

    public RemoveDuplicateFileContentsMigration(
        IServiceProvider serviceProvider,
        ILogger<RemoveDuplicateFileContentsMigration> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public async Task ProcessRemovingDuplicateFileContents(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Migration started for removing duplicate file contents.");
        await RetryPolicyHelper.ExecuteAsync<Exception>((ct) => this.ProcessBatchUpdate(ct), maxJitter: 1500, cancellationToken: cancellationToken);
    }

    [JobDisplayName("Startup Job: Process Removing Duplicate File Contents")]
    public async Task ProcessBatchUpdate(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            int batch = 1;
            while (true)
            {
                FileContentDuplicateCount duplicateCount = await this.GetFileContentDuplicateCount();
                this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {duplicateCount.TotalRowsWithDuplicates}, Total Occurences: {duplicateCount.TotalOccurrences}, Batch: {batch}, Size: {BatchSize}");
                if (duplicateCount.TotalRowsWithDuplicates == 0 && duplicateCount.TotalOccurrences == 0)
                {
                    break;
                }

                List<FileContent> fileContents = await this.UpdateRelatedTableAndDeleteDuplicateFileContents(cancellationToken);

                this.MemoryCleanup();
                await this.ShrinkOrCleanDBLogFile(cancellationToken);

                if (fileContents.Count < BatchSize)
                {
                    break;
                }

                batch++;
                await Task.Delay(500, cancellationToken);
            }

            this.MemoryCleanup();
            await this.ShrinkOrCleanDBLogFile(cancellationToken);
            this.logger.LogInformation($"Migration completed for removing duplicate file contents.");
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("Operation was cancelled.");
            throw;
        }
    }

    private async Task ShrinkOrCleanDBLogFile(CancellationToken cancellationToken)
    {
        using (var scope = this.serviceProvider.CreateScope())
        {
            var dbLogFileMaintenanceService = scope.ServiceProvider.GetRequiredService<IDbLogFileMaintenanceService>();
            await dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
        }
    }

    private async Task<List<FileContent>> UpdateRelatedTableAndDeleteDuplicateFileContents(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<FileContent> fileContents = await this.GetFileContents();
        int totalDeleted = 0;
        int totalUpdated = 0;
        foreach (var fileContent in fileContents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileContentIds = await this.GetFileContentIds(fileContent, cancellationToken);
            Guid fileContentIdToUpdate = fileContentIds.First();
            foreach (var fileContentId in fileContentIds.Skip(1))
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    totalUpdated += await this.UpdateAllTablesRelatedToFileContent(fileContentId, fileContentIdToUpdate, cancellationToken);
                    await this.DeleteFileContentId(fileContentId, cancellationToken);
                    totalDeleted++;
                }
                catch (SqlException ex) when (ex.Message.Contains("Timeout expired"))
                {
                    this.logger.LogWarning("A SQL timeout occurred during updating tables related to file content.");
                }
                catch (OutOfMemoryException ex)
                {
                    this.MemoryCleanup();
                    await Task.Delay(1000, cancellationToken);
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }

                if (totalDeleted % 500 == 0)
                {
                    this.logger.LogInformation($"Total updated records related to file content: {totalUpdated}");
                    this.logger.LogInformation($"Total deleted file content: {totalDeleted}");
                }

                if (totalDeleted % 250 == 0)
                {
                    this.MemoryCleanup();
                    await this.ShrinkOrCleanDBLogFile(cancellationToken);
                }

                await Task.Delay(500, cancellationToken);
            }
        }

        return fileContents;
    }

    private async Task<int> DeleteFileContentId(Guid fileContentId, CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            return await dbContext.Database.ExecuteSqlCommandAsync(
                System.Data.Entity.TransactionalBehavior.EnsureTransaction,
                @"DELETE FROM FileContents WHERE Id = @Id",
                cancellationToken,
                new SqlParameter("@Id", fileContentId));
        }
    }

    private async Task<List<Guid>> GetFileContentIds(FileContent fileContent, CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            var parameters = new[]
                {
                    new SqlParameter("@TenantId", fileContent.TenantId),
                    new SqlParameter("@HashCode", fileContent.HashCode),
                };

            var fileContentIds = await dbContext.Database.SqlQuery<Guid>(
                @"SELECT CAST(Id AS uniqueidentifier) FROM FileContents WHERE TenantId = @TenantId AND HashCode = @HashCode",
                parameters).ToListAsync(cancellationToken);
            return fileContentIds;
        }
    }

    private async Task<List<FileContent>> GetFileContents()
    {
        using var scope = this.serviceProvider.CreateScope();
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            return await dbContext.Database.SqlQuery<FileContent>($@"SELECT TOP {BatchSize} HashCode, TenantId, Occurrences
                                                                    FROM
                                                                        (SELECT HashCode, TenantId, COUNT(*) AS Occurrences
                                                                        FROM dbo.FileContents
                                                                        GROUP BY HashCode, TenantId
                                                                        HAVING COUNT(*) > 1) c
                                                                    ORDER BY Occurrences DESC").ToListAsync();
        }
    }

    private async Task<FileContentDuplicateCount> GetFileContentDuplicateCount()
    {
        using var scope = this.serviceProvider.CreateScope();
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            return await dbContext.Database.SqlQuery<FileContentDuplicateCount>($@"SELECT 
	                                                                                    CAST(COALESCE(count(*), 0) AS INT) AS TotalRowsWithDuplicates,
                                                                                        COALESCE(SUM(Occurrences - 1), 0) AS TotalOccurrences
                                                                                    FROM
                                                                                    (SELECT HashCode, TenantId, COUNT(*) AS Occurrences
                                                                                        FROM dbo.FileContents
                                                                                        GROUP BY HashCode, TenantId
                                                                                        HAVING COUNT(*) > 1) c").FirstOrDefaultAsync();
        }
    }

    private async Task<int> UpdateAllTablesRelatedToFileContent(Guid fileContentId, Guid newFileContentId, CancellationToken cancellationToken)
    {
        int totalUpdated = 0;
        totalUpdated += await this.UpdateTableRelatedToFileContent("Assets", fileContentId, newFileContentId, cancellationToken);
        totalUpdated += await this.UpdateTableRelatedToFileContent("DocumentFiles", fileContentId, newFileContentId, cancellationToken);
        totalUpdated += await this.UpdateTableRelatedToFileContent("ClaimFileAttachments", fileContentId, newFileContentId, cancellationToken);
        totalUpdated += await this.UpdateTableRelatedToFileContent("QuoteDocumentReadModels", fileContentId, newFileContentId, cancellationToken);
        totalUpdated += await this.UpdateTableRelatedToFileContent("QuoteFileAttachments", fileContentId, newFileContentId, cancellationToken);
        return totalUpdated;
    }

    private async Task<int> UpdateTableRelatedToFileContent(string tableName, Guid fileContentId, Guid newFileContentId, CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@FileContentId", fileContentId),
                    new SqlParameter("@NewFileContentId", newFileContentId),
                };

                var totalUpdated = await dbContext.Database.ExecuteSqlCommandAsync(
                    System.Data.Entity.TransactionalBehavior.EnsureTransaction,
                    $@"UPDATE {tableName} SET FileContentId = @NewFileContentId WHERE FileContentId = @FileContentId",
                    cancellationToken,
                    parameters);

                if (totalUpdated > 0)
                {
                    await Task.Delay(200, cancellationToken);
                }
                return totalUpdated;
            }
            catch (InvalidOperationException ex)
            {
                this.logger.LogError($"There exception for updating in {tableName} for FileContentId: {fileContentId}", ex);
                throw;
            }
        }
    }

    private void MemoryCleanup()
    {
        var memBefore = this.BytesToGb(GC.GetTotalMemory(false));
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
        GC.WaitForPendingFinalizers();
        var memAfter = this.BytesToGb(GC.GetTotalMemory(true));

        // it will log only if memory is more than 5 GB
        if (memBefore >= 5.0)
        {
            this.logger.LogInformation(
                $"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");
        }
    }

    private double BytesToGb(long bytes)
    {
        return bytes * 1E-9;
    }

    private class FileContent
    {
        public Guid TenantId { get; set; }

        public string? HashCode { get; set; }

        public int Occurrences { get; set; }
    }

    private class FileContentDuplicateCount
    {
        public int TotalRowsWithDuplicates { get; set; }

        public int TotalOccurrences { get; set; }
    }
}
