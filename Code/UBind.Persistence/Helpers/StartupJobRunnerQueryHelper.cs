// <copyright file="StartupJobRunnerQueryHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Helpers
{
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Query helper for startup job runners.
    /// </summary>
    public static class StartupJobRunnerQueryHelper
    {
        /// <summary>
        /// Generates an insert query for the new startup job. This is used mostly in Up() method of
        /// an EF migration to apply a new database migration to run on hangfire.
        /// </summary>
        /// <param name="methodName">The method name to be executed in the startup job.</param>
        /// <param name="blocking">If true, causes the app to run the startup job before completing
        /// app startup.</param>
        /// <returns>Query string of the new startup job.</returns>
        public static string GenerateInsertQueryForStartupJob(
            string methodName,
            bool blocking = false,
            bool runManuallyIfInMultiNode = false,
            IEnumerable<string> precedingStartupJobAliases = null)
        {
            return GenerateInsertQueryForStartupJobWithCreatedColumnName(
                methodName, blocking, runManuallyIfInMultiNode, precedingStartupJobAliases, "CreatedTicksSinceEpoch");
        }

        /// <summary>
        /// Generates an insert query for a startup job, with the old column name CreationTimeInTicksSinceEpoch.
        /// This is needed because old migrations will still expect the timestamp column name to be old, until a later migration changes it.
        /// </summary>
        public static string GenerateInsertQueryForStartupJobV1(
            string methodName,
            bool blocking = false,
            bool runManuallyIfInMultiNode = false,
            IEnumerable<string> precedingStartupJobAliases = null)
        {
            return GenerateInsertQueryForStartupJobWithCreatedColumnName(
                methodName, blocking, runManuallyIfInMultiNode, precedingStartupJobAliases, "CreationTimeInTicksSinceEpoch");
        }

        /// <summary>
        /// Generates a delete query for the new startup job. This is used during the Down()
        /// method of an EF migration, to rollback the creation of the startup job created
        /// in the Up() method.
        /// </summary>
        /// <param name="methodName">The method name to be dropped in the startup job.</param>
        /// <returns>Query string for the deletion of the startup job.</returns>
        public static string GenerateDeleteQueryForStartupJob(string methodName)
        {
            var query = $"DELETE FROM dbo.StartupJobs WHERE Alias = '{methodName}';";
            return query;
        }

        private static string GenerateInsertQueryForStartupJobWithCreatedColumnName(
            string methodName,
            bool blocking,
            bool runManuallyIfInMultiNode,
            IEnumerable<string> precedingStartupJobAliases = null,
            string createdTicksSinceEpochColumnName = "CreatedTicksSinceEpoch")
        {
            IClock clock = SystemClock.Instance;
            var createdTicksSinceEpoch = clock.GetCurrentInstant().ToUnixTimeTicks();
            int blockingValue = blocking ? 1 : 0;
            int runManuallyIfInMultiNodeValue = runManuallyIfInMultiNode ? 1 : 0;
            precedingStartupJobAliases = precedingStartupJobAliases != null && precedingStartupJobAliases.None()
                ? null
                : precedingStartupJobAliases;

            string query;
            if (runManuallyIfInMultiNode)
            {
                // To ensure new column is only accessed after it's scaffolded into the database, will add if-clause here to only set value of
                // new parameter if it's configured to be true. Otherwise - will let defaultValue setting for EF work with setting it to false.
                query =
                    $"IF(NOT EXISTS(SELECT 1 FROM dbo.StartupJobs WHERE Alias='{methodName}')) \n"
                    + $"BEGIN \n"
                    + $"INSERT INTO dbo.StartupJobs(Id, Alias, Complete, Blocking, RunManuallyInMultiNodeEnvironment, {createdTicksSinceEpochColumnName}"
                        + (precedingStartupJobAliases != null ? ", PrecedingStartupJobAliases" : string.Empty)
                    + ") \n"
                    + $"VALUES(NEWID(), '{methodName}', 0, {blockingValue}, {runManuallyIfInMultiNodeValue}, {createdTicksSinceEpoch}"
                        + (precedingStartupJobAliases != null ? $", '{string.Join(",", precedingStartupJobAliases)}'" : string.Empty)
                    + ") \n"
                    + $"END;";
            }
            else
            {
                query =
                    $"IF(NOT EXISTS(SELECT 1 FROM dbo.StartupJobs WHERE Alias='{methodName}')) \n"
                    + $"BEGIN \n"
                    + $"INSERT INTO dbo.StartupJobs(Id, Alias, Complete, Blocking, {createdTicksSinceEpochColumnName}"
                        + (precedingStartupJobAliases != null ? ", PrecedingStartupJobAliases" : string.Empty)
                    + ") \n"
                    + $"VALUES(NEWID(), '{methodName}', 0, {blockingValue}, {createdTicksSinceEpoch}"
                        + (precedingStartupJobAliases != null ? $", '{string.Join(",", precedingStartupJobAliases)}'" : string.Empty)
                    + ") \n"
                    + $"END;";
            }

            return query;
        }
    }
}
