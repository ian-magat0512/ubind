// <copyright file="SystemEventDeletionScheduler.cs" company="uBind">
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
    using UBind.Domain.Events;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This is job is used to run the deletion of system events
    /// whose expiry timestamp has passed.
    /// The job is ran every hour.
    /// </summary>
    public class SystemEventDeletionScheduler : BaseEntityScheduler, IEntityScheduler<SystemEvent>
    {
        private readonly ILogger<SystemEventDeletionScheduler> logger;

        public SystemEventDeletionScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJobManager,
            ILogger<SystemEventDeletionScheduler> logger,
            ICqrsMediator mediator)
            : base(tenantRepository, storageConnection, recurringJobManager, mediator)
        {
            this.logger = logger;
        }

        public override void CreateStateChangeJob()
        {
            var command = new DeleteExpiredSystemEventsCommand();
            this.RecurringJobManager.AddOrUpdate<SystemEventDeletionScheduler>(
                        this.GetRecurringJobId(),
                        (c) => this.ExecuteSystemEventDeletionCommand(command, CancellationToken.None),
                        Cron.Hourly);
        }

        [NonRetryOnOperationCanceledException]
        [JobDisplayName("System Events Deletion Job")]
        public async Task ExecuteSystemEventDeletionCommand(DeleteExpiredSystemEventsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // this lock stops 2 System Events Deletion jobs from running at the same time
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "System Events Deletion Job", TimeSpan.FromSeconds(1)))
                {
                    await this.Mediator.Send(command, cancellationToken);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous System Events Deletion Job is still running, so not executing another.");
            }
        }

        public override string GetRecurringJobId()
        {
            return "system-events-deletion-scheduler";
        }
    }
}
