// <copyright file="AdditionalPropertyValueController.cs" company="uBind">
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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Permissions;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for additional property values. For now it is getting from text additional property values.
    /// </summary>
    [Authorize]
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/additional-property-value")]
    public class AdditionalPropertyValueController : Controller
    {
        private readonly IMediator mediator;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly IAdditionalPropertyAuthorisationService additionalPropertyAuthorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyValueController"/> class.
        /// </summary>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public AdditionalPropertyValueController(
            IMediator mediator,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IAdditionalPropertyAuthorisationService additionalPropertyAuthorisationService)
        {
            this.mediator = mediator;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.additionalPropertyAuthorisationService = additionalPropertyAuthorisationService;
        }

        /// <summary>
        /// Gets the list of <see cref="AdditionalPropertyValueModel"/>.
        /// </summary>
        /// <param name="queryModel">Model query being used in filtering the result
        /// <see cref="AdditionalPropertyValueQueryOptionsModel"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [MustHavePermission(Permission.ViewAdditionalPropertyValues)]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(typeof(List<AdditionalPropertyValueModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] AdditionalPropertyValueQueryOptionsModel queryModel)
        {
            var userTenantId = this.User.GetTenantId();
            await this.additionalPropertyAuthorisationService.ThrowIfUserCannotViewEntity(
                this.User, userTenantId, queryModel.EntityType, queryModel.EntityId);

            List<AdditionalPropertyValueDto> results = new List<AdditionalPropertyValueDto>();
            foreach (AdditionalPropertyDefinitionType propertyType in Enum.GetValues(typeof(AdditionalPropertyDefinitionType)))
            {
                results.AddRange(await this.additionalPropertyValueService.GetAdditionalPropertyValuesByQueryModel(
                    queryModel.TenantId,
                    queryModel.AdditionalPropertyDefinitionId,
                    queryModel.Value,
                    queryModel.EntityId,
                    propertyType,
                    queryModel.EntityType));
            }

            List<AdditionalPropertyValueModel> additionalPropertyValues = null;
            if (results.Any())
            {
                additionalPropertyValues = results
                    .Select((dto) => new AdditionalPropertyValueModel(dto))
                    .ToList();
            }

            return this.Ok(additionalPropertyValues);
        }

        /// <summary>
        /// Update the additional property values for specific entity ID.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="updateModel">Update model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MustBeLoggedIn]
        [HttpPut]
        [ValidateModel]
        [MustHavePermission(Permission.EditAdditionalPropertyValues)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("{entityId}")]
        public async Task<IActionResult> Update(
            Guid entityId, [FromBody] UpdateAdditionalPropertyValuesModel updateModel)
        {
            var userTenantId = this.User.GetTenantId();
            await this.additionalPropertyAuthorisationService.ThrowIfUserCannotModifyEntity(
                this.User, userTenantId, updateModel.EntityType, entityId);
            var additionalProperties = updateModel.Properties
                .Select(prop => new Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = prop.DefinitionId,
                    Type = prop.PropertyType,
                    Value = prop.Value,
                })
                .ToList();
            var referenceNumber = await this.additionalPropertyValueService.UpdatePropertiesOfEntityAggregate(
                userTenantId,
                updateModel.EntityType,
                additionalProperties,
                entityId,
                updateModel.Environment);
            return this.Ok(referenceNumber);
        }

        /// <summary>
        /// Checks for the existence of a duplicate additional property value.
        /// </summary>
        /// <param name="queryModel">The query parameters for checking duplicates,
        /// encapsulated in a <see cref="AdditionalPropertyValueQueryOptionsModel"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation,
        /// returning a boolean indicating whether a duplicate exists.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [MustHavePermission(Permission.ViewAdditionalPropertyValues)]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [Route("unique")]
        public async Task<IActionResult> CheckForUniqueAdditionalPropertyValue(
            [FromQuery] UniqueAdditionalPropertyValueCheckModel queryModel)
        {
            // Create a query to check for unique additional property values.
            var query = new IsAdditionalPropertyValueUniqueQuery(
                queryModel.TenantId,
                queryModel.EntityType,
                queryModel.EntityId,
                queryModel.PropertyType,
                queryModel.AdditionalPropertyDefinitionId,
                queryModel.Value);
            var isUnique = await this.mediator.Send(query);
            return this.Ok(isUnique);
        }
    }
}
