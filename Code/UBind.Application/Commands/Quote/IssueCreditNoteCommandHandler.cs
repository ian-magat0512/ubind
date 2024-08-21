// <copyright file="IssueCreditNoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Releases;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    public class IssueCreditNoteCommandHandler : ICommandHandler<IssueCreditNoteCommand, NewQuoteReadModel>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IAccountingTransactionService accountingTransactionService;
        private readonly IReleaseQueryService releaseQueryService;

        public IssueCreditNoteCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAccountingTransactionService accountingTransactionService,
            IReleaseQueryService releaseQueryService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.accountingTransactionService = accountingTransactionService;
            this.releaseQueryService = releaseQueryService;
        }

        public async Task<NewQuoteReadModel> Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(request.TenantId, request.QuoteId);
            var quote = quoteAggregate.GetQuoteOrThrow(request.QuoteId);
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                request.TenantId,
                quote.Aggregate.ProductId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId);
            await this.accountingTransactionService.IssueCreditNote(releaseContext, quote, false);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
