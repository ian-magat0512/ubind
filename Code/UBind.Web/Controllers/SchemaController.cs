// <copyright file="SchemaController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Queries.Schema;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for Automation Schemas.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/schema")]
    public class SchemaController : Controller
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator object.</param>
        public SchemaController(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets all the list of publish schemas.
        /// </summary>
        /// <returns>Published schemas.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList()
        {
            return this.Ok(await this.mediator.Send(new GetSchemaListQuery()));
        }

        /// <summary>
        /// Gets a Schema by its filename.
        /// </summary>
        /// <param name="fileName">File name of the JSON file.</param>
        /// <returns>A schema.</returns>
        [HttpGet]
        [Route("{fileName}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByName(string fileName)
        {
            return this.Ok(await this.mediator.Send(new GetSchemaFileByFileNameQuery(fileName)));
        }
    }
}
