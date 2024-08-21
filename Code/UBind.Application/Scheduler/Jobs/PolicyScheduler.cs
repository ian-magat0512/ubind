// <copyright file="PolicyScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Commands.Policy;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This is the job to update the policy state every 5 mins,
    /// base on the time calculated.
    /// </summary>
    public class PolicyScheduler : BaseEntityScheduler, IEntityScheduler<Policy>
    {
        private readonly ILogger<PolicyScheduler> logger;

        public PolicyScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJob,
            ICqrsMediator mediator,
            ILogger<PolicyScheduler> logger)
            : base(tenantRepository, storageConnection, recurringJob, mediator)
        {
            this.logger = logger;
        }

        public override void CreateStateChangeJob()
        {
            var tenants = this.RetrieveTenantIdsForUpdate();
            var command = new UpdatePolicyStateFromRecentlyStateChangedCommand(tenants);
            this.RecurringJobManager.AddOrUpdate<PolicyScheduler>(
                        this.GetRecurringJobId(),
                        (c) => this.ExecutePolicyStateChangeCommand(command, CancellationToken.None),
                        "*/5 * * * *");
        }

        [JobDisplayName("Policy State Updater Job")]
        public async Task ExecutePolicyStateChangeCommand(
            UpdatePolicyStateFromRecentlyStateChangedCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                // this lock stops 2 policy state updater jobs from running at the same time.
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "Policy State Updater Job", TimeSpan.FromSeconds(1)))
                {
                    await this.Mediator.Send(command, cancellationToken);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running and hasn't completed within the 5 minute lock timeout.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous Policy State Updater Job is still running, so not executing another.");
            }
        }

        public override string GetRecurringJobId()
        {
            return $"policy-state-scheduler";
        }
    }
}
