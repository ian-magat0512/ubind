// <copyright file="GetLatestQuoteOfTypeForPolicyQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

public class GetLatestQuoteOfTypeForPolicyQueryHandler : IQueryHandler<GetLatestQuoteOfTypeForPolicyQuery, NewQuoteReadModel>
{
    private readonly IQuoteReadModelRepository quoteReadModelRepository;

    public GetLatestQuoteOfTypeForPolicyQueryHandler(IQuoteReadModelRepository quoteReadModelRepository)
    {
        this.quoteReadModelRepository = quoteReadModelRepository;
    }

    public Task<NewQuoteReadModel?> Handle(GetLatestQuoteOfTypeForPolicyQuery query, CancellationToken cancellationToken)
    {
        var result = this.quoteReadModelRepository.GetLatestQuoteOfTypeForPolicy(query.TenantId, query.PolicyId, query.QuoteType);
        return Task.FromResult(result);
    }
}
