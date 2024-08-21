// <copyright file="ReferQuoteForEndorsementCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote;

using UBind.Application.Releases;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

public class ReferQuoteForEndorsementCommandHandler : ICommandHandler<ReferQuoteForEndorsementCommand, NewQuoteReadModel>
{
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IQuoteEndorsementService quoteEndorsementService;
    private readonly IAggregateLockingService aggregateLockingService;

    public ReferQuoteForEndorsementCommandHandler(
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

    public async Task<NewQuoteReadModel> Handle(ReferQuoteForEndorsementCommand command, CancellationToken cancellationToken)
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
            await this.quoteEndorsementService.ReferQuoteForEndorsement(releaseContext, quote, command.FormData);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
