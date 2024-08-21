// <copyright file="AggregateEventsMigration.cs" company="uBind">
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
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class AggregateEventsMigration : IAggregateEventsMigration
    {
        /// <summary>
        /// This is usually used to test the orphaned events function.
        /// </summary>
        private const bool SkipToRemaininAggregateFunction = false;
        private const int RecordsPerBatch = 2000;
        private const int NoTimeout = 0;
        private const int RetryCount = 3;
        private const string RemainingAggregateTempTableName = "remainingAggregateTempTable";
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<AggregateEventsMigration> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly string connectionString;
        private readonly string tmpTableSuffix = "AggregateTenantPair";
        private Dictionary<AggregateType, Action> aggregateProcessesDictionary = new Dictionary<AggregateType, Action>();

        public AggregateEventsMigration(
            IUBindDbContext db, ILogger<AggregateEventsMigration> logger, IBackgroundJobClient jobClient)
        {
            this.dbContext = db;
            this.logger = logger;
            this.jobClient = jobClient;
            this.connectionString = db.Database.Connection.ConnectionString;

            this.aggregateProcessesDictionary.Add(AggregateType.Organisation, this.ProcessOrganisationAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.Report, this.ProcessReportAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.User, this.ProcessUserAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.Person, this.ProcessPersonAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.Customer, this.ProcessCustomerAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.Claim, this.ProcessClaimAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.TextAdditionalPropertyValue, this.ProcessTextAdditionalPropertyValueAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.Quote, this.ProcessQuoteAggregates);
            this.aggregateProcessesDictionary.Add(AggregateType.AdditionalPropertyDefinition, this.ProcessAdditionalPropertyDefinitionAggregates);
        }

        public void AddTenantIdsToEvents()
        {
            // process first of the list.
            this.aggregateProcessesDictionary.First().Value();
        }

        [JobDisplayName("AddTenantIdsToEvents For AggregateType:{0} Batch: {1}")]
        public void ProcessBatchForAggregateType(
            string aggregateTypeName,
            int batchCount,
            PerformContext context,
            bool executeLastFunctionWhenComplete = false)
        {
            var tmpTableName = aggregateTypeName + this.tmpTableSuffix;
            bool tableExists = this.dbContext.Database
                .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                .SingleOrDefault() != null;
            if (!tableExists)
            {
                return;
            }

            AggregateType aggregateType = (AggregateType)Enum.Parse(typeof(AggregateType), aggregateTypeName);
            var countQuery = $"SELECT COUNT(*) FROM {tmpTableName} WITH (NOLOCK) WHERE IsProcessed = 0";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");

            int recordsSoFar = 0;
            var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) AggregateId, TenantId FROM {tmpTableName} WITH (NOLOCK) WHERE IsProcessed = 0";
            var currentBatch = this.dbContext.Database.SqlQuery<AggregateTenantPair>(sqlBatchQuery).ToList();
            if (currentBatch.Any())
            {
                foreach (var batch in currentBatch)
                {
                    recordsSoFar++;
                    if (recordsSoFar % 25 == 0)
                    {
                        this.logger.LogInformation($"Updating {recordsSoFar}/{totalRecordsToProcess} records of {aggregateTypeName} events for " + batch.AggregateId);
                    }

                    var command = new StringBuilder();
                    command.AppendLine($"UPDATE EventRecordWithGuidIds SET TenantId = '{batch.TenantId}' , AggregateType = '{(int)aggregateType}'" +
                        $" WHERE AggregateId = '{batch.AggregateId}';");
                    command.AppendLine($"UPDATE {tmpTableName} SET IsProcessed = 1 WHERE " +
                        $"AggregateId = '{batch.AggregateId}';");
                    void ExecuteUpdate()
                    {
                        this.dbContext.Database.ExecuteSqlCommand(command.ToString());
                        this.dbContext.SaveChanges();
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 50, 150);
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
                this.jobClient.ContinueJobWith<AggregateEventsMigration>(
                    context.BackgroundJob.Id,
                    j => j.ProcessBatchForAggregateType(aggregateTypeName, batchCount, null, executeLastFunctionWhenComplete));
            }
            else
            {
                this.ShrinkLogs();
                this.DeallocateMemory();
                this.logger.LogInformation($"{recordsSoFar}/{totalRecordsToProcess} Finished... Cleaning up.");

                // CLEANUP.
                var dropTempCommand = $"DROP TABLE IF EXISTS {tmpTableName}";
                this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);

                // Process next aggregate.
                Action processAggregateFunction = this.GetNextAggregateToProcess(aggregateType);
                if (executeLastFunctionWhenComplete || SkipToRemaininAggregateFunction)
                {
                    this.logger.LogInformation($"Processing last function cleanup...");
                    this.jobClient.ContinueJobWith<AggregateEventsMigration>(
                     context.BackgroundJob.Id,
                     j => this.FillTempTableForRemainingAggregates());
                }
                else if (processAggregateFunction != null)
                {
                    this.logger.LogInformation($"Processing next function.");
                    processAggregateFunction();
                }
            }
        }

        /// <summary>
        /// This will fill up data that aggregates that doesnt link up with read models.
        /// </summary>
        [JobDisplayName("Process Remaining Aggregate Events w/o TenantId Batch: {0}")]
        public void ProcessRemainingAggregates(
            int batchCount,
            PerformContext context)
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                string firstSubBatchJobId = null;
                var subBatchNumberOfJobs = 2;
                this.dbContext.Database.CommandTimeout = NoTimeout;
                var tmpTableName = RemainingAggregateTempTableName;
                var countQuery = $"SELECT COUNT(*) FROM {tmpTableName} WHERE IsProcessed = 0";
                this.logger.LogInformation($"Querying count...");
                var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
                this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");
                var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) AggregateId FROM {tmpTableName} WHERE IsProcessed = 0";
                var currentBatch = this.dbContext.Database.SqlQuery<DistinctAggregate>(sqlBatchQuery).ToList();
                if (currentBatch.Any())
                {
                    var batchPerJob = RecordsPerBatch / subBatchNumberOfJobs;
                    var subBatch = currentBatch.Select(x => x.AggregateId).Batch(batchPerJob).ToList();
                    this.logger.LogInformation($"Creating sub-batches {batchPerJob}/{subBatch.Count} ...");

                    var subBatchCount = 0;
                    foreach (var batch in subBatch)
                    {
                        subBatchCount++;
                        var runShrink = subBatchCount == 1;
                        var delayMultipler = subBatchCount == 1 ? 1.15 : 1;
                        this.logger.LogInformation($"Running sub-batch {subBatchCount}...");

                        var jobId = this.jobClient.ContinueJobWith<AggregateEventsMigration>(
                             context.BackgroundJob.Id,
                             j => j.UpdateBatch(
                                    batchCount,
                                    batch.ToList(),
                                    true,
                                    Convert.ToInt32(75 * delayMultipler),
                                    runShrink));

                        firstSubBatchJobId = firstSubBatchJobId ?? jobId;
                    }
                }

                if (totalRecordsToProcess > 0 || currentBatch.Any())
                {
                    // every 5 batches shrink.
                    if (batchCount % 5 == 0)
                    {
                        this.ShrinkLogs();
                        this.DeallocateMemory();
                    }

                    this.logger.LogInformation($"Create a new job for the next batch...");
                    var jobId = firstSubBatchJobId ?? context.BackgroundJob.Id;
                    batchCount++;
                    this.jobClient.ContinueJobWith<AggregateEventsMigration>(
                        jobId,
                        j => j.ProcessRemainingAggregates(batchCount, null));
                }
                else if (currentBatch.Count == 0)
                {
                    this.ShrinkLogs();
                    this.DeallocateMemory();
                    this.logger.LogInformation($"Finished... Cleaning up.");

                    var dropTempCommand = $"DROP TABLE IF EXISTS {tmpTableName}";
                    this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);
                }
                else
                {
                    this.logger.LogError($"Some issue happened.");
                }

                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        [JobDisplayName("Sub-Batch for batch {0}")]
        public void UpdateBatch(
            int batchCount,
            IEnumerable<Guid> aggregateIds,
            bool queryIndividualAggregate = true,
            int pauseInMilliseconds = 100,
            bool shrink = false)
        {
            AggregateType aggregateType = default;
            string createdDateTime = string.Empty;
            int count = 0;
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                foreach (var aggregateId in aggregateIds)
                {
                    count++;
                    if (queryIndividualAggregate)
                    {
                        var aggregateQuery = $"SELECT TOP 1 AggregateId, EventJson, TicksSinceEpoch FROM EventRecordWithGuidIds WHERE AggregateId = '{aggregateId}'";
                        AggregateItem aggregateItem = null;
                        void ExecuteSelectQuery()
                        {
                            try
                            {
                                aggregateItem = this.dbContext.Database.SqlQuery<AggregateItem>(aggregateQuery).FirstOrDefault();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex?.InnerException?.Message ?? ex.Message);
                            }
                        }

                        RetryPolicyHelper.Execute<Exception>(() => ExecuteSelectQuery(), RetryCount, 50, pauseInMilliseconds);
                        aggregateType = this.GetAggregateTypeOfEventJson(aggregateItem.EventJson);
                        createdDateTime = aggregateItem.CreatedTimestamp.InUtc().ToString();
                    }

                    if (count % 200 == 0
                        || (queryIndividualAggregate && aggregateType != AggregateType.Quote)
                        || count == 1)
                    {
                        this.logger.LogInformation($"Id: '{aggregateId}' count: {count}");
                        this.logger.LogInformation($"Type: {aggregateType.ToString()} , CreatedDateTime: {(createdDateTime == string.Empty ? "--" : createdDateTime)}");
                    }

                    var command = new StringBuilder();
                    command.AppendLine($"UPDATE EventRecordWithGuidIds SET TenantId = '00000000-0000-0000-0000-000000000000', AggregateType = '{(int)aggregateType}'" +
                        $" WHERE AggregateId = '{aggregateId}';");
                    command.AppendLine($"DELETE from {RemainingAggregateTempTableName}" +
                      $" WHERE AggregateId = '{aggregateId}';");
                    void ExecuteUpdate()
                    {
                        try
                        {
                            this.dbContext.Database.ExecuteSqlCommand(command.ToString());
                            this.dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex?.InnerException?.Message ?? ex.Message);
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 50, pauseInMilliseconds);
                }

                if (shrink)
                {
                    this.ShrinkLogs();
                    this.DeallocateMemory();
                }

                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        [JobDisplayName("Fill Temp Table For Remaining Aggregates w/o TenantId")]
        public void FillTempTableForRemainingAggregates()
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                this.dbContext.Database.CommandTimeout = NoTimeout;
                var createTempTableCommand = $"SELECT DISTINCT [AggregateId] as [AggregateId], '00000000-0000-0000-0000-000000000000' AS [TenantId], 0 AS [IsProcessed] INTO {RemainingAggregateTempTableName} FROM EventRecordWithGuidIds WHERE TenantId IS NULL";

                bool exists = this.dbContext.Database
                    .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{RemainingAggregateTempTableName}'")
                    .SingleOrDefault() != null;
                if (!exists)
                {
                    this.logger.LogInformation($"Populating {RemainingAggregateTempTableName} list for batching purposes..");
                    this.dbContext.Database.ExecuteSqlCommand(createTempTableCommand);
                }
                else
                {
                    this.logger.LogInformation($"{RemainingAggregateTempTableName} table already exists, using that instead.");
                }

                this.jobClient.Enqueue(() => this.ProcessRemainingAggregates(1, null));

                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
            finally
            {
                // return to previous value
                this.dbContext.Database.CommandTimeout = previousTimeout;
            }
        }

        [JobDisplayName("Fill Temp Table For AggregateType {0} From Source {1}")]
        public void FillTempTable(
            AggregateType aggregateType,
            string sourceTableName,
            bool executeLastFunctionWhenComplete = false,
            string aggregateIdColumnName = "Id")
        {
            var tmpTableName = aggregateType.ToString() + this.tmpTableSuffix;
            var createTempTableCommand = $"SELECT [{aggregateIdColumnName}] as [AggregateId], [TenantId] AS [TenantId], 0 AS [IsProcessed] INTO {tmpTableName} FROM {sourceTableName}";

            bool exists = this.dbContext.Database
                .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                .SingleOrDefault() != null;
            if (!exists)
            {
                this.logger.LogInformation($"Populating {tmpTableName} list for batching purposes..");
                this.dbContext.Database.ExecuteSqlCommand(createTempTableCommand);
            }

            this.jobClient.Enqueue(() => this.ProcessBatchForAggregateType(aggregateType.ToString(), 1, null, executeLastFunctionWhenComplete));
        }

        /// <summary>
        /// Determines what is the next aggregate to process.
        /// </summary>
        /// <returns>Aggregate type.</returns>
        private Action GetNextAggregateToProcess(AggregateType aggregateType)
        {
            bool found = false;
            foreach (var procesParam in this.aggregateProcessesDictionary)
            {
                if (procesParam.Key == aggregateType && !found)
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

        private void ProcessPersonAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.Person, "PersonReadModels", false, "Id"));
        }

        private void ProcessUserAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.User, "UserReadModels", false, "Id"));
        }

        private void ProcessReportAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.Report, "ReportReadModels", false, "Id"));
        }

        private void ProcessQuoteAggregates()
        {
            var tmpTableName = AggregateType.Quote.ToString() + this.tmpTableSuffix;
            var createTempTableCommand = $"SELECT Id AS [AggregateId], TenantId, 0 AS [IsProcessed] INTO {tmpTableName} FROM PolicyReadModels UNION SELECT AggregateId AS [AggregateId], TenantId, 0 AS [IsProcessed] FROM Quotes";

            bool exists = this.dbContext.Database
                .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tmpTableName}'")
                .SingleOrDefault() != null;
            if (!exists)
            {
                this.logger.LogInformation($"Populating {tmpTableName} list for batching purposes..");
                this.dbContext.Database.ExecuteSqlCommand(createTempTableCommand);
            }

            this.jobClient.Enqueue(() => this.ProcessBatchForAggregateType(AggregateType.Quote.ToString(), 1, null, false));
        }

        private void ProcessOrganisationAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.Organisation, "OrganisationReadModels", false, "Id"));
        }

        private void ProcessCustomerAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.Customer, "CustomerReadModels", false, "Id"));
        }

        private void ProcessClaimAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.Claim, "ClaimReadModels", false, "Id"));
        }

        private void ProcessTextAdditionalPropertyValueAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.TextAdditionalPropertyValue, "TextAdditionalPropertyValueReadModels", false, "Id"));
        }

        private void ProcessAdditionalPropertyDefinitionAggregates()
        {
            this.jobClient.Enqueue(() => this.FillTempTable(AggregateType.AdditionalPropertyDefinition, "AdditionalPropertyDefinitions", true, "Id"));
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

        private AggregateType GetAggregateTypeOfEventJson(string eventJson)
        {
            eventJson = eventJson.ToLower();

            if (eventJson.Contains("user.useraggregate"))
            {
                return AggregateType.User;
            }

            if (eventJson.Contains("quote.quoteaggregate"))
            {
                return AggregateType.Quote;
            }

            if (eventJson.Contains("person.personaggregate"))
            {
                return AggregateType.Person;
            }

            if (eventJson.Contains("customer.customeraggregate"))
            {
                return AggregateType.Customer;
            }

            if (eventJson.Contains("claim.claimaggregate"))
            {
                return AggregateType.Claim;
            }

            if (eventJson.Contains("organisation.organisation"))
            {
                return AggregateType.Organisation;
            }

            if (eventJson.Contains("report.reportaggregate"))
            {
                return AggregateType.Report;
            }

            if (eventJson.Contains("additionalpropertyvalue.textadditionalpropertyvalue"))
            {
                return AggregateType.TextAdditionalPropertyValue;
            }

            if (eventJson.Contains("additionalpropertydefinition.additionalpropertydefinition"))
            {
                return AggregateType.AdditionalPropertyDefinition;
            }

            if (eventJson.Contains("accounting.paymentaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.refundaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.invoiceaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.financialtransactionaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            return default;
        }

        private double BytesToGb(long bytes)
        {
            return bytes * 1E-9;
        }

        private class AggregateTenantPair
        {
            public Guid AggregateId { get; set; }

            public Guid TenantId { get; set; }
        }

        private class DistinctAggregate
        {
            public Guid AggregateId { get; set; }
        }

        private class AggregateItem
        {
            public Guid AggregateId { get; set; }

            public string EventJson { get; set; }

            public long TicksSinceEpoch { get; set; }

            /// <summary>
            /// Gets the record created timestamp.
            /// </summary>
            public Instant CreatedTimestamp => Instant.FromUnixTimeTicks(this.TicksSinceEpoch);
        }
    }
}
