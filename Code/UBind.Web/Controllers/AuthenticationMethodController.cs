// <copyright file="AuthenticationMethodController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.AuthenticationMethod;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Application.Queries.AuthenicationMethod;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Organisation;

    /// <summary>
    /// Provides endpoints for creating, reading, updating, and deleting authentication methods
    /// for an organisation.
    /// Authentication Methods allow the configuration of things like SAML and SSO.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/authentication-method")]
    public class AuthenticationMethodController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;
        private readonly IOrganisationAuthorisationService organisationAuthorisationService;
        private readonly IAuthorisationService authorisationService;
        private readonly Web.Mappers.AuthenticationMethodMapper mapper
            = new Web.Mappers.AuthenticationMethodMapper();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMethodController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="organisationAuthorisationService">The service for checking authorisations for organisation
        /// access.</param>
        /// <param name="authorisationService">The service for checking authorisations.</param>
        public AuthenticationMethodController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IOrganisationAuthorisationService organisationAuthorisationService,
            IAuthorisationService authorisationService)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
            this.organisationAuthorisationService = organisationAuthorisationService;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Gets authentication methods.
        /// </summary>
        /// <param name="options">The filtering and querying options.</param>
        /// <returns>A list of authentication methods.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<AuthenticationMethodSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList([FromQuery] OrganisationQueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list organisations from a different tenancy");
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var filters = await options.ToFilters(tenantId, this.cachingResolver);
            await this.organisationAuthorisationService.ApplyRestrictionsToFilters(this.User, filters);
            var authenticationMethods
                = await this.mediator.Send(new GetAuthenticationMethodSummariesQuery(tenantId, filters));
            return this.Ok(authenticationMethods.Select(a => this.mapper.MapReadModelToResourceModel(a)));
        }

        /// <summary>
        /// Gets an authentication method.
        /// </summary>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        /// <param name="tenant">The ID or alias of the tenant (optional).</param>
        [HttpGet]
        [Route("{authenticationMethodId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<AuthenticationMethodSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid authenticationMethodId, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "list organisations from a different tenancy");
            var readModel = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantId, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(readModel, authenticationMethodId, "authentication method");
            await this.organisationAuthorisationService.ThrowIfUserCannotView(tenantId, readModel.OrganisationId, this.User);
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Creates a new authentication method for an organisation.
        /// </summary>
        /// <param name="model">The authentication method model.</param>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(IAuthenticationMethodReadModelSummary), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(
            [ModelBinder(BinderType = typeof(AuthenticationMethodModelBinder))] AuthenticationMethodUpsertModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "update an organisation in a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(model.Organisation));
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var readModel = await this.mediator.Send(
                new CreateAuthenticationMethodCommand(tenantId, organisationModel.Id, model));
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Updates an authentication method.
        /// </summary>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        /// <param name="model">The model.</param>
        [HttpPut]
        [Route("{authenticationMethodId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(
            Guid authenticationMethodId,
            [ModelBinder(BinderType = typeof(AuthenticationMethodModelBinder))] AuthenticationMethodUpsertModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "update an organisation in a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(model.Organisation));
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var readModel = await this.mediator.Send(
                new UpdateAuthenticationMethodCommand(tenantId, organisationModel.Id, authenticationMethodId, model));
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Deletes an authentication method.
        /// </summary>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        [HttpDelete]
        [Route("{authenticationMethodId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(Guid authenticationMethodId, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
            var readModel = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantId, authenticationMethodId));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, readModel.OrganisationId);
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            await this.mediator.Send(
                new DeleteAuthenticationMethodCommand(tenantId, organisationModel.Id, authenticationMethodId));
            return this.Ok();
        }

        /// <summary>
        /// Disables an authentication method.
        /// </summary>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        [HttpPatch]
        [Route("{authenticationMethodId}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(Guid authenticationMethodId, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
            var readModel = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantId, authenticationMethodId));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, readModel.OrganisationId);
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var result = await this.mediator.Send(
                new DisableAuthenticationMethodCommand(tenantId, organisationModel.Id, authenticationMethodId));
            var resourceModel = this.mapper.MapReadModelToResourceModel(result);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Enables an authentication method.
        /// </summary>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        [HttpPatch]
        [Route("{authenticationMethodId}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Enable(Guid authenticationMethodId, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
            var readModel = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantId, authenticationMethodId));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, readModel.OrganisationId);
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var result = await this.mediator.Send(
                new EnableAuthenticationMethodCommand(tenantId, organisationModel.Id, authenticationMethodId));
            var resourceModel = this.mapper.MapReadModelToResourceModel(result);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Gets the local account authentication method for an organisation.
        /// </summary>
        /// <param name="organisation">The ID or alias of the organisation.</param>
        /// <param name="tenant">The ID or alias of the tenant (optional).</param>
        [HttpGet]
        [Route("/api/v1/organisation/{organisation}/authentication-method/local-account")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<AuthenticationMethodSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationLocalAccountAuthenticationMethod(
            string organisation, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "list organisations from a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            var readModel = await this.mediator.Send(new GetLocalAccountAuthenticationMethodQuery(tenantId, organisationModel.Id));
            await this.organisationAuthorisationService.ThrowIfUserCannotView(tenantId, readModel.OrganisationId, this.User);
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Updates the local account authentication method for an organisation.
        /// </summary>
        /// <param name="organisation">The ID or alias of the organisation.</param>
        /// <param name="model">The authentication method upsert model.</param>
        [HttpPut]
        [Route("/api/v1/organisation/{organisation}/authentication-method/local-account")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLocalAccountForOrganisation(
            string organisation,
            [ModelBinder(BinderType = typeof(AuthenticationMethodModelBinder))] AuthenticationMethodUpsertModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "update an organisation in a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var readModel = await this.mediator.Send(
                new UpdateLocalAccountAuthenticationMethodCommand(tenantId, organisationModel.Id, model));
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Enables the local account authentication method for an organisation.
        /// </summary>
        /// <param name="organisation">The ID or alias of the organisation.</param>
        /// <param name="tenant">The ID or alias of the tenant (optional).</param>
        [HttpPatch]
        [Route("/api/v1/organisation/{organisation}/authentication-method/local-account/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableLocalAccountForOrganisation(
            string organisation,
            [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var readModel = await this.mediator.Send(
                new EnableLocalAccountAuthenticationMethodCommand(tenantId, organisationModel.Id));
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Disables the local account authentication method for an organisation.
        /// </summary>
        /// <param name="organisation">The ID or alias of the organisation.</param>
        /// <param name="tenant">The ID or alias of the tenant (optional).</param>
        [HttpPatch]
        [Route("/api/v1/organisation/{organisation}/authentication-method/local-account/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableLocalAccountForOrganisation(
            string organisation,
            [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
            var readModel = await this.mediator.Send(
                new DisableLocalAccountAuthenticationMethodCommand(tenantId, organisationModel.Id));
            var resourceModel = this.mapper.MapReadModelToResourceModel(readModel);
            return this.Ok(resourceModel);
        }
    }
}
