// <copyright file="DeploymentsController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Deployment;
    using UBind.Web.ResourceModels.ProductRelease;

    /// <summary>
    /// Controller for product requests.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/{tenant}/{product}/deployments")]
    public class DeploymentsController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IDeploymentRepository deploymentRepository;
        private readonly IDeploymentService deploymentService;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentsController"/> class.
        /// </summary>
        /// <param name="deploymentRepository">The deployment repository.</param>
        /// <param name="deploymentService">The deployment service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        public DeploymentsController(
            IDeploymentRepository deploymentRepository,
            IDeploymentService deploymentService,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
        {
            this.cachingResolver = cachingResolver;
            this.deploymentRepository = deploymentRepository;
            this.deploymentService = deploymentService;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Gets the current deployments in each environment for a given product..
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to retrieve deployments for.</param>
        /// <param name="product">The ID or Alias of the product to retrieve deployments for.</param>
        /// <returns>Current deployments.</returns>
        [HttpGet("current")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(List<DeploymentModel>), StatusCodes.Status200OK)]
        [MustHaveOneOfPermissions(Permission.ViewReleases, Permission.ViewProducts)]
        public async Task<IActionResult> GetLatestDeploymentsForProduct(string tenant, string product)
        {
            Domain.Product.Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var deployments = this.deploymentRepository.GetCurrentDeployments(productModel.TenantId, productModel.Id);

            List<DeploymentModel> deploymentVM = new List<DeploymentModel>();
            foreach (var deployment in deployments)
            {
                ReleaseModel releaseVM = null;
                if (deployment.Release != null)
                {
                    releaseVM = new ReleaseModel(deployment.Release);
                }

                deploymentVM.Add(new DeploymentModel(deployment, releaseVM));
            }

            return this.Ok(deploymentVM);
        }

        /// <summary>
        /// Gets the current deployment of a given product to a given environment.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to retrieve deployments for.</param>
        /// <param name="product">The ID or Alias of the product to retrieve deployments for.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>All products.</returns>
        [HttpGet("current/{environment}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(DeploymentModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLatestDeploymentsForProductEnvironment(string tenant, string product, DeploymentEnvironment environment)
        {
            Domain.Product.Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var deployment = this.deploymentRepository.GetCurrentDeployment(productModel.TenantId, productModel.Id, environment);
            if (deployment == null)
            {
                return Errors.General.NotFound("current deployment for product", productModel.Id).ToProblemJsonResult();
            }

            ReleaseModel releaseVM = null;
            if (deployment.Release != null)
            {
                releaseVM = new ReleaseModel(deployment.Release);
            }

            var deploymentVM = new DeploymentModel(deployment, releaseVM);

            return this.Ok(deploymentVM);
        }

        /// <summary>
        /// Gets all deployments of a given product to a given environment.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to retrieve deployments for.</param>
        /// <param name="product">The ID or Alias of the product to retrieve deployments for.</param>
        /// <param name="environment">The environment to fetch the deployment for.</param>
        /// <returns>All products.</returns>
        [HttpGet("all/{environment}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(IEnumerable<Deployment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDeploymentsForProduct(string tenant, string product, DeploymentEnvironment environment)
        {
            Domain.Product.Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var deployments = this.deploymentRepository.GetAllCurrentDeployments(
                productModel.TenantId,
                productModel.Id,
                environment);
            return this.Ok(deployments);
        }

        /// <summary>
        /// Create a new deployments.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the product's tenant.</param>
        /// <param name="product">The ID or Alias of the product to deploy.</param>
        /// <param name="environment">The environment to deploy to.</param>
        /// <param name="release">The release to deploy.</param>
        /// <returns>The newly created deployment.</returns>
        [HttpPost("current/{environment}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(DeploymentModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDeployments(string tenant, string product, string environment, [FromBody] ReleaseModel release)
        {
            var isSuccess = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            Domain.Product.Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var deployment = this.deploymentService.DeployRelease(
                productModel.TenantId,
                release?.Id,
                env,
                productModel.Id,
                out string releaseToRemoveFromFlexcelPool);

            ReleaseModel releaseVM = null;
            if (deployment.Release != null)
            {
                releaseVM = new ReleaseModel(deployment.Release);
            }

            var deploymentVM = new DeploymentModel(deployment, releaseVM);

            return this.Ok(deploymentVM);
        }

        /// <summary>
        /// Gets the current deployments for a given release.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to retrieve deployments for.</param>
        /// <param name="product">The ID or Alias of the product to retrieve deployments for.</param>
        /// <param name="releaseId">The release ID of the product to retrieve deployments for.</param>
        /// <returns>Current deployments.</returns>
        [HttpGet("current/release/{releaseId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(List<DeploymentModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLatestDeploymentForRelease(string tenant, string product, Guid releaseId)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var deployments = this.deploymentRepository.GetCurrentDeploymentsForRelease(
                productModel.TenantId,
                releaseId,
                productModel.Id);

            List<DeploymentModel> deploymentVMs = new List<DeploymentModel>();
            foreach (var deployment in deployments)
            {
                ReleaseModel releaseVM = null;
                if (deployment.Release != null)
                {
                    releaseVM = new ReleaseModel(deployment.Release);
                }

                deploymentVMs.Add(new DeploymentModel(deployment, releaseVM));
            }

            return this.Ok(deploymentVMs);
        }

        /// <summary>
        /// Purges old Deployments.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="daysRetention">The number of days we only want to retain the DB, from the latest Release.</param>
        /// <returns>Ok.</returns>
        [HttpDelete("purge/{daysRetention}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PurgeDeployments(string tenant, string product, int daysRetention = 60)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.authorisationService.ThrowIfUserCannotModifyProduct(productModel.TenantId, this.User, productModel.Id, "purge deployment");
            this.deploymentService.PurgeDeployments(productModel.TenantId, daysRetention, productModel.Id);
            return this.Ok();
        }
    }
}
