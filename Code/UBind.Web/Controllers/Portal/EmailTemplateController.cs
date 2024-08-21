// <copyright file="EmailTemplateController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.FeatureSettings;
    using UBind.Application.Queries.Principal;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for Email Template.
    /// </summary>
    [Produces("application/json")]
    [MustBeLoggedIn]
    [Route("api/v1/email-template")]
    public class EmailTemplateController : PortalBaseController
    {
        private readonly IEmailTemplateService emailTemplateService;
        private readonly IFeatureSettingService settingService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateController"/> class.
        /// </summary>
        /// <param name="emailTemplateService">The email template service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public EmailTemplateController(
            IEmailTemplateService emailTemplateService,
            IFeatureSettingService settingService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
            : base(cachingResolver)
        {
            this.emailTemplateService = emailTemplateService;
            this.settingService = settingService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the list of email templates filtered by tenant only, tenant and portal id, or tenant and product id.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="portal">The portal Id or Alias (optional).</param>
        /// <param name="product">The product Id or Alias(optional).</param>
        /// <returns>All tenant Email Template.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<EmailTemplateResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] string tenant, [FromQuery] string portal, [FromQuery] string product)
        {
            tenant = tenant ?? this.User.GetTenantIdOrNull()?.ToString();
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = string.IsNullOrEmpty(product)
                ? null
                : await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var portalModel = string.IsNullOrEmpty(portal)
                ? null
                : await this.CachingResolver.GetPortalOrThrow(tenantModel, new GuidOrAlias(portal));
            await this.VerifyAccess(tenantModel.Id, "view", portalModel?.Id, productModel?.Id);
            IEnumerable<SystemEmailTemplate> resources;
            if (productModel != null)
            {
                resources = await this.emailTemplateService.GetTemplatesByProductId(
                     tenantModel.Id,
                     productModel.Id);
            }
            else if (portalModel != null)
            {
                resources = await this.emailTemplateService.GetTemplatesByPortalId(tenantModel.Id, portalModel.Id);
            }
            else
            {
                resources = await this.emailTemplateService.GetTemplatesByTenantId(tenantModel.Id);
            }

            return this.Ok(resources.Select(x => new EmailTemplateResponseModel(x)));
        }

        /// <summary>
        /// Gets the specific email template under the given email template Id.
        /// </summary>
        /// <param name="emailTemplateId">The Id of the email template to look for.</param>
        /// <param name="tenant">The tenant Id or Alias (optional).</param>
        /// <returns>Result of email template model.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("{emailTemplateId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(EmailTemplateResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmailTemplateById(Guid emailTemplateId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                 tenant,
                                 "view the settings for another tenant",
                                 "your user account doesn't have access to the master tenancy");

            var emailTemplate = this.emailTemplateService.GetTemplateById(tenantId, emailTemplateId);
            await this.VerifyAccess(emailTemplate.TenantId, "view", emailTemplate.PortalId, emailTemplate.ProductId);
            return this.Ok(new EmailTemplateResponseModel(emailTemplate));
        }

        /// <summary>
        /// Update the specific email template under the given email template Id.
        /// </summary>
        /// <param name="emailTemplateId">The Id of the email template to update.</param>
        /// <param name="model">The view model containing the new details.</param>
        /// <returns>The newly updated email template.</returns>
        [MustBeLoggedIn]
        [HttpPut]
        [Route("{emailTemplateId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(EmailTemplateResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateEmailTemplate(Guid emailTemplateId, [FromBody] EmailTemplateRequestModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                 model.Tenant,
                                 "view the settings for another tenant",
                                 "your user account doesn't have access to the master tenancy");

            var emailTemplate = this.emailTemplateService.GetTemplateById(tenantId, emailTemplateId);
            await this.VerifyAccess(emailTemplate.TenantId, "update", emailTemplate.PortalId, emailTemplate.ProductId);
            var templateData = model.GetData();
            var serviceResult = await this.emailTemplateService.UpdateEmailTemplate(
                tenantId, emailTemplateId, templateData);
            var result = new EmailTemplateResponseModel(serviceResult);
            return this.Ok(result);
        }

        /// <summary>
        /// Disable the specific email template.
        /// </summary>
        /// <param name="emailTemplateId">The Id of the email template to disable.</param>
        /// <param name="tenant">The tenant ID or Alias (optional).</param>
        /// <returns>The newly updated email template.</returns>
        [MustBeLoggedIn]
        [HttpPatch]
        [Route("{emailTemplateId}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(EmailTemplateResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(Guid emailTemplateId, string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                        tenant,
                        "view the settings for another tenant",
                        "your user account doesn't have access to the master tenancy");

            var emailTemplate = this.emailTemplateService.GetTemplateById(tenantId, emailTemplateId);
            await this.VerifyAccess(emailTemplate.TenantId, "disable", emailTemplate.PortalId, emailTemplate.ProductId);
            await this.emailTemplateService.Disable(tenantId, emailTemplateId);
            return this.Ok(new EmailTemplateResponseModel(emailTemplate));
        }

        /// <summary>
        /// Enable the specific email template.
        /// </summary>
        /// <param name="emailTemplateId">The Id of the email template to enable.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>The newly updated email template.</returns>
        [MustBeLoggedIn]
        [HttpPatch]
        [Route("{emailTemplateId}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(EmailTemplateResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Enable(Guid emailTemplateId, string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                        tenant,
                        "view the settings for another tenant",
                        "your user account doesn't have access to the master tenancy");

            var emailTemplate = this.emailTemplateService.GetTemplateById(tenantId, emailTemplateId);
            await this.VerifyAccess(emailTemplate.TenantId, "enable", emailTemplate.PortalId, emailTemplate.ProductId);
            await this.emailTemplateService.Enable(tenantId, emailTemplateId);
            return this.Ok(new EmailTemplateResponseModel(emailTemplate));
        }

        private async Task VerifyAccess(Guid tenantId, string action, Guid? portalId, Guid? productId)
        {
            this.GetContextTenantIdOrThrow(
                        tenantId,
                        action + " email templates for another tenant",
                        "your user account doesn't have access to the master tenancy");

            if (portalId.HasValue)
            {
                if (!await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, action == "view" ? Permission.ViewPortals : Permission.ManagePortals)))
                {
                    throw new ErrorException(
                        Errors.General.NotAuthorized(action, "email template"));
                }
            }
            else if (productId == null)
            {
                if (!await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, action == "view" ? Permission.ViewTenants : Permission.ManageTenants)))
                {
                    throw new ErrorException(
                        Errors.General.NotAuthorized(action, "email template"));
                }
            }
            else
            {
                if (!await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, action == "view" ? Permission.ViewProducts : Permission.ManageProducts)))
                {
                    throw new ErrorException(
                        Errors.General.NotAuthorized(action, "email template"));
                }

                var command = new UserHasActiveFeatureSettingQuery(this.User, Feature.ProductManagement);
                var hasActiveFeature = await this.mediator.Send(command);

                if (!hasActiveFeature)
                {
                    throw new ErrorException(
                        Errors.General.Forbidden("manage email templates for a product", "you don't have this feature activated"));
                }
            }
        }
    }
}
