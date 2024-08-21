// <copyright file="RedbookUpdaterScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System.Threading;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.DataDownloader;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class RedbookUpdaterScheduler : BaseEntityScheduler, IRedbookUpdaterScheduler
    {
        public const string RedbookUpdaterCronSchedule = "0 12 * * 2";
        private readonly ILogger<RedbookUpdaterScheduler> logger;

        public RedbookUpdaterScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJobManager,
            ILogger<RedbookUpdaterScheduler> logger,
            ICqrsMediator mediator)
            : base(tenantRepository, storageConnection, recurringJobManager, mediator)
        {
            this.logger = logger;
        }

        public override void CreateStateChangeJob()
        {
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Ftp, false);
            var command = new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest);
            this.RecurringJobManager.AddOrUpdate<RedbookUpdaterScheduler>(
                        this.GetRecurringJobId(),
                        (c) => this.ExecuteRedbookUpdaterJobCommand(command, CancellationToken.None),
                        RedbookUpdaterCronSchedule);
        }

        [JobDisplayName("Redbook Updater Job")]
        public async Task ExecuteRedbookUpdaterJobCommand(CreateUpdaterJobCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // this lock stops 2 redbook updater jobs from running at the same time
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "Redbook Updater Job", TimeSpan.FromSeconds(1)))
                {
                    await this.Mediator.Send(command, cancellationToken);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous Redbook Updater Job is still running, so not executing another.");
            }
        }

        public override string GetRecurringJobId()
        {
            return $"redbook-updater-scheduler";
        }
    }
}
