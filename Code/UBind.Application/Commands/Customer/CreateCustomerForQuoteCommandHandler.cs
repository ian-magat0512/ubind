// <copyright file="CreateCustomerForQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Customer;

using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

public class CreateCustomerForQuoteCommandHandler : ICommandHandler<CreateCustomerForQuoteCommand, NewQuoteReadModel>
{
    private readonly IApplicationQuoteService applicationQuoteService;
    private readonly IAggregateLockingService aggregateLockingService;
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;

    public CreateCustomerForQuoteCommandHandler(
        IApplicationQuoteService applicationQuoteService,
        IAggregateLockingService aggregateLockingService,
        IQuoteAggregateResolverService quoteAggregateResolverService)
    {
        this.applicationQuoteService = applicationQuoteService;
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(CreateCustomerForQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
        {
            var aggregate = await this.applicationQuoteService.CreateCustomerForApplication(
                command.TenantId,
                command.QuoteId,
                quoteAggregateId,
                command.CustomerDetails,
                command.PortalId);
            var quote = aggregate.GetQuoteOrThrow(command.QuoteId);
            return quote.ReadModel;
        }
    }
}
