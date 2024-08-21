// <copyright file="IDatabaseConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Infrastructure
{
    /// <summary>
    /// Interface for database configuration settings.
    /// </summary>
    public interface IDatabaseConfiguration
    {
        /// <summary>
        /// Gets the threshold in milliseconds above which commands will be considered as slow, and will therefore be logged.
        /// </summary>
        int SlowCommandThresholdMs { get; }

        /// <summary>
        /// Gets the location where the database log file will be backed up to.
        /// This is used by the DbLogFileMaintenanceService to ensure the database log file does not grow too large
        /// during heavy data migrations.
        /// </summary>
        string LogFileBackupLocation { get; }
    }
}
