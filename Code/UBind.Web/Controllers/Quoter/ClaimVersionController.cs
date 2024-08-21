// <copyright file="ClaimVersionController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Claim;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Claim;

    /// <summary>
    /// Controller for handling claim version requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/claim")]
    public class ClaimVersionController : Controller
    {
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionController"/> class.
        /// </summary>
        /// <param name="authorisationService">The authorisation service.</param>
        public ClaimVersionController(
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Creates a claim version for a claim.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("claimVersion")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClaimVersion(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            var formDataJson = model.FormDataJson != null
                ? JsonConvert.SerializeObject(model.FormDataJson)
                : null;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var updatedClaim = await this.mediator.Send(new CreateClaimVersionCommand(tenantModel.Id, model.ClaimId, formDataJson));
            var outputModel = new ClaimApplicationModel(updatedClaim);
            return this.Ok(outputModel);
        }
    }
}
