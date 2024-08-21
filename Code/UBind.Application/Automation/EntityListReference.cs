// <copyright file="EntityListReference.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation;

using UBind.Domain;
using UBind.Domain.ReadModel;

public class EntityListReference<T> : IEntityListReference
{
    // <summary>
    // Parameterless constructor for EntityListReference class.
    // This is for serialization purposes, allowing the class to be instantiated without specific data.
    // Serialization frameworks often require a parameterless constructor to create instances during deserialization.
    // </summary>
    public EntityListReference()
    {
    }

    public EntityListReference(PortalPageData portalPageData)
    {
        this.TenantId = portalPageData.TenantId;
        this.Filters = portalPageData.Filters;
        this.Environment = portalPageData.Environment;
    }

    public Guid TenantId { get; }

    public EntityListFilters Filters { get; }

    public DeploymentEnvironment Environment { get; }

    /// <inheritdoc />
    public Type GetGenericType()
    {
        return typeof(T);
    }
}
