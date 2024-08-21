// <copyright file="IDbLogFileMaintenanceService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.Maintenance;

public interface IDbLogFileMaintenanceService
{
    Task ThrowIfUserDoesNotHaveRequiredPermissions();

    Task<bool> DoesLogNeedCleaning();

    Task<bool> DoesLogNeedShrinking();

    Task<string> GetLogFileName();

    Task<decimal> GetLogFileSizeMb(string? logFileName = null);

    Task<decimal> GetLogFileUsedPercentage();

    Task BackupDbLogFile();

    Task ForceCleanLogFile();

    Task ForceShrinkLogFile();

    Task CleanLogFileIfNeeded();

    Task ShrinkLogFileIfNeeded(CancellationToken? cancellationToken = null);

    Task ShrinkLogFileOrCleanIfNeeded(CancellationToken? cancellationToken = null);
}