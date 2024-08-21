// <copyright file="GetClaimsMatchingFiltersQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    public class GetClaimsMatchingFiltersQueryHandler : IQueryHandler<GetClaimsMatchingFiltersQuery, List<IClaimReadModelSummary>>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IClaimReadModelRepository claimReadModelRepository;

        public GetClaimsMatchingFiltersQueryHandler(
            IClaimReadModelRepository claimReadModelRepository,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.claimReadModelRepository = claimReadModelRepository;
        }

        public async Task<List<IClaimReadModelSummary>> Handle(GetClaimsMatchingFiltersQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.Filters.EntityType != null)
            {
                switch (request.Filters.EntityType)
                {
                    case EntityType.Policy:
                        return this.GetByPolicy(request.TenantId, request.Filters.EntityId.Value, request.Filters).ToList();
                    default:
                        break;
                }
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(request.Filters.TenantId));
            return this.claimReadModelRepository.ListClaims(tenant.Id, request.Filters).ToList();
        }

        private IEnumerable<IClaimReadModelSummary> GetByPolicy(Guid tenantId, Guid policyId, EntityListFilters filters)
        {
            return this.claimReadModelRepository.ListClaimsByPolicy(tenantId, policyId, filters);
        }
    }
}
