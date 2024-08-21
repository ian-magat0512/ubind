// <copyright file="DatabaseHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Helpers
{
    using System.Data.SqlClient;

    /// <summary>
    /// Helper class for database operations.
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        /// Create database if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public static void CreateDatabaseIfNotExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                var cmdText = $"If(db_id(N'{dbName}') IS NULL) CREATE DATABASE [{dbName}]";
                using (var cmd = new SqlCommand(cmdText, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
