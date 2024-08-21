// <copyright file="QuoteOperationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Quote;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Person;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Queries.User;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling quote requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class QuoteOperationController : Controller
    {
        private readonly IQuoteService quoteService;
        private readonly ICustomerService customerService;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly ICqrsMediator mediator;
        private readonly ICustomerHelper customerHelper;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteOperationController"/> class.
        /// </summary>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        /// <param name="mediator">The ubind mediator.</param>
        /// <param name="customerHelper">The customer helper.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">The clock.</param>
        public QuoteOperationController(
            IQuoteService quoteService,
            ICustomerService customerService,
            IQuoteReadModelRepository quoteReadModelRepository,
            ICqrsMediator mediator,
            ICustomerHelper customerHelper,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.quoteService = quoteService;
            this.customerService = customerService;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.mediator = mediator;
            this.customerHelper = customerHelper;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
        }

        /// <summary>
        /// Creates a quote for an application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [ValidateModel]
        [Route("actualise")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Quote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            // TODO: Check operation does not send additional data, and if so, what is meaning of updates accompanying form?
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ActualiseQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new FormData(model.FormDataJson) : null));
            if (quote == null)
            {
                quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
                quote = EntityHelper.ThrowIfNotFound(quote, model.QuoteId, "quote");
            }

            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Creates a quote version for an application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [ValidateModel]
        [Route("quoteVersion")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> QuoteVersion(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            // TODO: Check operation sends appropriate data.
            // Quote version operation currently includes calculation result ID but this is ignored.
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            FormData formData = new FormData(model.FormDataJson);
            var command = new CreateQuoteVersionCommand(
                productModel.TenantId,
                productModel.Id,
                environment,
                model.QuoteId,
                formData);

            var quote = await this.mediator.Send(command, CancellationToken.None);
            var outputModel = new QuoteApplicationModel(quote);
            return await Task.FromResult(this.Ok(outputModel));
        }

        /// <summary>
        /// Creates a customer for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [ValidateModel]
        [Route("customer")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Customer(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] CustomerUpdateModel model)
        {
            // TODO: Check operation does not send additional data, and if so, what is meaning of updates accompanying form?
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            if (model.CustomerDetails == null)
            {
                return Errors.Quote.CustomerDetailsNotFound(model.QuoteId).ToProblemJsonResult();
            }
            model.CustomerDetails.FixNameComponents();
            var quoteDetails = this.quoteReadModelRepository.GetQuoteDetails(productModel.TenantId, model.QuoteId);
            var quoteOrganisationId = quoteDetails?.OrganisationId ?? default;
            var resolvedCustomerDetails = await this.customerHelper.CreateResolvedCustomerPersonalDetailsModel(
                    productModel.TenantId, model.CustomerDetails, quoteOrganisationId);
            var quote = await this.mediator.Send(new CreateCustomerForQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                resolvedCustomerDetails,
                model.PortalId));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Updates the customer details for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to update the customer details.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">Th environment to work on.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [Route("customer/{customerId}")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> PatchCustomer(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            Guid customerId,
            [FromBody] CustomerUpdateModel model)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            if (model.CustomerDetails == null)
            {
                return Errors.Quote.CustomerDetailsNotFound(model.QuoteId).ToProblemJsonResult();
            }

            model.CustomerDetails.FixNameComponents();
            var quoteDetails = this.quoteReadModelRepository.GetQuoteDetails(productModel.TenantId, model.QuoteId);
            var quoteOrganisationId = quoteDetails?.OrganisationId ?? default;
            var resolvedCustomerDetails = await this.customerHelper.CreateResolvedCustomerPersonalDetailsModel(
                     productModel.TenantId, model.CustomerDetails, quoteOrganisationId, model.PortalId);
            await this.mediator.Send(new UpdateCustomerForQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                customerId,
                resolvedCustomerDetails,
                resolvedCustomerDetails.PortalId));
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
            quote = EntityHelper.ThrowIfNotFound(quote, model.QuoteId, "quote");
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Gets the latest customer of the quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Route("quote/{quoteId}/customer")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerDetails(
            string tenant, string product, DeploymentEnvironment environment, Guid quoteId)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, quoteId));
            quote = EntityHelper.ThrowIfNotFound(quote, quoteId, "quote");
            if (!quote.CustomerId.HasValue)
            {
                return Errors.Quote.CustomerDetailsNotFound(quoteId).ToProblemJsonResult();
            }
            var customerReadModel = this.customerService.GetCustomerById(productModel.TenantId, quote.CustomerId.Value);
            var outputModel = new QuoteApplicationModel(
                quote,
                customerReadModel: customerReadModel);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Creates a new prepopulated quote for an application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is from.</param>
        /// <param name="product">The ID or Alias of the product the quote is from.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [Route("copyExpiredQuote")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CopyExpiredQuote(string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteUpdateModel model)
        {
            // TODO: Check operation does not send additional data, and if so, what is meaning of updates accompanying form?
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new CloneQuoteFromExpiredQuoteCommand(
                    productModel.TenantId,
                    model.QuoteId,
                    environment));

            UserResourceModel? userResourceModel = null;
            var userId = this.User.GetId();
            if (userId.HasValue)
            {
                var loggedUser = await this.mediator.Send(
                    new GetUserByIdQuery(this.User.GetTenantId(), userId.Value));
                var loggedPerson = await this.mediator.Send(
                    new GetPersonSummaryByIdQuery(loggedUser.TenantId, loggedUser.PersonId));
                userResourceModel = new UserResourceModel(loggedUser, loggedPerson);
            }

            ICustomerReadModelSummary? customer = null;
            if (quote.CustomerId.HasValue)
            {
                customer = await this.mediator.Send(
                    new GetCustomerByIdQuery(productModel.Id, quote.CustomerId.Value));
            }

            var outputModel = new QuoteApplicationModel(
                quote,
                null,
                customer,
                userResourceModel);

            return this.Ok(outputModel);
        }

        /// <summary>
        /// Checks the availability of the association invitation.
        /// </summary>
        /// <param name="customerAssociationInvitationId">The ID of the association invitation to be checked.</param>
        /// <param name="quoteId">The ID of the quote to verify.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [Route("~/api/v1/customer-association-invitation/{customerAssociationInvitationId}/verify")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult CheckAssociationInvitation(Guid customerAssociationInvitationId, Guid quoteId)
        {
            var userId = this.User.GetId();
            if (!userId.HasValue)
            {
                return Errors.User.SessionNotFound().ToProblemJsonResult();
            }

            this.quoteService.VerifyQuoteCustomerAssociation(this.User.GetTenantId(), customerAssociationInvitationId, quoteId, userId.Value);
            return this.Ok();
        }

        /// <summary>
        /// Associate the quote to customer user account.
        /// </summary>
        /// <param name="customerAssociationInvitationId">The ID of the invitation.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [Route("~/api/v1/customer-association-invitation/{customerAssociationInvitationId}/associate")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssociateToCustomer(Guid customerAssociationInvitationId, Guid quoteId)
        {
            var userId = this.User.GetId();
            if (!userId.HasValue)
            {
                return Errors.User.SessionNotFound().ToProblemJsonResult();
            }

            await this.mediator.Send(new AssociateQuoteWithCustomerUserCommand(this.User.GetTenantId(), customerAssociationInvitationId, quoteId, userId.Value));
            return this.Ok();
        }
    }
}
