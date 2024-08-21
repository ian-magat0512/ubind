// <copyright file="PolicyLatestRenewalEffectiveTimeMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    /// <summary>
    /// This a migration to set the latest renewal effective time
    /// on existing policy data.
    /// </summary>
    public class PolicyLatestRenewalEffectiveTimeMigration : IPolicyLatestRenewalEffectiveTimeMigration
    {
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<PolicyLatestRenewalEffectiveTimeMigration> logger;

        public PolicyLatestRenewalEffectiveTimeMigration(
           IUBindDbContext dbContext,
           ILogger<PolicyLatestRenewalEffectiveTimeMigration> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void ProcessUpdatingPolicyRenewalEffectiveDate()
        {
            this.logger.LogInformation($"Migration started for policy latest renewal effective time update.");
            RetryPolicyHelper.Execute<Exception>(() => this.ProcessBatchUpdate(1), maxJitter: 2000);
        }

        [JobDisplayName("Startup Job: ProcessUpdatingPolicyRenewalEffectiveDate Process Batch {0}")]
        public void ProcessBatchUpdate(int batch)
        {
            const int batchSize = 5000;
            var fromQuery = $"from " +
                                $"PolicyReadModels p " +
                                $"INNER JOIN PolicyTransactions pt ON p.Id = pt.PolicyId " +
                            $"where " +
                                $"p.LatestRenewalEffectiveTicksSinceEpoch is null and " +
                                $"pt.Discriminator = 'RenewalTransaction' " +
                            $"group by " +
                                $"p.Id";
            var totalRows = this.dbContext.Database.SqlQuery<int>($"select count(*) " +
                                                                    $"from " +
                                                                    $"(select " +
                                                                    $"p.Id " +
                                                                    $"{fromQuery}) p").SingleOrDefault();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");

            var policyIds = this.dbContext.Database.SqlQuery<Guid>($"select TOP {batchSize} " +
                                                                        $"p.Id " +
                                                                    $"{fromQuery}").ToList();

            foreach (var policyId in policyIds)
            {
                var renewalTransaction = this.dbContext.Database.SqlQuery<PolicyTransaction>($"SELECT " +
                                                                        $"Id,     " +
                                                                        $"Discriminator,     " +
                                                                        $"EffectiveTicksSinceEpoch     " +
                                                                    $"FROM  " +
                                                                        $"PolicyTransactions     " +
                                                                    $"WHERE     " +
                                                                        $"PolicyId = '{policyId}'   AND  " +
                                                                        $"Discriminator = 'RenewalTransaction'   " +
                                                                    $"ORDER BY      " +
                                                                        $"EffectiveTicksSinceEpoch desc").FirstOrDefault();

                if (renewalTransaction != null)
                {
                    this.SetPolicyLatestRenewalEffectiveDateOnPolicyReadModel(policyId, renewalTransaction.EffectiveTicksSinceEpoch);
                    this.logger.LogInformation($"Updating policy with policy Id: {policyId}, set latestRenewalEffectiveTicksSinceEpoch: {renewalTransaction.EffectiveTicksSinceEpoch} ");
                    Thread.Sleep(1500);
                }
            }

            if (policyIds.Count == batchSize)
            {
                batch++;
                Thread.Sleep(3000);
                this.ProcessBatchUpdate(batch);
            }
        }

        private void SetPolicyLatestRenewalEffectiveDateOnPolicyReadModel(Guid policyId, long latestRenewalEffectiveTicksSinceEpoch)
        {
            var updateSql = "UPDATE dbo.PolicyReadModels "
                + $"SET LatestRenewalEffectiveTicksSinceEpoch = {latestRenewalEffectiveTicksSinceEpoch} "
                + $"WHERE Id = '{policyId}' ";
            this.dbContext.Database.ExecuteSqlCommand(updateSql);
        }

        private class PolicyTransaction
        {
            public Guid Id { get; set; }

            public string Discriminator { get; set; }

            public long EffectiveTicksSinceEpoch { get; set; }
        }
    }
}
