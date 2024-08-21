// <copyright file="GlassGuideUpdaterScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Scheduler.Jobs;

using System.Threading;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using UBind.Application.Commands.ThirdPartyDataSets;
using UBind.Application.DataDownloader;
using UBind.Application.ThirdPartyDataSets;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GlassGuideUpdaterScheduler : BaseEntityScheduler, IGlassGuideUpdaterScheduler
{
    public const string GlassGuideUpdaterCronSchedule = "0 12 * * 2";
    private readonly ILogger<GlassGuideUpdaterScheduler> logger;

    public GlassGuideUpdaterScheduler(
        ITenantRepository tenantRepository,
        IStorageConnection storageConnection,
        IRecurringJobManager recurringJobManager,
        ILogger<GlassGuideUpdaterScheduler> logger,
        ICqrsMediator mediator)
        : base(tenantRepository, storageConnection, recurringJobManager, mediator)
    {
        this.logger = logger;
    }

    public override void CreateStateChangeJob()
    {
        var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Sftp, false);
        var command = new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest);
        this.RecurringJobManager.AddOrUpdate<GlassGuideUpdaterScheduler>(
                    this.GetRecurringJobId(),
                    (c) => this.ExecuteGlassGuideUpdaterJobCommand(command, CancellationToken.None),
                    GlassGuideUpdaterCronSchedule);
    }

    [JobDisplayName("Glass's Guide Updater Job")]
    public async Task ExecuteGlassGuideUpdaterJobCommand(CreateUpdaterJobCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // this lock stops 2 Glass's Guide updater jobs from running at the same time
            using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                "GlassGuide Updater Job", TimeSpan.FromSeconds(1)))
            {
                await this.Mediator.Send(command, cancellationToken);
            }
        }
        catch (DistributedLockTimeoutException)
        {
            // A job is already running.
            // In this case, we just catch the exception and return.
            this.logger.LogInformation($"The previous Glass's Guide Updater Job is still running, so not executing another.");
        }
    }

    public override string GetRecurringJobId()
    {
        return $"glassguide-updater-scheduler";
    }
}
