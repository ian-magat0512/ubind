// <copyright file="DeclineQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using UBind.Application.Releases;
using UBind.Application.Services;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

public class DeclineQuoteCommandHandler : ICommandHandler<DeclineQuoteCommand, NewQuoteReadModel>
{
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IQuoteEndorsementService quoteEndorsementService;
    private readonly IAggregateLockingService aggregateLockingService;

    public DeclineQuoteCommandHandler(
        IQuoteAggregateResolverService quoteAggregateResolverService,
        IReleaseQueryService releaseQueryService,
        IQuoteAggregateRepository quoteAggregateRepository,
        IQuoteEndorsementService quoteEndorsementService,
        IAggregateLockingService aggregateLockingService)
    {
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.releaseQueryService = releaseQueryService;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.quoteEndorsementService = quoteEndorsementService;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(DeclineQuoteCommand command, CancellationToken cancellationToken)
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
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId);
            await this.quoteEndorsementService.DeclineQuote(releaseContext, quote, command.Formdata);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
