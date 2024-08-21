// <copyright file="SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Data.Entity.Migrations;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Dapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Maintenance;
    using UBind.Persistence;

    /// <summary>
    ///  Handler for setting the TotalPayable of existing quotes from LatestCalculationResult.
    ///  This is a migration command called on startup.
    /// </summary>
    public class SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommandHandler
         : SetTotalPayableFromLatestCalculationResultCommandHandlerBase<NewQuoteReadModel>,
        ICommandHandler<SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand, Unit>
    {
        public SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommandHandler(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection,
            ITenantRepository tenantRepository,
            IDbLogFileMaintenanceService dbLogFileMaintenanceService,
            ILogger<SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommandHandler> logger)
        {
            this.TemporaryTableName = "ProcessedQuotes";
            this.DbContext = dbContext;
            this.Logger = logger;
            this.TenantRepository = tenantRepository;
            this.Connection = connection;
            this.DbLogFileMaintenanceService = dbLogFileMaintenanceService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand command, CancellationToken cancellationToken)
        {
            await this.Execute("Quotes", cancellationToken);
            return Unit.Value;
        }

        protected override async Task<List<Guid>> SetTotalPayableAndReturnExclusions(List<NewQuoteReadModel> quotes, CancellationToken cancellationToken)
        {
            List<Guid> quoteIdsExcluded = new List<Guid>();
            var recordsSavingInterval = (int)PageSize.Normal;
            var numberOfRecordsToSave = 0;
            foreach (var q in quotes)
            {
                int retryTimes = 0;
                async Task SetTotalPayableOfQuote(NewQuoteReadModel quote, CancellationToken cancellationToken)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        var latestCalculationResult = new CalculationResultReadModel(quote.SerializedLatestCalculationResult);
                        if (latestCalculationResult.CalculationResult != null && latestCalculationResult.CalculationResult.PayablePrice != null)
                        {
                            quote.TotalPayable = latestCalculationResult.CalculationResult.PayablePrice.TotalPayable;
                            this.DbContext.QuoteReadModels.AddOrUpdate(quote);
                            numberOfRecordsToSave++;
                        }
                        else
                        {
                            quote.TotalPayable = null;
                            this.DbContext.QuoteReadModels.AddOrUpdate(quote);
                            numberOfRecordsToSave++;
                        }

                        if (quote.TotalPayable == null || quote.TotalPayable == 0)
                        {
                            quoteIdsExcluded.Add(quote.Id);
                        }

                        if (numberOfRecordsToSave % recordsSavingInterval == 0)
                        {
                            await this.DbContext.SaveChangesAsync();
                            this.Logger.LogInformation($"Save changes, count: {recordsSavingInterval}");
                        }

                        await Task.Delay(2000, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogError($"ERROR: Quote {quote.Id} for tenant {quote.TenantId}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                        throw;
                    }

                    await Task.Delay(50, cancellationToken);
                }

                await RetryPolicyHelper.ExecuteAsync<Exception>((c) => SetTotalPayableOfQuote(q, c), maxJitter: 1500, cancellationToken: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (numberOfRecordsToSave % recordsSavingInterval == 0)
                {
                    // approx every 2 minutes
                    await this.DbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                }
            }

            if (numberOfRecordsToSave % recordsSavingInterval != 0)
            {
                await this.SaveChangesForUnsavedRecords(cancellationToken);
                this.Logger.LogInformation($"Save changes, count: {numberOfRecordsToSave % recordsSavingInterval}");
                await this.DbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
            }

            return quoteIdsExcluded;
        }

        protected override async Task<List<NewQuoteReadModel>> GetRecordsWithTotalPayableNotSet(Guid tenantId, int batch, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Connection.UBind))
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@tenantId", tenantId);
                string sql =
                    @$"SELECT TOP {batch} *
                        FROM dbo.Quotes AS q
                        LEFT JOIN {this.TemporaryTableName} AS pq ON pq.Id = q.Id
                        WHERE 
                            pq.Id IS NULL 
                            AND (q.TotalPayable = 0)
                            AND (q.SerializedLatestCalculationResult IS NOT NULL)
                            AND (q.TenantId = @tenantId) 
                            AND (q.IsDiscarded <> 1) 
                        ORDER BY q.CreatedTicksSinceEpoch;";
                var command = new CommandDefinition(
                    sql,
                    parameters,
                    null,
                    180,
                    System.Data.CommandType.Text,
                    CommandFlags.Buffered,
                    cancellationToken: cancellationToken);
                return (await connection.QueryAsync<NewQuoteReadModel>(command)).ToList();
            }
        }
    }
}