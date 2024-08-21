// <copyright file="GetClaimDetailsByIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Gets a claim by ID.
    /// </summary>
    public class GetClaimDetailsByIdQuery : IQuery<IClaimReadModelDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetClaimDetailsByIdQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="claimId">The claim ID.</param>
        public GetClaimDetailsByIdQuery(Guid tenantId, Guid claimId)
        {
            this.TenantId = tenantId;
            this.ClaimId = claimId;
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the claim ID.
        /// </summary>
        public Guid ClaimId { get; }
    }
}
