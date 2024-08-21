// <copyright file="PortalExtensionsController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Commands.PortalExtensions;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Models.User;
    using UBind.Application.Queries.PortalExtensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Organisation;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for portal extensions settings.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/portal-extensions")]
    public class PortalExtensionsController : Controller
    {
        private const int MaxRowCount = 10000;
        private readonly IMediator mediator;
        private readonly ICachingResolver cachingResolver;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalExtensionsController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        public PortalExtensionsController(
            IMediator mediator,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Get a list of the portal page trigger configurations.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The portal extensions settings.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("tenant/{tenant}/environment/{environment}")]
        public async Task<IActionResult> GetPortalPageTriggers(
            string tenant, DeploymentEnvironment environment)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var request = new GetAutomationsForEnvironmentQuery(tenantModel.Id, environment);
            var productAutomations = await this.mediator.Send(request);
            var resourceModels = new List<ResourceModels.Automation.PortalPageTrigger>();
            foreach (var productAutomation in productAutomations)
            {
                foreach (var trigger in productAutomation.Automation.Triggers)
                {
                    if (trigger is PortalPageTrigger portalPageTrigger)
                    {
                        var resourceModel = new ResourceModels.Automation.PortalPageTrigger(
                            tenantModel.Id,
                            productAutomation.ProductId,
                            productAutomation.Environment,
                            productAutomation.Automation.Alias,
                            portalPageTrigger);
                        resourceModels.Add(resourceModel);
                    }
                }
            }

            return this.Ok(resourceModels);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/{entityType}/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTrigger(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            EntityType entityType,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] QueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                entityType,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver, nameof(Entity<Guid>.CreatedTicksSinceEpoch)),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/customer/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerCustomer(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] QueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Customer,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver, nameof(CustomerReadModel.CreatedTicksSinceEpoch)),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/quote/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerQuote(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] QuoteQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Quote,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/policy/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerPolicy(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] PolicyQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Policy,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/organisation/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerOrganisation(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] OrganisationQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Organisation,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/user/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerUser(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] UserQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.User,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/role/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerRole(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] RoleQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Role,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver),
                cancellationToken);
        }

        /// <summary>
        /// Execute portal page trigger.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="automationAlias">The automation alias.</param>
        /// <param name="triggerAlias">The trigger alias.</param>
        /// <param name="pageType">The page type.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>Done.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{tenant}/product/{product}/environment/{environment}/automation/{automationAlias}/trigger/{triggerAlias}/product/{pageType}/{tab}/{entityId}")]
        public async Task<IActionResult> ExecutePortalPageTriggerProduct(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            PageType pageType,
            string tab,
            Guid entityId,
            [FromQuery] ProductQueryOptionsModel options,
            CancellationToken cancellationToken)
        {
            options.PageSize = MaxRowCount;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ExecuteTrigger(
                tenant,
                product,
                environment,
                automationAlias,
                triggerAlias,
                EntityType.Product,
                pageType,
                tab,
                entityId,
                await options.ToFilters(tenantModel.Id, this.cachingResolver, $"{nameof(Product.Details)}.{nameof(Product.Details.Name)}"),
                cancellationToken);
        }

        private async Task<IActionResult> ExecuteTrigger(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            EntityType entityType,
            PageType pageType,
            string tab,
            Guid entityId,
            EntityListFilters filters,
            CancellationToken cancellationToken)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            var organisationId = tenantModel.Details?.DefaultOrganisationId ?? Guid.Empty;
            UserAuthorisationModel authModel = await this.authorisationService.GenerateUserAuthorisationModel(this.User, tenantModel);

            this.ValidatePermissionOrThrow(tenantModel.Id, entityType, entityId, authModel);

            var request = new ExecutePortalPageTriggerCommand(
               tenantModel.Id,
               organisationId,
               productModel.Id,
               environment,
               automationAlias,
               triggerAlias,
               entityType,
               pageType,
               tab,
               this.User,
               filters,
               entityId);

            var response = await this.mediator.Send(request, cancellationToken);
            this.Response.Headers.Add("Success-Message", response.successMessage);

            var file = response.file;
            if (file != null)
            {
                var filename = file.FileName.ToString().ToLower();
                return this.File(file.Content, ContentTypeHelper.GetMimeTypeForFileExtension(filename), filename);
            }

            return this.Ok();
        }

        private void ValidatePermissionOrThrow(Guid tenantId, EntityType entityType, Guid entityId, UserAuthorisationModel userAuthModel)
        {
            var userTenantId = this.User.GetTenantId();
            if (tenantId != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden("get a product from a different tenancy"));
            }

            var entityPermissions = new Dictionary<EntityType, Permission[]>
            {
                { EntityType.Customer, new Permission[] { Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations, Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations } },
                { EntityType.Quote, new Permission[] { Permission.ManageQuotes, Permission.ManageAllQuotes, Permission.ViewQuotes, Permission.ViewAllQuotes } },
                { EntityType.Policy, new Permission[] { Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ViewPolicies, Permission.ViewAllPolicies } },
                { EntityType.Claim, new Permission[] { Permission.ManageClaims, Permission.ManageAllClaims, Permission.ViewClaims, Permission.ViewAllClaims } },
                { EntityType.Message, new Permission[] { Permission.ManageMessages, Permission.ManageAllMessages, Permission.ViewMessages, Permission.ViewAllMessages } },
                { EntityType.EmailMessage, new Permission[] { Permission.ManageMessages, Permission.ManageAllMessages, Permission.ViewMessages, Permission.ViewAllMessages } },
                { EntityType.SmsMessage, new Permission[] { Permission.ManageMessages, Permission.ManageAllMessages, Permission.ViewMessages, Permission.ViewAllMessages } },
                { EntityType.Report, new Permission[] { Permission.ManageReports, Permission.ViewReports } },
                { EntityType.Organisation, new Permission[] { Permission.ManageOrganisations, Permission.ManageAllOrganisations, Permission.ViewOrganisations, Permission.ViewAllOrganisations } },
                { EntityType.User, new Permission[] { Permission.ManageUsers, Permission.ViewUsers } },
                { EntityType.Role, new Permission[] { Permission.ManageRoles, Permission.ViewRoles } },
                { EntityType.Portal, new Permission[] { Permission.ManagePortals, Permission.ViewPortals } },
                { EntityType.Product, new Permission[] { Permission.ManageProducts, Permission.ViewProducts } },
                { EntityType.QuoteVersion, new Permission[] { Permission.ManageQuoteVersions, Permission.ViewQuoteVersions } },
                { EntityType.ClaimVersion, new Permission[] { Permission.ManageClaims, Permission.ManageAllClaims, Permission.ViewClaims, Permission.ViewAllClaims } },
                { EntityType.PolicyTransaction, new Permission[] { Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ViewPolicies, Permission.ViewAllPolicies } },
                { EntityType.Person, new Permission[] { Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations, Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations } },
            };

            if (!userAuthModel.Permissions.Select(p => Enum.Parse<Permission>(p, true)).Any(p => entityPermissions[entityType].Contains(p)))
            {
                throw new ErrorException(
                    Errors.General.NotAuthorized("execute portal page trigger", entityType.ToString(), entityId));
            }
        }
    }
}
