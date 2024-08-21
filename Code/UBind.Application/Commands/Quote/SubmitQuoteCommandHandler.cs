// <copyright file="SubmitQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using UBind.Domain.Helpers;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;
using UBind.Domain.Aggregates.Quote;
using NodaTime;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Extensions;

/// <summary>
/// Represents the command handler for submitting a quote.
/// </summary>
public class SubmitQuoteCommandHandler : ICommandHandler<SubmitQuoteCommand, NewQuoteReadModel>
{
    private readonly IAggregateLockingService aggregateLockingService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IClock clock;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();

    public SubmitQuoteCommandHandler(
        IAggregateLockingService aggregateLockingService,
        IQuoteAggregateRepository quoteAggregateRepository,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock,
        IQuoteAggregateResolverService quoteAggregateResolverService)
    {
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(SubmitQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
        {
            var now = this.clock.Now();
            var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
            var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
            if (command.Formdata != null)
            {
                quote.UpdateFormData(command.Formdata, this.httpContextPropertiesResolver.PerformingUserId, now);
            }

            quote.Submit(this.httpContextPropertiesResolver.PerformingUserId, now, this.quoteWorkflow);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
            return quote.ReadModel;
        }
    }
}
