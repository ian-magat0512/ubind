// <copyright file="SynchroniseProductComponentCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Release;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Synchronises a claim or quote product component from files, into stored configuration.
/// </summary>
[RetryOnDbExceptionAttribute(0)]
public class SynchroniseProductComponentCommand : ICommand<DevRelease>
{
    public SynchroniseProductComponentCommand(Guid tenantId, Guid productId, WebFormAppType componentType)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.ComponentType = componentType;
    }

    public Guid TenantId { get; }

    public Guid ProductId { get; }

    public WebFormAppType ComponentType { get; }
}
