// <copyright file="AdditionalPropertyDefinitionController.cs" company="uBind">
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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.AdditionalPropertyDefinition;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.Schema;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Queries.AdditionalPropertyDefinition;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling additional property related requests.
    /// </summary>
    [Authorize]
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/additional-property-definition")]
    public class AdditionalPropertyDefinitionController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAdditionalPropertyModelResolverHelper additionalPropertyModelResolver;
        private readonly ICachingResolver cachingResolver;
        private readonly IAdditionalPropertyAuthorisationService additionalPropertyAuthorisationService;
        private readonly IAdditionalPropertyDefinitionValidator additionalPropertyValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionController"/> class.
        /// Entry point for additional property API.
        /// </summary>
        /// <param name="mediator"><see cref="MediatR"/>.</param>
        /// <param name="clock">Local time.</param>
        /// <param name="httpContextPropertiesResolver">User resolver.</param>
        /// <param name="additionalPropertyModelResolver">Context Resolver.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public AdditionalPropertyDefinitionController(
            ICqrsMediator mediator,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAdditionalPropertyDefinitionValidator additionalPropertyValidator,
            IAdditionalPropertyModelResolverHelper additionalPropertyModelResolver,
            ICachingResolver cachingResolver,
            IAdditionalPropertyAuthorisationService additionalPropertyAuthorisationService)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.additionalPropertyModelResolver = additionalPropertyModelResolver;
            this.cachingResolver = cachingResolver;
            this.additionalPropertyAuthorisationService = additionalPropertyAuthorisationService;
            this.additionalPropertyValidator = additionalPropertyValidator;
        }

        /// <summary>
        /// Creates additional property definition.
        /// </summary>
        /// <param name="model">The request model schema.</param>
        /// <returns>Http response.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(typeof(AdditionalPropertyDefinitionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(
            [FromBody] AdditionalPropertyDefinitionCreateOrUpdateModel model)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(model.Tenant);
            var definitionModel = await this.additionalPropertyModelResolver.ResolveToDefinitionModel(model);
            var createCommand = new CreateAdditionalPropertyDefinitionCommand(
                tenantId,
                definitionModel,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.GetCurrentInstant(),
                definitionModel.ContextType,
                definitionModel.ContextId);

            var newReadModel = await this.mediator.Send(createCommand);
            var viewModel = new AdditionalPropertyDefinitionModel(newReadModel);

            return this.Ok(viewModel);
        }

        /// <summary>
        /// Updates existing additional property definition.
        /// </summary>
        /// <param name="additionalPropertyDefinitionId">Primary ID of additional property definition.</param>
        /// <param name="model">The request model schema.</param>
        /// <returns>Http response.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [Route("{additionalPropertyDefinitionId}")]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(typeof(AdditionalPropertyDefinitionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(
            Guid additionalPropertyDefinitionId,
            [FromBody] AdditionalPropertyDefinitionCreateOrUpdateModel model)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(model.Tenant);
            var definitionModel = await this.additionalPropertyModelResolver.ResolveToDefinitionModel(model);
            var updateCommand = new UpdateAdditionalPropertyDefinitionCommand(
                tenantId,
                additionalPropertyDefinitionId,
                definitionModel,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.GetCurrentInstant(),
                definitionModel.ContextType);

            var updateReadModel = await this.mediator.Send(updateCommand);
            var viewModel = new AdditionalPropertyDefinitionModel(updateReadModel);
            return this.Ok(viewModel);
        }

        /// <summary>
        /// Marks additional property as deleted (soft delete).
        /// </summary>
        /// <param name="additionalPropertyDefinitionId">Primary key of the additional property.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>Http response.</returns>
        [HttpDelete]
        [Route("{additionalPropertyDefinitionId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(Guid additionalPropertyDefinitionId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant);
            var deleteCommand = new DeleteAdditionalPropertyDefinitionCommand(
                tenantId,
                additionalPropertyDefinitionId,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.GetCurrentInstant());

            await this.mediator.Send(deleteCommand);
            return this.Ok();
        }

        /// <summary>
        /// Gets an additional property view model based on its ID.
        /// </summary>
        /// <param name="additionalPropertyDefinitionId">The primary key of the additional property.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>Instance of <see cref="AdditionalPropertyDefinitionModel"/>.</returns>
        [HttpGet]
        [Route("{additionalPropertyDefinitionId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 180)]
        [ProducesResponseType(typeof(AdditionalPropertyDefinitionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid additionalPropertyDefinitionId, [FromQuery] string tenant)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(tenant);
            var readModelQuery = new GetAdditionalPropertyDefinitionByIdQuery(tenantId, additionalPropertyDefinitionId);
            var readModel = await this.mediator.Send(readModelQuery);
            if (this.User.GetTenantId() != Tenant.MasterTenantId)
            {
                await this.additionalPropertyAuthorisationService.ThrowIfUserCannotViewEntityType(
                    this.User,
                    readModel.TenantId,
                    readModel.EntityType);
            }

            var viewModel = readModel != null ? new AdditionalPropertyDefinitionModel(readModel) : null;
            return this.Ok(viewModel);
        }

        /// <summary>
        /// Gets the list of <see cref="AdditionalPropertyDefinitionModel"/> using filters.
        /// </summary>
        /// <param name="queryModel">Query model <see cref="AdditionalPropertyDefinitionQueryModel"/>.</param>
        /// <returns>List of <see cref="AdditionalPropertyDefinitionModel"/>.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<AdditionalPropertyDefinitionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdditionalPropertyDefinitions(
            [FromQuery] AdditionalPropertyDefinitionQueryModel queryModel)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(queryModel?.Tenant);
            if (this.User.GetTenantId() != Tenant.MasterTenantId)
            {
                if (queryModel.Entity.HasValue)
                {
                    if (queryModel.EntityId.HasValue)
                    {
                        await this.additionalPropertyAuthorisationService.ThrowIfUserCannotViewEntity(
                            this.User,
                            tenantId,
                            queryModel.Entity.Value,
                            queryModel.EntityId);
                    }
                    else
                    {
                        await this.additionalPropertyAuthorisationService.ThrowIfUserCannotViewEntityType(
                            this.User,
                            tenantId,
                            queryModel.Entity.Value);
                    }
                }
                else if (!await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, Permission.ViewAdditionalPropertyValues))
                    && !await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, Permission.EditAdditionalPropertyValues))
                    && !await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, Permission.ManageAdditionalPropertyDefinitions)))
                {
                    throw new ErrorException(Errors.General.NotAuthorized(
                        $"access additional properties because you dont have permission"));
                }
            }

            var modelFilter = await this.additionalPropertyModelResolver.ResolveToDomainReadModelFilter(queryModel);
            var query = new AdditionalPropertyDefinitionsByModelFilterQuery(tenantId, modelFilter);
            var readModels = await this.mediator.Send(query);
            var viewModel = readModels?.Select(rm => new AdditionalPropertyDefinitionModel(rm)).ToList();
            return this.Ok(viewModel);
        }

        /// <summary>
        /// As the user types in a value in the additional property name or alias field. This endpoint will be invoked
        /// to check if the current value being typed in already exists. If exists then the return to be true otherwise
        /// false.
        /// 1) If the tenant already has "Additional Property A" under Quote entity type then a user can no longer have
        /// same additional property name under same entity type for different context such as product and organisation
        /// 2) If the tenant already has "Additional Property A" under quote entity type then a user can no longer have
        /// same additional property name under same entity type for the same context.
        /// </summary>
        /// <param name="queryModel">Query model <see cref="AdditionalPropertyDefinitionQueryModel"/>.</param>
        /// <returns>List of <see cref="AdditionalPropertyDefinitionModel"/>.</returns>
        [HttpGet("verify-name-or-alias")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> NameOrAliasIsAvailableOnCreate(
            [FromQuery] AdditionalPropertyDefinitionQueryModel queryModel)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(queryModel?.Tenant);
            var modelFilter = await this.additionalPropertyModelResolver.ResolveToDomainReadModelFilter(queryModel);
            var validator = this.additionalPropertyValidator.GetValidatorByContextType(queryModel.ContextType.Value);
            var result = validator.IsNameOrAliasInUse(
                tenantId,
                queryModel.Name,
                queryModel.Alias,
                queryModel.Entity.Value,
                modelFilter.ContextId,
                modelFilter.ParentContextId,
                queryModel.ContextType.Value);
            return this.Ok(!result);
        }

        /// <summary>
        /// As the user types in a value in the additional property name or alias field. This endpoint will be invoked
        /// to check if the current value being typed in already exists. If exists then the return to be true otherwise
        /// false.
        /// 1) If the tenant already has "Additional Property A" under Quote entity type then a user can no longer have
        /// same additional property name under same entity type for different context such as product and organisation
        /// 2) If the tenant already has "Additional Property A" under quote entity type then a user can no longer have
        /// same additional property name under same entity type for the same context.
        /// </summary>
        /// <param name="id">Additional property definition ID.</param>
        /// <param name="queryModel">Query model <see cref="AdditionalPropertyDefinitionQueryModel"/>.</param>
        /// <returns>List of <see cref="AdditionalPropertyDefinitionModel"/>.</returns>
        [HttpGet("verify-name-or-alias/{id}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> NameOrAliasIsAvailableOnUpdate(
            Guid id,
            [FromQuery] AdditionalPropertyDefinitionQueryModel queryModel)
        {
            Guid tenantId = await this.GetContextTenantIdOrThrow(queryModel?.Tenant);
            var modelFilter = await this.additionalPropertyModelResolver.ResolveToDomainReadModelFilter(queryModel);
            var validator = this.additionalPropertyValidator.GetValidatorByContextType(queryModel.ContextType.Value);
            var result = validator.IsNameOrAliasInUseOnUpdate(
                tenantId,
                id,
                queryModel.Name,
                queryModel.Alias,
                queryModel.Entity.Value,
                modelFilter.ContextId,
                modelFilter.ParentContextId,
                queryModel.ContextType.Value);
            return this.Ok(!result);
        }

        /// <summary>
        /// Gets a Schema by schema type.
        /// </summary>
        /// <param name="schemaType">Schema type of the additional property.</param>
        /// <returns>A schema.</returns>
        [HttpGet("default-schema")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageAdditionalPropertyDefinitions)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDefaultSchema([FromQuery] string schemaType)
        {
            var fileName = this.GetFileName(schemaType);
            if (!fileName.IsNullOrEmpty())
            {
                var schema = await this.mediator.Send(new GetSchemaFileByFileNameQuery(fileName));
                return this.Ok(schema.ToString());
            }

            return this.BadRequest($"Schema type {schemaType} is not supported.");
        }

        private string GetFileName(string schemaType)
        {
            var parsedSchemaType = schemaType.ToEnumOrThrow<AdditionalPropertyDefinitionSchemaType>();
            switch (parsedSchemaType)
            {
                case AdditionalPropertyDefinitionSchemaType.OptionList:
                    return "option-list.schema.json";
                default:
                    throw new ErrorException(Errors.General.FileNameNotFoundForSchemaType(schemaType));
            }
        }
    }
}
