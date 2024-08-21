// <copyright file="PerilsController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Clients.DVA.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Clients.DVA.Perils.Services;
    using UBind.Domain.Clients.DVA.Perils.Entities;

    /// <summary>
    /// Controller for handling peril-ratings related requests.
    /// </summary>
    [Route("/dva/api/v1/propertyPerilRatings")]
    [Produces("application/json")]
    public class PerilsController : Controller
    {
        private readonly IPerilsService perilsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerilsController"/> class.
        /// </summary>
        /// <param name="perilsService">The perils service.</param>
        public PerilsController(
            IPerilsService perilsService)
        {
            this.perilsService = perilsService;
        }

        /// <summary>
        /// Gets the details of a quote.
        /// </summary>
        /// <param name="propertyId">The G-NAF PID.</param>
        /// <returns>The Peril details.</returns>
        [HttpGet]
        [Route("{propertyId}")]
        [ProducesResponseType(typeof(Peril), StatusCodes.Status200OK)]
        public ActionResult GetPerilDetails(string propertyId)
        {
            var perilDetails = this.perilsService.GetDetailsByPropertyId(propertyId);
            return this.Ok(perilDetails);
        }
    }
}
