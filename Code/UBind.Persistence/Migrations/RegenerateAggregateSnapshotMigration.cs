// <copyright file="RegenerateAggregateSnapshotMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Data.SqlClient;
using System.Runtime;
using System.Threading;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Models;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain.Services.Maintenance;
using UBind.Domain.Services.Migration;

/// <summary>
/// This service is used for migration purposes to regenerate aggregate snapshots.
/// Since some property was being stored twice in the database, we need to regenerate the snapshots.
/// This will free up space in the database and improve performance.
/// </summary>
public class RegenerateAggregateSnapshotMigration : IRegenerateAggregateSnapshotMigration
{
    private const int BatchSize = 1000;
    private readonly IServiceProvider serviceProvider;
    private readonly IClock clock;
    private readonly ILogger<RegenerateAggregateSnapshotMigration> logger;
    private readonly string tmpAggregateSnapshotTable = "TmpAggregateSnapshots";

    private bool doesTempTableExist;

    public RegenerateAggregateSnapshotMigration(
        IServiceProvider serviceProvider,
        IClock clock,
        ILogger<RegenerateAggregateSnapshotMigration> logger)
    {
        this.serviceProvider = serviceProvider;
        this.clock = clock;
        this.logger = logger;
    }

    public async Task RegenerateAggregateSnapshots(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.logger.LogInformation($"Migration started for regeneration of aggregate snapshots.");
        await RetryPolicyHelper.ExecuteAsync<Exception>(async (ct) => await this.ProcessBatchUpdate(ct), maxJitter: 1500, cancellationToken: cancellationToken);
    }

