// <copyright file="DynamicObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Dynamically provides a generated data object from a list of properties.
    /// </summary>
    public class DynamicObjectProvider : IObjectProvider
    {
        private readonly ILogger<DynamicObjectProvider> logger;
        private readonly IReadOnlyDictionary<IProvider<Data<string>>, IProvider<IData>?> properties;
        private readonly IServiceProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObjectProvider"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public DynamicObjectProvider(
            IReadOnlyDictionary<IProvider<Data<string>>, IProvider<IData>?> properties,
            ILogger<DynamicObjectProvider> logger,
            IServiceProvider dependencyProvider)
        {
            this.logger = logger;
            this.properties = properties;
            this.dependencyProvider = dependencyProvider;
        }

        public string SchemaReferenceKey => "#object";

        /// <summary>
        /// Provides an object.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>An object.</returns>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            var dataDictionary = new Dictionary<string, object>();
            foreach (var property in this.properties)
            {
                var propertyKey = (await property.Key.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                this.logger.LogInformation("Executing \"DynamicObjectProvider\" " + propertyKey);
                var valueProvider = property.Value;
                var value = (await valueProvider.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
                dataDictionary.Add(propertyKey, await this.ResolveInnerValues(value, providerContext));
            }

            var data = new Data<object>(new ReadOnlyDictionary<string, object>(dataDictionary));
            return ProviderResult<Data<object>>.Success(data);
        }

        private async Task<object> ResolveInnerValues(dynamic valueFromGeneric, IProviderContext providerContext)
        {
            if (DataObjectHelper.IsArray(valueFromGeneric))
            {
                var resolvedList = new List<object>();
                foreach (var valueItem in valueFromGeneric)
                {
                    var genericProviderBuilder = valueItem as IBuilder<IProvider<IData>>;
                    if (genericProviderBuilder != null)
                    {
                        var provider = genericProviderBuilder.Build(this.dependencyProvider);
                        var value = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                        resolvedList.Add(value.GetValueFromGeneric());
                    }
                    else
                    {
                        resolvedList.Add(valueItem);
                    }
                }

                return resolvedList;
            }
            else
            {
                return valueFromGeneric;
            }
        }
    }
}
