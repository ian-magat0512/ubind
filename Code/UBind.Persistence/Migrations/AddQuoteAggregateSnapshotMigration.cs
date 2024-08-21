// <copyright file="AddQuoteAggregateSnapshotMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations;

using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain.Services.Migration;
using UBind.Domain.Services.Maintenance;
using System;
using UBind.Domain.Extensions;
using UBind.Domain.Models;
using System.Data.SqlClient;
using UBind.Domain.Helpers;
using System.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

/// <summary>
/// This is the class for the migration service to add quote aggregate snapshot for
/// existing policy records that do not have a snapshot.
/// </summary>
public class AddQuoteAggregateSnapshotMigration : IAddQuoteAggregateSnapshotMigration
{
    private const int BatchSize = 1000;
    private readonly IDbLogFileMaintenanceService dbLogFileMaintenanceService;
    private readonly ILogger<AddQuoteAggregateSnapshotMigration> logger;
    private readonly IClock clock;
    private readonly IServiceProvider serviceProvider;
    private List<Guid> quoteAggregateIdsToSkip = new List<Guid>();

    public AddQuoteAggregateSnapshotMigration(
        IServiceProvider serviceProvider,
        IDbLogFileMaintenanceService dbLogFileMaintenanceService,
        ILogger<AddQuoteAggregateSnapshotMigration> logger,
        IClock clock)
    {
        this.dbLogFileMaintenanceService = dbLogFileMaintenanceService;
        this.logger = logger;
        this.clock = clock;
        this.serviceProvider = serviceProvider;
    }

    public async Task ProcessQuoteAggregateSnapshotForExistingRecords(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Migration started for adding existing policy to aggregate snapshot table.");
        await RetryPolicyHelper.ExecuteAsync<SqlException>((ct) => this.ProcessBatchUpdate(ct), maxJitter: 1500, cancellationToken: cancellationToken);
    }

    public async Task ProcessBatchUpdate(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            int batch = 1;
            bool isNoMoreRowsToProcess = false;
            List<AggregateSnapshot> aggregateSnapshots = new List<AggregateSnapshot>();
            StringBuilder serializedAggregateJson = new StringBuilder();
            while (true)
            {
                isNoMoreRowsToProcess = await this.ProcessBatchAndReturnTrueIfItsTheLast(
                    batch,
                    aggregateSnapshots,
                    serializedAggregateJson,
                    cancellationToken);

                this.logger.LogInformation($"Shrinking db log file.");
                await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                this.MemoryCleanup();

                if (isNoMoreRowsToProcess)
                {
                    break;
                }

                batch++;
                await Task.Delay(100, cancellationToken);
            }

            // Save the remaining records if any, after the batch is completed.
            var hasAddedAnyRemainingSnapshots = await this.TryAddIfAnyRemainingSnapshots(aggregateSnapshots, cancellationToken);
            if (hasAddedAnyRemainingSnapshots)
            {
                this.logger.LogInformation($"Shrinking db log file.");
                await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                this.MemoryCleanup();
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("Operation was cancelled.");
            throw;
        }
    }

