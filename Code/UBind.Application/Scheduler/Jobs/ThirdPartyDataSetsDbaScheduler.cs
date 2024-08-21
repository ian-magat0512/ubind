// <copyright file="ThirdPartyDataSetsDbaScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Scheduler.Jobs;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using UBind.Application.Configuration;
using UBind.Application.Services.Email;
using UBind.Domain.Repositories;
using UBind.Persistence.ThirdPartyDataSets;

/// <summary>
/// Contains the ThirdPartyDataSetsDbaScheduler DBA background monitoring operations.
/// </summary>
public class ThirdPartyDataSetsDbaScheduler : BaseDbaScheduler<ThirdPartyDataSetsDbContext>, IDbaScheduler<ThirdPartyDataSetsDbContext>
{
    public ThirdPartyDataSetsDbaScheduler(
        ILogger<ThirdPartyDataSetsDbaScheduler> logger,
        IDbaRepository<ThirdPartyDataSetsDbContext> dbaRepository,
        IStorageConnection storageConnection,
        IRecurringJobManager recurringJobManager,
        DbMonitoringConfiguration configuration,
        IErrorNotificationService errorNotificationService)
        : base(storageConnection, recurringJobManager, configuration, logger, errorNotificationService)
    {
        base.dbaRepository = dbaRepository;
    }

    public void RegisterDbaMonitoring()
    {
        try
        {
            this.jobId = $"third-party-data-sets-dba-monitoring-scheduler";
            this.RemoveScheduledJob();

            this.recurringJobManager.AddOrUpdate<ThirdPartyDataSetsDbaScheduler>(
            this.jobId,
                        (c) => this.ExecuteDbaMonitoring(default),
                        $"*/{this.configuration.SqlDatabaseConnectionCountReviewIntervalMinutes} * * * *");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "PerformDbMonitoringChecks encountered an error");
            throw;
        }
    }
}
