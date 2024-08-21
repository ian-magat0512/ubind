// <copyright file="PolicyLatestExpiryDateTimeMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Transactions;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    public class PolicyLatestExpiryDateTimeMigration : IPolicyLatestExpiryDateTimeMigration
    {
        private readonly ILogger<PolicyLatestExpiryDateTimeMigration> logger;
        private readonly IUBindDbContext ubindDbContext;
        private readonly string connectionString;

        public PolicyLatestExpiryDateTimeMigration(
            IUBindDbContext ubindDbContext,
            ILogger<PolicyLatestExpiryDateTimeMigration> logger)
        {
            this.logger = logger;
            this.ubindDbContext = ubindDbContext;
            this.connectionString = this.ubindDbContext.Database.Connection.ConnectionString;
        }

        public async Task ProcessUpdatingPolicyLatestExpiryDateTime(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting migration of policy latest expiry date time..");
            await RetryPolicyHelper.ExecuteAsync<Exception>(async () => await this.ProcessBatchUpdate(cancellationToken), maxJitter: 2000);
        }

        [JobDisplayName("Startup Job: ProcessUpdatingPolicyLatestExpiryDateTime Process Batch {0}")]
        public async Task ProcessBatchUpdate(CancellationToken cancellationToken)
        {
            int batch = 1;
            int batchSize = 100;
            var fromQuery = $"FROM PolicyReadModels prm LEFT JOIN " +
                                $"(SELECT p.* FROM " +
                                    $"(SELECT pt.Id, pt.PolicyId, pt.ExpiryDateTime, pt.ExpiryTicksSinceEpoch, " +
                                        $"ROW_NUMBER() OVER(PARTITION BY pt.PolicyId " +
                                        $"ORDER BY pt.ExpiryDateTime DESC) AS RowNum " +
                                    $"FROM PolicyTransactions pt) p " +
                                $"WHERE p.RowNum = 1) pTrans " +
                            $"ON prm.Id = pTrans.PolicyId " +
                            $"WHERE prm.ExpiryDateTime <> pTrans.ExpiryDateTime";

            // this will break if there's no remaining policies.
            while (true)
            {
                using (var dbContext = new UBindDbContext(this.connectionString))
                {
                    using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted },
                    TransactionScopeAsyncFlowOption.Enabled)) // Enabled to support async
                    {
                        var totalRows = dbContext.Database.SqlQuery<int>($"select count(*) " +
                                                                            $"{fromQuery}").SingleOrDefault();
                        this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");
                        if (totalRows == 0)
                        {
                            break;
                        }

                        var policies = dbContext.Database.SqlQuery<PolicyReadModel>($"select TOP {batchSize} " +
                                                                                            $"prm.Id, " +
                                                                                            $"pTrans.ExpiryDateTime, " +
                                                                                            $"pTrans.ExpiryTicksSinceEpoch " +
                                                                                        $"{fromQuery}").ToList();
                        foreach (var policy in policies)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            this.SetPolicyExpiryDateOnPolicyReadModel(policy, dbContext);
                            await Task.Delay(2000, cancellationToken);
                        }

                        scope.Complete();
                    }

                    batch++;
                }
            }

            this.logger.LogInformation("====== UPDATE ENDS ======");
        }

        private void SetPolicyExpiryDateOnPolicyReadModel(PolicyReadModel policy, UBindDbContext dbContext)
        {
            this.logger.LogInformation($"Updating policy with policy Id: {policy.Id}, " +
                                $"set ExpiryDateTime: {policy.ExpiryDateTime} and " +
                                $" ExpiryTicksSinceEpoch: {policy.ExpiryTicksSinceEpoch}");
            var updateSql = "UPDATE dbo.PolicyReadModels SET "
            + $"ExpiryDateTime = '{policy.ExpiryDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}', "
            + $"ExpiryTicksSinceEpoch = {policy.ExpiryTicksSinceEpoch} "
            + $"WHERE Id = '{policy.Id}' ";
            dbContext.Database.ExecuteSqlCommand(updateSql);
        }

        private class PolicyReadModel
        {
            public Guid Id { get; set; }

            public DateTime ExpiryDateTime { get; set; }

            public long ExpiryTicksSinceEpoch { get; set; }
        }
    }
}
