// <copyright file="StaticListProvider.cs" company="uBind">
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

    /// <summary>
    /// Provider for resolving static generic-typed lists.
    /// </summary>
    /// <typeparam name="TValue">The type of the items in the list.</typeparam>
    public class StaticListProvider<TValue> : IDataListProvider<TValue>
    {
        private readonly IDataList<TValue> list;
        private readonly IServiceProvider dependencyProvider;

        public StaticListProvider(IDataList<TValue> listValue, IServiceProvider serviceProvider)
        {
            this.list = listValue;
            this.dependencyProvider = serviceProvider;
        }

        public List<string> IncludedProperties { get; set; }

        public string SchemaReferenceKey => "#list";

        /// <inheritdoc/>
        public async ITask<IProviderResult<IDataList<TValue>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            List<object> resolvedList = new List<object>();
            foreach (var x in this.list)
            {
                if (x == null)
                {
                    resolvedList.Add(x);
                    continue;
                }

                var itemType = x.GetType();
                var interfaces = itemType.GetInterfaces();
                var hasBuilderInterface = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBuilder<>));
                if (hasBuilderInterface)
                {
                    var builderType = interfaces.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBuilder<>));
                    var genericArgs = builderType.GetGenericArguments()[0];
                    IBuilder<IProvider<IData>> builderModel = x as IBuilder<IProvider<IData>>;
                    var provider = builderModel.Build(this.dependencyProvider);
                    var providerValue = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    resolvedList.Add((TValue)providerValue?.GetValueFromGeneric());
                }
                else
                {
                    resolvedList.Add(x);
                }
            }

            return ProviderResult<IDataList<TValue>>.Success(new GenericDataList<object>(resolvedList) as IDataList<TValue>);
        }
    }
}
