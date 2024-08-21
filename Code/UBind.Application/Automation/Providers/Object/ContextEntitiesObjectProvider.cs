// <copyright file="ContextEntitiesObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is responsible for generating a dictionary object containing a property for each of the specified context objects,
    /// where the value is a data object representation of that entity.
    /// </summary>
    public class ContextEntitiesObjectProvider : IObjectProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextEntitiesObjectProvider"/> class.
        /// </summary>
        /// <param name="entityPaths">The list of paths of entities from the context which should be included in the result object.</param>
        public ContextEntitiesObjectProvider(
            IEnumerable<IProvider<Data<string>>> entityPaths)
        {
            this.ContextEntityPaths = entityPaths;
        }

        public string SchemaReferenceKey => "contextEntitiesObject";

        /// <summary>
        /// Gets the list of JSON pointers identifying properties (or hierarchies of properties) within the automation context,
        /// that should be included in the result object.
        /// </summary>
        private IEnumerable<IProvider<Data<string>>> ContextEntityPaths { get; } = Enumerable.Empty<IProvider<Data<string>>>();

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(ContextEntitiesObjectProvider) + "." + nameof(this.Resolve)))
            {
                Dictionary<string, object> dataObject = new Dictionary<string, object>();
                var paths = await this.ContextEntityPaths.SelectAsync(async path => await path.Resolve(providerContext));
                IEnumerable<string> contextPaths = paths.Select(path => path.GetValueOrThrowIfFailed().DataValue);
                foreach (var path in contextPaths)
                {
                    if (!PathHelper.IsValidJsonPointer(path))
                    {
                        var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                        errorData.Add("path", path);
                        throw new ErrorException(Errors.Automation.PathSyntaxError(
                                "Path uses invalid pointer syntax", this.SchemaReferenceKey, errorData));
                    }

                    var pathSegments = path.Split(new char[] { '/' }, options: System.StringSplitOptions.RemoveEmptyEntries);
                    var baseParentEntity = pathSegments.First();
                    if (dataObject.ContainsKey(baseParentEntity))
                    {
                        // Already resolved - continue to next iteration.
                        continue;
                    }

                    // Look for any paths in contextPaths with the given entity within the path.
                    var relatedPaths = contextPaths.Where(x => x.Contains($"/{baseParentEntity}/"));
                    var segmentData =
                        await providerContext.AutomationData.GetValue<object>($"/context/{baseParentEntity}", providerContext, relatedPaths);

                    // Check if entity is part of context and if so, include it in output. Otherwise, skip.
                    if (segmentData != default)
                    {
                        var isEntityType = typeof(IEntity).IsAssignableFrom(segmentData.GetType());

                        // Check if related entities need to be resolved.
                        if (isEntityType && (pathSegments.Length > 1 || relatedPaths.Any()))
                        {
                            var entityData = await LoadRelatedEntities(
                                segmentData, providerContext, path, pathSegments, baseParentEntity, relatedPaths);
                            dataObject[baseParentEntity] = entityData;
                        }
                        else
                        {
                            dataObject[baseParentEntity] = isEntityType ? segmentData.ToReadOnlyDictionary() : segmentData;
                        }
                    }
                }

                return ProviderResult<Data<object>>.Success(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject)));
            }
        }

        private static async Task<object> LoadRelatedEntities(
            object objEntity,
            IProviderContext providerContext,
            string path,
            string[] pathSegments,
            string baseParentEntity,
            IEnumerable<string> relatedPaths)
        {
            using (MiniProfiler.Current.Step(nameof(ContextEntitiesObjectProvider) + "." + nameof(LoadRelatedEntities)))
            {
                var objType = objEntity.GetType();
                PropertyInfo propertyInfo = objType.GetProperty(
                    "id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                object entityId = propertyInfo.GetValue(objEntity);
                var parentEntityIdBuilder = new StaticBuilder<Data<string>>() { Value = entityId.ToString() };
                var parentEntityType = new StaticBuilder<Data<string>>() { Value = objType.Name.ToCamelCase() };
                var dynamicEntityProviderConfig = new DynamicEntityProviderConfigModel() { EntityId = parentEntityIdBuilder, EntityType = parentEntityType };

                var paths = pathSegments.Length > 1 ? relatedPaths.Append(path) : relatedPaths;

                var relatedEntityPath = PathHelper.GetChildPaths(baseParentEntity, paths);
                var entityObjectProviderModel = new EntityObjectProviderConfigModel()
                {
                    Entity = dynamicEntityProviderConfig,
                    IncludeOptionalProperties = relatedEntityPath,
                };
                var entityObjectProvider = entityObjectProviderModel.Build(providerContext.AutomationData.ServiceProvider);
                var entity = await entityObjectProvider.Resolve(providerContext);
                return entity.GetValueOrThrowIfFailed().DataValue;
            }
        }
    }
}
