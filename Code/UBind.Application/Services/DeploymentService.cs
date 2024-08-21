// <copyright file="DeploymentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class DeploymentService : IDeploymentService
    {
        private readonly IReleaseRepository releaseRepository;
        private readonly IDeploymentRepository deploymentRepository;
        private readonly IClock clock;
        private readonly IProductRepository productRepository;
        private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentService"/> class.
        /// </summary>
        /// <param name="releaseRepository">For obtaining the release to deploy.</param>
        /// <param name="deploymentRepository">For persisting the deployment.</param>
        /// <param name="clock">For obtaining the current time.</param>
        /// <param name="productRepository">For obtaining the product to deploy.</param>
        /// <param name="periodicTriggerScheduler">The service that will add or remove periodic jobs.</param>
        public DeploymentService(
            IReleaseRepository releaseRepository,
            IDeploymentRepository deploymentRepository,
            IClock clock,
            IProductRepository productRepository,
            IAutomationPeriodicTriggerScheduler periodicTriggerScheduler)
        {
            this.releaseRepository = releaseRepository;
            this.deploymentRepository = deploymentRepository;
            this.clock = clock;
            this.productRepository = productRepository;
            this.periodicTriggerScheduler = periodicTriggerScheduler;
        }

        /// <inheritdoc/>
        public Deployment DeployRelease(
            Guid tenantId,
            Guid? releaseId,
            DeploymentEnvironment environment,
            Guid productId,
            out string releaseToRemoveFromFlexcelPool)
        {
            if (environment < DeploymentEnvironment.Staging)
            {
                throw new ArgumentException("Only staging and production environments can be deployed to.");
            }

            var release = (Release)null;
            var product = this.productRepository.GetProductById(tenantId, productId);
            if (product == null)
            {
                throw new ErrorException(Errors.General.NotFound("product", productId));
            }
            if (releaseId != Guid.Empty && releaseId != null)
            {
                release = this.releaseRepository.GetReleaseWithoutAssets(tenantId, releaseId.Value);
                if (release.ProductId != productId)
                {
                    throw new ArgumentException($"Release {releaseId} does not belong to product {productId}.");
                }

                if (release.TenantId != tenantId)
                {
                    throw new ArgumentException($"Release {releaseId} does not belong to tenant {tenantId}.");
                }
            }
            releaseToRemoveFromFlexcelPool = null;
            var defaultReleaseId = "00000000-0000-0000-0000-000000000000";
            var stagingDeploymentReleaseId = releaseId != null
                ? this.deploymentRepository.GetDefaultReleaseId(tenantId, productId, DeploymentEnvironment.Staging).ToString()
                : null;
            var productionDeploymentReleaseId = releaseId != null
                ? this.deploymentRepository.GetDefaultReleaseId(tenantId, productId, DeploymentEnvironment.Production).ToString()
                : null;
            var isPromotedToProd = !string.IsNullOrEmpty(productionDeploymentReleaseId) && productionDeploymentReleaseId.IndexOf(defaultReleaseId) < 0;
            var isPromotedToStaging = !string.IsNullOrEmpty(stagingDeploymentReleaseId) && stagingDeploymentReleaseId.IndexOf(defaultReleaseId) < 0;
            if (environment == DeploymentEnvironment.Production)
            {
                if (release != null && !isPromotedToStaging)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToProd ? productionDeploymentReleaseId : null;
                }
                else if (release == null && !isPromotedToStaging && isPromotedToProd)
                {
                    releaseToRemoveFromFlexcelPool = productionDeploymentReleaseId;
                }
                else if (release != null && isPromotedToStaging && stagingDeploymentReleaseId != release.Id.ToString() && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToProd ? productionDeploymentReleaseId : null;
                }
                else if (release != null && isPromotedToStaging && productionDeploymentReleaseId != release.Id.ToString() && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToProd ? productionDeploymentReleaseId : null;
                }
                else if (release == null && isPromotedToStaging && isPromotedToProd && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = productionDeploymentReleaseId;
                }
            }
            if (environment == DeploymentEnvironment.Staging)
            {
                if (release != null && !isPromotedToProd)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToStaging ? stagingDeploymentReleaseId : null;
                }
                else if (release == null && !isPromotedToProd && isPromotedToStaging)
                {
                    releaseToRemoveFromFlexcelPool = stagingDeploymentReleaseId;
                }
                else if (release != null && isPromotedToProd && productionDeploymentReleaseId != release.Id.ToString() && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToStaging ? stagingDeploymentReleaseId : null;
                }
                else if (release != null && isPromotedToProd && stagingDeploymentReleaseId != release.Id.ToString() && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = isPromotedToStaging ? stagingDeploymentReleaseId : null;
                }
                else if (release == null && isPromotedToStaging && isPromotedToProd && productionDeploymentReleaseId != stagingDeploymentReleaseId)
                {
                    releaseToRemoveFromFlexcelPool = stagingDeploymentReleaseId;
                }
            }
            var deployment = new Deployment(
                product.TenantId,
                product.Id,
                environment,
                release,
                this.clock.GetCurrentInstant());
            this.deploymentRepository.Insert(deployment);
            this.deploymentRepository.SaveChanges();
            if (releaseId == Guid.Empty || releaseId == null)
            {
                this.periodicTriggerScheduler.RemovePeriodicTriggerJobs(product.TenantId, product.Id, environment);
            }
            else
            {
                this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(product.TenantId, product.Id, environment);
            }

            // refreshes the cache when you sync to a different environment.
            MemoryCachingHelper.Remove("portalFeature+TenantId:" + tenantId + "|Environment:" + environment);

            return deployment;
        }

        /// <inheritdoc/>
        public void PurgeDeployments(
            Guid? tenantId,
            int daysRetention,
            Guid? productId = null)
        {
            this.deploymentRepository.PurgeDeployments(tenantId, daysRetention, productId);
            this.deploymentRepository.SaveChanges();
        }
    }
}
