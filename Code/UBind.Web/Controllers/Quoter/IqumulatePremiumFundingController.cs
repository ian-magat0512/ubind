// <copyright file="IqumulatePremiumFundingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application;
    using UBind.Application.Funding;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Funding.Iqumulate.Response;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Commands.Quote;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Exceptions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling principal iQumulate premium funding requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/{tenant}/{product}/{environment}/iqumulate")]
    public class IqumulatePremiumFundingController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IIqumulateService iqumulateService;
        private readonly ITenantService tenantService;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulatePremiumFundingController"/> class.
        /// </summary>
        /// <param name="iqumulateService">The funding service.</param>
        /// <param name="tenantService">The tenant service.</param>
        /// <param name="quoteAggregateResolver">The quote  aggregate resolver service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public IqumulatePremiumFundingController(
            IIqumulateService iqumulateService,
            ITenantService tenantService,
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.iqumulateService = iqumulateService;
            this.tenantService = tenantService;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the mappings of form fields into MPF parameters.
        /// </summary>
        /// <param name="tenant">the tenant ID or Alias.</param>
        /// <param name="product">the product ID or Alias.</param>
        /// <param name="environment">environment.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="calculationResultId">The ID of the calculation result the funding is for.</param>
        /// <returns>Ok.</returns>
        /// <remarks>Serve Javascript content to communicate events.</remarks>
        [HttpGet]
        [Route("funding-request-data")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ProducesResponseType(typeof(IqumulateRequestModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIQumulateFundingRequestData(
             string tenant,
             string product,
             DeploymentEnvironment environment,
             Guid quoteId,
             Guid calculationResultId)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, quoteId));
            var productReleaseId = quote.ProductReleaseId
                ?? await this.mediator.Send(new GetDefaultProductReleaseIdOrThrowQuery(
                    productModel.TenantId,
                    productModel.Id,
                    environment));
            var releaseContext = new ReleaseContext(productModel.TenantId, productModel.Id, environment, productReleaseId);
            WebFormValidator.ValidateQuoteRequest(quoteId, quote, releaseContext);
            var fundingData = await this.iqumulateService.GetIQumulateFundingRequestData(
                releaseContext, quoteId, calculationResultId);
            var model = new IqumulateRequestModel(fundingData);
            return this.Ok(model);
        }

        /// <summary>
        /// Handle Iqumulate premium funding response posting, which if a successful response would result in the
        /// binding of the policy.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">The update Model.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModelAttributeWithException]
        [Route("funding-response")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveIqumulatePremiumFundingResponseDetails(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] IQumulateFundingModel model)
        {
            this.ValidateIqumulateResponseDetails(model.PageResponse);
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
            var productReleaseId = quote.ProductReleaseId
                ?? await this.mediator.Send(new GetDefaultProductReleaseIdOrThrowQuery(
                    productModel.TenantId,
                    productModel.Id,
                    environment));
            var releaseContext = new ReleaseContext(productModel.TenantId, productModel.Id, environment, productReleaseId);
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, releaseContext);
            var bindRequirements = new BindRequirementDto(model.CalculationResultId);
            var proposalResult = this.MapResult(model.PageResponse);
            await this.mediator.Send(new RecordFundingProposalCommand(
                productModel.TenantId, model.QuoteId, proposalResult.Value, proposalResult.Succeeded));
            var bindCommand = BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext,
                model.QuoteId,
                bindRequirements,
                proposalResult.Value.InternalId,
                null,
                default);
            await this.mediator.Send(bindCommand);
            return this.Ok();
        }

        private FundingProposalResult MapResult(IqumulateFundingPageResponse response)
        {
            dynamic proposal = new
            {
                ContractID = response.General.Number,
                IQumulatePremiumFunding = "iqumulate.com",
            };

            var serializedProposalResponse = JsonConvert.SerializeObject(proposal);
            var serializedProposalData = JsonConvert.SerializeObject(response.General);

            var paymentFrequency = response.General.PaymentFrequency == "M" ?
                    Frequency.Monthly
                   : Frequency.Quarterly;

            var numberOfInstallments = response.General.NumberOfInstalments;
            var initialInstallment = response.General.InitialInstalmentAmount;
            var regularInstallment = response.General.OngoingInstalmentAmount;
            var paymentBreakdown = new FundingProposalPaymentBreakdown(
                    Convert.ToDecimal(response.General.AmountFinanced),
                    paymentFrequency,
                    numberOfInstallments,
                    initialInstallment,
                    regularInstallment);

            var fundingProposal = new FundingProposal(
                    proposal.ContractID,
                    paymentBreakdown,
                    proposal.IQumulatePremiumFunding,
                    serializedProposalData,
                    serializedProposalResponse,
                    false);

            return FundingProposalResult.Success(fundingProposal);
        }

        private void ValidateIqumulateResponseDetails(IqumulateFundingPageResponse response)
        {
            if (response?.General == null)
            {
                throw new BadRequestException("The Iqumulate response does not have General data.");
            }

            if (string.IsNullOrEmpty(response.General.Number))
            {
                throw new BadRequestException(
                    "The Iqumulate response does not have Reference Number for the payment.");
            }
        }
    }
}
