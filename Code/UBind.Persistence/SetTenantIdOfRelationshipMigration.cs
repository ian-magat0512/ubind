// <copyright file="SetTenantIdOfRelationshipMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using Hangfire;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class SetTenantIdOfRelationshipMigration : ISetTenantIdOfRelationshipMigration
    {
        private const int RecordsPerBatch = 2000;
        private const int NoTimeout = 0;
        private const int RetryCount = 3;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<SetTenantIdOfRelationshipMigration> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly string connectionString;
        private readonly string tmpTableSuffix = "TenantPair";
        private Dictionary<EntityType, Action> processDictionary = new Dictionary<EntityType, Action>();

        public SetTenantIdOfRelationshipMigration(
            IUBindDbContext db, ILogger<SetTenantIdOfRelationshipMigration> logger, IBackgroundJobClient jobClient)
        {
            this.dbContext = db;
            this.logger = logger;
            this.jobClient = jobClient;
            this.connectionString = db.Database.Connection.ConnectionString;

            this.processDictionary.Add(EntityType.Event, this.ProcessEvents);
            this.processDictionary.Add(EntityType.Message, this.ProcessEmails);
        }

        public void SetTenantIdfOfRelationships()
        {
            // process first of the list.
            this.processDictionary.First().Value();
        }

        [JobDisplayName("Fill Temp Table For EntityType {0} From Source {1}")]
        public void FillTempTable(
            EntityType entityType,
            string sourceTableName)
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                var tmpTableName = entityType.ToString() + this.tmpTableSuffix;
                var createTempTableCommand =
                    entityType == EntityType.Tenant ?
                        $"SELECT [Id], [Id] as [TenantId], 0 AS [IsProcessed] INTO {tmpTableName} FROM {sourceTableName}"
                    : entityType == EntityType.Quote ?
                        $"SELECT [QuoteId] as [Id], [TenantId], 0 AS [IsProcessed] INTO {tmpTableName} FROM {sourceTableName}"
                        : $"SELECT [Id], [TenantId], 0 AS [IsProcessed] INTO {tmpTableName} FROM {sourceTableName}";

                bool exists = this.dbContext.Database
                    .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                    .SingleOrDefault() != null;
                if (!exists)
                {
                    this.logger.LogInformation($"Populating {tmpTableName} list for batching purposes..");
                    this.dbContext.Database.ExecuteSqlCommand(createTempTableCommand);
                }

                this.jobClient.Enqueue(() => this.ProcessBatch(entityType.ToString(), 1, null));
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        [JobDisplayName("ProcessBatch For EntityType:{0} Batch: {1}")]
        public void ProcessBatch(
            string entityTypeName,
            int batchCount,
            PerformContext context)
        {
            string firstSubBatchJobId = null;
            var subBatchNumberOfJobs = 2;
            var tmpTableName = entityTypeName + this.tmpTableSuffix;
            EntityType entityType = (EntityType)Enum.Parse(typeof(EntityType), entityTypeName);
            var countQuery = $"SELECT COUNT(*) FROM {tmpTableName} WITH (NOLOCK) WHERE IsProcessed = 0;";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");

            // int recordsSoFar = 0;
            var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) Id, TenantId FROM {tmpTableName} WITH (NOLOCK) WHERE IsProcessed = 0";
            var currentBatch = this.dbContext.Database.SqlQuery<EntityTenantPair>(sqlBatchQuery).ToList();
            if (currentBatch.Any())
            {
                var batchPerJob = RecordsPerBatch / subBatchNumberOfJobs;
                var subBatches = currentBatch.Select(x => x).Batch(batchPerJob).ToList();
                this.logger.LogInformation($"Creating sub-batches {batchPerJob}/{subBatches.Count} ...");

                var subBatchCount = 0;
                foreach (var subBatch in subBatches)
                {
                    subBatchCount++;
                    var delayMultipler = subBatchCount == 1 ? 1.15 : 1;
                    this.logger.LogInformation($"Running sub-batch {subBatchCount}...");

                    var isFinalBatch = totalRecordsToProcess - currentBatch.Count <= 0 && subBatch.Count() < batchPerJob;
                    var jobId = this.jobClient.ContinueJobWith<SetTenantIdOfRelationshipMigration>(
                         context.BackgroundJob.Id,
                         j => j.UpdateBatch(
                                batchCount,
                                subBatch.ToList(),
                                tmpTableName,
                                Convert.ToInt32(60 * delayMultipler),
                                isFinalBatch));

                    firstSubBatchJobId = firstSubBatchJobId ?? jobId;
                }
            }

            if (totalRecordsToProcess - currentBatch.Count > 0)
            {
                // every 5 batches we shrink.
                if (batchCount % 5 == 0)
                {
                    this.ShrinkLogs();
                    this.DeallocateMemory();
                }

                batchCount++;
                var jobId = firstSubBatchJobId ?? context.BackgroundJob.Id;
                this.jobClient.ContinueJobWith<SetTenantIdOfRelationshipMigration>(
                    jobId,
                    j => j.ProcessBatch(entityTypeName, batchCount, null));
            }
            else
            {
                this.ShrinkLogs();
                this.DeallocateMemory();

                // Process next aggregate.
                Action processAggregateFunction = this.GetNextEntityToProcess(entityType);
                if (processAggregateFunction != null)
                {
                    this.logger.LogInformation($"Processing next function.");
                    processAggregateFunction();
                }
            }
        }

        [JobDisplayName("Sub-Batch for batch {0}")]
        public void UpdateBatch(
            int batchCount,
            IEnumerable<EntityTenantPair> entityTenantPair,
            string tmpTableName,
            int pauseInMilliseconds = 100,
            bool isFinalBatch = false)
        {
            int count = 0;
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                foreach (var batch in entityTenantPair)
                {
                    count++;

                    if (count % 200 == 0 || count == 1)
                    {
                        this.logger.LogInformation($"Id: '{batch.Id}' count: {count}");
                        this.logger.LogInformation($"Type: {tmpTableName}");
                    }

                    var command = new StringBuilder();
                    string whereClause = $" WHERE FromEntityId = '{batch.Id}' OR ToEntityId = '{batch.Id}';";
                    command.AppendLine($"UPDATE Relationships SET TenantId = '{batch.TenantId}'" + whereClause);
                    command.AppendLine($"DELETE {tmpTableName} where Id = '{batch.Id}';");

                    void ExecuteUpdate()
                    {
                        this.dbContext.Database.ExecuteSqlCommand(command.ToString());
                        this.dbContext.SaveChanges();
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 35, pauseInMilliseconds);
                }

                // delete table if final batch.
                if (isFinalBatch)
                {
                    this.logger.LogInformation($"Finished... Cleaning up.");

                    // CLEANUP.
                    var dropTempCommand = $"DROP TABLE {tmpTableName};";
                    this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);
                }
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        /// <summary>
        /// Determines what is the next aggregate to process.
        /// </summary>
        /// <returns>Aggregate type.</returns>
        private Action GetNextEntityToProcess(EntityType entityType)
        {
            bool found = false;
            foreach (var procesParam in this.processDictionary)
            {
                if (procesParam.Key == entityType && !found)
                {
                    found = true;
                }
                else if (found)
                {
                    return procesParam.Value;
                }
            }

            // if last of the batch. run the remaining aggregates
            return null;
        }

        private void ProcessEmails()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(EntityType.Message, "Emails"));
        }

        private void ProcessEvents()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(EntityType.Event, "SystemEvents"));
        }

        private void ShrinkLogs()
        {
            using (var dbContext = new UBindDbContext(this.connectionString))
            {
                this.logger.LogInformation("Shrinking DB log...");
                dbContext.Database.ExecuteSqlCommand(
                    TransactionalBehavior.DoNotEnsureTransaction, "DBCC SHRINKFILE (2, TRUNCATEONLY)");
            }
        }

        private void DeallocateMemory()
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
    }
}
