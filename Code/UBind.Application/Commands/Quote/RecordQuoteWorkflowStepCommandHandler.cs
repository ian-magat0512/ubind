// <copyright file="RecordQuoteWorkflowStepCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using NodaTime;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

public class RecordQuoteWorkflowStepCommandHandler : ICommandHandler<RecordQuoteWorkflowStepCommand, NewQuoteReadModel>
{
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IProductFeatureSettingService productFeatureSettingService;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IClock clock;
    private readonly IAggregateLockingService aggregateLockingService;

    public RecordQuoteWorkflowStepCommandHandler(
        IQuoteAggregateRepository quoteAggregateRepository,
        IQuoteAggregateResolverService quoteAggregateResolverService,
        IProductFeatureSettingService productFeatureSettingService,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock,
        IAggregateLockingService aggregateLockingService)
    {
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.productFeatureSettingService = productFeatureSettingService;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(RecordQuoteWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
        {
            var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
            var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
            quoteAggregate.RecordWorkflowStep(command.WorkflowStep, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quote.Id);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
