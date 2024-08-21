// <copyright file="IsRefundAllowedQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <summary>
    /// Query for determining if refund is allowed.
    /// </summary>
    public class IsRefundAllowedQuery : IQuery<bool>
    {
        public IsRefundAllowedQuery(
            Policy policy,
            StandardQuoteDataRetriever quoteDataRetriever,
            ReleaseContext releaseContext)
        {
            this.Policy = policy;
            this.QuoteDataRetriever = quoteDataRetriever;
            this.ReleaseContext = releaseContext;
        }

        public Policy Policy { get; }

        public StandardQuoteDataRetriever QuoteDataRetriever { get; }

        public ReleaseContext ReleaseContext { get; }
    }
}
