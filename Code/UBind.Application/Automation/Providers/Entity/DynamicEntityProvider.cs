// <copyright file="DynamicEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Collections.Generic;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching an entity by type and Id.
    /// </summary>
    public class DynamicEntityProvider : BaseEntityProvider
    {
        private readonly IServiceProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEntityProvider"/> class.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="entityProviders">The list of entity providers.</param>
        public DynamicEntityProvider(
            IProvider<Data<string>> type,
            IProvider<Data<string>> entityId,
            IServiceProvider dependencyProvider)
            : base(entityId, "dynamicEntity")
        {
            this.EntityType = type;
            this.dependencyProvider = dependencyProvider;
        }

        /// <summary>
        /// Gets or sets the entity type.
        /// </summary>
        private IProvider<Data<string>> EntityType { get; set; }

        /// <summary>
        /// Method for retrieving dynamic entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The entity object.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityType = this.resolvedEntityType ?? (await this.EntityType.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;

            if (string.IsNullOrWhiteSpace(this.resolvedEntityType))
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.NoEntityType(
                    await this.GetErrorData(providerContext)));
            }
            else if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                var entityReferenceType = $"{this.resolvedEntityType.ToString().ToCamelCase()}Id";
                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    this.resolvedEntityType, entityReferenceType, string.Empty, await this.GetErrorData(providerContext)));
            }

            try
            {
                var entityProvider = await EntityProviderFactory.Create(
                    this.resolvedEntityType,
                    this.resolvedEntityId,
                    this.dependencyProvider,
                    () => this.GetErrorData(providerContext));

                entityProvider.SetResolvedEntityId(this.resolvedEntityId);
                entityProvider.SetResolvedEntityType(this.resolvedEntityType);
                entityProvider.IncludedProperties = this.IncludedProperties;
                return await entityProvider.Resolve(providerContext);
            } // Catches unsupported existing entity types.
            catch (KeyNotFoundException)
            {
                var errorData = await this.GetErrorData(providerContext);
                errorData.Add(ErrorDataKey.EntityType, this.resolvedEntityType);
                throw new ErrorException(Errors.Automation.Provider.Entity.TypeNotSupported(
                    this.resolvedEntityType, errorData));
            }
        }

        private async Task<JObject> GetErrorData(IProviderContext providerContext)
        {
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.EntityType, this.resolvedEntityType ?? "null");
            errorData.Add(ErrorDataKey.EntityId, this.resolvedEntityId ?? "null");
            return errorData;
        }
    }
}
