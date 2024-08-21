// <copyright file="ISerialisedEntityFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// Interface for a factory which creates instances of automation serialised entities from instances of
    /// IEntityWithRelatedEntities.
    /// </summary>
    public interface ISerialisedEntityFactory
    {
        /// <summary>
        /// Method for creating a serialised entity from an instance of IEntityWithRelatedEntities.
        /// </summary>
        Task<IEntity> Create(IEntityWithRelatedEntities model, IEnumerable<string> includedProperties);
    }
}
