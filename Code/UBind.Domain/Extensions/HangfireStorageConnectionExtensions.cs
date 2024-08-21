// <copyright file="HangfireStorageConnectionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using Hangfire.Storage;
    using UBind.Domain.Processing;

    /// <summary>
    /// Defines the <see cref="HangfireStorageConnectionExtensions" />.
    /// </summary>
    public static class HangfireStorageConnectionExtensions
    {
        /// <summary>
        /// Sets the tenant, product and environment parameters against a particular hangfire job.
        /// </summary>
        /// <param name="connection">The IStorageConnection instance.</param>
        /// <param name="jobId">the Id of the hangfire job.</param>
        /// <param name="tenantAlias">the tenant alias.</param>
        /// <param name="productAlias">the product alias.</param>
        /// <param name="environment">the environment alias.</param>
        public static void SetJobParameters(this IStorageConnection connection, string jobId, string tenantAlias = null, string productAlias = null, string environment = null)
        {
            connection.SetJobParameter(jobId, JobParameter.IsAcknowledgedParameterName, "false");

            if (tenantAlias != null)
            {
                connection.SetJobParameter(jobId, JobParameter.TenantParameterName, tenantAlias);
            }

            if (productAlias != null)
            {
                connection.SetJobParameter(jobId, JobParameter.ProductParameterName, productAlias);
            }

            if (environment != null)
            {
                connection.SetJobParameter(jobId, JobParameter.EnvironmentParameterName, environment);
            }
        }
    }
}
