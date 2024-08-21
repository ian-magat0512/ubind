// <copyright file="CreateAdditionalPropertyDefinitionCommandHandler.cs" company="uBind">
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
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.AdditionalPropertyDefinition;

/// <summary>
/// Handler for <see cref="CreateAdditionalPropertyDefinitionCommand"/> command.
/// </summary>
public class CreateAdditionalPropertyDefinitionCommandHandler
    : ICommandHandler<CreateAdditionalPropertyDefinitionCommand, AdditionalPropertyDefinitionReadModel>
{
    private readonly IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository;
    private readonly IAdditionalPropertyDefinitionRepository additionalPropertyRepository;
    private readonly IAdditionalPropertyDefinitionValidator additionalPropertyValidator;
    private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
    private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;
    private readonly IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAdditionalPropertyDefinitionCommandHandler"/> class.
    /// </summary>
    /// <param name="additionalPropertyAggregateRepository">Aggregate repository for additional property.</param>
    /// <param name="additionalPropertyRepository">Repository of additional property.</param>
    /// <param name="tenantRepository">Repository of tenant.</param>
    public CreateAdditionalPropertyDefinitionCommandHandler(
        IAdditionalPropertyDefinitionAggregateRepository additionalPropertyAggregateRepository,
        IAdditionalPropertyDefinitionRepository additionalPropertyRepository,
        ITenantSystemEventEmitter tenancySystemEventEmitter,
        IOrganisationSystemEventEmitter organisationSystemEventEmitter,
        IAdditionalPropertyDefinitionValidator additionalPropertyValidator,
        IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator)
    {
        this.additionalPropertyAggregateRepository = additionalPropertyAggregateRepository;
        this.additionalPropertyRepository = additionalPropertyRepository;
        this.tenantSystemEventEmitter = tenancySystemEventEmitter;
        this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        this.additionalPropertyValidator = additionalPropertyValidator;
        this.additionalPropertyDefinitionJsonValidator = additionalPropertyDefinitionJsonValidator;
    }

    /// <inheritdoc/>
    /// <exception cref="Error">Throws UBind custom error exception in the following instances:
    /// 1. Name and or alias already used by other additional property.
    /// 2. The context ID is invalid (It doesn't exists in the database).
    /// </exception>
    public async Task<AdditionalPropertyDefinitionReadModel> Handle(
        CreateAdditionalPropertyDefinitionCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.ValidatePropertyDetails(command);
        var validator = this.additionalPropertyValidator.GetValidatorByContextType(command.ContextType);
        await validator.ThrowIfValidationFailsOnCreate(
            command.TenantId,
            command.PropertyDetails.Name,
            command.PropertyDetails.Alias,
            command.ContextId,
            command.PropertyDetails.EntityType,
            command.PropertyDetails.ParentContextId);

        AdditionalPropertyDefinition additionalPropertyDefinition = null;
        switch (command.PropertyDetails.Type)
        {
            case AdditionalPropertyDefinitionType.Text:
                additionalPropertyDefinition = AdditionalPropertyDefinition.CreateForText(
                    command.TenantId,
                    command.PropertyDetails.Alias,
                    command.PropertyDetails.Name,
                    command.PropertyDetails.EntityType,
                    command.ContextType,
                    command.PropertyDetails.IsRequired,
                    command.PropertyDetails.IsUnique,
                    command.ContextId,
                    command.PropertyDetails.ParentContextId,
                    command.PropertyDetails.DefaultValue,
                    command.PerformingUserId,
                    command.CreatedTimestampStamp);

                break;
            case AdditionalPropertyDefinitionType.StructuredData:
                additionalPropertyDefinition = AdditionalPropertyDefinition.CreateForStructedData(
                    command.TenantId,
                    command.PropertyDetails.Alias,
                    command.PropertyDetails.Name,
                    command.PropertyDetails.EntityType,
                    command.ContextType,
                    command.PropertyDetails.IsRequired,
                    command.PropertyDetails.IsUnique,
                    command.ContextId,
                    command.PropertyDetails.ParentContextId,
                    command.PropertyDetails.DefaultValue,
                    command.PerformingUserId,
                    command.CreatedTimestampStamp,
                    command.PropertyDetails.SchemaType.Value,
                    command.PropertyDetails.CustomSchema);
                break;
            default:
                throw new ErrorException(
                    Errors.AdditionalProperties.PropertyTypeNotYetSupported(command.PropertyDetails.Type.ToString()));
        }

        await this.additionalPropertyAggregateRepository.Save(additionalPropertyDefinition);
        var readModel = await this.additionalPropertyRepository.GetById(
            command.TenantId, additionalPropertyDefinition.Id);

        if (readModel == null)
        {
            throw new ErrorException(
                Errors.AdditionalProperties.AdditionalPropertyDefinitionFailedToBePersisted(
                readModel.Name, readModel.DefaultValue, additionalPropertyDefinition.Id));
        }

        if (readModel.ContextType == Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant)
        {
            await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(command.TenantId, SystemEventType.TenantModified);
        }
        else if (readModel.ContextType == AdditionalPropertyDefinitionContextType.Organisation)
        {
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(command.TenantId, readModel.ContextId);
        }

        return readModel;
    }

    private void ValidatePropertyDetails(CreateAdditionalPropertyDefinitionCommand command)
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
