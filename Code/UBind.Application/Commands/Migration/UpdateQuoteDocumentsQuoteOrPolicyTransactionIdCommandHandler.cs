// <copyright file="UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Persistence;
    using UBind.Persistence.Extensions;

    public class UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommandHandler : ICommandHandler<UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommand, Unit>
    {
        private const int NoTimeout = 0;
        private readonly ILogger<UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommandHandler> logger;
        private readonly IUBindDbContext dbContext;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly string connectionString;
        private int batchSize = 1000;
        private string tmpTable = "TempDocumentsToUpdate";
        private string tempDocumentsList = "TempDocumentsNotInQuoteOrPolicyTransaction";
        private string tempPolicyTransactionList = "TempPolicyTransactionNoDocuments";

        public UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommandHandler(
            ILogger<UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommandHandler> logger,
            IUBindDbContext dbContext,
            IBackgroundJobClient backgroundJobClient)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.backgroundJobClient = backgroundJobClient;
            this.connectionString = dbContext.Database.Connection.ConnectionString;
        }

        [JobDisplayName("Startup Job: Update Quote Documents Table")]
        public async Task<Unit> Handle(UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommand request, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Started {DateTime.Now}");
            await this.UpdateQuoteDocumentsTable(cancellationToken);
            return Unit.Value;
        }

        public async Task UpdateQuoteDocumentsTable(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Started UpdateQuoteDocumentsTable {DateTime.Now}");

            bool tempTableForDocumentsExists = await this.TableExists(this.tempDocumentsList);
            bool tempReferenceTableExists = await this.TableExists(this.tmpTable);
            bool tempPolicyTransactionAndQuotesListTableExists = await this.TableExists(this.tempPolicyTransactionList);

            if (!tempTableForDocumentsExists && !tempReferenceTableExists)
            {
                this.logger.LogInformation("Start checking Quote Documents with improper tagging");
                await this.PrepareDocumentsToUpdate(cancellationToken);
                this.logger.LogInformation("End checking Quote Documents with improper tagging");
            }

            if (!tempPolicyTransactionAndQuotesListTableExists && !tempReferenceTableExists)
            {
                this.logger.LogInformation("Start getting list of affected quotes and policy transactions");
                await this.GetListOfPolicyTransactionAndQuoteWithoutDocuments(cancellationToken);
                this.logger.LogInformation("End getting list of affected quotes and policy transactions");
            }

            if (!tempReferenceTableExists)
            {
                this.logger.LogInformation("Start creating temp table reference for updating");
                await this.CreateTempTableReferenceForUpdating(cancellationToken);
                this.logger.LogInformation("End creating temp table reference for updating");
            }

            tempReferenceTableExists = await this.TableExists(this.tmpTable);

            if (tempReferenceTableExists)
            {
                await this.ProcessBatch(1);
            }
        }

        public async Task ProcessBatch(int batch)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("IsProcessed", SqlDbType.Int) { Value = 0 });

            int remainingRowsToProcess = await ((UBindDbContext)this.dbContext).GetRowCount(this.tmpTable, parameters);
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {remainingRowsToProcess}, Batch: {batch}, Size: {this.batchSize}");
            if (!this.IsValidTableName(this.tmpTable))
            {
                this.logger.LogError($"Invalid table name: {this.tmpTable}");
                return;
            }
            var sqlBatch = $"SELECT TOP {this.batchSize} * FROM [{this.tmpTable}] WITH (NOLOCK) WHERE IsProcessed=0";

            var documentsToProcess = this.dbContext.Database.SqlQuery<DocumentsToUpdate>(sqlBatch).ToList();
            bool isLastBatch = documentsToProcess.Count < this.batchSize;
            await this.UpdateDocumentsBatch(documentsToProcess, isLastBatch);
            if (documentsToProcess.Count == this.batchSize)
            {
                batch++;
                await this.ProcessBatch(batch);
            }
        }

        public async Task PrepareDocumentsToUpdate(CancellationToken cancellationToken)
        {
            var dbTimeout = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = NoTimeout;

            string sql =
                @$"IF OBJECT_ID('TempDocumentsNotInQuoteOrPolicyTransaction') IS NOT NULL
                    BEGIN
                        DROP TABLE TempDocumentsNotInQuoteOrPolicyTransaction;
                    END

                    SELECT DISTINCT
                        QuoteOrPolicyTransactionId AS Id,
                        PolicyId As PolicyId,
                        FORMAT(dbo.ToDateTime2(CreatedTicksSinceEpoch), 'yyyy-MM-dd hh:mm:00') AS ModifiedDate
                    INTO TempDocumentsNotInQuoteOrPolicyTransaction
                    FROM QuoteDocumentReadModels WITH(NOLOCK)
                    WHERE QuoteOrPolicyTransactionId NOT IN (
		                    SELECT id From dbo.quotes WITH(NOLOCK)
		                    UNION
		                    SELECT Id FROM dbo.PolicyTransactions WITH(NOLOCK)
		                    UNION
		                    SELECT AggregateId FROM dbo.Quotes WITH(NOLOCK)
		                    UNION
		                    SELECT ID FROM [dbo].[QuoteVersionReadModels] WITH(NOLOCK))

                    DELETE FROM TempDocumentsNotInQuoteOrPolicyTransaction
                    WHERE PolicyId NOT IN (
		                    SELECT Id From dbo.PolicyReadModels);";
            await this.dbContext.Database.ExecuteSqlCommandAsync(sql);

            this.dbContext.Database.CommandTimeout = dbTimeout;
        }

        public async Task GetListOfPolicyTransactionAndQuoteWithoutDocuments(CancellationToken cancellationToken)
        {
            var dbTimeout = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = NoTimeout;

            var sql = @"
                    IF OBJECT_ID('TempPolicyTransactionNoDocuments') IS NOT NULL
                    BEGIN
                        DROP TABLE TempPolicyTransactionNoDocuments;
                    END

                    SELECT DISTINCT
                        Id AS Id,
	                    PolicyId AS PolicyId,
	                    FORMAT(dbo.ToDateTime2(CreatedTicksSinceEpoch), 'yyyy-MM-dd hh:mm:00') AS ModifiedDate,
	                    'PolicyTransactions' AS FromTable
                    INTO TempPolicyTransactionNoDocuments
                    FROM PolicyTransactions WITH(NOLOCK)
                    WHERE PolicyId IN (SELECT PolicyId FROM TempDocumentsNotInQuoteOrPolicyTransaction);

                    INSERT INTO TempPolicyTransactionNoDocuments
                    SELECT DISTINCT Id,
	                    PolicyId,
	                    FORMAT(dbo.ToDateTime2(LastModifiedTicksSinceEpoch), 'yyyy-MM-dd hh:mm:00'),
	                    'Quotes'
                    FROM Quotes WITH(NOLOCK)
                    WHERE PolicyId IN (SELECT PolicyId FROM TempDocumentsNotInQuoteOrPolicyTransaction);
";
            await this.dbContext.Database.ExecuteSqlCommandAsync(sql);

            this.dbContext.Database.CommandTimeout = dbTimeout;
        }

        public async Task CreateTempTableReferenceForUpdating(CancellationToken cancellationToken)
        {
            var dbTimeout = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = NoTimeout;

            var sql = @"
                    IF OBJECT_ID('TempDocumentsToUpdate') IS NOT NULL
                    BEGIN
                        DROP TABLE TempDocumentsToUpdate;
                    END
                    
                    SELECT D.Id AS QuoteOrPolicyTransactionId,
	                    PT.Id AS ExpectedQuoteOrPolicyTransactionId,
	                    D.PolicyId AS PolicyId,
	                    PT.FromTable AS FromTable,
	                    D.ModifiedDate AS ModifiedDate,
                        0 AS IsProcessed
                    INTO TempDocumentsToUpdate
                    FROM TempDocumentsNotInQuoteOrPolicyTransaction D
	                    LEFT JOIN TempPolicyTransactionNoDocuments PT ON D.PolicyId = PT.PolicyId
                    WHERE DATEDIFF(HOUR, D.ModifiedDate, Pt.ModifiedDate) BETWEEN -1 AND 1;

                    WITH CTE AS (
                        SELECT 
                            QuoteOrPolicyTransactionId,
		                    FromTable,
                            ROW_NUMBER() OVER (PARTITION BY QuoteOrPolicyTransactionId ORDER BY FromTable) AS RowNumber
                        FROM 
                            TempDocumentsToUpdate
                    )
                    SELECT * FROM CTE WHERE RowNumber > 1;
	
                    WITH CTE AS (
                        SELECT 
                            QuoteOrPolicyTransactionId,
		                    FromTable,
                            ROW_NUMBER() OVER (PARTITION BY QuoteOrPolicyTransactionId ORDER BY FromTable) AS RowNumber
                        FROM 
                            TempDocumentsToUpdate
                    )
                    DELETE FROM CTE WHERE RowNumber > 1;

                    DROP TABLE TempDocumentsNotInQuoteOrPolicyTransaction;
                    DROP TABLE TempPolicyTransactionNoDocuments;";

            await this.dbContext.Database.ExecuteSqlCommandAsync(sql);
            this.dbContext.Database.CommandTimeout = dbTimeout;
        }

        public async Task UpdateDocumentsBatch(IEnumerable<DocumentsToUpdate> documentsToUpdate, bool isLastBatch)
        {
            foreach (var document in documentsToUpdate)
            {
                var jobContext = new UBindDbContext(this.connectionString);
                jobContext.Configuration.AutoDetectChangesEnabled = false;

                using (var dbContext = new UBindDbContext(this.connectionString))
                {
                    try
                    {
                        this.logger.LogInformation($"PROCESSING UPDATE FOR QuoteOrPolicyTransactionId: {document.ExpectedQuoteOrPolicyTransactionId} {DateTime.Now}");
                        string sql = @$"UPDATE QuoteDocumentReadModels
                            SET QuoteOrPolicyTransactionId = @ExpectedQuoteOrPolicyTransactionId
                            WHERE QuoteOrPolicyTransactionId = @QuoteOrPolicyTransactionId";
                        var parameters = new[]
                        {
                            new SqlParameter("@ExpectedQuoteOrPolicyTransactionId", SqlDbType.UniqueIdentifier)
                            {
                                Value = document.ExpectedQuoteOrPolicyTransactionId,
                            },
                            new SqlParameter("@QuoteOrPolicyTransactionId", SqlDbType.UniqueIdentifier)
                            {
                                Value = document.QuoteOrPolicyTransactionId,
                            },
                        };

                        // Convert parameters to an object array
                        object[] parameterArray = parameters;

                        await dbContext.Database.ExecuteSqlCommandAsync(sql, parameterArray);
                        this.SetProcessedFlag(jobContext, document.QuoteOrPolicyTransactionId, document.ExpectedQuoteOrPolicyTransactionId, 1);
                    }
                    catch (OutOfMemoryException ex)
                    {
                        this.logger.LogError(ex, ex.Message);
                        Environment.FailFast($"Out of Memory: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.SetProcessedFlag(jobContext, document.QuoteOrPolicyTransactionId, document.ExpectedQuoteOrPolicyTransactionId, 0);
                        this.logger.LogError(ex, ex.Message);
                    }
                }
            }
            if (isLastBatch)
            {
                this.dbContext.Database.ExecuteSqlCommand($"DROP TABLE [{this.tmpTable}]");
            }
        }

        public async Task<bool> TableExists(string tableName)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("TABLE_NAME", SqlDbType.VarChar) { Value = tableName });

            int totalRows = await ((UBindDbContext)this.dbContext).GetRowCount("INFORMATION_SCHEMA.TABLES", parameters);
            return totalRows > 0;
        }

        public async void SetProcessedFlag(
            IUBindDbContext jobContext,
            Guid quoteOrPolicyTransactionId,
            Guid expectedQuoteOrPolicyTransactionId,
            int isProcessed)
        {
            var commandText =
                @$"UPDATE {this.tmpTable} SET IsProcessed = @IsProcessed
                    WHERE QuoteOrPolicyTransactionId = @QuoteOrPolicyTransactionId
                    AND ExpectedQuoteOrPolicyTransactionId = @ExpectedQuoteOrPolicyTransactionId";
            var parameters = new[]
            {
                new SqlParameter("@IsProcessed", SqlDbType.Int)
                {
                    Value = isProcessed,
                },
                new SqlParameter("@QuoteOrPolicyTransactionId", SqlDbType.UniqueIdentifier)
                {
                    Value = quoteOrPolicyTransactionId,
                },
                new SqlParameter("@ExpectedQuoteOrPolicyTransactionId", SqlDbType.UniqueIdentifier)
                {
                    Value = expectedQuoteOrPolicyTransactionId,
                },
            };

            object[] parameterArray = parameters;

            await jobContext.Database.ExecuteSqlCommandAsync(commandText, parameterArray);
        }

        public bool IsValidTableName(string tableName)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$");
        }

        public class DocumentsToUpdate
        {
            public DocumentsToUpdate() { }

            public Guid QuoteOrPolicyTransactionId { get; set; }

            public Guid ExpectedQuoteOrPolicyTransactionId { get; set; }

            public Guid PolicyTransactionId { get; set; }

            public string? FromTable { get; set; }

            public string? ModifiedDate { get; set; }

            public int IsProcessed { get; set; }
        }
    }
}
