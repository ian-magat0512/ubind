// <copyright file="LuceneHealthCheck.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.HealthChecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using UBind.Persistence.Search;

    /// <summary>
    /// Health check for lucene.
    /// </summary>
    public class LuceneHealthCheck : IHealthCheck
    {
        private readonly ILuceneDirectoryConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneHealthCheck"/> class.
        /// </summary>
        public LuceneHealthCheck(ILuceneDirectoryConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Runs the health check, returning the status of the lucene indexing.
        /// </summary>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var luceneDirectoryExists = Directory.Exists(this.configuration.BaseLuceneDirectory);
            if (!luceneDirectoryExists)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Lucene index directory does not exists."));
            }

            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(this.configuration.BaseLuceneDirectory));
            if (driveInfo.IsReady)
            {
                long availableSpace = driveInfo.AvailableFreeSpace;

                // Convert 50MB to bytes
                long requiredSpace = 50 * 1024 * 1024;
                if (availableSpace < requiredSpace)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("The available space is less than 500 MB."));
                }
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Lucene index drive is not ready."));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Lucene index is healthy."));
        }
    }
}
