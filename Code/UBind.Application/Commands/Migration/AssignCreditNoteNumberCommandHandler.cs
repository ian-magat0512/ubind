// <copyright file="AssignCreditNoteNumberCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Releases;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence;

    /// <summary>
    /// Assign a credit note by finding all cancellation and adjustment quotes whose
    /// total payable is less than zero and credit note is not issued.
    /// </summary>
    public class AssignCreditNoteNumberCommandHandler : ICommandHandler<AssignCreditNoteNumberCommand, Unit>
    {
        private readonly IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository;
        private readonly IUBindDbContext dbContext;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IAccountingTransactionService accountingTransactionService;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ILogger<FileContentRepository> logger;
        private readonly string connectionString;
        private readonly string temporaryTable = "TmpQuotesWithOutcreditNotes";
        private int notimeOut;

        public AssignCreditNoteNumberCommandHandler(
            IUBindDbContext dbContext,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository,
            IAccountingTransactionService accountingTransactionService,
            IReleaseQueryService releaseQueryService,
            ILogger<FileContentRepository> logger)
        {
            this.dbContext = dbContext;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.accountingTransactionService = accountingTransactionService;
            this.releaseQueryService = releaseQueryService;
            this.logger = logger;
            this.connectionString = dbContext.Database.Connection.ConnectionString;
            this.notimeOut = 0;
        }

        public async Task<Unit> Handle(AssignCreditNoteNumberCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"Starting Assign credit note.");
            this.PopulateTemporaryTable();
            await this.AssignCreditNotes();
            this.DropTempTableIfNoUnProcessedQuotes();
            this.logger.LogInformation($"Migration for assign credit note complete.");
            return await Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// Process quotes .
        /// </summary>
        [JobDisplayName("Startup Job: AssignCreditNotes")]
        public async Task AssignCreditNotes()
        {
            var sqlCount = $"SELECT COUNT(*) FROM {this.temporaryTable} WITH (NOLOCK) WHERE IsProcessed=0";
            var totalRows = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}");
            var query = $@"SELECT TenantId, QuoteId FROM {this.temporaryTable} WHERE IsProcessed=0 order by TenantId";
            var quotesWithOutcreditNotes = this.dbContext.Database.SqlQuery<QuoteWithoutCreditNote>(query).ToList();
            int counter = 0;
            foreach (var quote in quotesWithOutcreditNotes)
            {
                this.logger.LogInformation($"Processing credit note migration: {counter + 1} of {quotesWithOutcreditNotes.Count}");
                this.logger.LogInformation($"Issue credit note for tenant Id {quote.TenantId}, quote Id {quote.QuoteId}");
                await this.IssueCreditNoteAsync(quote.TenantId, quote.QuoteId);
                await Task.Delay(2000);
                counter++;
                if (counter % 100 == 0)
                {
                    this.ShrinkLog();
                    this.MemoryCleanup();
                }
            }
        }

        private void PopulateTemporaryTable()
        {
            if (this.TableExists(this.temporaryTable) && this.GetNumberOfUnprocessedQuote() > 0)
            {
                return;
            }

            this.logger.LogInformation($"populating temporary table.");
            var timeOut = this.dbContext.Database.CommandTimeout;
            this.dbContext.Database.CommandTimeout = this.notimeOut;
            try
            {
                var populateTemporaryTableQuery = $@"SELECT
                [Quote].[TenantId] as TenantId,
                [Quote].[Id] AS QuoteId,
                0 AS isProcessed
	            INTO {this.temporaryTable}
                FROM [dbo].[Quotes] AS [Quote]
                WHERE ([Quote].[IsCreditNoteIssued] = 0) AND 
                ([Quote].[CreditNoteNumber] IS NULL) AND (([Quote].[Type] = 3) OR ([Quote].[Type] = 2)) 
                AND ([Quote].[QuoteNumber] IS NOT NULL) AND ([Quote].[IsDiscarded] <> 1)";
                this.dbContext.Database.ExecuteSqlCommand(populateTemporaryTableQuery);
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = timeOut;
            }
        }

        private async Task IssueCreditNoteAsync(Guid tenantId, Guid quoteId)
        {
            var quoteAggregate = this.quoteAggregateResolver.GetQuoteAggregateForQuote(tenantId, quoteId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            if (quote == null || quote.LatestCalculationResult == null || quote?.LatestCalculationResult?.Data.PayablePrice.TotalPayable >= 0)
            {
                this.MarkQuotesWithoutCreditNotesAsComplete(tenantId, quoteId);
                return;
            }

            if (quote?.LatestCalculationResult?.Data.PayablePrice.TotalPayable < 0)
            {
                if (!quote.CreditNoteIssued)
                {
                    var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                        tenantId,
                        quote.Aggregate.ProductId,
                        quote.Aggregate.Environment,
                        quote.ProductReleaseId);
                    await this.accountingTransactionService.IssueCreditNote(releaseContext, quote, true);
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                    this.MarkQuotesWithoutCreditNotesAsComplete(tenantId, quoteId);
                }
                else
                {
                    var quoteReadModel = this.quoteReadModelRepository.GetById(tenantId, quoteId);
                    if (!quoteReadModel.IsCreditNoteIssued)
                    {
                        quoteReadModel.RecordCreditNoteIssued(quote.CreditNoteNumber, quote.CreditNote.CreatedTimestamp);
                        await this.quoteAggregateRepository.Save(quoteAggregate);
                    }

                    this.MarkQuotesWithoutCreditNotesAsComplete(tenantId, quoteId);
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

        private void MarkQuotesWithoutCreditNotesAsComplete(Guid tenantId, Guid quoteId)
        {
            string updateQuery = $@"update {this.temporaryTable} set isProcessed = 1 
						where TenantId = '{tenantId}'
						and QuoteId = '{quoteId}'";
            this.dbContext.Database.ExecuteSqlCommand(updateQuery);
        }

        private void DropTempTableIfNoUnProcessedQuotes()
        {
            if (this.GetNumberOfUnprocessedQuote() == 0)
            {
                string dropTableQuery = $@"DROP TABLE IF EXISTS {this.temporaryTable};";
                this.dbContext.Database.ExecuteSqlCommand(dropTableQuery);
            }
        }

        private int GetNumberOfUnprocessedQuote()
        {
            var sqlCount = $"SELECT COUNT(*) FROM {this.temporaryTable} WITH (NOLOCK) WHERE IsProcessed=0";
            var unProcessedQuotes = this.dbContext.Database.SqlQuery<int>(sqlCount).Single();
            return unProcessedQuotes;
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

        private bool TableExists(string tableName)
        {
            bool exists = this.dbContext.Database
                     .SqlQuery<int?>($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'")
                     .SingleOrDefault() != null;
            return exists;
        }

        private double BytesToGb(long bytes)
        {
            return bytes * 1E-9;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    internal class QuoteWithoutCreditNote
#pragma warning restore SA1402 // File may only contain a single type
    {
        public Guid TenantId { get; set; }

        public Guid QuoteId { get; set; }
    }
}
