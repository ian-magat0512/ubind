// <copyright file="GetQuoteVersionsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.QuoteVersions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query Handler for getting quote versions.
    /// </summary>
    public class GetQuoteVersionsQueryHandler : IQueryHandler<GetQuoteVersionsQuery, IEnumerable<IQuoteVersionReadModelSummary>>
    {
        private readonly IQuoteVersionReadModelRepository quoteVersionRepository;

        public GetQuoteVersionsQueryHandler(IQuoteVersionReadModelRepository quoteVersionRepository)
        {
            this.quoteVersionRepository = quoteVersionRepository;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IQuoteVersionReadModelSummary>> Handle(GetQuoteVersionsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.quoteVersionRepository.GetDetailVersionsOfQuote(request.TenantId, request.QuoteId));
        }
    }
}
