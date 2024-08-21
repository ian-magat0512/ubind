// <copyright file="GetClaimVersionDetailsByVersionNumberQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim.Version
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    public class GetClaimVersionDetailsByVersionNumberQueryHandler
        : IQueryHandler<GetClaimVersionDetailsByVersionNumberQuery, IClaimVersionReadModelDetails>
    {
        private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;

        public GetClaimVersionDetailsByVersionNumberQueryHandler(
            IClaimVersionReadModelRepository claimVersionReadModelRepository)
        {
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
        }

        public Task<IClaimVersionReadModelDetails> Handle(GetClaimVersionDetailsByVersionNumberQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claimVersion = this.claimVersionReadModelRepository.GetDetailsByVersionNumber(
                query.TenantId, query.ClaimId, query.VersionNumber);
            return Task.FromResult(claimVersion);
        }
    }
}
