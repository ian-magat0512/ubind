// <copyright file="SystemEventRelationshipTableMaintenanceScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Scheduler.Jobs
{
    using System.Threading;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Commands.SystemEvents;
    using UBind.Domain.Attributes;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This scheduler is used to run the job for rebuilding of index for system events
    /// and relationships table if necessary.
    /// The job is ran every night at 10pm Australia (10pm AEST/11PM AEDT).
    /// </summary>
    public class SystemEventRelationshipTableMaintenanceScheduler : BaseEntityScheduler, ISystemEventRelationshipTableMaintenanceScheduler
    {
        private readonly ILogger<SystemEventRelationshipTableMaintenanceScheduler> logger;

        public SystemEventRelationshipTableMaintenanceScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJobManager,
            ILogger<SystemEventRelationshipTableMaintenanceScheduler> logger,
            ICqrsMediator mediator)
            : base(tenantRepository, storageConnection, recurringJobManager, mediator)
        {
            this.logger = logger;
        }

        public override void CreateStateChangeJob()
        {
            var command = new RebuildSystemEventsAndRelationshipsIndexCommand();
            this.RecurringJobManager.AddOrUpdate<SystemEventRelationshipTableMaintenanceScheduler>(
                this.GetRecurringJobId(),
                (c) => this.ExecuteSystemEventDeletionCommand(command, CancellationToken.None),
                Cron.Daily(12)); // run the job at Australia (10pm AEST/11PM AEDT) every night
        }

        [NonRetryOnOperationCanceledException]
        [JobDisplayName("System Events and Relationships Table Index Rebuild Job")]
        public async Task ExecuteSystemEventDeletionCommand(RebuildSystemEventsAndRelationshipsIndexCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // this lock stops 2 jobs from running at the same time
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "System Events and Relationships Table Index Rebuild Job", TimeSpan.FromSeconds(1)))
                {
                    await this.Mediator.Send(command, cancellationToken);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous System Events and Relationships Table Index Rebuild Job is still running, so not executing another.");
            }
        }

        public override string GetRecurringJobId()
        {
            return "system-events-relationships-index-rebuild-scheduler";
        }
    }
}
