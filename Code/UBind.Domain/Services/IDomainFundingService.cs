// <copyright file="IDomainFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Service for accepting funding proposals.
    /// </summary>
    /// <remarks>To be used only for acceptance of funding proposals as part of quote aggregate.</remarks>
    public interface IDomainFundingService
    {
        /// <summary>
        /// Handle an attempt to accept a funding proposal without payment using quoteId.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="internalUBindProposalId">The internal UBind ID of the proposal that was accepted.</param>
        /// <returns>The updated quote.</returns>
        Task<QuoteAggregate> RecordExternalAcceptanceByQuote(Guid tenantId, Guid quoteId, Guid internalUBindProposalId);
    }
}
