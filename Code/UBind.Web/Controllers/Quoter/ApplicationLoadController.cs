// <copyright file="ApplicationLoadController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.Quote;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Person;
    using UBind.Application.Queries.Claim;
    using UBind.Application.Queries.Claim.Version;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Claim;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for form data update requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class ApplicationLoadController : Controller
    {
        private readonly IQuoteService quoteService;
        private readonly ICachingResolver cachingResolver;
        private readonly IApplicationQuoteService applicationQuoteService;
        private readonly Application.User.IUserService userService;
        private readonly IPersonService personService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLoadController"/> class.
        /// </summary>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="applicationQuoteService">The service for handling requests to applications.</param>
        /// <param name="userService">The user service of <see cref="Application.User"/>.</param>
        /// <param name="personService">The person service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public ApplicationLoadController(
            IQuoteService quoteService,
            IApplicationQuoteService applicationQuoteService,
            Application.User.IUserService userService,
            IPersonService personService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.quoteService = quoteService;
            this.applicationQuoteService = applicationQuoteService;
            this.userService = userService;
            this.personService = personService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets a quote's form data and calculationResult.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote should be from.</param>
        /// <param name="product">The ID or Alias of the product the quote should be from.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="quoteId">The ID of the quote to retrieve the data for. If you don't supply a quote ID, you'll
        /// need to supply a policy ID and quote type.</param>
        /// <param name="policyId">The ID of the policy. If you supply a policy ID you'll need to supply the quote type
        /// too.</param>
        /// <param name="quoteType">The quote type, e.g. "newBusiness", "renewal", "adjustment", "cancellation".</param>
        /// <param name="version">The version number, if you would like to load a specific quote version.</param>
        /// <param name="createIfNotExists">If a quote of the type passed does not exist against the given policy, then
        /// create one.</param>
        /// <param name="discardExistingQuoteOnCreate">If creating a quote, and an existing change quote of a different type
        /// exists for that policy, that change quote will be discarded.</param>
        /// <returns>The latest form data and calculation result (if it exists).</returns>
        [HttpGet]
        [Route("quote/load")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        [RequestIntent(RequestIntent.ReadWrite)]
        public async Task<IActionResult> LoadQuote(
            [Required] string tenant,
            [Required] string product,
            [Required] DeploymentEnvironment environment,
            Guid? quoteId = null,
            Guid? policyId = null,
            QuoteType? quoteType = null,
            int? version = null,
            bool createIfNotExists = false,
            bool discardExistingQuoteOnCreate = false,
            string? productRelease = null)
        {
            Tenant tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            NewQuoteReadModel? quote = null;
            if (quoteId == null)
            {
                if (policyId == null || quoteType == null)
                {
                    throw new ErrorException(Errors.General.ModelValidationFailed("When loading a quote, if you don't "
                        + "supply a quote ID, you must supply both a policy ID and quote type."));
                }

                quote = await this.mediator.Send(new GetLatestQuoteOfTypeForPolicyQuery(tenantModel.Id, policyId.Value, quoteType.Value));
                bool quoteIsEditable = quote != null && quote.IsEditable();
                if (!quoteIsEditable || quote?.Type != quoteType || productRelease != null)
                {
                    if (createIfNotExists)
                    {
                        switch (quoteType)
                        {
                            case QuoteType.Renewal:
                                quote = await this.mediator.Send(new CreateRenewalQuoteCommand(
                                    productModel.TenantId,
                                    policyId.Value,
                                    discardExistingQuoteOnCreate,
                                    initialQuoteState: StandardQuoteStates.Incomplete,
                                    productRelease: productRelease));
                                break;
                            case QuoteType.Adjustment:
                                quote = await this.mediator.Send(new CreateAdjustmentQuoteCommand(
                                    productModel.TenantId,
                                    policyId.Value,
                                    discardExistingQuoteOnCreate,
                                    initialQuoteState: StandardQuoteStates.Incomplete,
                                    productRelease: productRelease));
                                break;
                            case QuoteType.Cancellation:
                                quote = await this.mediator.Send(new CreateCancellationQuoteCommand(
                                    productModel.TenantId,
                                    policyId.Value,
                                    discardExistingQuoteOnCreate,
                                    initialQuoteState: StandardQuoteStates.Incomplete,
                                    productRelease: productRelease));
                                break;
                            default:
                                throw new ErrorException(Errors.Quote.NotFound(policyId.Value, quoteType.Value));
                        }
                    }
                    else
                    {
                        throw new ErrorException(Errors.Quote.NotFound(policyId.Value, quoteType.Value));
                    }
                }
            }

            if (quote == null && quoteId != null)
            {
                quote = await this.mediator.Send(new GetQuoteByIdQuery(tenantModel.Id, quoteId.Value));
            }

            quote = EntityHelper.ThrowIfNotFound(quote, "ID", quoteId?.ToString() ?? string.Empty);
            if (quote.Environment != environment)
            {
                throw new ErrorException(Errors.Operations.EnvironmentMisMatch("quote"));
            }

            if (productModel.Id != quote.ProductId)
            {
                throw new ErrorException(Errors.Quote.ProductMismatch(productModel.Details.Alias, quote.Id));
            }

            if (quote.IsDiscarded)
            {
                throw new ErrorException(Errors.Quote.CannotEditWhenDiscarded(quote.Id));
            }

            await this.applicationQuoteService.TriggerEventWhenQuoteIsOpened(quote, this.User.IsCustomer());

            if (quote.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Expired))
            {
                throw new ErrorException(Errors.Quote.CannotLoadExpiredQuote(quote.Id));
            }

            QuoteVersionReadModelDto? quoteVersion = null;
            if (version != null && quoteId != null)
            {
                quoteVersion = this.quoteService.GetQuoteVersionForQuote(productModel.TenantId, quoteId.Value, version.Value);
            }

            UserResourceModel? userResourceModel = this.GetUserResourceModel();

            ICustomerReadModelSummary? customer = null;
            if (quote.CustomerId.HasValue)
            {
                customer = await this.mediator.Send(
                    new GetCustomerByIdQuery(productModel.TenantId, quote.CustomerId.Value));
            }

            var resourceModel = new QuoteApplicationModel(
                quote,
                quoteVersion,
                customer,
                userResourceModel);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Gets a claim's form data and calculationResult.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the application is for.</param>
        /// <param name="product">The ID or Alias of the product the application is for.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="claimId">The ID of the claim to retrieve the data for.</param>
        /// <param name="version">version of the claim.</param>
        /// <returns>The latest form data and calculation result (if it exists).</returns>
        [HttpGet]
        [Route("claim/load")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoadClaim(
            string tenant,
            string product,
            [Required] DeploymentEnvironment environment,
            Guid claimId = default,
            int? version = null)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var claim = await this.mediator.Send(new GetClaimByIdQuery(productModel.TenantId, claimId));
            WebFormValidator.ValidateClaimRequest(claimId, claim, new ProductContext(productModel.TenantId, productModel.Id, environment));
            IClaimVersionReadModelDetails? claimVersion = null;

            if (version != null)
            {
                claimVersion = await this.mediator.Send(new GetClaimVersionDetailsByVersionNumberQuery(
                    productModel.TenantId, claimId, version.Value));
            }

            if (claim == null)
            {
                return Errors.General.NotFound("claim", claimId).ToProblemJsonResult();
            }

            if (this.User.Identity == null || !this.User.Identity.IsAuthenticated)
            {
                return this.Ok(new ClaimApplicationModel(claim, claimVersion));
            }

            var userResourceModel = this.GetUserResourceModel();
            return this.Ok(new ClaimApplicationModel(claim, claimVersion, userResourceModel));
        }

        private UserResourceModel? GetUserResourceModel()
        {
            var userId = this.User.GetId();
            if (userId == null)
            {
                return null;
            }

            var performingUser = this.userService.GetUserDetail(this.User.GetTenantId(), userId.Value);
            if (performingUser == null)
            {
                return null;
            }

            var performingPerson = this.personService.Get(performingUser.TenantId, performingUser.PersonId);
            return new UserResourceModel(performingUser, performingPerson);
        }
    }
}
