// <copyright file="IsAdditionalPropertyValueUniqueQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyValue;

using System;
using UBind.Domain.Enums;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents a query model used to check for the uniqueness of additional property values.
/// </summary>
public class IsAdditionalPropertyValueUniqueQuery : IQuery<bool>
{
    public IsAdditionalPropertyValueUniqueQuery(
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        Guid? entityId,
        AdditionalPropertyDefinitionType propertyDefinitionType,
        Guid additionalPropertyDefinitionId,
        string? value)
    {
        this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
        this.EntityId = entityId;
        this.PropertyType = propertyDefinitionType;
        this.Value = value;
        this.TenantId = tenantId;
        this.EntityType = entityType;
    }

    public Guid AdditionalPropertyDefinitionId { get; }

    public string? Value { get; }

    public Guid? EntityId { get; }

    public AdditionalPropertyDefinitionType PropertyType { get; }

    public Guid TenantId { get; }

    public AdditionalPropertyEntityType EntityType { get; }
}
