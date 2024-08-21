// <copyright file="UBindDbaScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application.Scheduler.Jobs;

using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using UBind.Application.Configuration;
using UBind.Application.Services.Email;
using UBind.Domain.Repositories;
using UBind.Persistence;

/// <summary>
/// Contains the UBindDbaScheduler DBA background monitoring operations.
/// </summary>
public class UBindDbaScheduler : BaseDbaScheduler<UBindDbContext>, IDbaScheduler<UBindDbContext>
{
    public UBindDbaScheduler(
        ILogger<UBindDbaScheduler> logger,
        IDbaRepository<UBindDbContext> dbaRepository,
        IStorageConnection storageConnection,
        IRecurringJobManager recurringJobManager,
        DbMonitoringConfiguration configuration,
        IErrorNotificationService errorNotificationService)
        : base(storageConnection, recurringJobManager, configuration, logger, errorNotificationService)
    {
        base.dbaRepository = dbaRepository;
    }

    // <inheritdoc />
    public void RegisterDbaMonitoring()
    {
        try
        {
            this.jobId = $"ubind-dba-monitoring-scheduler";
            this.RemoveScheduledJob();

            this.recurringJobManager.AddOrUpdate<UBindDbaScheduler>(
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