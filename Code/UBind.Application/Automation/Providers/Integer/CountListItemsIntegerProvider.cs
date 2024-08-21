// <copyright file="CountListItemsIntegerProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Integer
{
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// For providing entity collectionsn queried from the database.
    /// </summary>
    public class CountListItemsIntegerProvider : IProvider<Data<long>>
    {
        private readonly IDataListProvider<object> collectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountListItemsIntegerProvider"/> class.
        /// </summary>
        /// <param name="collectionProvider">A provider for the collection to be filtered.</param>
        public CountListItemsIntegerProvider(IDataListProvider<object> collectionProvider)
        {
            this.collectionProvider = collectionProvider;
        }

        public string SchemaReferenceKey => "countListItemsInteger";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<long>>> Resolve(IProviderContext providerContext)
        {
            var collection = (await this.collectionProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            return ProviderResult<Data<long>>.Success((long)collection.Count());
        }
    }
}
