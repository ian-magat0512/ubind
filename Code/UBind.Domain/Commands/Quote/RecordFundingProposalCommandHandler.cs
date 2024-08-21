// <copyright file="RecordFundingProposalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class RecordFundingProposalCommandHandler : ICommandHandler<RecordFundingProposalCommand, Unit>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IAggregateLockingService aggregateLockingService;

        public RecordFundingProposalCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IAggregateLockingService aggregateLockingService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(RecordFundingProposalCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolver.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                quote.RecordFundingProposalCreated(
                    command.FundingProposal,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.GetCurrentInstant(),
                    command.QuoteId);
                if (command.Accepted)
                {
                    quoteAggregate.RecordFundingProposalAccepted(
                        command.FundingProposal.InternalId,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.GetCurrentInstant(),
                        quote.Id);
                }

                await this.quoteAggregateRepository.Save(quoteAggregate);
            }
            return Unit.Value;
        }
    }
}
