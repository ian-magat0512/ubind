// <copyright file="FilterListItemsListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System.Collections.Generic;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// For providing lists by filtering existing lists.
    /// </summary>
    public class FilterListItemsListProvider : IDataListProvider<object>
    {
        private readonly IDataListProvider<object> collectionProvider;
        private readonly IFilterProvider filterProvider;
        private readonly IProvider<Data<string>>? itemAliasProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterListItemsListProvider"/> class.
        /// </summary>
        /// <param name="collectionProvider">A provider for the collection to be filtered.</param>
        /// <param name="filterProvider">A provider for the filter.</param>
        /// <param name="itemAliasProvider">A provider for the string to use as the item alias in filters.</param>
        public FilterListItemsListProvider(
            IDataListProvider<object> collectionProvider,
            IFilterProvider filterProvider,
            IProvider<Data<string>>? itemAliasProvider = null)
        {
            this.collectionProvider = collectionProvider;
            this.filterProvider = filterProvider;
            this.itemAliasProvider = itemAliasProvider;
        }

        /// <inheritdoc/>
        public List<string> IncludedProperties { get; set; } = new List<string>();

        public string SchemaReferenceKey => "filterListItemsList";

        /// <inheritdoc/>
        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            this.collectionProvider.IncludedProperties = this.IncludedProperties;
            IDataList<object> collection = (await this.collectionProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var itemAlias = (await this.itemAliasProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            ITask<IDataList<object>> filter = collection.Where(itemAlias, this.filterProvider, providerContext);

            // Need to do like this since the collection was returning a generic type cause of IProviderResult.
            // This is a workaround to get the result of the filter. There an exception RuntimeBinderException if you await directly the collection at line 52.
            var result = await filter;
            return ProviderResult<IDataList<object>>.Success(result);
        }
    }
}