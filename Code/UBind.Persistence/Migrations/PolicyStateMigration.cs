// <copyright file="PolicyStateMigration.cs" company="uBind">
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
    using NodaTime;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    /// <summary>
    /// This is to update the state of all policies per batch.
    /// where the polict state is null and set the policy state
    /// based on the current timestamp. we need to set the state of the policy
    /// to ensure that we dont need to compare the timestamp to get the state of it.
    /// </summary>
    public class PolicyStateMigration : IPolicyStateMigration
    {
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<PolicyStateMigration> logger;
        private readonly IClock clock;

        public PolicyStateMigration(
            IUBindDbContext dbContext,
            ILogger<PolicyStateMigration> logger,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public void ProcessUpdatingPolicyState()
        {
            this.logger.LogInformation($"Migration started for policy state update.");
            this.ProcessBatchUpdate(1);
        }

        [JobDisplayName("Startup Job: ProcessUpdatingPolicyState Process Batch {0}")]
        public void ProcessBatchUpdate(int batch)
        {
            const int batchSize = 5000;
            var currentTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();
            var totalRows = this.dbContext.Database.SqlQuery<int>($"SELECT COUNT(*) FROM PolicyReadModels WHERE InceptionTicksSinceEpoch > 0 AND PolicyState IS NULL").Single();
            this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");

            var policies = this.dbContext.Database.SqlQuery<Policy>($"SELECT TOP {batchSize}  " +
                                                                    $"Id,   " +
                                                                    "case when(CancellationEffectiveTicksSinceEpoch != 0) AND " +
                                                                                    $"((CancellationEffectiveTicksSinceEpoch < {currentTime}) OR  " +
                                                                                        $"(CancellationEffectiveTicksSinceEpoch = InceptionTicksSinceEpoch)) then   " +
                                                                                        $"'Cancelled'   " +
                                                                    $"when((CancellationEffectiveTicksSinceEpoch = 0) OR      " +
                                                                                    $"((CancellationEffectiveTicksSinceEpoch > {currentTime}) AND     " +
                                                                                        $"(CancellationEffectiveTicksSinceEpoch <> InceptionTicksSinceEpoch))) AND    " +
                                                                                $"(InceptionTicksSinceEpoch > {currentTime}) then     " +
                                                                                $"'Pending'     " +
                                                                    $"when((CancellationEffectiveTicksSinceEpoch = 0) OR      " +
                                                                                    $"((CancellationEffectiveTicksSinceEpoch > {currentTime}) AND     " +
                                                                                        $"(CancellationEffectiveTicksSinceEpoch != InceptionTicksSinceEpoch))) AND  " +
                                                                                $"(InceptionTicksSinceEpoch < {currentTime}) AND      " +
                                                                                $"(ExpiryTicksSinceEpoch > {currentTime}) then " +
                                                                                $"'Active'  " +
                                                                    $"ELSE 'Expired' " +
                                                                    $"end State     " +
                                                                $"FROM " +
                                                                    $"PolicyReadModels  " +
                                                                $"WHERE " +
                                                                    $"InceptionTicksSinceEpoch > 0 AND " +
                                                                    $"PolicyState IS NULL")
                .ToList();

            foreach (var policy in policies)
            {
                this.SetPolicyStateOnPolicyReadModelWhenPolicyStateIsNull(policy.Id, policy.State);
                this.logger.LogInformation($"Updating policy state with Id of {policy.Id}, state of {policy.State}");
            }

            if (policies.Count == batchSize)
            {
                batch++;
                this.ProcessBatchUpdate(batch);
            }
        }

        /// <inheritdoc/>
        public void ProcessUpdatePolicyStateFromPendingToIssued()
        {
            this.logger.LogInformation($"Migration started for policy state update from pending to issued.");
            this.ProcessBatchUpdateFromPendingToIssued(1);
        }

        [JobDisplayName("Startup Job: ProcessUpdatePolicyStateFromPendingToIssued Process Batch {0}")]
        public void ProcessBatchUpdateFromPendingToIssued(int batch)
        {
            while (true)
            {
                const int batchSize = 5000;
                var totalRows = this.dbContext.Database.SqlQuery<int>($"SELECT COUNT(*) FROM dbo.PolicyReadModels WHERE PolicyState = 'Pending'").Single();
                this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRows}, Batch: {batch}, Size: {batchSize}");

                this.dbContext.Database.ExecuteSqlCommand($"UPDATE TOP({batchSize}) dbo.PolicyReadModels SET PolicyState = 'Issued' WHERE PolicyState = 'Pending'");

                if (totalRows > 0)
                {
                    batch++;
                    Thread.Sleep(200);
                }
                else
                {
                    break;
                }
            }
        }

        private void SetPolicyStateOnPolicyReadModelWhenPolicyStateIsNull(Guid policyId, string status)
        {
            var updateSql = "UPDATE dbo.PolicyReadModels "
                + $"SET PolicyState = '{status}' "
                + $"WHERE Id = '{policyId}' ";
            this.dbContext.Database.ExecuteSqlCommand(updateSql);
        }

        private class Policy
        {
            public Guid Id { get; set; }

            public string State { get; set; }
        }
    }
}
