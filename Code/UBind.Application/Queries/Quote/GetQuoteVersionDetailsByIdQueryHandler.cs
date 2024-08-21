// <copyright file="GetQuoteVersionDetailsByIdQueryHandler.cs" company="uBind">
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

    public class GetQuoteVersionDetailsByIdQueryHandler
        : IQueryHandler<GetQuoteVersionDetailsByIdQuery, IQuoteVersionReadModelDetails>
    {
        private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;

        public GetQuoteVersionDetailsByIdQueryHandler(
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository)
        {
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
        }

        public Task<IQuoteVersionReadModelDetails> Handle(GetQuoteVersionDetailsByIdQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteVersion = this.quoteVersionReadModelRepository.GetVersionDetailsById(query.TenantId, query.QuoteVersionId);
            return Task.FromResult(quoteVersion);
        }
    }
}
