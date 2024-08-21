// <copyright file="UpdateAdditionalPropertyDefinitionCommand.cs" company="uBind">
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

/// <summary>
/// Command in updating additional property definition.
/// </summary>
public class UpdateAdditionalPropertyDefinitionCommand : ICommand<AdditionalPropertyDefinitionReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAdditionalPropertyDefinitionCommand"/> class.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="id">Primary ID.</param>
    /// <param name="propertyDetails">The data for which the additional property is to be updated with.</param>
    /// <param name="performingUserId">User id.</param>
    /// <param name="modifiedTimeStamp">Current time the command has been instantiated.</param>
    /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/> enum.</param>
    public UpdateAdditionalPropertyDefinitionCommand(
        Guid tenantId,
        Guid id,
        IAdditionalPropertyDefinitionDetails propertyDetails,
        Guid? performingUserId,
        Instant modifiedTimeStamp,
        AdditionalPropertyDefinitionContextType contextType)
    {
        this.TenantId = tenantId;
        this.PerformingUserId = performingUserId;
        this.ModifiedTimestamp = modifiedTimeStamp;
        this.ContextType = contextType;
        this.Id = id;
        this.PropertyDetails = propertyDetails;
    }

    public Guid TenantId { get; }

    /// <summary>
    /// Gets the value o of the user id.
    /// </summary>
    public Guid? PerformingUserId { get; }

    /// <summary>
    /// Gets the value of the current time the command has been instantiated.
    /// </summary>
    public Instant ModifiedTimestamp { get; }

    /// <summary>
    /// Gets the value of context type.
    /// </summary>
    public AdditionalPropertyDefinitionContextType ContextType { get; }

    /// <summary>
    /// Gets the value of primary ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the value of property detail model.
    /// </summary>
    public IAdditionalPropertyDefinitionDetails PropertyDetails { get; }
}
