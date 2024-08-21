// <copyright file="EntityObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Entity;

    /// <summary>
    /// This class is needed because we need to have a functionality for converting entity to an object representation of an entity.
    /// </summary>
    public class EntityObjectProvider : IObjectProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityObjectProvider"/> class.
        /// </summary>
        /// <param name="entity">The entity provider.</param>
        public EntityObjectProvider(IEntityProvider entity)
        {
            this.Entity = entity;
            this.RelatedEntities = new List<RelatedEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityObjectProvider"/> class.
        /// </summary>
        /// <param name="entity">The entity provider.</param>
        /// <param name="relatedEntities">The related entities with child.</param>
        public EntityObjectProvider(IEntityProvider entity, List<RelatedEntity> relatedEntities)
        {
            this.Entity = entity;
            this.RelatedEntities = relatedEntities;
        }

        public string SchemaReferenceKey => "entityObject";

        /// <summary>
        /// Gets the entity to read.
        /// </summary>
        private IEntityProvider Entity { get; }

        /// <summary>
        /// Gets or sets the related child entities to read.
        /// </summary>
        private List<RelatedEntity> RelatedEntities { get; set; }

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(EntityObjectProvider) + "." + nameof(this.Resolve)))
            {
                var properties = new Dictionary<string, object>();
                var resolveEntity = (await this.Entity.Resolve(providerContext)).GetValueOrThrowIfFailed();
                var entity = resolveEntity.DataValue;
                if (entity == null)
                {
                    return ProviderResult<Data<object>>.Success(
                        new Data<object>(new ReadOnlyDictionary<string, object>(properties)));
                }

                foreach (var relatedEntity in this.RelatedEntities)
                {
                    await entity.Include(relatedEntity, providerContext);
                }

                properties = entity.ToReadOnlyDictionary(this.RelatedEntities, this.Entity.IncludedProperties);
                return ProviderResult<Data<object>>.Success(
                    new Data<object>(new ReadOnlyDictionary<string, object>(properties)));
            }
        }
    }
}
