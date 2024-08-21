// <copyright file="EntityObjectListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a functionality for converting list of entities to an object representation of the list of entities.
    /// </summary>
    public class EntityObjectListProvider : IDataListProvider<object>
    {
        private readonly IDataListProvider<object> entityListProvider;
        private readonly List<RelatedEntity> relatedEntities;
        private readonly ISerialisedEntityFactory serialisedEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityObjectListProvider"/> class.
        /// </summary>
        /// <param name="relatedEntities">The related entities with child.</param>
        public EntityObjectListProvider(IDataListProvider<object> entityListProvider, List<RelatedEntity> relatedEntities, ISerialisedEntityFactory serialisedEntityFactory)
        {
            this.serialisedEntityFactory = serialisedEntityFactory;
            this.entityListProvider = entityListProvider;
            this.relatedEntities = relatedEntities;
        }

        public string SchemaReferenceKey => "entityObjectList";

        public List<string> IncludedProperties { get; set; }

        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            this.IncludedProperties = this.entityListProvider.IncludedProperties;
            var entities = new List<IEntity>();
            var entityListResolver = (await this.entityListProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var entityList = entityListResolver?.ToList();
            if (entityList == null || !entityList.Any())
            {
                return ProviderResult<IDataList<object>>.Success(new GenericDataList<object>(entities));
            }

            foreach (var entity in entityList)
            {
                if (entity is IEntityWithRelatedEntities typedEntity)
                {
                    providerContext.CancellationToken.ThrowIfCancellationRequested();
                    var entityObject = await this.serialisedEntityFactory.Create(typedEntity, this.IncludedProperties);
                    foreach (var relatedEntity in this.relatedEntities)
                    {
                        await entityObject.Include(relatedEntity, providerContext);
                    }

                    entities.Add(entityObject);
                }
                else
                {
                    throw new InvalidOperationException("When using the EntityObjectListProvider to get a list of "
                        + "entities, the type of entity returned was not an instance of IEntityWithRelatedEntities. ");
                }
            }

            return ProviderResult<IDataList<object>>.Success(new GenericDataList<object>(entities));
        }
    }
}
