// <copyright file="SystemEventDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Application.Services.Email;
using UBind.Domain.Services;
using UBind.Domain.Services.Maintenance;
using UBind.Persistence;

/// <summary>
/// Service for uBind database record deletion.
/// </summary>
public class SystemEventDeletionService : ISystemEventDeletionService
{
    private const int ExpectedEventsDeletedInOneHour = 9000; // every approx hour
    private const int ExpectedEventsDeletedBeforeCheckingDBLog = 2250; // every aprox. 15 minutes
    private const int TimeoutCountBeforeJobTermination = 3;
    private readonly IConnectionConfiguration connection;
    private readonly IClock clock;
    private readonly ILogger<SystemEventDeletionService> logger;
    private readonly IDbLogFileMaintenanceService dbLogFileMaintenanceService;
    private readonly IErrorNotificationService errorNotificationService;

    public SystemEventDeletionService(
        IConnectionConfiguration connection,
        IClock clock,
        ILogger<SystemEventDeletionService> logger,
        IDbLogFileMaintenanceService dbLogFileMaintenanceService,
        IErrorNotificationService errorNotificationService)
    {
        this.connection = connection;
        this.clock = clock;
        this.logger = logger;
        this.dbLogFileMaintenanceService = dbLogFileMaintenanceService;
        this.errorNotificationService = errorNotificationService;
    }

