// <copyright file="DatabaseConnectionHealthCheck.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.HealthChecks
{
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    /// Health check for database connection.
    /// </summary>
    public class DatabaseConnectionHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionHealthCheck"/> class.
        /// </summary>
        public DatabaseConnectionHealthCheck(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Runs the health check, returning the status of the database connection.
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var connection = new SqlConnection(this.configuration.GetConnectionString("UBind")))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    if (connection.State == ConnectionState.Open)
                    {
                        return HealthCheckResult.Healthy("UBind database is healthy.");
                    }
                    else
                    {
                        return HealthCheckResult.Unhealthy("UBind database connection is not available.");
                    }
                }
                catch (SqlException)
                {
                    return HealthCheckResult.Unhealthy("Error encountered when connecting to UBind database.");
                }
            }
        }
    }
}
