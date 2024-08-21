// <copyright file="IDeploymentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Service for creating deployments.
    /// </summary>
    public interface IDeploymentService
    {
        /// <summary>
        /// Deploy a given release to a given environment.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant and part of the product's compound key.</param>
        /// <param name="releaseId">The ID of the release to deploy.</param>
        /// <param name="environment">The environment to deploy to.</param>
        /// <param name="productId">The ID of the product to deploy and part of the product's compound key.</param>
        /// <param name="releaseToRemoveFromFlexcelPool">an output variable to return the releaseId to be purged.</param>
        /// <returns>The deployment that is created.</returns>
        Deployment DeployRelease(
            Guid tenantId,
            Guid? releaseId,
            DeploymentEnvironment environment,
            Guid productId,
            out string releaseToRemoveFromFlexcelPool);

        /// <summary>
        /// Purges old Deployments.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="daysRetention">The days we only want to retain the DB.</param>
        /// <param name="productId">The product ID.</param>
        void PurgeDeployments(
            Guid? tenantId,
            int daysRetention,
            Guid? productId = null);
    }
}
