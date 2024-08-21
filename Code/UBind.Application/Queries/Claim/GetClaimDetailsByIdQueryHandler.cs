// <copyright file="GetClaimDetailsByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Gets a claim by ID.
    /// </summary>
    public class GetClaimDetailsByIdQueryHandler : IQueryHandler<GetClaimDetailsByIdQuery, IClaimReadModelDetails>
    {
        private readonly IClaimReadModelRepository claimReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetClaimDetailsByIdQueryHandler"/> class.
        /// </summary>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        public GetClaimDetailsByIdQueryHandler(
            IClaimReadModelRepository claimReadModelRepository)
        {
            this.claimReadModelRepository = claimReadModelRepository;
        }

        /// <inheritdoc/>
        public Task<IClaimReadModelDetails> Handle(GetClaimDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claim = this.claimReadModelRepository.GetClaimDetails(request.TenantId, request.ClaimId);
            if (claim == null)
            {
                throw new ErrorException(Errors.Claim.NotFound(request.ClaimId));
            }

            return Task.FromResult(claim);
        }
    }
}
