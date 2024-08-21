// <copyright file="CalculationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.Infrastructure;
    using UBind.Web.ResourceModels.Claim;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for calculation-related requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class CalculationController : Controller
    {
        private static int testResponse;
        private static bool returnError;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly ICqrsMediator mediator;
        private readonly QuoteCalculationRequestTracker quoteCalculationRequestTracker;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">The claim aggregate repository.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="quoteCalculationRequestTracker">The quote calculation request tracker..</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">The clock.</param>
        public CalculationController(
            IClaimAggregateRepository claimAggregateRepository,
            ICqrsMediator mediator,
            QuoteCalculationRequestTracker quoteCalculationRequestTracker,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.claimAggregateRepository = claimAggregateRepository;
            this.mediator = mediator;
            this.quoteCalculationRequestTracker = quoteCalculationRequestTracker;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
        }

        /// <summary>
        /// Handle quote calculation requests.
        /// </summary>
        [HttpPost]
        [Route("quote/calculation")]
        [RequestRateLimit(Period = 120, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [ProducesResponseType(typeof(QuoteCalculationResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateQuoteCalculation(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] QuoteCalculationFormDataUpdateModel model,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (testResponse > 0)
            {
                Thread.Sleep(testResponse);
                testResponse = 0;
            }

            if (returnError)
            {
                returnError = false;
                return Errors.General.ModelValidationFailed(
                    "for testing purposes, a prior request was made to have the next calculation request return an error")
                    .ToProblemJsonResult();
            }

            Product resolvedProduct = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));

            var productContext = new ProductContext(
                resolvedProduct.TenantId,
                resolvedProduct.Id,
                environment);

            var command = new QuoteCalculationCommand(
                productContext,
                model.QuoteId,
                null,
                null,
                model.ProductReleaseId,
                model.FormDataJson,
                model.PaymentData,
                true,
                true,
                null);

            // UI is canceling all previous calculation requests so we need to cancel these requests also in the back-end.
            // The reason we need to cancel previous requests is because it causes memory and concurrency issue.
            this.quoteCalculationRequestTracker.Requests.Values
                .Where(c => c.QuoteId == model.QuoteId && c.CreatedTimeInTicksSinceEpoch < this.clock.GetCurrentInstant().ToUnixTimeTicks())
                .ToList().ForEach(c => c.TokenSource.Cancel());
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var requestId = Guid.NewGuid();

            try
            {
                this.quoteCalculationRequestTracker.Requests.TryAdd(requestId, new QuoteCalculationRequest(model.QuoteId, source, this.clock.GetCurrentInstant()));
                var calculationResult = await this.mediator.Send(command, source.Token);
                var outputModel = new QuoteCalculationResultModel(calculationResult.Quote);
                return this.Ok(outputModel);
            }
            finally
            {
                // When cancel operation thrown inside the quote calculation command handlers we need to make sure that the token will
                // also be removed from the requests collection and dispose the cancellation token source.
                this.quoteCalculationRequestTracker.Requests.TryRemove(requestId, out QuoteCalculationRequest result);
                source.Dispose();
            }
        }

        /// <summary>
        /// Handle claim calculation requests.
        /// </summary>
        [HttpPost]
        [Route("claim/calculation")]
        [RequestRateLimit(Period = 120, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [ProducesResponseType(typeof(ClaimCalculationResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateClaimCalculation(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] ClaimCalculationFormDataUpdateModel model)
        {
            var cancellationToken = this.HttpContext.RequestAborted;

            if (testResponse > 0)
            {
                Thread.Sleep(testResponse);
                testResponse = 0;
            }

            if (returnError)
            {
                returnError = false;
                return Errors.General.ModelValidationFailed(
                    "for testing purposes, a prior request was made to have the next calculation request return an error")
                    .ToProblemJsonResult();
            }

            Product resolvedProduct = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));

            var formDataJson = JsonConvert.SerializeObject(model.FormDataJson);
            var productContext = new ProductContext(
               resolvedProduct.TenantId,
               resolvedProduct.Id,
               environment);
            var claimAggregate = this.claimAggregateRepository.GetById(resolvedProduct.TenantId, model.ClaimId);
            WebFormValidator.ValidateClaimRequest(model.ClaimId, claimAggregate, productContext);

            var command = new CreateClaimCalculationCommand(productContext, model.ClaimId, formDataJson);
            var calculationResult = await this.mediator.Send(command, CancellationToken.None);

            if (calculationResult.IsFailure)
            {
                return calculationResult.Error.ToProblemJsonResult();
            }

            var updatedClaimAggregate = this.claimAggregateRepository.GetById(resolvedProduct.TenantId, calculationResult.Value);
            var outputModel = new ClaimCalculationResultModel(updatedClaimAggregate);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Mimics response timeout.
        /// </summary>
        /// <param name="timeout">The desired timeout.</param>
        /// <returns>true.</returns>
        [Route("timeout-test")]
        [HttpGet]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult SetResponseTimeout(int timeout)
        {
            testResponse = timeout;
            return this.Ok($"The next calculation request will sleep for {timeout} milliseconds before returning");
        }

        /// <summary>
        /// Mimics an error.
        /// </summary>
        /// <returns>true.</returns>
        [Route("error-test")]
        [HttpGet]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult SetError()
        {
            returnError = true;
            return this.Ok($"The next calculation request will return an error");
        }
    }
}
