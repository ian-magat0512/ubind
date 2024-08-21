// <copyright file="FileContentMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// Class to contain file content migration scripts for a cleaner separation and easier maintenance.
    /// </summary>
    public partial class FileContentMigration : IFileContentMigration
    {
        private const int NoTimeout = 0;
        private readonly string connectionString;
        private readonly string tmpTable = "TmpFileAttachedEvents";

        private readonly IUBindDbContext dbContext;
        private readonly ILogger<FileContentRepository> logger;
        private readonly IBackgroundJobClient backgroundJobClient;

        public FileContentMigration(
            IUBindDbContext dbContext,
            ILogger<FileContentRepository> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.backgroundJobClient = backgroundJobClient;
            this.connectionString = dbContext.Database.Connection.ConnectionString;
        }

        /// <inheritdoc/>
        public void PopulateFileContentsForQuoteFileAttachments()
        {
            if (typeof(FileAttachedEvent).GetProperty("Content") == null)
            {
                // Content already copied over to file contents table.
                this.logger.LogInformation($"Migration for quote file attachments DONE.");
                return;
            }

            // Release 1. Populate quote file contents from events.
            this.logger.LogInformation($"Started {DateTime.Now}");

            if (!this.TableExists(this.tmpTable))
            {
                var dbTimeout = this.dbContext.Database.CommandTimeout;
                this.dbContext.Database.CommandTimeout = NoTimeout;

                this.logger.LogInformation($"Populating temp table with event records to process...");
                var createTempTableSql = $"SELECT [AggregateId], [Sequence], 0 AS [IsProcessed], " +
                    $"CAST(null as uniqueidentifier) AS FileContentId, CAST(null as BIGINT) AS FileSize INTO {this.tmpTable} " +
                    "FROM EventRecordWithGuidIds WITH (NOLOCK) " +
                    "WHERE EventJson LIKE '{\"$type\":\"UBind.Domain.Aggregates.Quote.QuoteAggregate+FileAttachedEvent%'";
                this.dbContext.Database.ExecuteSqlCommand(createTempTableSql);

                this.dbContext.Database.CommandTimeout = dbTimeout;
            }

            this.backgroundJobClient.Enqueue(() => this.ProcessBatch(1));
        }

        /// <summary>
        /// Process batch.
        /// </summary>
        /// <param name="batch">The batch number.</param>
        [JobDisplayName("Startup Job: PopulateFileContentsForQuoteFileAttachments Process Batch {0}")]
        public void ProcessBatch(int batch)
        {
            const int batchSize = 5000;

            var sqlCount = $"SELECT COUNT(*) FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed=0";
            var totalRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");

            var sqlBatch = $"SELECT TOP {batchSize} AggregateId, Sequence FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed=0";

            var eventsToProcess = this.dbContext.Database.SqlQuery<AttachedFile>(sqlBatch).ToList();

            this.ProcessEvents(eventsToProcess);

            if (eventsToProcess.Count == batchSize)
            {
                batch++;
                this.backgroundJobClient.Enqueue(() => this.ProcessBatch(batch));
            }
        }

        /// <inheritdoc/>
        public void RemoveEventJsonFileContent()
        {
            if (!this.TableExists(this.tmpTable))
            {
                this.logger.LogInformation($"{this.tmpTable} NOT FOUND! You have to run Release 1 first.");
                return;
            }

            this.logger.LogInformation("RemoveEventJsonFileContent started...");
            this.logger.LogInformation($"Starting DB log size: {this.GetLogSize()}");

            this.backgroundJobClient.Enqueue(() => this.ProcessEventJsonBatch(1));

            this.backgroundJobClient.Enqueue(() => this.RemoveEventJsonFileContentCleanup());
        }

        /// <summary>
        /// Process event JSON batch.
        /// </summary>
        /// <param name="batch">The batch number.</param>
        [JobDisplayName("Startup Job: RemoveEventJsonFileContent ProcessEventJson Batch {0}")]
        public void ProcessEventJsonBatch(int batch)
        {
            const int batchSize = 5000;

            var sqlCount = $"SELECT COUNT(*) FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed=1";
            var totalRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            this.logger.LogInformation($"* * * REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize} * * *");

            var sqlBatch = $"SELECT TOP {batchSize} AggregateId, Sequence, FileContentId, CAST(FileSize AS bigint) FileSize FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed=1";

            var eventsToProcess = this.dbContext.Database.SqlQuery<AttachedFile>(sqlBatch).ToList();

            this.DoProcessEventJsonBatch(eventsToProcess);
        }

        /// <summary>
        /// Remove eventJson for newly attached files after R1 has been deployed and running.
        /// </summary>
        public void RemoveEventJsonFileContentCleanup()
        {
            // Check if this cleanup already ran before
            Guid cleanupId = default;
            var sqlCleanupRecord = $"SELECT COUNT(*) FROM {this.tmpTable} WHERE AggregateId=@cleanupId AND FileContentId=@cleanupId";
            int count = this.dbContext.Database.SqlQuery<int>(
                sqlCleanupRecord,
                new SqlParameter("@cleanupId", cleanupId)).Single();
            if (count > 0)
            {
                return;
            }

            // Insert row to be used as flag that this method already ran
            this.dbContext.Database.ExecuteSqlCommand(
                $"INSERT INTO {this.tmpTable} (AggregateId, Sequence, IsProcessed, FileContentId) VALUES (@cleanupId, 0, 2, @cleanupId)",
                new SqlParameter("@cleanupId", cleanupId));

            // Release 2. Remove file content from event json
            this.logger.LogInformation($"RemoveEventJsonFileContent Cleanup Started {DateTime.Now}");
            this.logger.LogInformation("Retrieving event records to process...");

            const string sqlEvents = "SELECT e.* FROM EventRecordWithGuidIds e WITH (NOLOCK) " +
                "WHERE e.EventJson LIKE '{\"$type\":\"UBind.Domain.Aggregates.Quote.QuoteAggregate+FileAttachedEvent%' " +
                "AND NOT EXISTS(SELECT 1 FROM TmpFileAttachedEvents t WITH (NOLOCK) " +
                "WHERE t.AggregateId = e.AggregateId AND t.Sequence = e.Sequence)";

            var dbTimeout = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = NoTimeout;
            var fileAttachedEvents = this.dbContext.Database.SqlQuery<EventRecordWithGuidId>(sqlEvents).ToList();
            this.dbContext.Database.CommandTimeout = dbTimeout;

            int total = fileAttachedEvents.Count;
            this.logger.LogInformation($"Retrieved {total} Events.");

            int ctr = 1;

            foreach (var attachedEvent in fileAttachedEvents)
            {
                var aggregateId = attachedEvent.AggregateId;
                var sequence = attachedEvent.Sequence;

                using (var dbContext = new UBindDbContext(this.connectionString))
                {
                    try
                    {
                        dbContext.Configuration.AutoDetectChangesEnabled = false;

                        this.logger.LogInformation(
                            $"Processing ({ctr++}/{total}) AggregateID='{aggregateId}' AND Sequence={sequence}");

                        var obj = JsonConvert.DeserializeObject<FileAttachedEvent>(attachedEvent.EventJson);

                        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                        var newEventJson = JsonConvert.SerializeObject(obj, settings);

                        dbContext.Database.ExecuteSqlCommand(
                            "UPDATE EventRecordWithGuidIds SET EventJson=@newEventJson WHERE AggregateId=@aggregateId AND Sequence=@sequence",
                            new SqlParameter("@newEventJson", newEventJson),
                            new SqlParameter("@aggregateId", aggregateId),
                            new SqlParameter("@sequence", sequence));

                        if (ctr % 500 == 0)
                        {
                            this.MemoryCleanup();
                        }
                    }
                    catch (OutOfMemoryException ex)
                    {
                        this.logger.LogError(ex, ex.Message);
                        Environment.FailFast($"Out of Memory: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.SetProcessedFlag(dbContext, aggregateId, sequence, -2);
                        this.logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        dbContext.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
            }

            this.logger.LogInformation("Migration DONE: Removed file content from event json.");
        }

        private void ProcessEvents(List<AttachedFile> eventsToProcess)
        {
            int total = eventsToProcess.Count;
            this.logger.LogInformation($"Retrieved {total} Events {DateTime.Now}");

            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            int ctr = 1;
            foreach (var attachedEvent in eventsToProcess)
            {
                var jobContext = new UBindDbContext(this.connectionString);
                jobContext.Configuration.AutoDetectChangesEnabled = false;

                var aggregateId = attachedEvent.AggregateId;
                var sequence = attachedEvent.Sequence;

                this.logger.LogInformation(
                   $"PROCESSING ({ctr++}/{total}) AggregateID='{aggregateId}' AND Sequence={sequence}");
                try
                {
                    var eventRecord = jobContext.EventRecordsWithGuidIds.AsNoTracking()
                        .FirstOrDefault(e => e.AggregateId == aggregateId && e.Sequence == sequence);

                    if (eventRecord == null)
                    {
                        continue;
                    }

                    var obj = JsonConvert.DeserializeObject<DbFileAttachedEvent>(eventRecord.EventJson);

                    var quoteId = obj.QuoteId;
                    var attachmentId = obj.AttachmentId;
                    var name = obj.Name;
                    var type = obj.Type;
                    var newGuid = Guid.NewGuid();

                    this.logger.LogInformation($"obj.FileContentId={obj.FileContentId}, newGuid={newGuid}");
                    var fileContentId = obj.FileContentId != default ? obj.FileContentId : newGuid;
                    var fileContent = FileContent.CreateFromBase64String(obj.TenantId, fileContentId, obj.Content ?? string.Empty);
                    var createdTimestamp = obj.Timestamp;
                    var fileSize = fileContent.Size;

                    // Inserts new file content and returns the new/existing file content ID
                    fileContentId = this.UpsertFileContent(fileContent, jobContext);
                    this.logger.LogInformation($"FileContent updated: {fileContentId}");

                    var updateFileInfo =
                        $"UPDATE {this.tmpTable} SET FileContentId='{fileContentId}', FileSize={fileSize} " +
                        $"WHERE AggregateId='{aggregateId}' AND Sequence={sequence}";
                    jobContext.Database.ExecuteSqlCommand(updateFileInfo);

                    var quoteFileAttachment = new QuoteFileAttachment(
                        obj.TenantId,
                        attachmentId,
                        quoteId,
                        fileContentId,
                        name,
                        type,
                        fileSize,
                        createdTimestamp);
                    this.UpsertQuoteFileAttachment(quoteFileAttachment, jobContext);

                    this.SetProcessedFlag(jobContext, aggregateId, sequence, 1);

                    jobContext.SaveChanges();
                    if (ctr % 100 == 0)
                    {
                        this.MemoryCleanup();
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.SetProcessedFlag(jobContext, aggregateId, sequence, -1);
                    this.logger.LogError(ex, ex.Message);
                }
                finally
                {
                    jobContext.Configuration.AutoDetectChangesEnabled = true;
                }
            }
        }

        private void DoProcessEventJsonBatch(List<AttachedFile> eventsToProcess)
        {
            const int releaseVersion = 2;
            int total = eventsToProcess.Count;
            this.logger.LogInformation($"ProcessEventJsonBatch Retrieved {total} Events {DateTime.Now}");

            int ctr = 1;
            foreach (var attachedEvent in eventsToProcess)
            {
                var jobContext = new UBindDbContext(this.connectionString);
                jobContext.Configuration.AutoDetectChangesEnabled = false;

                var aggregateId = attachedEvent.AggregateId;
                var sequence = attachedEvent.Sequence;

                this.logger.LogInformation(
                   $"PROCESSING ({ctr++}/{total}) AggregateID='{aggregateId}' AND Sequence={sequence}");
                try
                {
                    using (var dbContext = new UBindDbContext(this.connectionString))
                    {
                        dbContext.Configuration.AutoDetectChangesEnabled = false;
                        EventRecordWithGuidId record = dbContext.EventRecordsWithGuidIds.AsNoTracking()
                        .FirstOrDefault(e => e.AggregateId == aggregateId && e.Sequence == sequence);

                        if (record == null)
                        {
                            this.SetProcessedFlag(dbContext, aggregateId, sequence, -3);
                            continue;
                        }

                        var eventObj = JsonConvert.DeserializeObject<FileAttachedEvent>(record.EventJson);
                        var newEventObj = new FileAttachedEvent(
                            eventObj.TenantId,
                            eventObj.AggregateId,
                            eventObj.QuoteId,
                            eventObj.AttachmentId,
                            eventObj.Name,
                            eventObj.Type,
                            attachedEvent.FileContentId,
                            attachedEvent.FileSize,
                            eventObj.PerformingUserId.GetValueOrDefault(),
                            eventObj.Timestamp);

                        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                        var newEventJson = JsonConvert.SerializeObject(newEventObj, settings);

                        dbContext.Database.ExecuteSqlCommand(
                           "UPDATE EventRecordWithGuidIds SET EventJson=@newEventJson WHERE AggregateId=@aggregateId AND Sequence=@sequence;" +
                           "UPDATE TmpFileAttachedEvents SET IsProcessed=@releaseVersion WHERE AggregateId=@aggregateId AND Sequence=@sequence",
                           new SqlParameter("@newEventJson", newEventJson),
                           new SqlParameter("@aggregateId", aggregateId),
                           new SqlParameter("@sequence", sequence),
                           new SqlParameter("@releaseVersion", releaseVersion));

                        if (ctr % 500 == 0)
                        {
                            this.ShrinkLog();
                            this.MemoryCleanup();
                        }
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    this.logger.LogError(ex, ex.Message);
                    Environment.FailFast($"Out of Memory: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.SetProcessedFlag(jobContext, aggregateId, sequence, -1);
                    this.logger.LogError(ex, ex.Message);
                }
            }
        }

        private void ShrinkLog()
        {
            using (var dbContext = new UBindDbContext(this.connectionString))
            {
                this.logger.LogInformation("Shrinking DB log...");
                dbContext.Database.ExecuteSqlCommand(
                    TransactionalBehavior.DoNotEnsureTransaction, "DBCC SHRINKFILE (2, TRUNCATEONLY)");
            }
        }

        private Guid UpsertFileContent(FileContent fileContent, IUBindDbContext dbContext)
        {
            var sql = "INSERT INTO FileContents (Id, Content, HashCode, TenantId) SELECT @Id, @Content, @HashCode, @TenantId " +
                "WHERE NOT EXISTS(SELECT 1 FROM FileContents WHERE TenantId=@TenantId AND HashCode=@HashCode OR Id=@id);" +
                "SELECT TOP 1 Id FROM FileContents WHERE TenantId=@TenantId AND HashCode=@HashCode OR Id=@id ORDER BY Id";
            var id = dbContext.Database.SqlQuery<Guid>(
                sql,
                new SqlParameter("@Id", fileContent.Id),
                new SqlParameter("@Content", fileContent.Content),
                new SqlParameter("@HashCode", fileContent.HashCode),
                new SqlParameter("@TenantId", fileContent.TenantId))
                .Single();

            return id;
        }

        private void UpsertQuoteFileAttachment(QuoteFileAttachment quoteFileAttachment, IUBindDbContext jobContext)
        {
            var sqlUpsert = @"IF NOT EXISTS(SELECT 1 FROM QuoteFileAttachments WHERE Id = @Id) 
BEGIN 
    INSERT INTO QuoteFileAttachments(Id, QuoteId, FileContentId, Name, Type, FileSize, CreatedTicksSinceEpoch, Discriminator) 
    VALUES(@id, @quoteId, @fileContentId, @name, @type, @fileSize, @createdTicksSinceEpoch, @discriminator) 
END";
            jobContext.Database.ExecuteSqlCommand(
                sqlUpsert,
                new SqlParameter("@id", quoteFileAttachment.Id),
                new SqlParameter("@quoteId", quoteFileAttachment.QuoteId),
                new SqlParameter("@fileContentId", quoteFileAttachment.FileContentId),
                new SqlParameter("@name", quoteFileAttachment.Name),
                new SqlParameter("@type", quoteFileAttachment.Type),
                new SqlParameter("@fileSize", quoteFileAttachment.FileSize),
                new SqlParameter("@createdTicksSinceEpoch", quoteFileAttachment.CreatedTicksSinceEpoch),
                new SqlParameter("@discriminator", quoteFileAttachment.GetType().Name));
        }

        private void SetProcessedFlag(IUBindDbContext jobContext, Guid aggregateId, int sequence, int isProcessed)
        {
            jobContext.Database.ExecuteSqlCommand(
                $"UPDATE {this.tmpTable} SET IsProcessed={isProcessed} WHERE AggregateId='{aggregateId}' AND Sequence={sequence}");
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

        private decimal GetLogSize()
        {
            const string sqlGetLogSize = "SELECT size/128.0 AS CurrentSizeMB FROM sys.database_files WHERE type=1";
            var logSize = this.dbContext.Database.SqlQuery<decimal>(sqlGetLogSize).Single();
            return logSize;
        }

        private bool TableExists(string tableName)
        {
            bool exists = this.dbContext.Database
                     .SqlQuery<int?>($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'")
                     .SingleOrDefault() != null;
            return exists;
        }

        private class AttachedFile
        {
            public Guid AggregateId { get; set; }

            public int Sequence { get; set; }

            public Guid FileContentId { get; set; }

            public long FileSize { get; set; }
        }

        private class DbFileAttachedEvent : FileAttachedEvent
        {
            public DbFileAttachedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid attachmentId,
                string name,
                string type,
                Guid fileContentId,
                long fileSize,
                Guid performingUserId,
                NodaTime.Instant createdTimestamp)
                : base(tenantId, aggregateId, quoteId, attachmentId, name, type, fileContentId, fileSize, performingUserId, createdTimestamp)
            {
            }

            public string Content { get; set; }
        }
    }
}
