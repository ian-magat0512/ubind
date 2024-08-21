// <copyright file="SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Data.Entity.Migrations;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Maintenance;
    using UBind.Persistence;

    /// <summary>
    ///  Handler for setting the TotalPayable of existing policyTransactions from LatestCalculationResult.
    ///  This is a migration command called on startup.
    /// </summary>
    public class SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommandHandler
         : SetTotalPayableFromLatestCalculationResultCommandHandlerBase<PolicyTransaction>,
            ICommandHandler<SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand, Unit>
    {
        public SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommandHandler(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection,
            ITenantRepository tenantRepository,
            IDbLogFileMaintenanceService dbLogFileMaintenanceService,
            ILogger<SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommandHandler> logger)
        {
            this.TemporaryTableName = "ProcessedPolicyTransactions";
            this.DbContext = dbContext;
            this.Logger = logger;
            this.TenantRepository = tenantRepository;
            this.Connection = connection;
            this.DbLogFileMaintenanceService = dbLogFileMaintenanceService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand command, CancellationToken cancellationToken)
        {
            await this.Execute("Policy Transactions", cancellationToken);
            return Unit.Value;
        }

        protected override async Task<List<Guid>> SetTotalPayableAndReturnExclusions(List<PolicyTransaction> policyTransactions, CancellationToken cancellationToken)
        {
            List<Guid> policyTransactionsExcluded = new List<Guid>();
            var recordsSavingInterval = (int)PageSize.Normal;
            var numberOfRecordsToSave = 0;
            foreach (var q in policyTransactions)
            {
                int retryTimes = 0;
                async Task SetTotalPayableOfPolicyTransaction(PolicyTransaction policyTransaction, CancellationToken cancellationToken)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        if (policyTransaction.PolicyData != null)
                        {
                            var latestCalculationResult = new CalculationResultReadModel(policyTransaction.PolicyData.SerializedCalculationResult);
                            if (latestCalculationResult.CalculationResult != null && latestCalculationResult.CalculationResult.PayablePrice != null)
                            {
                                policyTransaction.TotalPayable = latestCalculationResult.CalculationResult.PayablePrice.TotalPayable;
                                this.DbContext.PolicyTransactions.AddOrUpdate(policyTransaction);
                                numberOfRecordsToSave++;
                            }
                        }

                        if (policyTransaction.TotalPayable == null || policyTransaction.TotalPayable == 0)
                        {
                            policyTransactionsExcluded.Add(policyTransaction.Id);
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
                        this.Logger.LogError($"ERROR: PolicyTransaction {policyTransaction.Id} for tenant {policyTransaction.TenantId}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                        throw;
                    }

                    await Task.Delay(50, cancellationToken);
                }

                await RetryPolicyHelper.ExecuteAsync<Exception>((ct) => SetTotalPayableOfPolicyTransaction(q, ct), maxJitter: 1500, cancellationToken: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (numberOfRecordsToSave % recordsSavingInterval == 0)
                {
                    await this.DbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                }
            }

            if (numberOfRecordsToSave % recordsSavingInterval != 0)
            {
                await this.SaveChangesForUnsavedRecords(cancellationToken);
                this.Logger.LogInformation($"Save changes, count: {numberOfRecordsToSave % recordsSavingInterval}");
                await this.DbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
            }

            return policyTransactionsExcluded;
        }

        protected override async Task<List<PolicyTransaction>> GetRecordsWithTotalPayableNotSet(Guid tenantId, int batch, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Connection.UBind))
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@tenantId", tenantId);
                parameters.Add("@quoteNascent", QuoteStatus.Nascent.ToString());
                string sql =
                    @$"SELECT TOP {batch} pt.*, 
                        pt.PolicyData_FormData as [FormData],
                        pt.PolicyData_SerializedCalculationResult as [SerializedCalculationResult]
                        FROM dbo.PolicyTransactions AS pt
                        LEFT JOIN {this.TemporaryTableName} AS ppt ON ppt.Id = pt.Id
                        WHERE 
                            ppt.Id IS NULL 
                            AND (pt.TotalPayable IS NULL)
                            AND (pt.PolicyData_SerializedCalculationResult IS NOT NULL)
                            AND (pt.TenantId = @tenantId) 
                        ORDER BY pt.CreatedTicksSinceEpoch;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    null,
                    180,
                    System.Data.CommandType.Text,
                    CommandFlags.Buffered,
                    cancellationToken: cancellationToken);
                var results = await connection.QueryAsync<PolicyTransactionDiscriminator, PolicyTransactionData, PolicyTransaction>(
                    command: command,
                    splitOn: "FormData",
                    map: (data, policyTransactionData) =>
                    {
                        PolicyTransaction result = null;
                        switch (data.Discriminator)
                        {
                            case "NewBusinessTransaction":
                                result = new NewBusinessTransaction(
                                    data.TenantId,
                                    data.Id,
                                    data.PolicyId,
                                    data.QuoteId.Value,
                                    data.QuoteNumber,
                                    data.EventSequenceNumber,
                                    data.EffectiveDateTime,
                                    data.EffectiveTimestamp,
                                    data.ExpiryDateTime,
                                    data.ExpiryTimestamp,
                                    data.CreatedTimestamp,
                                    policyTransactionData,
                                    data.ProductReleaseId);
                                break;
                            case "RenewalTransaction":
                                result = new RenewalTransaction(
                                    data.TenantId,
                                    data.Id,
                                    data.PolicyId,
                                    data.QuoteId,
                                    data.QuoteNumber,
                                    data.EventSequenceNumber,
                                    data.EffectiveDateTime,
                                    data.EffectiveTimestamp,
                                    data.ExpiryDateTime.Value,
                                    data.ExpiryTimestamp.Value,
                                    data.CreatedTimestamp,
                                    policyTransactionData,
                                    data.ProductReleaseId);
                                break;
                            case "CancellationTransaction":
                                result = new CancellationTransaction(
                                    data.TenantId,
                                    data.Id,
                                    data.PolicyId,
                                    data.QuoteId,
                                    data.QuoteNumber,
                                    data.EventSequenceNumber,
                                    data.EffectiveDateTime,
                                    data.EffectiveTimestamp,
                                    data.CreatedTimestamp,
                                    policyTransactionData,
                                    data.ProductReleaseId);
                                break;
                            case "AdjustmentTransaction":
                                result = new AdjustmentTransaction(
                                    data.TenantId,
                                    data.Id,
                                    data.PolicyId,
                                    data.QuoteId,
                                    data.QuoteNumber,
                                    data.EventSequenceNumber,
                                    data.EffectiveDateTime,
                                    data.EffectiveTimestamp,
                                    data.ExpiryDateTime,
                                    data.ExpiryTimestamp,
                                    data.CreatedTimestamp,
                                    policyTransactionData,
                                    data.ProductReleaseId);
                                break;
                            default:
                                result = null;
                                break;
                        }

                        if (result != null)
                        {
                            result.Environment = data.Environment;
                            result.CustomerId = data.CustomerId;
                            result.OwnerUserId = data.OwnerUserId;
                            result.ProductId = data.ProductId;
                            result.OrganisationId = data.OrganisationId;
                            result.IsTestData = data.IsTestData;
                            result.TotalPayable = data.TotalPayable;
                        }

                        return result;
                    });
                var policiesToSet = results.Where(p => p != null).ToList();
                return policiesToSet;
            }
        }

        private class PolicyTransactionDiscriminator : PolicyTransaction
        {
            public string Discriminator { get; set; }
        }
    }
}