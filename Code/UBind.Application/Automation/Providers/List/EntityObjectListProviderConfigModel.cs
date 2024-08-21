// <copyright file="EntityObjectListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Json.Pointer;
    using UBind.Application.Automation.Providers.Entity;

    /// <summary>
    /// Model for building an instance of <see cref="EntityObjectListProvider"/>.
    /// </summary>
    public class EntityObjectListProviderConfigModel : IBuilder<EntityObjectListProvider>
    {
        /// <summary>
        /// Gets or sets the entity object.
        /// </summary>
        public IBuilder<IDataListProvider<object>> EntityListProvider { get; set; }

        /// <summary>
        /// Gets or sets if related entities will be included in the object.
        /// </summary>
        public IEnumerable<string> IncludeOptionalProperties { get; set; }

        /// <inheritdoc/>
        public EntityObjectListProvider Build(IServiceProvider dependencyProvider)
        {
            var serialiserResolver = dependencyProvider.GetService<ISerialisedEntityFactory>();
            var entityListProvider = this.EntityListProvider?.Build(dependencyProvider);
            var relatedEntitiesWithChild = new List<RelatedEntity>();
            if (!this.IncludeOptionalProperties.Any())
            {
                return new EntityObjectListProvider(entityListProvider, relatedEntitiesWithChild, serialiserResolver);
            }

            var childEntityPropertyNames = this.GetChildEntityPropertyNames();
            relatedEntitiesWithChild = this.GetNestedRelatedEntities(dependencyProvider);
            entityListProvider.IncludedProperties = childEntityPropertyNames;

            return new EntityObjectListProvider(entityListProvider, relatedEntitiesWithChild, serialiserResolver);
        }

        private List<RelatedEntity> GetChildEntities(List<(int level, string name, List<string> relatedEntities)> includeRelatedEntitiesChild, int level, IServiceProvider dependencyProvider)
        {
            var primaryLevel = includeRelatedEntitiesChild.Where(c => c.level == level);
            var childEntities = new List<RelatedEntity>();
            foreach (var child in primaryLevel)
            {
                var relatedEntity = new RelatedEntity();
                var dynamicEntityProviderConfigModel = new DynamicEntityProviderConfigModel();
                var dynamicEntityProvider = (DynamicEntityProvider)dynamicEntityProviderConfigModel.Build(dependencyProvider);
                dynamicEntityProvider.IncludedProperties = child.relatedEntities;

                var jPointer = new JsonPointer(child.name);
                relatedEntity.PropertyName = jPointer.ReferenceTokens.Last();
                relatedEntity.EntityProvider = dynamicEntityProvider;
                relatedEntity.ChildEntities = this.GetChildEntities(includeRelatedEntitiesChild, level + 1, dependencyProvider);
                childEntities.Add(relatedEntity);
            }

            return childEntities;
        }

        private List<string> GetChildEntityPropertyNames()
        {
            var includeRelatedEntities = new List<string>();
            foreach (var relatedEntity in this.IncludeOptionalProperties)
            {
                var jPointer = new JsonPointer(relatedEntity);
                var propertyName = jPointer.ReferenceTokens.FirstOrDefault();
                if (propertyName != null && !includeRelatedEntities.Any(c => c == propertyName))
                {
                    includeRelatedEntities.Add(propertyName);
                }
            }

            return includeRelatedEntities;
        }

        private List<RelatedEntity> GetNestedRelatedEntities(IServiceProvider dependencyProvider)
        {
            var relatedEntities = new List<RelatedEntity>();
            var includeRelatedEntitiesChild = new List<(int level, string name, List<string> relatedEntities)>();
            foreach (var relatedEntity in this.IncludeOptionalProperties)
            {
                var jPointer = new JsonPointer(relatedEntity);
                if (jPointer.ReferenceTokens.Length > 1)
                {
                    for (var i = 1; i <= jPointer.ReferenceTokens.Length; i++)
                    {
                        var parentName = $"/{string.Join("/", jPointer.ReferenceTokens.Take(i))}";
                        var childPropertyName = jPointer.ReferenceTokens.Skip(i).FirstOrDefault();
                        if (childPropertyName != null)
                        {
                            var item = includeRelatedEntitiesChild.FirstOrDefault(c => c.name == parentName);
                            if (item == default)
                            {
                                var childProperties = new List<string>();
                                childProperties.Add(childPropertyName);
                                includeRelatedEntitiesChild.Add((i, parentName, childProperties));
                            }
                            else
                            {
                                var childProperties = item.relatedEntities;
                                childProperties.Add(childPropertyName);
                            }
                        }
                    }
                }
            }

            var primaryLevel = includeRelatedEntitiesChild.Where(c => c.level == 1);

            foreach (var child in primaryLevel)
            {
                var relatedEntity = new RelatedEntity();
                var dynamicEntityProviderConfigModel = new DynamicEntityProviderConfigModel();
                var dynamicEntityProvider = (DynamicEntityProvider)dynamicEntityProviderConfigModel.Build(dependencyProvider);
                dynamicEntityProvider.IncludedProperties = child.relatedEntities;

                var jPointer = new JsonPointer(child.name);
                relatedEntity.PropertyName = jPointer.ReferenceTokens.Last();
                relatedEntity.EntityProvider = dynamicEntityProvider;
                relatedEntity.ChildEntities = this.GetChildEntities(includeRelatedEntitiesChild, 2, dependencyProvider);
                relatedEntities.Add(relatedEntity);
            }

            return relatedEntities;
        }
    }
}
