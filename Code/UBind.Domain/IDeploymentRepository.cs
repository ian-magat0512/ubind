// <copyright file="IDeploymentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Repository for storing deployment records.
    /// </summary>
    public interface IDeploymentRepository : IReleaseRepository
    {
        /// <summary>
        /// Get the current deployment for a given product and environment.
        /// Doesn't include the File Contents - just the file objects.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The current deployment, or none if none ever deployed.</returns>
        Deployment? GetCurrentDeployment(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Get the current deployment for a given product and environment, without assets or files.
        /// This is faster since it doesn't retreive the list of files associated with a release.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The current deployment, or none if none ever deployed.</returns>
        Deployment? GetCurrentDeploymentWithoutAssets(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Get the ID of the default release deployed for a given product and environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The ID of the current release, or null if none ever deployed.</returns>
        Guid? GetDefaultReleaseId(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Get the current deployments for every environment for a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>The current deployments of a product for each environment that has a deployment.</returns>
        IEnumerable<Deployment> GetCurrentDeployments(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment = DeploymentEnvironment.None);

        /// <summary>
        /// Get the current deployments for every environment for all products.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The current deployments of all products for each environment that has a deployment.</returns>
        IEnumerable<Deployment> GetAllCurrentDeployments(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Get the current deployments for every environment for a given product.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="releaseId">The release ID of the product.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The current deployments of a product for each environment that has a deployment.</returns>
        IEnumerable<Deployment> GetCurrentDeploymentsForRelease(Guid tenantId, Guid releaseId, Guid productId);

        /// <summary>
        /// Get the history of deployments for a given product and environment.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The history of deployments to that environment.</returns>
        IEnumerable<Deployment> GetDeploymentHistory(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Purges old Deployments.
        /// </summary>
        /// <param name="tenantId">The tenant.</param>
        /// <param name="daysRetention">The days we only want to retain the DB.</param>
        /// <param name="productId">The product Id.</param>
        /// <returns>The deleted deployments.</returns>
        IEnumerable<Deployment> PurgeDeployments(Guid? tenantId, int daysRetention, Guid? productId = null);

        /// <summary>
        /// Insert a new deployment into the repository.
        /// </summary>
        /// <param name="deployment">The deployment to insert.</param>
        void Insert(Deployment deployment);

        /// <summary>
        /// Persist insertions.
        /// </summary>
        void SaveChanges();
    }
}
