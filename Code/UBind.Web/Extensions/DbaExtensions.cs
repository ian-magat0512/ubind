// <copyright file="DbaExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions;

using Hangfire;
using UBind.Application.Configuration;
using UBind.Application.Scheduler.Jobs;
using UBind.Persistence;
using UBind.Persistence.Clients.DVA.Migrations;
using UBind.Persistence.ThirdPartyDataSets;
using ILogger = Serilog.ILogger;

public static class DbaExtensions
{
    /// <summary>
    /// Initializes the scheduler for the db pool for monitoring.
    /// </summary>
    /// <param name="serviceProvider">IServiceProvider provider object</param>
    /// <param name="logger">Serilog.ILogger object</param>
    public static void InitializeDbaSchedules(this IServiceProvider serviceProvider, ILogger logger)
    {
        var dbMonitoringConfiguration = serviceProvider.GetService<DbMonitoringConfiguration>();
        if (dbMonitoringConfiguration != null)
        {

            logger.Information("DBA scheduling start.");
            var ubindScheduler = serviceProvider.GetService<IDbaScheduler<UBindDbContext>>();
            BackgroundJob.Schedule(() => ubindScheduler.RegisterDbaMonitoring(), TimeSpan.FromMinutes(2));

            var tpdScheduler = serviceProvider.GetService<IDbaScheduler<ThirdPartyDataSetsDbContext>>();
            BackgroundJob.Schedule(() => tpdScheduler.RegisterDbaMonitoring(), TimeSpan.FromMinutes(2));

            var dvaScheduler = serviceProvider.GetService<IDbaScheduler<DvaDbContext>>();
            BackgroundJob.Schedule(() => dvaScheduler.RegisterDbaMonitoring(), TimeSpan.FromMinutes(2));
            logger.Information("DBA scheduling ended.");
        }
    }
}