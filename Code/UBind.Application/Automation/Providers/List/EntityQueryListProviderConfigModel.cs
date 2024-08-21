// <copyright file="EntityQueryListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;

    /// <summary>
    /// Model for building an entity collection provider.
    /// </summary>
    public class EntityQueryListProviderConfigModel : IBuilder<IDataListProvider<object>>
    {
        /// <summary>
        /// Gets or sets the model for building the entity type provider.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> EntityType { get; set; }

        /// <summary>
        /// Gets or sets page size for result.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<long>>> PageSize { get; set; }

        /// <summary>
        /// Gets or sets the current page number needed from the paginated result.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<long>>> PageNumber { get; set; }

        /// <inheritdoc/>
        public IDataListProvider<object> Build(IServiceProvider dependencyProvider)
        {
            var queryService = (IEntityQueryService)dependencyProvider.GetService(typeof(IEntityQueryService));
            var entityTypeProvider = this.EntityType.Build(dependencyProvider);
            var pageSizeProvider = this.PageSize != null ? this.PageSize.Build(dependencyProvider) : null;
            var pageNumberProvider = this.PageNumber != null ? this.PageNumber.Build(dependencyProvider) : null;
            var cachingResolver = (ICachingResolver)dependencyProvider.GetService(typeof(ICachingResolver));
            return new EntityQueryListProvider(queryService, entityTypeProvider, cachingResolver, pageSizeProvider, pageNumberProvider);
        }
    }
}
