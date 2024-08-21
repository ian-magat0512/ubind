// <copyright file="IQuoteAggregateResolverService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// This service is needed to get a QuoteAggregate or QuoteAggregateId for a give quoteId.
    /// </summary>
    public interface IQuoteAggregateResolverService
    {
        /// <summary>
        /// Gets the quote aggregate from the provided quote.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>The quote aggregate entity.</returns>
        QuoteAggregate GetQuoteAggregateForQuote(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Gets the quote aggregate from the provided quote.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <returns>The quote aggregate entity.</returns>
        QuoteAggregate GetQuoteAggregateForPolicy(Guid tenantId, Guid policyId);

        /// <summary>
        /// Gets the QuoteAggregate Id.
        /// </summary>
        /// <param name="quoteOrPolicyId">THe quote or policy ID.</param>
        /// <returns>THe quote Aggregate ID.</returns>
        Guid GetQuoteAggregateIdForQuoteIdOrPolicyId(Guid quoteOrPolicyId);

        /// <summary>
        /// Gets the QuoteAggregate Id for a given quoteId.
        /// </summary>
        /// <param name="quoteId">THe quote ID.</param>
        /// <returns>THe quote Aggregate ID for the given quoteId.</returns>
        Guid GetQuoteAggregateIdForQuoteId(Guid quoteId);

        /// <summary>
        /// Gets the QuoteAggregate Id for a given Policy ID.
        /// </summary>
        /// <param name="policyId">The policy ID.</param>
        /// <returns>THe quote Aggregate ID for the given policy ID.</returns>
        Guid GetQuoteAggregateIdForPolicyId(Guid policyId);
    }
}
