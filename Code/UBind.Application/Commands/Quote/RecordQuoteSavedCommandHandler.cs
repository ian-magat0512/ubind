// <copyright file="RecordQuoteSavedCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class RecordQuoteSavedCommandHandler : ICommandHandler<RecordQuoteSavedCommand, Unit>
    {
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IAggregateLockingService aggregateLockingService;

        public RecordQuoteSavedCommandHandler(
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IAggregateLockingService aggregateLockingService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(RecordQuoteSavedCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var now = this.clock.Now();
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                quote.SaveQuote(this.httpContextPropertiesResolver.PerformingUserId, now);
                await this.quoteAggregateRepository.Save(quoteAggregate);
            }
            return Unit.Value;
        }
    }
}
