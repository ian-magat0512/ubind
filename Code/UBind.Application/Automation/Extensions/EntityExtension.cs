// <copyright file="EntityExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain.Extensions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need convert the serialized entity to dictionary of properties that will be used in JsonTextProvider.
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// Extension method for converting serialized entity to read only dictionary.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="nestedRelatedEntities">The list of nested related entities to include.</param>
        /// <param name="includedProperties">The list of properties to include.</param>
        /// <returns>Read only dictionary that contains all the properties of the entity.</returns>
        public static Data<Dictionary<string, object>> ToReadOnlyDictionary(
            this IEntity entity, List<RelatedEntity> nestedRelatedEntities, List<string>? includedProperties = null)
        {
            var relatedEntities = nestedRelatedEntities.Select(c => c.PropertyName).ToList();
            if (includedProperties?.Any() == true)
            {
                relatedEntities.AddRange(includedProperties);
            }

            return GetProperties(entity, relatedEntities)
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Select(p =>
                {
                    var propertyName = ParsePropertyName().Invoke(p);
                    var relatedEntity = nestedRelatedEntities.FirstOrDefault(c => c.PropertyName == propertyName);
                    var propertyValue = GetSerializableValue(p.GetValue(entity, null), relatedEntity);
                    return new KeyValuePair<string, object>(propertyName, propertyValue);
                })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<string, object> ToReadOnlyDictionary(this object obj)
        {
            var relatedEntities = new List<string>();
            var properties = GetProperties(obj, relatedEntities).ToList();
            return properties.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Select(p =>
                {
                    var propertyName = p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;
                    var propertyValue = GetSerializableValue(p.GetValue(obj, null), null);
                    return new KeyValuePair<string, object>(propertyName, propertyValue);
                })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Method for including child entity as property to parent entity.
        /// </summary>
        /// <param name="parentEntity">The parent entity.</param>
        /// <param name="entity">The related entity to include.</param>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        public static async Task Include(this IEntity parentEntity, RelatedEntity entity, IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(EntityExtension) + "." + nameof(Include)))
            {
                PropertyInfo entityProperty = GetProperty(parentEntity, entity.PropertyName);
                var entityPropertyValue = entityProperty?.GetValue(parentEntity, null);
                if (entityPropertyValue == null)
                {
                    return;
                }

                if (DataObjectHelper.IsArray(entityPropertyValue))
                {
                    // If it's a List<T> then we want to know the type of T.
                    string entityTypeName = entityProperty.PropertyType.GetGenericArguments().First().Name;

                    // If the property to include is a list of entity like versions, emails, documents
                    // and we specify e.g. '/versions/organisation'
                    var items = (IList)entityPropertyValue;
                    for (var i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var dynamicEntityProvider = entity.EntityProvider;
                        var entityId = GetPropertyValue(item, nameof(IEntity.Id))?.ToString();
                        dynamicEntityProvider.SetResolvedEntityId(entityId);
                        dynamicEntityProvider.SetResolvedEntityType(entityTypeName);
                        var relatedEntity = (await dynamicEntityProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                        foreach (var child in entity.ChildEntities)
                        {
                            await relatedEntity.Include(child, providerContext);
                        }

                        items[i] = relatedEntity;
                    }

                    SetPropertyValue(parentEntity, entity.PropertyName, items);
                }
                else
                {
                    // if the property to include is an entity like policy, product tenant
                    // and we specify e.g. '/policy/product'
                    string entityTypeName = entityProperty.PropertyType.Name;
                    var dynamicEntityProvider = entity.EntityProvider;
                    var entityId = GetPropertyValue(entityPropertyValue, nameof(IEntity.Id))?.ToString();
                    dynamicEntityProvider.SetResolvedEntityId(entityId);
                    dynamicEntityProvider.SetResolvedEntityType(entityTypeName);
                    var relatedEntity = (await dynamicEntityProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                    foreach (var child in entity.ChildEntities)
                    {
                        await relatedEntity.Include(child, providerContext);
                    }

                    SetPropertyValue(parentEntity, entity.PropertyName, relatedEntity);
                }
            }
        }

        private static object GetSerializableValue(object obj, RelatedEntity relatedEntity)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();
            if (typeof(IEntity).IsAssignableFrom(type))
            {
                var relatedEntities = relatedEntity?.EntityProvider.IncludedProperties ?? new List<string>();
                return GetProperties(obj, relatedEntities)
                    .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                    .Select(p =>
                    {
                        var propertyName = ParsePropertyName().Invoke(p);
                        var childEntity = relatedEntity?.ChildEntities.FirstOrDefault(c => c.PropertyName == propertyName);
                        var propertyValue = GetSerializableValue(p.GetValue(obj, null), childEntity);
                        return new KeyValuePair<string, object>(propertyName, propertyValue);
                    })
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            var itemType = type.GetGenericArguments()?.FirstOrDefault();
            if (itemType != null && typeof(ISchemaObject).IsAssignableFrom(itemType))
            {
                var listItems = new List<Dictionary<string, object>>();
                foreach (var listitem in obj as IEnumerable<ISchemaObject>)
                {
                    var relatedEntities = relatedEntity?.EntityProvider.IncludedProperties ?? new List<string>();
                    var item = GetProperties(listitem, relatedEntities)
                        .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                        .Select(p =>
                        {
                            var propertyName = ParsePropertyName().Invoke(p);
                            var childEntity =
                                relatedEntity?.ChildEntities.FirstOrDefault(c => c.PropertyName == propertyName);
                            var propertyValue = GetSerializableValue(p.GetValue(listitem, null), childEntity);
                            return new KeyValuePair<string, object>(propertyName, propertyValue);
                        })
                        .ToDictionary(x => x.Key, x => x.Value);
                    listItems.Add(item);
                }

                return listItems;
            }

            if (obj is JObject)
            {
                return obj;
            }

            if (typeof(ISchemaObject).IsAssignableFrom(obj.GetType()))
            {
                return obj.ToReadOnlyDictionary();
            }

            return obj;
        }

        private static IOrderedEnumerable<PropertyInfo> GetProperties(object obj, List<string> relatedEntities)
        {
            var properties = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null);
            var filteredProperties = properties.Where(p =>
                {
                    var value = p.GetValue(obj, null);
                    var isEmptyList = false;
                    if (value != null)
                    {
                        if (typeof(IEnumerable<object>).IsAssignableFrom(value.GetType()))
                        {
                            var items = (IEnumerable<object>)value;
                            isEmptyList = !items.Any();
                        }
                    }

                    var defaultValue = p.GetCustomAttribute<DefaultValueAttribute>();
                    var excludeDefaultValue = defaultValue != null && defaultValue.Value.Equals(value);
                    var isRequired = p.GetCustomAttribute<RequiredAttribute>() != null;
                    var propertyName = ParsePropertyName().Invoke(p);
                    var hasProperty = ((value != null || isRequired) && !isEmptyList) ||
                        relatedEntities.Any(a => a.EqualsIgnoreCase(propertyName));
                    return hasProperty && !excludeDefaultValue;
                });
            return filteredProperties.OrderBy(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.Order);
        }

        private static PropertyInfo GetProperty(object obj, string propertyName)
        {
            var objType = obj.GetType();
            return objType.GetProperties().FirstOrDefault(p =>
            {
                var parsedPropertyName = ParsePropertyName().Invoke(p);
                return parsedPropertyName.EqualsIgnoreCase(propertyName);
            });
        }

        private static object GetPropertyValue(object obj, string propertyName) => GetProperty(obj, propertyName)?.GetValue(obj, null);

        private static void SetPropertyValue(object obj, string propertyName, object value) => GetProperty(obj, propertyName)?.SetValue(obj, value);

        private static Func<MemberInfo, string> ParsePropertyName()
        {
            return p =>
            {
                var jsonPropertyName = p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
                return jsonPropertyName ?? p.Name.ToCamelCase();
            };
        }
    }
}