    public async Task ProcessBatchUpdate(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.doesTempTableExist = false; // reset so it can be checked again on retry.
            using (var scope = this.serviceProvider.CreateScope())
            {
                var scopedDbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
                var dbLogFileMaintenanceService = scope.ServiceProvider.GetRequiredService<IDbLogFileMaintenanceService>();
                int batch = 1;
                await this.InsertAggregateSnapshotToTempTable(scopedDbContext);

                while (true)
                {
                    this.logger.LogInformation($"Batch started: {batch}");
                    var totalRows = scopedDbContext.Database.SqlQuery<int>($"SELECT COUNT(*) FROM {this.tmpAggregateSnapshotTable} WITH (NOLOCK) WHERE IsProcessed = 0").SingleOrDefault();
                    this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {BatchSize}");

                    if (totalRows == 0)
                    {
                        this.logger.LogInformation($"No record to be processed.");
                        break;
                    }

                    var aggregateSnapshotsTemp = scopedDbContext.Database.SqlQuery<AggregateSnapshotTemp>($@"SELECT TOP {BatchSize} 
	                                                                                                            Id,
	                                                                                                            AggregateId,
	                                                                                                            TenantId
                                                                                                            FROM
	                                                                                                            {this.tmpAggregateSnapshotTable}
                                                                                                            WHERE
	                                                                                                            IsProcessed = 0").ToList();

                    await this.UpdateAndProcessAggregateSnapshots(aggregateSnapshotsTemp, cancellationToken);
                    this.MemoryCleanup();
                    await dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded();

                    if (aggregateSnapshotsTemp.Count < BatchSize)
                    {
                        this.logger.LogInformation($"This is the last batch to be processed. Batch: {batch}, Count: {aggregateSnapshotsTemp.Count}");
                        break;
                    }

                    await Task.Delay(300, cancellationToken);
                    batch++;
                }

                this.MemoryCleanup();
                await dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded();
                await this.DropTempTable(scopedDbContext);
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("Operation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }

        this.logger.LogInformation("Migration completed for regeneration of aggregate snapshots.");
    }

    private async Task UpdateAndProcessAggregateSnapshots(List<AggregateSnapshotTemp> aggregateSnapshotsTemp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (var innerScope = this.serviceProvider.CreateScope())
        {
            var scopedDbContext = innerScope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            var dbLogFileMaintenanceService = innerScope.ServiceProvider.GetRequiredService<IDbLogFileMaintenanceService>();
            var quoteAggregateRepository = innerScope.ServiceProvider.GetRequiredService<IQuoteAggregateRepository>();
            var aggregateSnapshotService = innerScope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<QuoteAggregate>>();
            var aggregateSnapshotRepository = innerScope.ServiceProvider.GetRequiredService<IAggregateSnapshotRepository>();
            int totalProcessed = 0;
            foreach (var aggregateSnapshotTemp in aggregateSnapshotsTemp)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var quoteAggregate = await quoteAggregateRepository.GetByIdWithoutUsingSnapshot(aggregateSnapshotTemp.TenantId, aggregateSnapshotTemp.AggregateId);
                if (quoteAggregate == null)
                {
                    this.logger.LogWarning($"Quote aggregate not found for aggregate snapshot Id: {aggregateSnapshotTemp.Id}, AggregateId: {aggregateSnapshotTemp.AggregateId}");
                    await this.UpdateTempTableIsProcessedFlag(scopedDbContext, aggregateSnapshotTemp.Id, 1);
                    continue;
                }

                if (quoteAggregate.PersistedEventCount < quoteAggregateRepository.GetSnapshotSaveInterval())
                {
                    this.logger.LogInformation($"Quote aggregate has below 200 events so we don't need to regenerate it. We need to delete it on aggregateSnapshot table. Snapshot Id: {aggregateSnapshotTemp.Id}, AggregateId: {aggregateSnapshotTemp.AggregateId}");

                    // We need to delete the snapshot from the database as well.
                    await scopedDbContext.Database.ExecuteSqlCommandAsync(
                            $@"DELETE FROM AggregateSnapshots WHERE Id = @Id",
                            cancellationToken,
                            new SqlParameter("@Id", aggregateSnapshotTemp.Id));
                    await this.DeleteTempTableEntry(scopedDbContext, aggregateSnapshotTemp.Id);

                    scopedDbContext.RemoveContextAggregates<QuoteAggregate>();
                    quoteAggregate.ClearUnstrippedEvents();
                    continue;
                }

                try
                {
                    var serializedAggregateJson = aggregateSnapshotService.SerializeAggregate(quoteAggregate);
                    var aggregateSnapshot = new AggregateSnapshot(
                        quoteAggregate.TenantId,
                        quoteAggregate.Id,
                        quoteAggregate.AggregateType,
                        quoteAggregate.PersistedEventCount,
                        serializedAggregateJson,
                        this.clock.Now());
                    scopedDbContext.AggregateSnapshots.Add(aggregateSnapshot);
                    scopedDbContext.SaveChanges();

                    await aggregateSnapshotRepository.DeleteOlderAggregateSnapshots(
                        quoteAggregate.TenantId,
                        aggregateSnapshot.Id,
                        aggregateSnapshot.AggregateId,
                        aggregateSnapshot.AggregateType);

                    await this.InsertAggregateSnapshotToTempTable(scopedDbContext);
                    await this.DeleteTempTableEntry(scopedDbContext, aggregateSnapshotTemp.Id);
                    await this.UpdateTempTableIsProcessedFlag(scopedDbContext, aggregateSnapshot.Id, 1);

                    totalProcessed++;

                    if (totalProcessed % 50 == 0)
                    {
                        this.logger.LogInformation($"Processed {totalProcessed} aggregate snapshots.");
                        this.MemoryCleanup();
                        await dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded();
                    }
                }
                catch (SqlException ex) when (ex.Message.Contains("Timeout expired"))
                {
                    await this.UpdateTempTableIsProcessedFlag(scopedDbContext, aggregateSnapshotTemp.Id, 0);
                    this.logger.LogWarning($"SQL timeout occurred during saving the record. AggregateId: {quoteAggregate.Id}");
                }
                catch (OutOfMemoryException ex)
                {
                    await this.UpdateTempTableIsProcessedFlag(scopedDbContext, aggregateSnapshotTemp.Id, 0);
                    await Task.Delay(1000);
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await this.UpdateTempTableIsProcessedFlag(scopedDbContext, aggregateSnapshotTemp.Id, 0);
                    this.logger.LogError($"Error occurred during saving the record. AggregateId: {quoteAggregate.Id}\n" + ex, ex.Message);
                    throw;
                }
                finally
                {
                    scopedDbContext.RemoveContextAggregates<QuoteAggregate>();
                    quoteAggregate.ClearUnstrippedEvents();
                }

                await Task.Delay(300, cancellationToken);
            }
        }
    }

    private async Task InsertAggregateSnapshotToTempTable(IUBindDbContext dbContext)
    {
        string selectColumns = "SELECT a.[Id], a.[AggregateId], a.[TenantId], 0 AS [IsProcessed]";
        string tmpTableSql;

        this.CheckIfTableExist(dbContext);
        if (!this.doesTempTableExist)
        {
            tmpTableSql = $"{selectColumns} INTO {this.tmpAggregateSnapshotTable} " +
                "FROM AggregateSnapshots a WITH (NOLOCK)";
        }
        else
        {
            tmpTableSql = $"INSERT INTO {this.tmpAggregateSnapshotTable} " +
                $"{selectColumns} FROM AggregateSnapshots a WITH(NOLOCK) " +
                $"LEFT OUTER JOIN {this.tmpAggregateSnapshotTable} t ON t.Id = a.Id " +
                "WHERE t.Id IS NULL";
        }

        await dbContext.Database.ExecuteSqlCommandAsync(tmpTableSql);
    }

    private void MemoryCleanup()
    {
        var memBefore = this.BytesToGb(GC.GetTotalMemory(false));
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
        GC.WaitForPendingFinalizers();
        var memAfter = this.BytesToGb(GC.GetTotalMemory(true));

        if (memAfter > 5.0)
        {
            this.logger.LogInformation($"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");
        }
    }

    private double BytesToGb(long bytes)
    {
        return bytes * 1E-9;
    }

    private void CheckIfTableExist(IUBindDbContext dbContext)
    {
        if (!this.doesTempTableExist)
        {
            this.doesTempTableExist = dbContext.Database.SqlQuery<int?>($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{this.tmpAggregateSnapshotTable}'").SingleOrDefault() != null;
        }
    }

    private async Task UpdateTempTableIsProcessedFlag(IUBindDbContext dbContext, Guid id, int isProcessed)
    {
        // Legend IsProcessed [0 - not yet processed] [1 - done]
        var parameters = new[]
        {
            new SqlParameter("@Id", id),
            new SqlParameter("@IsProcessed", isProcessed),
        };

        await dbContext.Database.ExecuteSqlCommandAsync(
            $@"UPDATE {this.tmpAggregateSnapshotTable} SET IsProcessed = @IsProcessed WHERE Id = @Id",
            parameters);
    }

    private async Task DeleteTempTableEntry(IUBindDbContext dbContext, Guid id)
    {
        await dbContext.Database.ExecuteSqlCommandAsync(
            $@"DELETE FROM {this.tmpAggregateSnapshotTable} WHERE Id = @Id",
            new SqlParameter("@Id", id));
    }

    private async Task DropTempTable(IUBindDbContext dbContext)
    {
        this.CheckIfTableExist(dbContext);
        if (!this.doesTempTableExist)
        {
            return;
        }
        bool noRecordToProcess = dbContext.Database.SqlQuery<int>($"SELECT COUNT(1) FROM {this.tmpAggregateSnapshotTable} WHERE IsProcessed = 0").SingleOrDefault() == 0;
        if (noRecordToProcess)
        {
            await dbContext.Database.ExecuteSqlCommandAsync($"DROP TABLE {this.tmpAggregateSnapshotTable}");
        }
    }

    private class AggregateSnapshotTemp
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public Guid TenantId { get; set; }
    }
}
