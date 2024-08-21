// <copyright file="UpdatePolicyStateFromRecentlyStateChangedCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
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
using UBind.Domain.ReadModel.Policy;

/// <summary>
/// This is a command handler to set the state of the policy every 5 mins.
/// And getting the list of policy with in the day of last modified date,
/// The policy state will calculate base on the current timestamp.
/// If the current timestamp is greater than to effective cancellation date or
/// equal to inception date this state is `Cancelled`.
/// If the inception date is greater than current timestamp this state is `Pending`,
/// If the expiration date is greater than the current timestam this state is `Active`
/// Else Expired.
/// </summary>
public class UpdatePolicyStateFromRecentlyStateChangedCommandHandler : ICommandHandler<UpdatePolicyStateFromRecentlyStateChangedCommand, Unit>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IPolicyReadModelRepository readModelRepository;
    private readonly IClock clock;
    private readonly ILogger<UpdatePolicyStateFromRecentlyStateChangedCommandHandler> logger;
    private readonly ICqrsMediator mediator;
    private readonly ISystemEventService systemEventService;
    private List<string> failures = new List<string>();

    public UpdatePolicyStateFromRecentlyStateChangedCommandHandler(
        ICachingResolver cachingResolver,
        IClock clock,
        IPolicyReadModelRepository readModelRepository,
        ILogger<UpdatePolicyStateFromRecentlyStateChangedCommandHandler> logger,
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

    public async Task<Unit> Handle(UpdatePolicyStateFromRecentlyStateChangedCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int totalCount = 0;
        int totalFailed = 0;
        var failureMessage = new StringBuilder();
        foreach (var tenantId in request.TenantIds)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            this.logger.LogInformation(
                $"Updating policies for tenant {tenant.Details.Alias}:");
            var policiesWhichRecentlyBecameActive
                = this.readModelRepository.GetPoliciesThatHaveRecentlyBecomeActive(tenant.Id);
            totalCount += policiesWhichRecentlyBecameActive.Count();
            foreach (var policy in policiesWhichRecentlyBecameActive)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.UpdatePolicyState(policy, PolicyStatus.Active, cancellationToken);
            }

            var policiesWhichRecentlyExpired
                = this.readModelRepository.GetPoliciesThatHaveRecentlyExpired(tenant.Id);
            totalCount += policiesWhichRecentlyExpired.Count();
            foreach (var policy in policiesWhichRecentlyExpired)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.UpdatePolicyState(policy, PolicyStatus.Expired, cancellationToken);
            }

            var policiesWhichRecentlyCancelled
                = this.readModelRepository.GetPoliciesThatHaveRecentlyCancelled(tenant.Id);
            totalCount += policiesWhichRecentlyCancelled.Count();
            foreach (var policy in policiesWhichRecentlyCancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.UpdatePolicyState(policy, PolicyStatus.Cancelled, cancellationToken);
            }

            if (this.failures.Any())
            {
                totalFailed += this.failures.Count;
                failureMessage.AppendLine(
                    "The following policies were not expired by the Policy Status Updater Job "
                    + $"under the tenant {tenant.Details.Alias} ({tenantId}), with their corresponding errors. ");
                failureMessage.AppendLine();

                this.failures.ForEach(policyId =>
                {
                    failureMessage.AppendLine($" - {policyId}");
                    failureMessage.AppendLine();
                });
                failureMessage.AppendLine(
                    "Please investigate to confirm that each policy has their state updated correctly");
                failureMessage.AppendLine("--------------------------------------------------------");
                this.failures.Clear();
            }
        }

        if (totalFailed > 0)
        {
            await this.mediator.Send(new CaptureSentryMessageCommand(failureMessage.ToString()));
        }

        this.logger.LogInformation($"Processed {totalCount} policies of which {totalFailed} failed to update.");
        return Unit.Value;
    }

    private static void AddStandardPolicyRelationships(
    SystemEvent systemEvent, PolicyReadModel policy)
    {
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.OrganisationEvent, EntityType.Organisation, policy.OrganisationId);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.ProductEvent, EntityType.Product, policy.ProductId);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.PolicyEvent, EntityType.Policy, policy.Id);

        if (policy.CustomerId.HasValue)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.CustomerEvent, EntityType.Customer, policy.CustomerId.Value);
        }
    }

    private async Task UpdatePolicyState(
        PolicyReadModel policy,
        PolicyStatus newPolicyState,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            string originalState = policy.PolicyState;
            this.logger.LogInformation(
                $"Changing policy state from {originalState} to {newPolicyState} for policy {policy.Id}.");
            policy.PolicyState = newPolicyState.Humanize();
            policy.LastModifiedTimestamp = this.clock.Now();
            await this.EmitSystemEvent(policy, originalState, newPolicyState.Humanize());

            // no need to ask the dbContext to save changes because the system event service will do it for us
            /* await this.dbContext.SaveChangesAsync(); */

            await Task.Delay(5000, cancellationToken);
        }
        catch (Exception ex)
        {
            // Skipping record that threw an error..
            this.logger.LogError($"Error: An exception was encountered when updating policy {policy.Id}. "
                + $"Exception Message: {ex.Message}. Skipping record...");

            // collecting error to be reported at end of tenant/environment loop
            this.failures.Add($"{policy.Id}. Exception: {ex.Message}. "
                + $"Stacktrace: {ex.StackTrace?.LimitLengthWithEllipsis(3000)}");
        }
    }

    private async Task EmitSystemEvent(PolicyReadModel policy, string originalState, string resultingState)
    {
        var eventPayload = new Dictionary<string, string>
        {
            { "originalState", originalState.ToCamelCase() },
            { "resultingState", resultingState.ToCamelCase() },
        };
        var systemEvent = SystemEvent.CreateWithPayload(
          policy.TenantId,
          policy.OrganisationId,
          policy.ProductId,
          policy.Environment,
          SystemEventType.PolicyStateChanged,
          eventPayload,
          this.clock.Now());
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.QuoteEvent, EntityType.Quote, policy.Id);
        AddStandardPolicyRelationships(systemEvent, policy);
        await this.systemEventService.PersistAndEmit(new List<SystemEvent> { systemEvent });
    }
}
