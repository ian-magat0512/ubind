// <copyright file="UpdateAdditionalPropertyDefinitionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AdditionalPropertyDefinition;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
using UBind.Domain.Enums;
using UBind.Domain.Events;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.AdditionalPropertyDefinition;
using static UBind.Domain.Errors;

/// <summary>
/// Handler for <see cref="UpdateAdditionalPropertyDefinitionCommand"/> command.
/// </summary>
public class UpdateAdditionalPropertyDefinitionCommandHandler
    : ICommandHandler<UpdateAdditionalPropertyDefinitionCommand, AdditionalPropertyDefinitionReadModel>
{
    private readonly IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository;
    private readonly IAdditionalPropertyDefinitionRepository additionalPropertyRepository;
    private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
    private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;
    private readonly IAdditionalPropertyDefinitionValidator additionalPropertyValidator;
    private readonly IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator;

    public UpdateAdditionalPropertyDefinitionCommandHandler(
        IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository,
        IAdditionalPropertyDefinitionRepository additionalPropertyRepository,
        ITenantSystemEventEmitter tenantSystemEventEmitter,
        IOrganisationSystemEventEmitter organisationSystemEventEmitter,
        IAdditionalPropertyDefinitionValidator additionalPropertyValidator,
        IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator)
    {
        this.additionalPropertyAggregateRepository = additionalPropertyAggregateRepository;
        this.additionalPropertyRepository = additionalPropertyRepository;
        this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        this.additionalPropertyValidator = additionalPropertyValidator;
        this.additionalPropertyDefinitionJsonValidator = additionalPropertyDefinitionJsonValidator;
    }

    /// <inheritdoc/>
    /// <exception cref="Error">Throws UBind customer error exception in the following instances:
    /// 1. Name and or alias already used by other additional property.
    /// 2. The context ID is invalid (It doesn't exists in the database).
    /// 3. The primary ID of the additional property being targeted to modify doesn't exists.
    /// </exception>
    public async Task<AdditionalPropertyDefinitionReadModel> Handle(
        UpdateAdditionalPropertyDefinitionCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.ValidatePropertyDetails(command);
        var additionalPropertyAggregate
            = this.additionalPropertyAggregateRepository.GetById(command.TenantId, command.Id);
        if (additionalPropertyAggregate == null)
        {
            throw new ErrorException(AdditionalProperties.DefinitionIdNotFound(
                command.Id,
                "update an additional property definition"));
        }

        var validator = this.additionalPropertyValidator.GetValidatorByContextType(command.ContextType);
        await validator.ThrowIfValidationFailsOnUpdate(
            command.TenantId,
            command.Id,
            command.PropertyDetails.Name,
            command.PropertyDetails.Alias,
            additionalPropertyAggregate.ContextId,
            additionalPropertyAggregate.EntityType,
            additionalPropertyAggregate.ParentContextId);

        async Task<AdditionalPropertyDefinition> Update()
        {
            additionalPropertyAggregate.Update(
                command.Id, command.PropertyDetails, command.PerformingUserId, command.ModifiedTimestamp);
            await this.additionalPropertyAggregateRepository.Save(additionalPropertyAggregate);
            return additionalPropertyAggregate;
        }

        var additionalPropertyDefinition = await ConcurrencyPolicy.ExecuteWithRetriesAsync(
            Update,
            () => additionalPropertyAggregate = this.additionalPropertyAggregateRepository.GetById(command.TenantId, command.Id));
        var updatedReadModel = await this.additionalPropertyRepository.GetById(command.TenantId, command.Id);
        if (updatedReadModel == null)
        {
            throw new ErrorException(Errors.AdditionalProperties.DefinitionIdNotFound(
                command.Id,
                "update an additional property definition"));
        }

        if (updatedReadModel.ContextType == Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant)
        {
            await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(command.TenantId, SystemEventType.TenantModified);
        }
        else if (updatedReadModel.ContextType == AdditionalPropertyDefinitionContextType.Organisation)
        {
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(command.TenantId, updatedReadModel.ContextId);
        }

        return updatedReadModel;
    }

    private void ValidatePropertyDetails(UpdateAdditionalPropertyDefinitionCommand command)
    {
        var propertyDetails = command.PropertyDetails;
        if (propertyDetails.Type != AdditionalPropertyDefinitionType.StructuredData
                    && propertyDetails.SchemaType != null
                    && string.IsNullOrEmpty(propertyDetails.CustomSchema))
        {
            throw new ArgumentException("Invalid additional property definition provided");
        }

        if (propertyDetails.Type == AdditionalPropertyDefinitionType.StructuredData
            && propertyDetails.SchemaType == null)
        {
            throw new ArgumentException("SchemaType cannot be null for structured data property type");
        }

        if (propertyDetails.Type == AdditionalPropertyDefinitionType.StructuredData
            && propertyDetails.SchemaType == AdditionalPropertyDefinitionSchemaType.Custom)
        {
            if (propertyDetails.CustomSchema == null)
            {
                throw new ArgumentException("CustomSchema cannot be null for schema type custom");
            }

            this.additionalPropertyDefinitionJsonValidator.ThrowIfSchemaIsNotValid(
                propertyDetails.CustomSchema,
                propertyDetails.Alias,
                command.TenantId,
                propertyDetails.EntityType,
                command.ContextType);
        }

        if (propertyDetails.Type == AdditionalPropertyDefinitionType.StructuredData
            && propertyDetails.SchemaType != AdditionalPropertyDefinitionSchemaType.None
            && !string.IsNullOrEmpty(propertyDetails.DefaultValue))
        {
            this.additionalPropertyDefinitionJsonValidator.ThrowIfValueFailsSchemaAssertion(
                propertyDetails.SchemaType.Value,
                "Default Value",
                propertyDetails.DefaultValue,
                propertyDetails.CustomSchema);
        }
    }
}
