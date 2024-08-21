// <copyright file="InsertQuoteAndClaimFileContentsByTenantCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class InsertQuoteAndClaimFileContentsByTenantCommandHandler
        : ICommandHandler<InsertQuoteAndClaimFileContentsByTenantCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ILogger<InsertQuoteAndClaimFileContentsByTenantCommandHandler> logger;
        private readonly string tmpTable = "TmpQuoteClaimFileContents";
        private readonly int? defaultDbTimeout;

        public InsertQuoteAndClaimFileContentsByTenantCommandHandler(
            IUBindDbContext dbContext,
            IBackgroundJobClient backgroundJobClient,
            ILogger<InsertQuoteAndClaimFileContentsByTenantCommandHandler> logger)
        {
            this.dbContext = dbContext;
            this.defaultDbTimeout = this.dbContext.Database.CommandTimeout;
            this.backgroundJobClient = backgroundJobClient;
            this.logger = logger;
        }

        public Task<Unit> Handle(InsertQuoteAndClaimFileContentsByTenantCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.backgroundJobClient.Enqueue(() => this.InsertQuoteAndClaimFileContents());
            return Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// Insert quote and claim file contents.
        /// </summary>
        [JobDisplayName("Startup Job: Insert quote and claim file contents")]
        public void InsertQuoteAndClaimFileContents()
        {
            this.logger.LogInformation($"Started {DateTime.Now}");
            this.CreateTempTableForProcessing();
            this.InsertFileContents();
            this.UpdateEventRecords();
            this.dbContext.Database.ExecuteSqlCommand($"DROP TABLE IF EXISTS {this.tmpTable}");
            this.logger.LogInformation("DONE.");
        }

        private void CreateTempTableForProcessing()
        {
            this.logger.LogInformation($"Creating temp table for processing...");
            var createTempTableSql = $@"IF (SELECT 1 FROM INFORMATION_SCHEMA.TABLES AS T
WHERE T.TABLE_NAME = '{this.tmpTable}') IS NULL
SELECT NEWID() NewFileContentId, *, CAST('' AS VARCHAR(20)) Status
INTO {this.tmpTable}
FROM (
	SELECT fc.Id FileContentId, a.TenantId, q.AggregateId, '$.FileContentId' JsonPath
	FROM QuoteFileAttachments a
	INNER JOIN FileContents fc ON a.FileContentId=fc.Id
	INNER JOIN Quotes q ON q.Id=a.QuoteId
	WHERE fc.TenantId <> a.TenantId
	UNION
	SELECT fc.Id FileContentId, a.TenantId, a.ClaimId AggregateId, '$.Attachment.FileContentId' JsonPath
	FROM ClaimFileAttachments a INNER JOIN FileContents fc ON a.FileContentId=fc.Id
	WHERE fc.TenantId <> a.TenantId
	UNION
	SELECT fc.Id FileContentId, a.TenantId, a.PolicyId AggregateId, '$.Document.FileContentId' JsonPath
	FROM QuoteDocumentReadModels a INNER JOIN FileContents fc ON a.FileContentId=fc.Id
	WHERE fc.TenantId <> a.TenantId) u";

            this.dbContext.Database.CommandTimeout = 0; // No timeout

            this.dbContext.Database.ExecuteSqlCommand(createTempTableSql);

            this.dbContext.Database.CommandTimeout = this.defaultDbTimeout;
            this.logger.LogInformation($"Temp table created.");
        }

        private void InsertFileContents()
        {
            var sql = @"INSERT INTO FileContents(Id, Content, HashCode, TenantId)
SELECT DISTINCT TOP (100) t.NewFileContentId, f.Content, f.HashCode, t.TenantId FROM TmpQuoteClaimFileContents t WITH (NOLOCK)
INNER JOIN FileContents f WITH (NOLOCK) ON f.Id = t.FileContentId
WHERE NOT EXISTS(SELECT 1 FROM FileContents fc WITH (NOLOCK) WHERE fc.Id = t.NewFileContentId)";

            this.logger.LogInformation($"Inserting into file contents...");
            this.dbContext.Database.CommandTimeout = 0; // No timeout
            int rows = 1;
            while (rows > 0)
            {
                rows = this.dbContext.Database.ExecuteSqlCommand(sql);
                this.logger.LogInformation($"Inserted {rows} file contents.");
            }

            this.dbContext.Database.CommandTimeout = this.defaultDbTimeout;
            this.logger.LogInformation($"Insert file contents DONE.");
        }

        private void UpdateEventRecords()
        {
            var sqlSetRecordsToProcess =
                @"IF (SELECT COUNT(*) FROM TmpQuoteClaimFileContents WHERE Status='Active') = 0 
UPDATE TOP (50) TmpQuoteClaimFileContents SET Status='Active' WHERE Status=''";
            var sqlSetRecordsToFinished =
                @"UPDATE TmpQuoteClaimFileContents SET Status='Finished' WHERE Status='Active'";
            var sqlUpdateEventRecords = @"UPDATE e
SET EventJson = JSON_MODIFY(EventJson, JsonPath, convert(nvarchar(36), NewFileContentId))
FROM EventRecordWithGuidIds e
INNER JOIN TmpQuoteClaimFileContents t ON t.AggregateId = e.AggregateId
WHERE e.EventJson LIKE '%FileContentId%' AND t.Status='Active'";

            this.dbContext.Database.CommandTimeout = 0; // No timeout
            var rows = this.dbContext.Database.ExecuteSqlCommand(sqlSetRecordsToProcess);
            while (rows > 0)
            {
                try
                {
                    this.logger.LogInformation($"Updating event records for {rows} file contents...");
                    var updatedRows = this.dbContext.Database.ExecuteSqlCommand(sqlUpdateEventRecords);
                    this.dbContext.Database.ExecuteSqlCommand(sqlSetRecordsToFinished);
                    this.logger.LogInformation($"Updated {updatedRows} event records.");

                    rows = this.dbContext.Database.ExecuteSqlCommand(sqlSetRecordsToProcess);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"UpdateEventRecords Error: {ex}");
                }
            }

            this.dbContext.Database.CommandTimeout = this.defaultDbTimeout;
            this.logger.LogInformation($"Update event records DONE.");
        }
    }
}
