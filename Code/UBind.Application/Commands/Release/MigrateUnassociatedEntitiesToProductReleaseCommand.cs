// <copyright file="MigrateUnassociatedEntitiesToProductReleaseCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Release;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents a command for migrating unassociated entities to a new product release.
/// Contains information such as the tenantId, new releaseId, and deployment environment.
/// </summary>
public class MigrateUnassociatedEntitiesToProductReleaseCommand : ICommand
{
    public MigrateUnassociatedEntitiesToProductReleaseCommand(
        Guid tenantId, Guid productId, Guid newReleaseId, DeploymentEnvironment environment)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.NewReleaseId = newReleaseId;
        this.Environment = environment;
    }

    public Guid TenantId { get; }

    public Guid ProductId { get; }

    public Guid NewReleaseId { get; }

    public DeploymentEnvironment Environment { get; }
}
