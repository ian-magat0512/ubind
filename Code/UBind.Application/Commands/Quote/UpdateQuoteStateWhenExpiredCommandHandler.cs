// <copyright file="UpdateQuoteStateWhenExpiredCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Application.Commands.Sentry;
using UBind.Application.SystemEvents;
using UBind.Domain;
using UBind.Domain.Events;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.ValueTypes;

/// <summary>
/// Command handler for <see cref="UpdateQuoteStateWhenExpiredCommand"/>.
/// </summary>
public class UpdateQuoteStateWhenExpiredCommandHandler : ICommandHandler<UpdateQuoteStateWhenExpiredCommand, Unit>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IQuoteReadModelRepository readModelRepository;
    private readonly IClock clock;
    private readonly ILogger<UpdateQuoteStateWhenExpiredCommandHandler> logger;
    private readonly ICqrsMediator mediator;
    private readonly ISystemEventService systemEventService;
    private List<string> failures = new List<string>();

    public UpdateQuoteStateWhenExpiredCommandHandler(
        ICachingResolver cachingResolver,
        IClock clock,
        IQuoteReadModelRepository readModelRepository,
        ILogger<UpdateQuoteStateWhenExpiredCommandHandler> logger,
        ICqrsMediator mediator,
        ISystemEventService systemEventService)
    {
        this.cachingResolver = cachingResolver;
        this.clock = clock;
        this.readModelRepository = readModelRepository;
        this.logger = logger;
        this.mediator = mediator;
        this.systemEventService = systemEventService;
    }

    public async Task<Unit> Handle(UpdateQuoteStateWhenExpiredCommand request, CancellationToken cancellationToken = default(CancellationToken))
    {
        int totalCount = 0;
        int totalFailed = 0;
        var failureMessage = new StringBuilder();
        foreach (var tenantId in request.TenantIds)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var quotesThatAreExpiring = this.readModelRepository.GetQuotesThatAreRecentlyExpired(tenant.Id);
            int count = quotesThatAreExpiring.Count();
            totalCount += count;
            if (count > 0)
            {
                this.logger.LogInformation(
                    $"Updating {count} quotes for tenant {tenant.Details.Alias}:");
                foreach (var quote in quotesThatAreExpiring)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await this.UpdateQuoteState(
                        quote, StandardQuoteStates.Expired, cancellationToken);
                }
            }

            if (this.failures.Any())
            {
                totalFailed += this.failures.Count;
                failureMessage.AppendLine(
                    "The following quotes were not expired by the Quote Status Updater Job "
                    + $"under the tenant {tenant.Details.Alias} ({tenantId}), with their corresponding errors. ");
                failureMessage.AppendLine();

                this.failures.ForEach(quoteId =>
                {
                    failureMessage.AppendLine($" - {quoteId}");
                    failureMessage.AppendLine();
                });
                failureMessage.AppendLine(
                    "Please investigate to confirm that each quote has their state updated correctly");
                failureMessage.AppendLine("--------------------------------------------------------");
                this.failures.Clear();
            }
        }

        if (totalFailed > 0)
        {
            await this.mediator.Send(new CaptureSentryMessageCommand(failureMessage.ToString()));
        }

        this.logger.LogInformation($"Processed {totalCount} quotes of which {totalFailed} failed to update.");
        return Unit.Value;
    }

    private static void AddStandardQuoteRelationships(
        SystemEvent systemEvent, NewQuoteReadModel quote)
    {
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.OrganisationEvent, EntityType.Organisation, quote.OrganisationId);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.ProductEvent, EntityType.Product, quote.ProductId);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.PolicyEvent, EntityType.Policy, quote.PolicyId);

        if (quote.CustomerId.HasValue)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.CustomerEvent, EntityType.Customer, quote.CustomerId.Value);
        }
    }

    private async Task UpdateQuoteState(
        NewQuoteReadModel quote,
        string newQuoteState,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            string originalState = quote.QuoteState;
            this.logger.LogInformation(
                $"Changing quote state from {originalState} to {newQuoteState} for quote {quote.Id}.");
            quote.QuoteState = newQuoteState;
            quote.LastModifiedTimestamp = this.clock.Now();
            await this.EmitSystemEvent(quote, originalState, newQuoteState);

            // no need to ask the dbContext to save changes because the system event service will do it for us
            /* await this.dbContext.SaveChangesAsync(); */

            await Task.Delay(5000, cancellationToken);
        }
        catch (Exception ex)
        {
            // Skipping record that threw an error..
            this.logger.LogError($"Error: An exception was encountered when updating quote {quote.Id}. "
                + $"Exception Message: {ex.Message}. Skipping record...");

            // collecting error to be reported at end of tenant/environment loop
            this.failures.Add($"{quote.Id}. Exception: {ex.Message}. "
                + $"Stacktrace: {ex.StackTrace?.LimitLengthWithEllipsis(3000)}");
        }
    }

    private async Task EmitSystemEvent(NewQuoteReadModel quote, string originalState, string resultingState)
    {
        var eventPayload = new Dictionary<string, string>
        {
            { "originalState", originalState.ToCamelCase() },
            { "resultingState", resultingState.ToCamelCase() },
        };
        var systemEvent = SystemEvent.CreateWithPayload(
          quote.TenantId,
          quote.OrganisationId,
          quote.ProductId,
          quote.Environment,
          SystemEventType.QuoteStateChanged,
          eventPayload,
          this.clock.Now());
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.QuoteEvent, EntityType.Quote, quote.Id);
        AddStandardQuoteRelationships(systemEvent, quote);
        await this.systemEventService.PersistAndEmit(new List<SystemEvent> { systemEvent });
    }
}
