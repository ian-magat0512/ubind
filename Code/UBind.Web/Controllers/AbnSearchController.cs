// <copyright file="AbnSearchController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Queries.AbnLookup;
    using UBind.Domain;
    using UBind.Domain.AbnLookup;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// This class is needed because we need a controller for ABN search related requests.
    /// This will display a swagger section called AbnSearch.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/abr/abn-registration")]
    public class AbnSearchController : Controller
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbnSearchController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public AbnSearchController(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Search for ABN registration details by business name.
        /// </summary>
        /// <param name="search">Used to perform a search using the ABR API.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <param name="maxResults">The max number of results, default is 10.</param>
        /// <param name="includeEntityNames">Specifies whether results matched against entityNames should be included in the response.</param>
        /// <param name="includeBusinessNames">Specifies whether results matched against businessNames should be included in the response.</param>
        /// <param name="includeTradingNames">Specifies whether results matched against tradingNames should be included in the response.</param>
        /// <param name="includeCancelledRegistrations">Specifies whether results matched against cancelled registrations should be included in the response.</param>
        /// <returns>List of ABN information.</returns>
        [HttpGet]
        [Route("name-match")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(AbnNameSearchResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByName(
            string search,
            CancellationToken cancellationToken,
            int maxResults = 10,
            bool includeEntityNames = true,
            bool includeBusinessNames = true,
            bool includeTradingNames = true,
            bool includeCancelledRegistrations = false)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Errors.General.BadRequest("Search parameter missing").ToProblemJsonResult();
            }

            var query = new SearchAustralianBusinessRegisterByNameQuery(
                search,
                maxResults,
                includeEntityNames,
                includeBusinessNames,
                includeTradingNames,
                includeCancelledRegistrations);

            var searchResult = await this.mediator.Send(query, cancellationToken);

            if (searchResult.IsFailure)
            {
                return searchResult.Error.ToProblemJsonResult();
            }

            return this.Ok(searchResult.Value);
        }

        /// <summary>
        /// Search for ABN registration details by ABN.
        /// </summary>
        /// <param name="abn">The ABN registration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The ABN information.</returns>
        [HttpGet]
        [Route("{abn}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(AbnSearchResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByAbn(string abn, CancellationToken cancellationToken)
        {
            var query = new SearchAustralianBusinessRegisterByAbnQuery(abn);

            var searchResult =
                await this.mediator.Send(query, cancellationToken);

            return this.GetActionResult(searchResult);
        }

        private IActionResult GetActionResult<T>(CSharpFunctionalExtensions.Result<T, Error> searchResult)
        {
            if (searchResult.IsFailure)
            {
                return searchResult.Error.ToProblemJsonResult();
            }

            return this.Ok(searchResult.Value);
        }
    }
}
