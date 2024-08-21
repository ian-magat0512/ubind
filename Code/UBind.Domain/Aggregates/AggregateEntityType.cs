// <copyright file="AggregateEntityType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates;

/// <summary>
/// Provides a list of entity types which are aggregates. This is used to resolve the correct repository for an aggregate,
/// and is used by the maintenance controller to allow you to select an aggregate to replay events for.
/// </summary>
public enum AggregateEntityType
{
    Quote = EntityType.Quote,
    Policy = EntityType.Policy,
    User = EntityType.User,
    Customer = EntityType.Customer,
    Person = EntityType.Person,
    Organisation = EntityType.Organisation,
    Claim = EntityType.Claim,
    Report = EntityType.Report,
}
