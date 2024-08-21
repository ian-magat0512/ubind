// <copyright file="GetQuoteCountQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetQuoteCountQueryHandler : IQueryHandler<GetQuoteCountQuery, int>
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;

        public GetQuoteCountQueryHandler(IQuoteReadModelRepository quoteReadModelRepository)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
        }

        public Task<int> Handle(GetQuoteCountQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int count = this.quoteReadModelRepository.CountQuotes(request.TenantId, request.Filters);
            return Task.FromResult(count);
        }
    }
}
