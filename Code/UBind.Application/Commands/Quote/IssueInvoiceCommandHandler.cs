// <copyright file="IssueInvoiceCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Releases;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    public class IssueInvoiceCommandHandler : ICommandHandler<IssueInvoiceCommand, NewQuoteReadModel>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IAccountingTransactionService accountingTransactionService;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IAggregateLockingService aggregateLockingService;

        public IssueInvoiceCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAccountingTransactionService accountingTransactionService,
            IReleaseQueryService releaseQueryService,
            IAggregateLockingService aggregateLockingService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.accountingTransactionService = accountingTransactionService;
            this.releaseQueryService = releaseQueryService;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<NewQuoteReadModel> Handle(IssueInvoiceCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, Domain.AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                    command.TenantId,
                    quote.Aggregate.ProductId,
                    quote.Aggregate.Environment,
                    quote.ProductReleaseId);
                await this.accountingTransactionService.IssueInvoice(releaseContext, quote, false);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return quote.ReadModel;
            }
        }
    }
}