    public async Task ExecuteDeletionInBatches(int batchSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var jobStartInstant = this.clock.GetCurrentInstant();
        await this.LogTotalEventsToDelete(jobStartInstant.ToUnixTimeTicks(), cancellationToken);
        var totalDeleted = 0;
        var deletedCount = batchSize;
        var timeoutCount = 0;
        var isJobTerminated = false;
        while (deletedCount == batchSize)
        {
            if (totalDeleted % ExpectedEventsDeletedBeforeCheckingDBLog == 0)
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
            }

            try
            {
                if (totalDeleted > 0 && totalDeleted % ExpectedEventsDeletedInOneHour == 0)
                {
                    // log every 1 hour so we know it's still running
                    var durationHoursSinceStarted = Math.Round((this.clock.GetCurrentInstant() - jobStartInstant).TotalHours, 2);
                    this.logger.LogInformation($"Total deleted: {totalDeleted} events" +
                        $" and associated relationships at {batchSize} events every other second" +
                        $" since job is started {durationHoursSinceStarted} hours ago.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                var ticksNow = this.clock.GetCurrentInstant().ToUnixTimeTicks();
                deletedCount = await this.DeleteEvents(batchSize, ticksNow, cancellationToken);
                totalDeleted += deletedCount;
                timeoutCount = 0;
            }
            catch (SqlException ex)
            {
                deletedCount = batchSize;

                // if its a timeout, we can retry
                if (ex.Number != -2)
                {
                    this.SendEmailForFailedSystemEventsDeletion(ex);
                    throw;
                }

                timeoutCount++;
                if (timeoutCount > TimeoutCountBeforeJobTermination)
                {
                    // then we simply send email so it can be investigated
                    this.SendEmailForFailedSystemEventsDeletion(
                        ex,
                        $"The job has been terminated because {TimeoutCountBeforeJobTermination} consecutive SQL timeouts occured.");
                    this.logger.LogError($"Terminating the job due to {timeoutCount} timeouts in a row.");
                    isJobTerminated = true;

                    // Terminate the job so it will naturally retry in the next schedule
                    break;
                }

                // delay for retrying
                await Task.Delay(30000, cancellationToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                this.SendEmailForFailedSystemEventsDeletion(ex);
                throw;
            }

            await Task.Delay(1500, cancellationToken);
        }

        if (isJobTerminated)
        {
            this.logger.LogInformation($"Deleted a total of {totalDeleted} system event records before terminating.");
            return;
        }
        this.logger.LogInformation($"Completed. Deleted a total of {totalDeleted} system event records.");
    }

    private async Task LogTotalEventsToDelete(long ticksNow, CancellationToken cancellationToken)
    {
        string query = @"SELECT COUNT(*) from dbo.SystemEvents s
            WHERE s.ExpiryTicksSinceEpoch is not null
            AND s.ExpiryTicksSinceEpoch <> 0
            AND s.ExpiryTicksSinceEpoch <> 2534023007999999999
            AND ExpiryTicksSinceEpoch < @ticksNow";

        using (var connection = new SqlConnection(this.connection.UBind))
        {
            await connection.OpenAsync();
            var parameters = new DynamicParameters();
            parameters.Add("@ticksNow", ticksNow);
            var commandDefinition = new CommandDefinition(
                query,
                parameters,
                cancellationToken: cancellationToken,
                commandTimeout: 180);
            var totalEventsToDelete = (await connection.QueryAsync<int>(commandDefinition)).Single();
            this.logger.LogInformation($"Total events to delete: {totalEventsToDelete}");
        }
    }

    private async Task<int> DeleteEvents(int batchSize, long ticksNow, CancellationToken cancellationToken)
    {
        var deletedCount = 0;
        cancellationToken.ThrowIfCancellationRequested();
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    string deleteSql =
                        "DECLARE @rowsDeleted INT = 0;" +
                        "DECLARE @deletedIds table (id UNIQUEIDENTIFIER); " +
                        " DELETE TOP (@batchSize) FROM dbo.SystemEvents " +
                        " OUTPUT DELETED.Id into  @deletedIds " +
                        " WHERE ExpiryTicksSinceEpoch IS NOT NULL " +
                        " AND ExpiryTicksSinceEpoch <> 0 " +
                        " AND ExpiryTicksSinceEpoch < @ticksNow; " +
                        "SET @rowsDeleted = @@ROWCOUNT; " +
                        "DELETE FROM rFrom " +
                        " FROM dbo.Relationships as rFrom " +
                        " INNER JOIN @deletedIds as d ON d.id = rFrom.FromEntityId " +
                        " WHERE rFrom.FromEntityType = 13; " +
                        "DELETE FROM rTo " +
                        " FROM dbo.Relationships as rTo " +
                        " INNER JOIN @deletedIds as d ON d.id = rTo.ToEntityId " +
                        " WHERE rTo.ToEntityType = 13; " +
                        "DELETE FROM t " +
                        " FROM dbo.Tags as t " +
                        " INNER JOIN @deletedIds as d ON d.id = t.EntityId " +
                        " WHERE t.EntityType = 13;" +
                        "SELECT @rowsDeleted AS RowsDeleted";
                    var parameters = new DynamicParameters();
                    parameters.Add("@ticksNow", ticksNow);
                    parameters.Add("@batchSize", batchSize);
                    var commandDefinition = new CommandDefinition(
                        deleteSql,
                        parameters,
                        transaction,
                        commandTimeout: 10);
                    deletedCount = (await connection.QueryAsync<int>(commandDefinition)).Single();
                    transaction.Commit();
                }
                catch
                {
                    if (transaction != null && transaction.Connection != null)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Failed to rollback transaction.");
                        }
                    }
                    throw;
                }
            }
        }
        return deletedCount;
    }

    private void SendEmailForFailedSystemEventsDeletion(Exception exception, string? reason = null)
    {
        var message = new StringBuilder();
        message.AppendLine($"System Events Deletion Job has failed to complete.");
        message.AppendLine("<br/>");
        if (!string.IsNullOrEmpty(reason))
        {
            message.AppendLine($"Reason: {reason}");
            message.AppendLine("<br/>");
        }
        message.AppendLine("This is recurring job that runs every hour so there is no need to retry manually.");
        message.AppendLine("Please investigate the following exception. ");
        message.AppendLine("<br/>");
        message.AppendLine("<br/>");
        message.AppendLine("The following contains details of the exception:");
        message.AppendLine("<br/>");
        message.AppendLine("Exception:");
        message.AppendLine(exception.Message);
        message.AppendLine("<br/>");
        message.AppendLine("Stacktrace:");
        message.AppendLine(exception.StackTrace);
        if (exception.InnerException != null)
        {
            message.AppendLine("Inner Exception:");
            message.AppendLine(exception.InnerException.Message);
            message.AppendLine("<br/>");
            message.AppendLine("Inner Exception Stacktrace:");
            message.AppendLine(exception.InnerException.StackTrace);
        }

        this.errorNotificationService.SendSystemNotificationEmail(
            "uBind:System Events Deletion Job has failed to complete.", message.ToString());
    }
}