    private async Task<bool> ProcessBatchAndReturnTrueIfItsTheLast(
        int batch,
        List<AggregateSnapshot> aggregateSnapshots,
        StringBuilder serializedAggregateJson,
        CancellationToken cancellationToken)
    {
        int noOfProcessedInBatch = 0;
        cancellationToken.ThrowIfCancellationRequested();
        using (var scope = this.serviceProvider.CreateScope())
        {
            var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<QuoteAggregate>>();
            var quoteAggregateRepository = scope.ServiceProvider.GetRequiredService<IQuoteAggregateRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();

            var skipWhereClause = string.Empty;
            if (this.quoteAggregateIdsToSkip.Count > 0)
            {
                var aggregateIdsToSkip = string.Join(",", this.quoteAggregateIdsToSkip.Select(id => $"'{id}'"));
                skipWhereClause = $" and p.Id NOT IN ({aggregateIdsToSkip})";
            }

            var totalRows = dbContext.Database.SqlQuery<int>($@"select count(*) 
                                                                from
                                                                    PolicyReadModels p
                                                                    left join AggregateSnapshots a on p.Id = a.AggregateId
                                                                where
                                                                    a.AggregateId is null {skipWhereClause}").Single();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {BatchSize}");
            if (totalRows == 0)
            {
                // means all policies have had it's quoteaggregate snapshot added.
                return true;
            }

            var policies = dbContext.Database.SqlQuery<Policy>($@"select top {BatchSize}
	                                                                    p.Id, 
	                                                                    p.TenantId 
                                                                    from 
	                                                                    PolicyReadModels p 
	                                                                    left join AggregateSnapshots a on p.Id = a.AggregateId 
                                                                    where
	                                                                    a.AggregateId is null {skipWhereClause}
                                                                    order by p.CreatedTicksSinceEpoch asc").ToList();
            foreach (var policy in policies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var quoteAggregate = await quoteAggregateRepository.GetByIdWithoutUsingSnapshot(policy.TenantId, policy.Id);
                if (quoteAggregate == null)
                {
                    this.quoteAggregateIdsToSkip.Add(policy.Id);
                    this.logger.LogWarning($"Quote aggregate not found for policy {policy.Id}");
                    continue;
                }

                if (quoteAggregate.PersistedEventCount < quoteAggregateRepository.GetSnapshotSaveInterval())
                {
                    this.quoteAggregateIdsToSkip.Add(quoteAggregate.Id);
                    this.logger.LogWarning($"Quote aggregate has below 200 events. AggregateId: {quoteAggregate.Id}");
                    continue;
                }

                try
                {
                    serializedAggregateJson.Clear();
                    serializedAggregateJson.Append(aggregateSnapshotService.SerializeAggregate(quoteAggregate));
                    AggregateSnapshot aggregateSnapshot = new AggregateSnapshot(
                                    quoteAggregate.TenantId,
                                    quoteAggregate.Id,
                                    quoteAggregate.AggregateType,
                                    quoteAggregate.PersistedEventCount,
                                    serializedAggregateJson.ToString(),
                                    this.clock.Now());
                    aggregateSnapshots.Add(aggregateSnapshot);
                    noOfProcessedInBatch++;
                    if (noOfProcessedInBatch % 50 == 0)
                    {
                        dbContext.AggregateSnapshots.AddRange(aggregateSnapshots);
                        await dbContext.SaveChangesAsync();
                        this.logger.LogInformation($"Save changes. count: {aggregateSnapshots.Count}");
                        aggregateSnapshots.Clear();

                        this.MemoryCleanup();
                    }
                }
                catch (SqlException ex) when (ex.Message.Contains("Timeout expired"))
                {
                    // we let to ignore the exception and continue with the next record. this record will be retried in the next batch.
                    this.logger.LogWarning($"There a SQl timeout happen during saving the record. AggregateId: {quoteAggregate.Id}");
                }
                catch (OutOfMemoryException ex)
                {
                    await Task.Delay(1000);
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"There a error happen during saving the record. AggregateId: {quoteAggregate.Id}\n" + ex, ex.Message);
                    throw;
                }
                finally
                {
                    // Clear the aggregate to prevent memory leak.
                    dbContext.RemoveContextAggregates<QuoteAggregate>();
                    quoteAggregate.ClearUnstrippedEvents();
                }

                // This is to prevent the database from being overwhelmed.
                await Task.Delay(50, cancellationToken);
            }

            // Save the remaining records if any
            if (aggregateSnapshots.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                dbContext.AggregateSnapshots.AddRange(aggregateSnapshots);
                await this.SaveChangesForUnsavedRecords(dbContext, aggregateSnapshots.Count, cancellationToken);
                aggregateSnapshots.Clear();
            }

            this.logger.LogInformation($"Total aggregate added to snapshot table: {noOfProcessedInBatch} out of {totalRows}, Batch: {batch}");

            // Case 1: means this the last batch
            // Case 2: means there are no more records to process
            //         and whatever records left (totalRows > 0) are all skipped (i.e. Quote aggregate not found for policy or aggregate event count less than 200)
            return policies.Count < BatchSize || (noOfProcessedInBatch == 0 && totalRows == policies.Count);
        }
    }

    private async Task<bool> TryAddIfAnyRemainingSnapshots(List<AggregateSnapshot> aggregateSnapshots, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (aggregateSnapshots.Count == 0)
        {
            return false;
        }

        using (var scope = this.serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IUBindDbContext>();
            dbContext.AggregateSnapshots.AddRange(aggregateSnapshots);
            await this.SaveChangesForUnsavedRecords(dbContext, aggregateSnapshots.Count, cancellationToken);
            aggregateSnapshots.Clear();
        }

        return true;
    }

    private async Task SaveChangesForUnsavedRecords(IUBindDbContext dbContext, int noOfRecordsToSave, CancellationToken cancellationToken)
    {
        async Task Save(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await dbContext.SaveChangesAsync();
            this.logger.LogInformation($"Save changes. count: {noOfRecordsToSave}");
        }

        await RetryPolicyHelper.ExecuteAsync<Exception>((ct) => Save(ct), maxJitter: 1500, cancellationToken: cancellationToken);
    }

    private void MemoryCleanup()
    {
        var memBefore = this.BytesToGb(GC.GetTotalMemory(false));
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
        GC.WaitForPendingFinalizers();
        var memAfter = this.BytesToGb(GC.GetTotalMemory(true));

        this.logger.LogInformation(
            $"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");
    }

    private double BytesToGb(long bytes)
    {
        return bytes * 1E-9;
    }

    private class Policy
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }
    }
}