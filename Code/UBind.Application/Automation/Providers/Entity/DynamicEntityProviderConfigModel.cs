// <copyright file="DynamicEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity;

using System;
using UBind.Application.Automation.Providers;

/// <summary>
/// Model for building an instance of <see cref="DynamicEntityProvider"/>.
/// </summary>
public class DynamicEntityProviderConfigModel : IBuilder<BaseEntityProvider>
{
    /// <summary>
    /// Gets or sets the type of the dynamic entity.
    /// </summary>
    public IBuilder<IProvider<Data<string>>> EntityType { get; set; }

    /// <summary>
    /// Gets or sets the id of the dynamic entity.
    /// </summary>
    public IBuilder<IProvider<Data<string>>> EntityId { get; set; }

    /// <inheritdoc/>
    public BaseEntityProvider Build(IServiceProvider dependencyProvider)
    {
        return new DynamicEntityProvider(this.EntityType?.Build(dependencyProvider), this.EntityId?.Build(dependencyProvider), dependencyProvider);
    }
}
