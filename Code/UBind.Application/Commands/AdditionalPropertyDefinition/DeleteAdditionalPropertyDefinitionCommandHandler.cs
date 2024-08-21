// <copyright file="DeleteAdditionalPropertyDefinitionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AdditionalPropertyDefinition;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
using UBind.Domain.Enums;
using UBind.Domain.Events;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using static UBind.Domain.Errors;

/// <summary>
/// Handler for <see cref="DeleteAdditionalPropertyDefinitionCommand"/> command.
/// </summary>
public class DeleteAdditionalPropertyDefinitionCommandHandler
    : ICommandHandler<DeleteAdditionalPropertyDefinitionCommand>
{
    private readonly IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository;
    private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
    private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAdditionalPropertyDefinitionCommandHandler"/> class.
    /// </summary>
    /// <param name="additionalPropertyAggregateRepository">Aggregate repository for additional property.</param>
    public DeleteAdditionalPropertyDefinitionCommandHandler(
        IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository,
        ITenantSystemEventEmitter tenantSystemEventEmitter,
        IOrganisationSystemEventEmitter organisationSystemEventEmitter)
    {
        this.additionalPropertyAggregateRepository = additionalPropertyAggregateRepository;
        this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        this.organisationSystemEventEmitter = organisationSystemEventEmitter;
    }

    /// <inheritdoc/>
    /// <exception cref="Error">Throws UBind customer error exception in the following instances:
    /// 1. The primary ID of the additional property being targeted to be soft deleted doesn't exists.
    /// </exception>
    public async Task<Unit> Handle(
        DeleteAdditionalPropertyDefinitionCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var additionalPropertyAggregate = this.additionalPropertyAggregateRepository.GetById(
            request.TenantId, request.AdditionalPropertyId);
        if (additionalPropertyAggregate == null)
        {
            throw new ErrorException(AdditionalProperties.DefinitionIdNotFound(
                request.AdditionalPropertyId,
                "delete an additional property definition"));
        }

        async Task<AdditionalPropertyDefinition> Delete()
        {
            additionalPropertyAggregate.MarkAsDeleted(
                request.AdditionalPropertyId,
                request.PerformingUserId,
                request.Instant);
            await this.additionalPropertyAggregateRepository.Save(additionalPropertyAggregate);
            return additionalPropertyAggregate;
        }

        await ConcurrencyPolicy.ExecuteWithRetriesAsync(
            Delete,
            () => this.additionalPropertyAggregateRepository.GetById(request.TenantId, request.AdditionalPropertyId));
        if (additionalPropertyAggregate.ContextType == Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant)
        {
            await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(request.TenantId, SystemEventType.TenantModified);
        }
        else if (additionalPropertyAggregate.ContextType == AdditionalPropertyDefinitionContextType.Organisation)
        {
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(request.TenantId, additionalPropertyAggregate.ContextId);
        }

        return Unit.Value;
    }
}
