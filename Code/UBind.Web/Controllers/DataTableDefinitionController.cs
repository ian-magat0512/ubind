// <copyright file="DataTableDefinitionController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.DataTable;
    using UBind.Application.Queries.DataTable;
    using UBind.Application.Queries.Schema;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for data table definition services.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/data-table-definition")]
    public class DataTableDefinitionController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableDefinitionController"/> class.
        /// </summary>
        /// <param name="mediator">The cqrs mediator object.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public DataTableDefinitionController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Gets the data table definitions.
        /// </summary>
        /// <returns>An OK response.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewDataTables, Permission.ManageDataTables)]
        [ProducesResponseType(typeof(IEnumerable<Domain.Entities.DataTableDefinition>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDataTableDefinitions(
            [FromQuery] DataTableDefinitionQueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                 options.Tenant, "get data table definition for another tenant");
            var filters = options.ToFilters(
               tenantId,
               nameof(Domain.Entities.DataTableDefinition.CreatedTicksSinceEpoch));
            var query = new GetDataTableDefinitionsQuery(filters);
            var result = await this.mediator.Send(query);
            return this.Ok(result);
        }

        /// <summary>
        /// Gets the data table definition by ID.
        /// </summary>
        /// <param name="definitionId">The definition ID.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>An OK response.</returns>
        [HttpGet]
        [Route("{definitionId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewDataTables, Permission.ManageDataTables)]
        [ProducesResponseType(typeof(Domain.Entities.DataTableDefinition), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDataTableDefinitionById(Guid definitionId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "get data table definition for another tenant");
            var query = new GetDataTableDefinitionByIdQuery(tenantId, definitionId);
            var result = await this.mediator.Send(query);
            return this.Ok(result);
        }

        /// <summary>
        /// Create a data table definition entry and create a new database table
        /// and load the contents of the CSV data into the new database table.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="entityIdOrAlias,">The entity id or alias.</param>
        /// <param name="model">Create resource model for data table definition.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>An OK response.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageDataTables)]
        [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDataTableFromCsvData(
           [FromQuery] EntityType entityType,
           [FromQuery] string entityIdOrAlias,
           [FromBody] DataTableCreateFromCsvDataModel model,
           [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "create data table from csv data for another tenant");
            Guid entityId = await this.GetEntityIdFromAliasAndEntityType(tenantId, entityType, new GuidOrAlias(entityIdOrAlias));
            var createDataTableDefinitionCommand = new CreateDataTableFromCsvDataCommand(
                tenantId,
                entityType,
                entityId,
                model.Name,
                model.Alias,
                model.CsvData,
                model.TableSchema,
                model.MemoryCachingEnabled,
                model.CacheExpiryInSeconds);
            var result = await this.mediator.Send(createDataTableDefinitionCommand);
            return this.Ok(result);
        }

        /// <summary>
        /// Update the data table definition entry and drop the associated database table.
        /// Then, create a new database table and load the contents of the CSV data into the new database table.
        /// </summary>
        /// <param name="model">Create resource model for data table definition.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>An OK response.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageDataTables)]
        [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDataTableFromCsvData(
           [FromBody] DataTableUpdateFromCsvDataModel model,
           [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "update data table from csv data for another tenant");
            var updateDataTableDefinitionCommand = new UpdateDataTableFromCsvDataCommand(
                 tenantId,
                 model.DefinitionId,
                 model.Name,
                 model.Alias,
                 model.CsvData,
                 model.TableSchema,
                 model.MemoryCachingEnabled,
                 model.CacheExpiryInSeconds);
            var result = await this.mediator.Send(updateDataTableDefinitionCommand);
            return this.Ok(result);
        }

        /// <summary>
        /// Flagged the data table definition entry as deleted
        /// and drop the database table and its content.
        /// </summary>
        /// <param name="definitionId">The data table definition id.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>An OK response.</returns>
        [HttpDelete]
        [Route("{definitionId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageDataTables)]
        [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteDataTableDefinitionAndContents(
            Guid definitionId,
            [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "delete data table definition and contents for another tenant");
            var updateDataTableDefinitionCommand = new DeleteDataTableDefinitionAndContentCommand(
                 tenantId, definitionId);
            var result = await this.mediator.Send(updateDataTableDefinitionCommand);
            return this.Ok(result);
        }

        /// <summary>
        /// Gets the json configuration schema
        /// </summary>
        /// <returns>An OK response.</returns>
        [HttpGet]
        [Route("validation-schema")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewDataTables, Permission.ManageDataTables)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJsonConfigurationSchema()
        {
            var fileName = "data-table-table-schema-validation.schema.1.0.0.json";
            var schema = await this.mediator.Send(new GetSchemaFileByFileNameQuery(fileName));
            return this.Ok(schema.ToString());
        }

        private async Task<Guid> GetEntityIdFromAliasAndEntityType(Guid contextTenantId, EntityType entityType, GuidOrAlias entityGuidOrAlias)
        {
            Guid? entityId = Guid.Empty;
            switch (entityType)
            {
                case EntityType.Organisation:
                    var organisationDetails = await this.cachingResolver.GetOrganisationOrNull(contextTenantId, entityGuidOrAlias);
                    entityId = organisationDetails?.Id ?? null;
                    break;
                case EntityType.Product:
                    entityId = await this.cachingResolver
                        .GetProductIdOrNull(new Domain.Helpers.GuidOrAlias(contextTenantId), entityGuidOrAlias);
                    break;
                case EntityType.Tenant:
                    entityId = await this.cachingResolver
                        .GetTenantIdOrNull(entityGuidOrAlias);
                    break;
            }
            if (entityId == null)
            {
                throw new ErrorException(Errors.General.NotFound(entityType.ToString(), entityGuidOrAlias.Alias, "Alias"));
            }
            return (Guid)entityId;
        }
    }
}
