// <copyright file="FundingServiceRedirectUrlHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Infrastructure
{
    using System;
    using Microsoft.AspNetCore.Routing;
    using UBind.Application.Funding.EFundExpress;
    using UBind.Domain.Product;

    /// <inheritdoc />
    public class FundingServiceRedirectUrlHelper : IFundingServiceRedirectUrlHelper
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingServiceRedirectUrlHelper"/> class.
        /// </summary>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="httpContextAccessor">The http context accessor.</param>
        public FundingServiceRedirectUrlHelper(
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string GetSuccessRedirectUrl(IProductContext productContext, Guid quoteId, string proposalId)
        {
            return this.linkGenerator.GetUriByRouteValues(
                this.httpContextAccessor.HttpContext,
                RouteNames.FundingAccepted,
                new
                {
                    tenant = productContext.TenantId.ToString(),
                    environment = productContext.Environment,
                    product = productContext.ProductId.ToString(),
                    quoteId = quoteId,
                    proposalId = proposalId,
                });
        }

        /// <inheritdoc />
        public string GetCancellationRedirectUrl(IProductContext productContext, Guid quoteId, string proposalId)
        {
            return this.linkGenerator.GetUriByRouteValues(
                this.httpContextAccessor.HttpContext,
                RouteNames.FundingCancelled,
                new
                {
                    tenant = productContext.TenantId.ToString(),
                    environment = productContext.Environment,
                    product = productContext.ProductId.ToString(),
                    quoteId = quoteId,
                    proposalId = proposalId,
                });
        }
    }
}
