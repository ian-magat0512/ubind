// <copyright file="CreateAdditionalPropertyDefinitionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AdditionalPropertyDefinition;

using System;
using NodaTime;
using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
using UBind.Domain.Enums;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.AdditionalPropertyDefinition;

/// <summary>
/// Command in creating additional property definition.
/// </summary>
public class CreateAdditionalPropertyDefinitionCommand : ICommand<AdditionalPropertyDefinitionReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAdditionalPropertyDefinitionCommand"/> class.
    /// </summary>
    /// <param name="propertyDetails">The details of the additional property to be created.</param>
    /// <param name="performingUserId">The ID of the user who triggered the command.</param>
    /// <param name="createdTimestamp">Current time the command has been instantiated.</param>
    /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/> enum.</param>
    /// <param name="contextId">Context ID that owns the additional property.</param>
    /// <param name="validator">Concrete validator that has a base <see cref="AdditionalPropertyDefinitionContextValidator"/> class.</param>
    public CreateAdditionalPropertyDefinitionCommand(
        Guid tenantId,
        IAdditionalPropertyDefinitionDetails propertyDetails,
        Guid? performingUserId,
        Instant createdTimestamp,
        AdditionalPropertyDefinitionContextType contextType,
        Guid contextId)
    {
        this.TenantId = tenantId;
        this.PropertyDetails = propertyDetails;
        this.PerformingUserId = performingUserId;
        this.CreatedTimestampStamp = createdTimestamp;
        this.ContextType = contextType;
        this.ContextId = contextId;
    }

    public Guid TenantId { get; }

    /// <summary>
    /// Gets the details of the additional property to be created.
    /// </summary>
    public IAdditionalPropertyDefinitionDetails PropertyDetails { get; }

    /// <summary>
    /// Gets the ID of the user who performed the command.
    /// </summary>
    public Guid? PerformingUserId { get; }

    /// <summary>
    /// Gets the value of the current time the command has been instantiated.
    /// </summary>
    public Instant CreatedTimestampStamp { get; }

    /// <summary>
    /// Gets the value of the context type.
    /// </summary>
    public AdditionalPropertyDefinitionContextType ContextType { get; }

    /// <summary>
    /// Gets the value of the ID of the context.
    /// </summary>
    public Guid ContextId { get; }
}
