// <copyright file="FixCorruptPolicyDocumentsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class FixCorruptPolicyDocumentsCommandHandler : ICommandHandler<FixCorruptPolicyDocumentsCommand, Unit>
{
    private const int NoTimeout = 0;
    private readonly string tmpTable = "TmpCorruptPolicyDocuments";
    private readonly string tmpCorruptFileContents = "TmpCorruptFileContents";
    private readonly IUBindDbContext dbContext;
    private readonly ILogger<FixCorruptPolicyDocumentsCommandHandler> logger;
    private readonly IBackgroundJobClient backgroundJobClient;

    public FixCorruptPolicyDocumentsCommandHandler(
        IUBindDbContext dbContext,
        ILogger<FixCorruptPolicyDocumentsCommandHandler> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.backgroundJobClient = backgroundJobClient;
    }

    public Task<Unit> Handle(FixCorruptPolicyDocumentsCommand request, CancellationToken cancellationToken)
    {
        this.backgroundJobClient.Enqueue(() => this.ProcessCorruptPolicyDocuments(cancellationToken));
        return Task.FromResult(Unit.Value);
    }

    [JobDisplayName("Fix Corrupt Policy Documents")]
    public async Task ProcessCorruptPolicyDocuments(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Started {DateTime.Now}");
        this.PopulateTempTable();
        var sqlRowsToProcess =
            $"SELECT PolicyId, EmailId FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed<>1 ORDER BY PolicyId";
        var eventsToProcess = this.dbContext.Database.SqlQuery<CorruptPolicyDocuments>(sqlRowsToProcess).ToList();

        int ctr = 0;
        foreach (var policyDoc in eventsToProcess)
        {
            var updateSql = $@"WITH cte AS
(SELECT dc.FileContentId, dc.Name, ea.DocumentFile_Id
FROM Emails e
INNER JOIN EmailAttachments ea on ea.EmailId=e.Id
INNER JOIN DocumentFiles dc on dc.Id=ea.DocumentFile_Id
WHERE e.Id='{policyDoc.EmailId}')
UPDATE qd
SET qd.FileContentId=cte.FileContentId
FROM QuoteDocumentReadModels qd
INNER JOIN cte ON cte.Name=qd.Name
INNER JOIN {this.tmpCorruptFileContents} fc ON fc.Id=qd.FileContentId
WHERE qd.PolicyId='{policyDoc.PolicyId}';
UPDATE {this.tmpTable} SET IsProcessed=1 WHERE PolicyId='{policyDoc.PolicyId}' AND EmailId='{policyDoc.EmailId}';";

            this.dbContext.Database.ExecuteSqlCommand(updateSql);
            this.logger.LogInformation(
                $"{++ctr}/{eventsToProcess.Count} fixed corrupt documents for " +
                $"Policy: {policyDoc.PolicyId} using Email: {policyDoc.EmailId}");

            // Delay every 100 records so we don't overwhelm the database
            if (ctr % 100 == 0)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        this.dbContext.Database.ExecuteSqlCommand($"DROP TABLE IF EXISTS {this.tmpTable};");
        this.dbContext.Database.ExecuteSqlCommand($"DROP TABLE IF EXISTS {this.tmpCorruptFileContents};");
        this.logger.LogInformation($"Finished {DateTime.Now}");
    }

    private void PopulateTempTable()
    {
        if (this.TableExists(this.tmpTable))
        {
            return;
        }

        var dbTimeout = this.dbContext.Database.CommandTimeout;
        try
        {
            this.dbContext.Database.CommandTimeout = NoTimeout;
            this.logger.LogInformation($"Populating temp table '{this.tmpTable}' to process...");

            if (!this.TableExists(this.tmpCorruptFileContents))
            {
                var sqlCorruptFileContents = $"SELECT Id INTO {this.tmpCorruptFileContents} " +
                    $"FROM FileContents WHERE CHECKSUM(Content) = 0";
                this.dbContext.Database.ExecuteSqlCommand(sqlCorruptFileContents);
            }

            var sqlPoliciesWithCorruptDocuments = $@"SELECT DISTINCT qd.PolicyId, r.ToEntityId EmailId, 0 IsProcessed
INTO {this.tmpTable}
FROM {this.tmpCorruptFileContents} fc WITH (NOLOCK)
INNER JOIN QuoteDocumentReadModels qd ON qd.FileContentId=fc.Id
INNER JOIN Relationships r ON r.FromEntityId=qd.PolicyId
	AND r.FromEntityType=1	-- Policy
	AND r.Type=5	-- PolicyMessage";
            this.dbContext.Database.ExecuteSqlCommand(sqlPoliciesWithCorruptDocuments);
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

    private bool TableExists(string tableName)
    {
        bool exists = this.dbContext.Database
            .SqlQuery<int?>($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'")
            .SingleOrDefault() != null;
        return exists;
    }

    private class CorruptPolicyDocuments
    {
        public Guid PolicyId { get; set; }

        public Guid EmailId { get; set; }
    }
}
