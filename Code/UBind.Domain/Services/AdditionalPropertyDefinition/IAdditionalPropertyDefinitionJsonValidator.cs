// <copyright file="IAdditionalPropertyDefinitionJsonValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition;

using UBind.Domain.Enums;

/// <summary>
/// Defines the contract for validating JSON representations within additional property definitions of structured data types.
/// This interface outlines methods for ensuring the integrity of JSON representations within additional property definitions
/// of structured data type, including validation against custom and default schemas.
/// </summary>
public interface IAdditionalPropertyDefinitionJsonValidator
{
    void ThrowIfValueFailsSchemaAssertion(
        AdditionalPropertyDefinitionSchemaType schemaType,
        string fieldName,
        string jsonString,
        string? customSchema);

    void ThrowIfSchemaIsNotValid(
        string jsonSchema,
        string alias,
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        AdditionalPropertyDefinitionContextType contextType);
}
