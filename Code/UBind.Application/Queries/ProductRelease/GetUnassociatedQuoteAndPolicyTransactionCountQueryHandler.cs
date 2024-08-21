// <copyright file="GetUnassociatedQuoteAndPolicyTransactionCountQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

public class GetUnassociatedQuoteAndPolicyTransactionCountQueryHandler
        : IQueryHandler<GetUnassociatedQuoteAndPolicyTransactionCountQuery, QuotePolicyTransactionCountModel>
{
    private readonly IQuoteReadModelRepository quoteReadModelRepository;
    private readonly IPolicyReadModelRepository policyReadModelRepository;

    public GetUnassociatedQuoteAndPolicyTransactionCountQueryHandler(
        IQuoteReadModelRepository quoteReadModelRepository,
        IPolicyReadModelRepository policyReadModelRepository)
    {
        this.quoteReadModelRepository = quoteReadModelRepository;
        this.policyReadModelRepository = policyReadModelRepository;
    }

    public Task<QuotePolicyTransactionCountModel> Handle(
        GetUnassociatedQuoteAndPolicyTransactionCountQuery query,
        CancellationToken cancellationToken)
    {
        var quotes = this.quoteReadModelRepository.GetUnassociatedQuoteAggregateIds(query.TenantId, query.ProductId, query.Environment);
        var policies = this.policyReadModelRepository.GetUnassociatedPolicyTransactionAggregateIds(query.TenantId, query.ProductId, query.Environment);
        return Task.FromResult(new QuotePolicyTransactionCountModel(quotes.Count(), policies.Count()));
    }
}
