// <copyright file="GetClaimVersionDetailsByClaimIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    public class GetClaimVersionDetailsByClaimIdQueryHandler
        : IQueryHandler<GetClaimVersionDetailsByClaimIdQuery, IEnumerable<IClaimVersionReadModelDetails>>
    {
        private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;

        public GetClaimVersionDetailsByClaimIdQueryHandler(
            IClaimVersionReadModelRepository claimVersionReadModelRepository)
        {
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
        }

        public Task<IEnumerable<IClaimVersionReadModelDetails>> Handle(GetClaimVersionDetailsByClaimIdQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = this.claimVersionReadModelRepository.GetVersionsOfClaim(query.TenantId, query.ClaimId);
            return Task.FromResult(results);
        }
    }
}
