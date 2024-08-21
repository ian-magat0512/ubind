// <copyright file="IEntityListReference.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation;

using UBind.Domain;
using UBind.Domain.ReadModel;

public interface IEntityListReference
{
    Guid TenantId { get; }

    EntityListFilters Filters { get; }

    DeploymentEnvironment Environment { get; }

    // <summary>
    // Retrieves the generic type parameter associated with this EntityListReference instance.
    // This method is used when the generic type information needs to be accessed dynamically at runtime.
    // </summary>
    // <returns>The Type object representing the generic type parameter.</returns>
    public Type GetGenericType();
}
