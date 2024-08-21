// <copyright file="DbContextExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Extensions
{
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extension methods for DbContext.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Reset the DbContext by removing added and modified entities.
        /// </summary>
        /// <param name="dbContext">The dbContext.</param>
        /// <param name="logger">A logger.</param>
        public static void Reset(this DbContext dbContext, ILogger logger)
        {
            logger.LogInformation("Starting resetting dbContext in response to concurrency issue.");
            var entries = dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }

            var entryCount = entries.Count;
            logger.LogInformation(@"Completed resetting dbContext in response to concurrency issue. Entry count: {entryCount}.", entryCount);
        }

        /// <summary>
        /// Get the total row count of a table.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="table">Table to count</param>
        /// <param name="condition">Conditions to put as to a where clause of a SQL Script would be structured</param>
        /// <returns></returns>
        public static async Task<int> GetRowCount(
            this DbContext dbContext,
            string tableName,
            List<SqlParameter> conditions = null)
        {
            tableName = tableName.SanitizeTableName();

            var connection = dbContext.Database.Connection;
            await using (var command = connection.CreateCommand())
            {
                string commandText = $"SELECT COUNT(*) FROM {tableName} WHERE 1=1";
                if (conditions != null && conditions.Any())
                {
                    foreach (var condition in conditions)
                    {
                        commandText += $" AND [{condition.ParameterName}] = @{condition.ParameterName}";
                        command.Parameters.Add(new SqlParameter($"@{condition.ParameterName}", condition.DbType)
                        {
                            Value = condition.Value
                        });
                    }
                }
                command.CommandText = commandText;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var result = await command.ExecuteScalarAsync();
                int totalRows = Convert.ToInt32(result);

                return totalRows;
            }
        }

        private static bool IsValidTableName(string tableName)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z0-9_.]+$");
        }

        private static string SanitizeTableName(this string tableName)
        {
            if (!IsValidTableName(tableName))
            {
                throw new ArgumentException("Invalid table name", nameof(tableName));
            }
            string[] parts = tableName.Split('.');
            string sanitizedName = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                sanitizedName += $"[{parts[i]}]";
                if (i < parts.Length - 1)
                {
                    sanitizedName += '.';
                }
            }
            return sanitizedName;
        }
    }
}
