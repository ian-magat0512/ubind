// <copyright file="EfundExpressController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.Infrastructure;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling principal finance premium funding requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/principalFinance")]
    public class EfundExpressController : Controller
    {
        private readonly IDomainFundingService domainFundingService;
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfundExpressController"/> class.
        /// </summary>
        /// <param name="domainFundingService">The funding service.</param>
        /// <param name="quoteAggregateResolverService">The aggregate resolver service, for obtaining the aggregate from the quote id.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        public EfundExpressController(
            IDomainFundingService domainFundingService,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.domainFundingService = domainFundingService;
        }

        /// <summary>
        /// Handle redirections from external funding sites after funding completed.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="proposalId">The ID of the funding proposal.</param>
        /// <returns>Ok.</returns>
        /// <remarks>Serve Javascript content to communicate events.</remarks>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [Route("/api/v1/{tenant}/{environment}/{product}/principalFinance/FundingAccepted", Name = RouteNames.FundingAccepted)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleFundingCompletedRedirect(
            string tenant, DeploymentEnvironment environment, string product, [FromQuery, Required] Guid quoteId, [FromQuery, Required] string proposalId)
        {
            // The Principal Finance iframe is trying to call the FundingAccepted endpoint (which is correct),
            // but the proposalID they are trying to send to us is not GUID
            // because it has 8 more characters in it. They said those extra characters are for quote/contract number(they purposely appended that to the proposalId). Since we are not saving that, we are shaving off those extra characters.
            if (!Guid.TryParse(proposalId.AsSpan(0, 36), out Guid principalFinanceProposalId))
            {
                return Errors.General.BadRequest(string.Format("Invalid Proposal Id: {0}", proposalId)).ToProblemJsonResult();
            }

            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(productModel.TenantId, quoteId);
            WebFormValidator.ValidateQuoteRequest(quoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));

            await this.domainFundingService.RecordExternalAcceptanceByQuote(productModel.TenantId, quoteId, principalFinanceProposalId);
            return this.View();
        }

        /// <summary>
        /// Handle redirections from external funding sites after funding completed.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="proposalId">The ID of the funding proposal.</param>
        /// <param name="nonce">A nonce, you nonce.</param>
        /// <returns>Ok.</returns>
        /// <remarks>Serve javascript content to communicate event.</remarks>
        [HttpGet]
        [Route("/api/v1/{tenant}/{environment}/{product}/principalFinance/FundingCancelled", Name = RouteNames.FundingCancelled)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HandleFundingCancellationRedirect([FromQuery] Guid quoteId, [FromQuery] Guid proposalId, [FromQuery] Guid nonce)
        {
            return this.View();
        }

        /// <summary>
        /// Handle check requests for premium funding proposal.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="proposalId">The ID of the funding proposal.</param>
        /// <returns>OK or Cancel.</returns>
        [HttpPost]
        [Route("/api/v1/{tenant}/{environment}/{product}/principalFinance/{quoteId}")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [ProducesResponseType(typeof(PrincipalFinanceProposalResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatusOfProposal(
            string tenant, DeploymentEnvironment environment, string product, [Required] Guid quoteId, [FromQuery] Guid proposalId)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(productModel.TenantId, quoteId);
            WebFormValidator.ValidateQuoteRequest(quoteId, quoteAggregate, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            return this.Ok(new PrincipalFinanceProposalResultModel(quote));
        }
    }
}
