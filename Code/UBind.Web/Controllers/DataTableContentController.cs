// <copyright file="DataTableContentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Queries.DataTable;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for data table content services.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/data-table-content")]
    public partial class DataTableContentController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableContentController"/> class.
        /// </summary>
        /// <param name="mediator">The cqrs mediator object.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public DataTableContentController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the data table content as CSV string.
        /// </summary>
        /// <param name="dataTableDefinitionId">The data table definition ID.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>A data table content CSV string.</returns>
        [HttpGet]
        [Route("{dataTableDefinitionId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewDataTables, Permission.ManageDataTables)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDataTableContentAsCsvString(Guid dataTableDefinitionId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "get the data table content as csv for another tenant");
            var dataTableContentQuery = new GetDataTableContentCsvByDefinitionIdQuery(tenantId, dataTableDefinitionId);
            var dataTableContent = await this.mediator.Send(dataTableContentQuery);

            return this.Ok(dataTableContent.CsvData);
        }

        /// <summary>
        /// Gets the data table content as CSV file.
        /// </summary>
        /// <param name="dataTableDefinitionId">The data table definition ID.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>A data table content CSV file.</returns>
        [HttpGet]
        [Route("{dataTableDefinitionId}/download")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewDataTables, Permission.ManageDataTables)]
        [ProducesResponseType(typeof(CsvFileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDataTableContentAsCsvFile(Guid dataTableDefinitionId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                                tenant, "download the data table content as csv file for another tenant");
            var dataTableContentQuery = new GetDataTableContentCsvByDefinitionIdQuery(tenantId, dataTableDefinitionId);
            var dataTableContent = await this.mediator.Send(dataTableContentQuery);
            return new CsvFileResult(dataTableContent.CsvData, dataTableContent.DownloadFileName);
        }
    }
}
