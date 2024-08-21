// <copyright file="ActualiseQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using UBind.Application.Releases;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

/// <summary>
/// This command returns an instance of <see cref="NewQuoteReadModel"/> after actualising a quote, only if
/// the quote was actualised successfully. If it was already actualised, it will return null.
/// </summary>
public class ActualiseQuoteCommandHandler : ICommandHandler<ActualiseQuoteCommand, NewQuoteReadModel?>
{
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IApplicationQuoteService applicationQuoteService;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IAggregateLockingService aggregateLockingService;

    public ActualiseQuoteCommandHandler(
        IQuoteAggregateResolverService quoteAggregateResolverService,
        IApplicationQuoteService applicationQuoteService,
        IReleaseQueryService releaseQueryService,
        IQuoteAggregateRepository quoteAggregateRepository,
        IAggregateLockingService aggregateLockingService)
    {
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.applicationQuoteService = applicationQuoteService;
        this.releaseQueryService = releaseQueryService;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(ActualiseQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
        {
            var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
            var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                command.TenantId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId);
            await this.applicationQuoteService.Actualise(releaseContext, quote, command.FormData);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
