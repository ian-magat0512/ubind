// <copyright file="SetMissingTenantIdOfRelationshipMigration.cs" company="uBind">
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
    using System.Threading;
    using Hangfire;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class SetMissingTenantIdOfRelationshipMigration : ISetMissingTenantIdOfRelationshipMigration
    {
        private const int RecordsPerBatch = 2000;
        private const int NoTimeout = 0;
        private const int RetryCount = 3;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<SetTenantIdOfRelationshipMigration> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly string connectionString;
        private readonly string tmpTableSuffix = "TenantPair";
        private Dictionary<string, Action> processDictionary = new Dictionary<string, Action>();

        public SetMissingTenantIdOfRelationshipMigration(
            IUBindDbContext db, ILogger<SetTenantIdOfRelationshipMigration> logger, IBackgroundJobClient jobClient)
        {
            this.dbContext = db;
            this.logger = logger;
            this.jobClient = jobClient;
            this.connectionString = db.Database.Connection.ConnectionString;

            this.processDictionary.Add("Relationship", this.ProcessRelatiohips);
        }

        public void SetMissingTenantIdfOfRelationships()
        {
            // process first of the list.
            this.processDictionary.First().Value();
        }

        [JobDisplayName("Fill Temp Table For EntityType {0} From Source {1}")]
        public void FillTempTable(
            string entityType,
            string sourceTableName)
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                var tmpTableName = entityType + this.tmpTableSuffix;
                bool exists = this.dbContext.Database
                    .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                    .SingleOrDefault() != null;
                if (!exists)
                {
                    var createTempTableCommand = $"SELECT [Id], [TenantId], [FromEntityType], [FromEntityId], [ToEntityType], [ToEntityId] INTO {tmpTableName} FROM {sourceTableName} where TenantId = '00000000-0000-0000-0000-000000000000'";
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

        [JobDisplayName("ProcessBatch For Set missing tenantId for relationship EntityType:{0} Batch: {1}")]
        public void ProcessBatch(
            string entityTypeName,
            int batchCount,
            PerformContext context)
        {
            string firstSubBatchJobId = null;
            var tmpTableName = entityTypeName + this.tmpTableSuffix;
            var countQuery = $"SELECT COUNT(*) FROM {tmpTableName} WITH (NOLOCK);";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");
            var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) * FROM {tmpTableName} WITH (NOLOCK)";
            var currentBatch = this.dbContext.Database.SqlQuery<RelationshipModel>(sqlBatchQuery).ToList();
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
                this.jobClient.ContinueJobWith<SetTenantIdOfRelationshipMigration>(
                    jobId,
                    j => j.ProcessBatch(entityTypeName, batchCount, null));
            }
            else
            {
                this.logger.LogInformation($"Finished... Cleaning up.");
                this.ShrinkLogs();
                this.DeallocateMemory();

                // CLEANUP.
                var dropTempCommand = SqlHelper.DropTableIfExists(tmpTableName);
                this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);
            }
        }

        [JobDisplayName("Set missing tenantId for relationship Sub-Batch for batch {0}")]
        private void UpdateBatch(
            int batchCount,
            IEnumerable<RelationshipModel> relationships,
            string tmpTableName,
            int pauseInMilliseconds = 100)
        {
            int count = 0;
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                foreach (var relationship in relationships)
                {
                    count++;

                    if (count % 200 == 0 || count == 1)
                    {
                        this.logger.LogInformation($"Id: '{relationship.Id}' count: {count}");
                        this.logger.LogInformation($"Type: {tmpTableName}");
                    }

                    Guid? tenantId = null;
                    var entityType = Enum.Parse(typeof(EntityType), relationship.ToEntityType.ToString());
                    switch (entityType)
                    {
                        case EntityType.Event:
                            var systemEvent = this.dbContext.SystemEvents.FirstOrDefault(x => x.Id == relationship.ToEntityId);
                            tenantId = systemEvent?.TenantId;
                            break;
                        case EntityType.User:
                            var user = this.dbContext.Users.FirstOrDefault(x => x.Id == relationship.ToEntityId);
                            tenantId = user?.TenantId;
                            break;
                        default:
                            this.logger.LogInformation($"Id: '{relationship.Id}' has no tenantId");
                            break;
                    }

                    var command = new StringBuilder($"DELETE {tmpTableName} where Id = '{relationship.Id}';");
                    if (tenantId.HasValue)
                    {
                        command.AppendLine($"UPDATE Relationships SET TenantId = '{tenantId.Value}' WHERE Id = '{relationship.Id}';");
                    }

                    void ExecuteUpdate()
                    {
                        this.dbContext.Database.ExecuteSqlCommand(command.ToString());
                        this.dbContext.SaveChanges();
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 35, pauseInMilliseconds);

                    Thread.Sleep(200);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        private void ProcessRelatiohips()
        {
            this.jobClient.Enqueue(() => this.FillTempTable("Relationship", "Relationships"));
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

#pragma warning disable SA1402 // File may only contain a single type
        private class RelationshipModel
#pragma warning restore SA1402 // File may only contain a single type
        {
            public Guid Id { get; set; }

            public Guid TenantId { get; set; }

            public int FromEntityType { get; set; }

            public Guid FromEntityId { get; set; }

            public int ToEntityType { get; set; }

            public Guid ToEntityId { get; set; }
        }
    }
}
