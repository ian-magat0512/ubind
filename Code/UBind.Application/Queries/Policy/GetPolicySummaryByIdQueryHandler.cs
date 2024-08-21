// <copyright file="GetPolicySummaryByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Policy
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query handler that returns the policy summary based on the given query.
    /// </summary>
    public class GetPolicySummaryByIdQueryHandler : IQueryHandler<GetPolicySummaryByIdQuery, IPolicyReadModelSummary>
    {
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ICachingResolver cachingResolver;

        public GetPolicySummaryByIdQueryHandler(
            IPolicyReadModelRepository policyReadModelRepository,
            ICachingResolver cachingResolver)
        {
            this.policyReadModelRepository = policyReadModelRepository;
            this.cachingResolver = cachingResolver;
        }

        public async Task<IPolicyReadModelSummary> Handle(GetPolicySummaryByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var policyReadModelDetails = this.policyReadModelRepository.GetPolicyDetails(request.TenantId, request.PolicyId);
            if (policyReadModelDetails == null)
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(request.TenantId);
                var isMutual = TenantHelper.IsMutual(tenantAlias);
                throw new ErrorException(
                    Errors.General.NotFound(
                        TenantHelper.CheckAndChangeTextToMutual("policy", isMutual), request.PolicyId));
            }

            return policyReadModelDetails as IPolicyReadModelSummary;
        }
    }
}
