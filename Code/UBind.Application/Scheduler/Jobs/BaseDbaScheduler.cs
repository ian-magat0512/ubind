// <copyright file="BaseDbaScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs;

using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Text;
using UBind.Application.Configuration;
using UBind.Application.Services.Email;
using UBind.Domain.ReadModels.Dba;
using UBind.Domain.Repositories;

public abstract class BaseDbaScheduler<T> where T : DbContext, new()
{
    protected readonly IStorageConnection storageConnection;
    protected readonly IRecurringJobManager recurringJobManager;
    protected readonly DbMonitoringConfiguration configuration;
    protected readonly ILogger<BaseDbaScheduler<T>> logger;
    protected readonly IErrorNotificationService errorNotificationService;
    protected IDbaRepository<T> dbaRepository;
    protected string jobId = string.Empty;

    public BaseDbaScheduler(
        IStorageConnection storageConnection,
        IRecurringJobManager recurringJobManager,
        DbMonitoringConfiguration configuration,
        ILogger<BaseDbaScheduler<T>> logger,
        IErrorNotificationService errorNotificationService)
    {
        this.storageConnection = storageConnection;
        this.recurringJobManager = recurringJobManager;
        this.configuration = configuration;
        this.logger = logger;
        this.errorNotificationService = errorNotificationService;
    }

    [JobDisplayName("UBind DB Monitoring Job")]
    public async Task ExecuteDbaMonitoring(CancellationToken cancellationToken = default)
    {
        if (this.dbaRepository.GetMaxConnectionPool() == 0)
        {
            // No action needed if the max pool size is not set on the connection string.
            this.logger.LogInformation($"Max Pool Size not set for the current connection ({this.dbaRepository.GetDbName()}).");
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"Start of DBA Monitoring.");
            var res = this.dbaRepository.GetActiveConnections();

            int sum = res.Sum(s => s.NumberOfConnections);
            string dbName = res.FirstOrDefault().DbName;

            if (dbName != null && sum != 0)
            {
                double actualPercentage = this.CalculatePercentage(sum, this.dbaRepository.GetMaxConnectionPool());
                if (actualPercentage >= this.configuration.SqlDatabaseConnectionCountNotificationThreshold)
                {
                    this.logger.LogDebug($"{this.dbaRepository.GetDbName()} connections has exceeded threshold: {sum}");
                    string message = $"The database connection pool is now at {actualPercentage.ToString("F2")}%." +
                        $"<br/>The warning percentage is {this.configuration.SqlDatabaseConnectionCountNotificationThreshold}%." +
                        $"<br/>DB Max Pool Size: {this.dbaRepository.GetMaxConnectionPool()}" +
                        $"<br/>Active Pool Size: {sum}<br/><br/>";
                    this.EscalateEmailNotification(res, message);
                    this.logger.LogInformation("Email warning threshold warning escalated.");
                }
                else
                {
                    this.logger.LogInformation($"{dbName} connections has a normal threshold: {sum}");
                }
            }

            await Task.Delay(100);
            this.logger.LogInformation($"End of DBA Monitoring.");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "ExecuteDbaMonitoring encountered an error");
            throw;
        }
    }

    // <inheritdoc />
    public virtual void EscalateEmailNotification(List<SqlServerSysProcessViewModel> sysViewReadModels, string initialMessage = null)
    {
        this.logger.LogDebug("Starting email notification.");
        try
        {
            if (this.errorNotificationService == null)
            {
                this.logger.LogDebug("IErrorNotificationService is not initialized.");
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"<h3>{sysViewReadModels.FirstOrDefault().DbName} concurrent connections total: {sysViewReadModels.Sum(model => model.NumberOfConnections)}</h3>");

            if (!string.IsNullOrEmpty(initialMessage))
            {
                builder.AppendLine($"<p>{initialMessage}<br/><br/>Connection Details<br/>");
            }
            else
            {
                builder.AppendLine("<p>Connection Details<br/>");
            }

            foreach (var sysView in sysViewReadModels)
            {
                builder.AppendLine("---------------------------------------------------------------<br/>");
                builder.AppendLine($"Server: {sysView.LoginName}<br/>");
                builder.AppendLine($"Number of connections: {sysView.NumberOfConnections}<br/>");
                builder.AppendLine($"Connection Status: {sysView.Status}<br/>");
                builder.AppendLine("---------------------------------------------------------------<br/>");
            }

            builder.AppendLine(@"<h3>It is recommeded that you increase the pool size of the Database. Consider doing the following:</h3>
                                <ul>
                                   <li>Find the connection string of the database in the appsettings.json file</li>
                                   <li>Increase the Max Pool Size to the acceptable count</li>
                                   <li>Restart the web application.</li>
                                </li>
                                <br>
                                <h4>Example</h4>
                                <br/>
                                ""ConnectionStrings"": {
                                  UBind: "".........Max Pool Size=20"";
                                ..........................and etc...}
                                <br/>"
            );
            builder.AppendLine("</p>");

            this.errorNotificationService.SendEmail(
                $"{sysViewReadModels.FirstOrDefault().DbName} DB Threshold Exceeded",
                builder.ToString()
                );
            this.logger.LogDebug("Email notification sent.");
            return;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "DB threshold warning notification encountered an error.");
            throw;
        }

        this.logger.LogDebug("End email notification.");
    }

    public double CalculatePercentage(double value, double totalValue)
    {
        return (value / totalValue) * 100.0;
    }

    protected virtual void RemoveScheduledJob()
    {
        if (this.storageConnection == null)
        {
            return;
        }

        var recurringJobs = this.storageConnection.GetRecurringJobs();
        var jobs = recurringJobs.Where(p => p.Id.Equals(this.jobId, StringComparison.InvariantCultureIgnoreCase));
        foreach (var job in jobs)
        {
            this.recurringJobManager.RemoveIfExists(job.Id);
        }
    }
}