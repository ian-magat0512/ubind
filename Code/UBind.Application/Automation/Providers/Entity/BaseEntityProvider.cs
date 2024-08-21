// <copyright file="BaseEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CSharpFunctionalExtensions;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a base class that will contain common properties and method for all entity providers.
    /// </summary>
    public abstract class BaseEntityProvider : IEntityProvider
    {
        protected string? resolvedEntityId;
        protected string? resolvedEntityType;

        /// <param name="id">The entity Id.</param>
        /// <param name="schemaReferenceKey">The schema reference key that maps this provider in the schema.</param>
        protected BaseEntityProvider(
            IProvider<Data<string>>? id,
            string schemaReferenceKey)
        {
            this.EntityId = id;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        protected BaseEntityProvider(
            string? id,
            string schemaReferenceKey)
        {
            this.resolvedEntityId = id;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        public IProvider<Data<string>>? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the properties to include.
        /// </summary>
        public List<string> IncludedProperties { get; set; } = new List<string>();

        /// <summary>
        /// Gets the schema reference key of this provider.
        /// </summary>
        /// <remarks>Schema reference key should be obtained from the entity-specific implementation.</remarks>
        public string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public abstract ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext);

        public void SetResolvedEntityId(string entityId)
        {
            this.resolvedEntityId = entityId;
        }

        public void SetResolvedEntityType(string entityType)
        {
            this.resolvedEntityType = entityType;
        }

        /// <summary>
        /// Get the list of properties that will be included in the entity.
        /// </summary>
        /// <param name="type">The type of entity.</param>
        /// <returns>The list of properties that will be included in the entity.</returns>
        protected List<string> GetPropertiesToInclude(Type type)
        {
            if (this.IncludedProperties?.Any() != true)
            {
                return new List<string>();
            }

            var includeRelatedEntities = new List<string>();
            foreach (var propertyName in this.IncludedProperties)
            {
                var property = type.GetProperties()
                    .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null
                        && p.GetCustomAttribute<JsonPropertyAttribute>() != null)
                    .FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName?
                        .EqualsIgnoreCase(propertyName) ?? false);

                if (!string.IsNullOrWhiteSpace(property?.Name))
                {
                    includeRelatedEntities.Add(property.Name);
                }
            }

            return includeRelatedEntities;
        }

        protected DeploymentEnvironment GetEnvironment(
            string? providerEnvironment, IProviderContext providerContext)
        {
            var environment = DeploymentEnvironment.None;
            if (!string.IsNullOrWhiteSpace(providerEnvironment))
            {
                Enum.TryParse(providerEnvironment, true, out environment);
            }

            if (environment == DeploymentEnvironment.None)
            {
                environment = providerContext.AutomationData.System.Environment;
            }

            return environment;
        }
    }
}
