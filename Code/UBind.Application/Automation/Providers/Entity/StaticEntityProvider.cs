// <copyright file="StaticEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity;

public abstract class StaticEntityProvider : BaseEntityProvider
{
    /// <param name="id">The entity Id.</param>
    /// <param name="schemaReferenceKey">The schema reference key that maps this provider in the schema.</param>
    protected StaticEntityProvider(
        IProvider<Data<string>>? id,
        ISerialisedEntityFactory serialisedEntityFactory,
        string schemaReferenceKey)
        : base(id, schemaReferenceKey)
    {
        this.SerialisedEntityFactory = serialisedEntityFactory;
    }

    protected StaticEntityProvider(
        string? id,
        ISerialisedEntityFactory serialisedEntityFactory,
        string schemaReferenceKey)
        : base(id, schemaReferenceKey)
    {
        this.SerialisedEntityFactory = serialisedEntityFactory;
    }

    /// <summary>
    /// Gets the serialization manager.
    /// </summary>
    protected ISerialisedEntityFactory SerialisedEntityFactory { get; }
}
