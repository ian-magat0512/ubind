// <copyright file="LoggingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using UBind.Application;
    using UBind.Domain.Enums;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling logging..
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class LoggingController : Controller
    {
        private readonly ILogger<LoggingController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoggingController(ILogger<LoggingController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handle Logging.
        /// </summary>
        /// <param name="model">The update Model.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("log")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 300)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Log([FromBody] LogModel model)
        {
            if (model.Level == LogLevel.Debug)
            {
                this.logger.LogDebug(string.Format("{0} - {1}.", model.Description, model.Value));
            }
            else if (model.Level == LogLevel.Information)
            {
                this.logger.LogInformation(string.Format("{0} - {1}.", model.Description, model.Value));
            }
            else if (model.Level == LogLevel.Warning)
            {
                this.logger.LogWarning(string.Format("{0} - {1}.", model.Description, model.Value));
            }
            else if (model.Level == LogLevel.Error)
            {
                this.logger.LogError(string.Format("{0} - {1}.", model.Description, model.Value));
            }

            return this.Ok();
        }
    }
}
