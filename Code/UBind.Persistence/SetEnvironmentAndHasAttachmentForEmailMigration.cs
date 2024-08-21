// <copyright file="SetEnvironmentAndHasAttachmentForEmailMigration.cs" company="uBind">
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
    using System.Threading;
    using Hangfire;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class SetEnvironmentAndHasAttachmentForEmailMigration
    {
        private const int RecordsPerBatch = 2000;
        private const int NoTimeout = 0;
        private const int RetryCount = 3;
        private readonly IEmailRepository emailRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<SetEnvironmentAndHasAttachmentForEmailMigration> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly string connectionString;
        private readonly string tmpTableSuffix = "TenantPairs";
        private Dictionary<EntityType, Action> processDictionary = new Dictionary<EntityType, Action>();

        public SetEnvironmentAndHasAttachmentForEmailMigration(
            IUBindDbContext db,
            ILogger<SetEnvironmentAndHasAttachmentForEmailMigration> logger,
            IEmailRepository emailRepository,
            IBackgroundJobClient jobClient)
        {
            this.emailRepository = emailRepository;
            this.dbContext = db;
            this.logger = logger;
            this.jobClient = jobClient;
            this.connectionString = db.Database.Connection.ConnectionString;

            this.processDictionary.Add(EntityType.Message, this.ProcessEmails);
        }

        public void Process()
        {
            // process first of the list.
            this.processDictionary.First().Value();
        }

        [JobDisplayName("Fill Temp Table For EntityType {0}")]
        public void FillTempTable(string sourceTableName)
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                var tmpTableName = sourceTableName + this.tmpTableSuffix;
                var createTempTableCommand =
                        $"SELECT [Id], [TenantId], 0 AS [IsProcessed] INTO {tmpTableName} FROM {sourceTableName}";

                bool exists = this.dbContext.Database
                    .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                    .SingleOrDefault() != null;
                if (!exists)
                {
                    this.logger.LogInformation($"Populating {tmpTableName} list for batching purposes..");
                    this.dbContext.Database.ExecuteSqlCommand(createTempTableCommand);
                }

                this.jobClient.Enqueue(() => this.ProcessBatch(1, null));
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        [JobDisplayName("ProcessBatch For Emails Migration Batch: {0}")]
        public void ProcessBatch(
            int batchCount,
            PerformContext context)
        {
            string firstSubBatchJobId = null;
            var tmpTableName = "Emails" + this.tmpTableSuffix;
            var countQuery = $"SELECT COUNT(*) FROM {tmpTableName}";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");

            var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) Id,TenantId FROM {tmpTableName} WITH (NOLOCK)";
            var currentBatch = this.dbContext.Database.SqlQuery<EntityTenantPair>(sqlBatchQuery).ToList();
            if (currentBatch.Any())
            {
                this.UpdateBatch(
                       batchCount,
                       currentBatch,
                       tmpTableName,
                       100);
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
                this.jobClient.ContinueJobWith<SetEnvironmentAndHasAttachmentForEmailMigration>(
                    jobId,
                    j => j.ProcessBatch(batchCount, null));
            }
            else
            {
                this.ShrinkLogs();
                this.DeallocateMemory();

                this.logger.LogInformation($"Finished... Cleaning up.");

                // CLEANUP.
                var dropTempCommand = SqlHelper.DropTableIfExists(tmpTableName);
                this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);
            }
        }

        [JobDisplayName("Emails Migration: Sub-Batch for batch {0}")]
        public void UpdateBatch(
            int batchCount,
            IEnumerable<EntityTenantPair> items,
            string tmpTableName,
            int pauseInMilliseconds = 100)
        {
            int count = 0;
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                foreach (var item in items)
                {
                    count++;

                    if (count % 200 == 0 || count == 1)
                    {
                        this.logger.LogInformation($"Id: '{item.Id}' count: {count}");
                        this.logger.LogInformation($"Type: {tmpTableName}");
                    }

                    var tags = this.emailRepository.GetTags(item.TenantId, item.Id);
                    var hasAttachments = this.emailRepository.CheckIfHasAttachments(item.TenantId, item.Id);
                    var environment = tags.FirstOrDefault(x => x.TagType == TagType.Environment)?.Value;

                    var deleteCommand = $"DELETE {tmpTableName} where Id = '{item.Id}';";

                    void ExecuteUpdate()
                    {
                        var email = this.emailRepository.GetById(item.TenantId, item.Id);
                        if (Enum.TryParse(environment, out DeploymentEnvironment deploymentEnvironment))
                        {
                            email.Environment = deploymentEnvironment;
                        }

                        email.HasAttachments = hasAttachments;
                        this.emailRepository.SaveChanges();

                        this.dbContext.Database.ExecuteSqlCommand(deleteCommand);
                        this.dbContext.SaveChanges();
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 35, pauseInMilliseconds);
                    Thread.Sleep(100);
                }
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        private void ProcessEmails()
        {
            this.jobClient.Enqueue(() => this.FillTempTable("Emails"));
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
