// <copyright file="RegenerateQuoteLuceneIndexCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.LuceneIndex
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    public class RegenerateQuoteLuceneIndexCommandHandler : ICommandHandler<RegenerateQuoteLuceneIndexCommand, Unit>
    {
        private readonly ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchIndexService;
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger<RegenerateQuoteLuceneIndexCommandHandler> logger;

        public RegenerateQuoteLuceneIndexCommandHandler(
            ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchIndexService,
            ICachingResolver cachingResolver,
            ILogger<RegenerateQuoteLuceneIndexCommandHandler> logger)
        {
            this.quoteSearchIndexService = quoteSearchIndexService;
            this.cachingResolver = cachingResolver;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(RegenerateQuoteLuceneIndexCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("RegenerateQuoteLuceneIndexCommandHandler: Regenerating quote lucene index");
            var activeProducts = await this.cachingResolver.GetActiveProducts();
            this.quoteSearchIndexService.RegenerateLuceneIndexes(
                command.Environment,
                activeProducts,
                cancellationToken,
                command.Tenants);
            return Unit.Value;
        }
    }
}
