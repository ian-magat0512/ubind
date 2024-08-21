// <copyright file="DbLogFileMaintenanceService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Maintenance;

using DnsClient.Internal;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using UBind.Application.Services.Email;
using UBind.Domain.Exceptions;
using UBind.Domain.Repositories;
using UBind.Domain.Services.Maintenance;
using UBind.Persistence.Infrastructure;

public class DbLogFileMaintenanceService : IDbLogFileMaintenanceService
{
    private const decimal LogUsageCleanThreshold = 75.0m; // Threshold set to 75%
    private const decimal LogSizeCleanThresholdMb = 5000m; // Threshold set to 5GB
    private const decimal LogSizeShrinkThresholdMb = 10000m; // Threshold set to 10GB
    private const int ShrinkTimeoutSeconds = 300; // 5 minutes.
    private const int BackupTimeoutSeconds = 2700; // 45 minutes.
    private const int MaxShrinkAttempts = 10;

    /// <summary>
    /// The number of seconds to wait between shrink attempts.
    /// </summary>
    private const int ShrinkIntervalDelaySeconds = 30; // 30 seconds
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    private readonly IUBindDbContext dbContext;
    private readonly ILogger<DbLogFileMaintenanceService> logger;
    private readonly string connectionString;
    private readonly IDatabaseConfiguration databaseConfiguration;
    private readonly IErrorNotificationService errorNotificationService;
    private readonly IClock clock;

