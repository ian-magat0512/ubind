// <copyright file="IsRefundAllowedQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Query Handler for determining if refund allowed.
    /// </summary>
    public class IsRefundAllowedQueryHandler : IQueryHandler<IsRefundAllowedQuery, bool>
    {
        private readonly IPolicyService policyService;
        private readonly ICachingResolver cachingResolver;

        public IsRefundAllowedQueryHandler(
            IPolicyService policyService,
            ICachingResolver cachingResolver)
        {
            this.policyService = policyService;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public Task<bool> Handle(IsRefundAllowedQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var productFeatureSetting = this.cachingResolver.GetProductSettingOrThrow(
                query.ReleaseContext.TenantId,
                query.ReleaseContext.ProductId);
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                query.Policy, query.QuoteDataRetriever, productFeatureSetting);
            return Task.FromResult(isRefundAllowed);
        }
    }
}
