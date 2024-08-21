// <copyright file="MiniProfilerDeletionManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Reduction
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Loggers;
    using UBind.Domain.Reduction;

    /// <summary>
    /// The mini profiler reductor.
    /// </summary>
    public class MiniProfilerDeletionManager : IMiniProfilerDeletionManager
    {
        private IProgressLogger logger;
        private string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniProfilerDeletionManager"/> class.
        /// </summary>
        /// <param name="connectionString">A connection string for the database where quotes are persisted.</param>
        public MiniProfilerDeletionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <inheritdoc/>
        public void Truncate(IProgressLogger logger)
        {
            this.logger = logger;
            this.logger.Log(LogLevel.Information, $"Finished deleting mini profiler data.");
            this.Execute();
            this.logger.Log(LogLevel.Information, "Starting to execute mini profiler deletion by batch size.");
        }

        private void Execute()
        {
            ICollection<string> truncateCommands = new List<string>
            {
                "TRUNCATE TABLE [dbo].[MiniProfilerTimings];",
                "TRUNCATE TABLE [dbo].[MiniProfilerClientTimings];",
                "TRUNCATE TABLE [dbo].[MiniProfilers];",
            };

            using (var dbContext = new UBindDbContext(this.connectionString, 180))
            {
                var singleTripQuery = string.Join(string.Empty, truncateCommands);
                dbContext.Database.ExecuteSqlCommand(singleTripQuery);
            }
        }
    }
}