    public DbLogFileMaintenanceService(
        IUBindDbContext dbContext,
        ILogger<DbLogFileMaintenanceService> logger,
        IDatabaseConfiguration databaseConfiguration,
        IConfiguration configuration,
        IErrorNotificationService errorNotificationService,
        IClock clock)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.databaseConfiguration = databaseConfiguration;
        this.connectionString = configuration.GetConnectionString("UBind");
        this.errorNotificationService = errorNotificationService;
        this.clock = clock;
    }

    public async Task ForceCleanLogFile()
    {
        await semaphore.WaitAsync();
        try
        {
            await this.CleanLogFile(true);
        }
        catch (Exception ex) when (!(ex is ErrorException))
        {
            this.SendEmailForFailedMaintenanceTask("clean", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task ForceShrinkLogFile()
    {
        await semaphore.WaitAsync();
        try
        {
            await this.ShrinkLogFile();
        }
        catch (Exception ex) when (!(ex is ErrorException))
        {
            this.SendEmailForFailedMaintenanceTask("shrink", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task CleanLogFileIfNeeded()
    {
        if (await this.DoesLogNeedCleaning())
        {
            await semaphore.WaitAsync();
            try
            {
                await this.CleanLogFile(false);
            }
            catch (Exception ex) when (!(ex is ErrorException))
            {
                this.SendEmailForFailedMaintenanceTask("clean", ex);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    public async Task ShrinkLogFileIfNeeded(CancellationToken? cancellationToken = null)
    {
        if (await this.DoesLogNeedShrinking())
        {
            await semaphore.WaitAsync();
            try
            {
                await this.ShrinkLogFileRepeatedlyUntilSizeIsUnderThreshold(cancellationToken);
            }
            catch (Exception ex) when (!(ex is ErrorException))
            {
                this.SendEmailForFailedMaintenanceTask("shrink", ex);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    /// <summary>
    /// Do shrinking or cleaning based on log size and space percentage
    /// Clean log instead if log size is lower than shrink threshold
    /// but log space usage percentage is higher than threshold
    /// </summary>
    public async Task ShrinkLogFileOrCleanIfNeeded(CancellationToken? cancellationToken = null)
    {
        if (await this.DoesLogNeedShrinking())
        {
            await this.ShrinkLogFileIfNeeded(cancellationToken);
        }
        else
        {
            await this.CleanLogFileIfNeeded();
        }
    }

    public async Task BackupDbLogFile()
    {
        await semaphore.WaitAsync();
        try
        {
            await this.BackupLogFile();
        }
        catch (Exception ex) when (!(ex is ErrorException))
        {
            this.SendEmailForFailedMaintenanceTask("backup", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task ThrowIfUserDoesNotHaveRequiredPermissions()
    {
        StringBuilder missingPermissions = new StringBuilder();
        if (!await this.CanAlterAnyDatabase())
        {
            missingPermissions.AppendLine("ALTER ANY DATABASE");
        }
        if (!await this.CanViewAnyDefinition())
        {
            missingPermissions.AppendLine("VIEW ANY DEFINITION");
        }
        if (!await this.CanViewServerState())
        {
            missingPermissions.AppendLine("VIEW SERVER STATE");
        }

        if (missingPermissions.Length > 0)
        {
            throw new ArgumentException($"You will not be able to shrink log files. Missing permissions:\n"
                + $"{missingPermissions}");
        }
    }

    public async Task<bool> DoesLogNeedCleaning()
    {
        if (await this.GetLogFileSizeMb() < LogSizeCleanThresholdMb)
        {
            return false;
        }

        var usedPercentage = await this.GetLogFileUsedPercentage();
        if (usedPercentage > LogUsageCleanThreshold)
        {
            this.logger.LogInformation($"The database log file is {usedPercentage}% full, which is more than "
                + $"{LogUsageCleanThreshold}%, so it needs cleaning.");
            return true;
        }

        return false;
    }

    public async Task<bool> DoesLogNeedShrinking()
    {
        // Check db log size if threshold of 35GB is reached so we can cleanup the DB log
        var logFileName = await this.GetLogFileName();
        var logSize = await this.GetLogFileSizeMb(logFileName);
        return logSize > LogSizeShrinkThresholdMb;
    }

    public async Task<decimal> GetLogFileSizeMb(string? logFileName = null)
    {
        logFileName = logFileName ?? await this.GetLogFileName();
        string logFileSizeQuery
            = $@"SELECT (size * 8.0 / 1024) as SizeInMB FROM sys.master_files WHERE name = '{logFileName}'";
        return await this.dbContext.Database.SqlQuery<decimal>(logFileSizeQuery).FirstOrDefaultAsync();
    }

    public async Task<decimal> GetLogFileUsedPercentage()
    {
        string sql = @"
            SELECT 
                (used_log_space_in_bytes * 1.0 / total_log_size_in_bytes) * 100 AS LogFileUsedPercentage
            FROM sys.dm_db_log_space_usage
            WHERE database_id = DB_ID();
        ";

        decimal logFileUsedPercentage = await this.dbContext.Database.SqlQuery<decimal>(sql).FirstOrDefaultAsync();
        if (logFileUsedPercentage == 0)
        {
            this.logger.LogError("Unable to retrieve log file used space percentage.");
            await this.ThrowIfUserDoesNotHaveRequiredPermissions();
            throw new InvalidOperationException("Unable to retrieve log file used space percentage.");
        }

        return logFileUsedPercentage;
    }

    public async Task<string> GetLogFileName()
    {
        string logFileNameQuery
            = @"SELECT name FROM sys.master_files WHERE database_id = DB_ID() AND type_desc = 'LOG'";
        string logFilename = await this.dbContext.Database.SqlQuery<string>(logFileNameQuery).FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(logFilename))
        {
            this.logger.LogError("Unable to find the database log file name.");
            await this.ThrowIfUserDoesNotHaveRequiredPermissions();
            throw new ArgumentException("Unable to find the database log file name.");
        }

        return logFilename;
    }

    /// <summary>
    /// Clears out unnecessary data from within the log file and reorganises it for future logs.
    /// When using NOTRUNCATE, SQL Server doesn't technically "delete" data within the log file; rather, it moves
    /// active log records towards the front of the log file, freeing up space at the end for reuse by subsequent
    /// transactions. This is what's known as "clearing" the log.
    /// The log file will maintain its physical size on disk after running DBCC SHRINKFILE with NOTRUNCATE.
    /// What remains in the log after a NOTRUNCATE operation?
    /// - Active log records, meaning any records that are still necessary for SQL Server's logging mechanism to
    /// maintain data integrity, remain.
    /// - The space made available by moving active log records will be reused by SQL Server for new log records.
    /// How does SQL Server decide what to remove and what to keep?
    /// - SQL Server retains any active log records that are required for any of its logging and recovery mechanisms.
    /// - Log records that are no longer necessary (i.e., those that don't serve a purpose for ongoing transactions,
    /// replication, or point-in-time recovery) are the ones that get "cleared" from the log file. They are
    /// essentially marked as space that can be reused for new log entries.
    /// </summary>
    /// <param name="forced">If true, the log file will be cleaned regardless of whether it needs it or not.</param>
    private async Task CleanLogFile(bool forced)
    {
        if (!forced && !await this.DoesLogNeedCleaning())
        {
            return;
        }

        await this.BackupLogFile();
        var startInstant = this.clock.GetCurrentInstant();
        var logFileName = await this.GetLogFileName();
        var usedPercentage = await this.GetLogFileUsedPercentage();
        this.logger.LogInformation($"Log file is {usedPercentage}% used. Freeing up space within DB log file "
            + $"\"{logFileName}\" (timeout is {TimeSpan.FromSeconds(ShrinkTimeoutSeconds).Humanize()})...");
        var cleanLogSql = $"DBCC SHRINKFILE([{logFileName}], NOTRUNCATE);";

        using (SqlConnection conn = new SqlConnection(this.connectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(cleanLogSql, conn))
            {
                cmd.CommandTimeout = ShrinkTimeoutSeconds;
                cmd.ExecuteNonQuery();
            }
        }

        decimal newSizeMb = await this.GetLogFileSizeMb(logFileName);
        var durationSecondsSinceStarted = Math.Round((this.clock.GetCurrentInstant() - startInstant).TotalSeconds, 2);
        usedPercentage = await this.GetLogFileUsedPercentage();
        this.logger.LogInformation($"The DB log file \"{logFileName}\" is now {usedPercentage}% full, and is "
            + $"{newSizeMb} MB on disk. It took {TimeSpan.FromSeconds(durationSecondsSinceStarted).Humanize()} to clean.");

        this.logger.LogInformation("Waiting for 5 seconds to allow the clean operation to propagate to the replicas.");
        await Task.Delay(5000);
    }

    /// <summary>
    /// Frees up space on the disk by cleaning then shrinking the log file on disk, releasing the space back
    /// to the operating system.
    /// This will not free up any space unless the log file has been backed up first. Without a backup having been
    /// done, MSSQL will not allow the logs to be deleted.
    /// Please note, this will clean the log file automatically before shrinking it.
    /// Calling ShrinkLogFile() once is often not effective. You may need to call it multiple times to get the log
    /// file to shrink. This is because MSSQL could have active transactions and locks in place which prevent the
    /// backup and shrink from having any effect.
    /// Therefore, you should instead call ShrinkLogFileRepeatedlyUntilSizeIsUnderThreshold().
    /// </summary>
    private async Task ShrinkLogFile()
    {
        await this.CleanLogFile(true);
        var startInstant = this.clock.GetCurrentInstant();
        var logFileName = await this.GetLogFileName();
        decimal sizeMb = await this.GetLogFileSizeMb(logFileName);
        this.logger.LogInformation($"The log file \"{logFileName}\" is {sizeMb} MB on disk. "
            + $"Attempting to shrink it to return space to the operating system (timeout is {TimeSpan.FromSeconds(ShrinkTimeoutSeconds).Humanize()})...");
        var shrinkLogSql = $"DBCC SHRINKFILE([{logFileName}], TRUNCATEONLY);";

        using (SqlConnection conn = new SqlConnection(this.connectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(shrinkLogSql, conn))
            {
                cmd.CommandTimeout = ShrinkTimeoutSeconds;
                cmd.ExecuteNonQuery();
            }
        }

        var durationSecondsSinceStarted = Math.Round((this.clock.GetCurrentInstant() - startInstant).TotalSeconds, 2);
        decimal newSizeMb = await this.GetLogFileSizeMb(logFileName);
        this.logger.LogInformation($"The DB log file \"{logFileName}\" is now {newSizeMb} MB on disk." +
            $" It took  {TimeSpan.FromSeconds(durationSecondsSinceStarted).Humanize()} to shrink.");

        this.logger.LogInformation("Waiting for 5 seconds to allow the shrink operation to propagate to the replicas.");
        await Task.Delay(5000);
    }

    private async Task ShrinkLogFileRepeatedlyUntilSizeIsUnderThreshold(CancellationToken? cancellationToken = null)
    {
        int count = 0;
        while (await this.DoesLogNeedShrinking())
        {
            if (cancellationToken.HasValue)
            {
                cancellationToken.Value.ThrowIfCancellationRequested();
            }

            if (count == 0)
            {
                this.logger.LogInformation($"Attempting to shrink the log file up to {MaxShrinkAttempts} times to "
                    + $"free up disk space until the log is less than {LogSizeShrinkThresholdMb} MB...");
            }
            else if (count > MaxShrinkAttempts)
            {
                var actualSizeMb = await this.GetLogFileSizeMb();
                string message = $"The database log file shrink failed after {count} attempts. "
                    + $"The log file size is {actualSizeMb} MB, which is greater than the threshold of "
                    + $"{LogSizeShrinkThresholdMb} MB. ";
                this.logger.LogError(message);
                this.errorNotificationService.SendSystemNotificationEmail("Database Log File Shrink Failed", message);
                throw new ErrorException(Domain.Errors.Maintenance.DbLogFileShrinkFailed(
                    count,
                    LogSizeShrinkThresholdMb,
                    actualSizeMb));
            }
            else if (count > 0)
            {
                await Task.Delay(ShrinkIntervalDelaySeconds * 1000);
            }

            count++;
            await this.ShrinkLogFile();
        }
    }

    private async Task BackupLogFile()
    {
        var startInstant = this.clock.GetCurrentInstant();
        string backupPath = this.databaseConfiguration.LogFileBackupLocation;

        this.logger.LogInformation($"Backing up the Log File to \"{backupPath}\" (timeout is {TimeSpan.FromSeconds(BackupTimeoutSeconds).Humanize()})...");

        // Prepare SQL command to backup the log file
        string sqlCommand = @"
            DECLARE @fileName VARCHAR(100)
            DECLARE @DbName VARCHAR(100)

            SET @DbName = DB_NAME()

            SET @fileName = @BackupPath + '\' + @DbName + '-Log-' +
            + CONVERT(VARCHAR, GETDATE(), 105) + '_' +
            CAST(DATEPART(HOUR, GETDATE()) AS VARCHAR) + '_' +
            CAST(DATEPART(MINUTE, GETDATE()) AS VARCHAR) + '_' +
            CAST(DATEPART(SECOND,GETDATE()) AS VARCHAR) + '.bak'

            BACKUP LOG @DbName TO  DISK = @fileName WITH NOFORMAT, NOINIT,  
            NAME = N'Database Log Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

        using (SqlConnection connection = new SqlConnection(this.connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(sqlCommand, connection))
            {
                command.CommandTimeout = BackupTimeoutSeconds;
                command.Parameters.Add(new SqlParameter("@BackupPath", backupPath));
                await command.ExecuteNonQueryAsync();
            }
        }
        var durationSecondsSinceStarted = Math.Round((this.clock.GetCurrentInstant() - startInstant).TotalSeconds, 2);
        this.logger.LogInformation($"The Log File Backup has completed. It took {TimeSpan.FromSeconds(durationSecondsSinceStarted).Humanize()} to backup.");
    }

    private async Task<bool> CanAlterAnyDatabase()
    {
        string query = "SELECT HAS_PERMS_BY_NAME(NULL, NULL, 'ALTER ANY DATABASE')";
        int result = await this.dbContext.Database.SqlQuery<int>(query).FirstOrDefaultAsync();
        return result == 1;
    }

    private async Task<bool> CanViewAnyDefinition()
    {
        string query = "SELECT HAS_PERMS_BY_NAME(NULL, NULL, 'VIEW ANY DEFINITION')";
        int result = await this.dbContext.Database.SqlQuery<int>(query).FirstOrDefaultAsync();
        return result == 1;
    }

    private async Task<bool> CanViewServerState()
    {
        string query = "SELECT HAS_PERMS_BY_NAME(NULL, NULL, 'VIEW SERVER STATE')";
        int result = await this.dbContext.Database.SqlQuery<int>(query).FirstOrDefaultAsync();
        return result == 1;
    }

    private void SendEmailForFailedMaintenanceTask(string operation, Exception exception)
    {
        var message = new StringBuilder();
        message.AppendLine($"Db log {operation} operation has failed to complete.");
        message.AppendLine("<br/>");
        message.AppendLine("Please investigate the following exception and retry manually by running the query via SSMS.");
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
            $"uBind: Db log {operation} has failed to complete.", message.ToString());
    }
}
