// <copyright file="ObjectPathLookupExpressionProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Converter for deserializing object path lookup expression provider config models.
    /// </summary>
    public class ObjectPathLookupExpressionProviderConfigModelConverter : DeserializationConverter
    {
        private static readonly Dictionary<Type, Func<IBuilder<IProvider<Data<string>>>, IBuilder<IProvider<IData>>, IBuilder<IObjectProvider>, object>> BuilderFactoriesByType =
            new Dictionary<Type, Func<IBuilder<IProvider<Data<string>>>, IBuilder<IProvider<IData>>, IBuilder<IObjectProvider>, object>>
            {
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<IEnumerable>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<IDataList<object>>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<long>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<long>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<decimal>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<decimal>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<string>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<string>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupConditionFilterProviderConfigModel),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModelForFilter(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<LocalDate>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<LocalDate>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<LocalTime>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<LocalTime>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
                {
                    typeof(ObjectPathLookupExpressionProviderConfigModel<object>),
                    (pathProviderModel, defaultValueProviderModel, dataObjectProvider) => CreatePathLookupConfigModel<object>(pathProviderModel, defaultValueProviderModel, dataObjectProvider)
                },
            };

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => BuilderFactoriesByType.ContainsKey(objectType);

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IBuilder<IProvider<Data<string>>> pathStringProviderModel = null;
            IBuilder<IProvider<IData>> defaultValueProviderModel = null;
            IBuilder<IObjectProvider> dataObjectProviderModel = null;
            if (reader.TokenType == JsonToken.String)
            {
                pathStringProviderModel = serializer.Deserialize(reader, typeof(IBuilder<IProvider<Data<string>>>)) as IBuilder<IProvider<Data<string>>>;
            }
            else
            {
                var jObject = JObject.Load(reader);
                var properties = jObject.Properties().Select(na => na.Name).ToList();
                if (properties.Count == 1)
                {
                    // this could be a string provider for the path.
                    pathStringProviderModel = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(jObject.CreateReader());
                }
                else
                {
                    pathStringProviderModel = jObject["path"].ToObject<IBuilder<IProvider<Data<string>>>>(serializer);
                    dataObjectProviderModel = jObject["dataObject"]?.ToObject<IBuilder<IObjectProvider>>(serializer);
                    Type innerType = objectType.GetGenericArguments().Length > 0 ? objectType.GetGenericArguments()[0] : typeof(bool);
                    defaultValueProviderModel = this.ParseDefaultValueProvider(jObject, innerType, serializer);
                }
            }

            BuilderFactoriesByType.TryGetValue(objectType, out Func<IBuilder<IProvider<Data<string>>>, IBuilder<IProvider<IData>>, IBuilder<IObjectProvider>, object> factoryMethod);
            if (factoryMethod != null)
            {
                return factoryMethod.Invoke(pathStringProviderModel, defaultValueProviderModel, dataObjectProviderModel);
            }

            return null;
        }

        private static IBuilder<IExpressionProvider> CreatePathLookupConfigModel<TData>(
            IBuilder<IProvider<Data<string>>> pathProviderModel,
            IBuilder<IProvider<IData>> defaultValueProviderModel = null,
            IBuilder<IObjectProvider> dataObjectProvider = null) =>
            new ObjectPathLookupExpressionProviderConfigModel<TData>
            {
                Path = pathProviderModel,
                ValueIfNotFound = defaultValueProviderModel,
                DataObject = dataObjectProvider,
            };

        private static IBuilder<IFilterProvider> CreatePathLookupConfigModelForFilter(
            IBuilder<IProvider<Data<string>>> pathProviderModel,
            IBuilder<IProvider<IData>> defaultValueProviderModel = null,
            IBuilder<IObjectProvider> dataObjectProvider = null) =>
            new ObjectPathLookupConditionFilterProviderConfigModel(
                pathProviderModel,
                defaultValueProviderModel,
                dataObjectProvider);

        private IBuilder<IProvider<IData>> ParseDefaultValueProvider(JObject jObject, Type pathLookupType, JsonSerializer serializer)
        {
            var defaultValueToken = jObject["valueIfNotFound"];
            if (defaultValueToken != null)
            {
                var tokenReader = defaultValueToken.CreateReader();
                switch (defaultValueToken.Type)
                {
                    case JTokenType.Object:
                        return serializer.Deserialize<IBuilder<IProvider<IData>>>(tokenReader);
                    case JTokenType.Array:
                        if (pathLookupType == typeof(IEnumerable))
                        {
                            return serializer.Deserialize<IBuilder<IDataListProvider<object>>>(tokenReader);
                        }
                        else
                        {
                            return serializer.Deserialize<IBuilder<IProvider<IData>>>(tokenReader);
                        }

                    case JTokenType.Integer:
                        return serializer.Deserialize<IBuilder<IProvider<Data<long>>>>(tokenReader);
                    case JTokenType.Float:
                        return serializer.Deserialize<IBuilder<IProvider<Data<decimal>>>>(tokenReader);
                    case JTokenType.String:
                        var serializedTypes = new Dictionary<Type, Func<IBuilder<IProvider<IData>>>>
                        {
                            { typeof(LocalDateTime), () => serializer.Deserialize<IBuilder<IProvider<Data<Instant>>>>(tokenReader) },
                            { typeof(LocalDate), () => serializer.Deserialize<IBuilder<IProvider<Data<Instant>>>>(tokenReader) },
                            { typeof(LocalTime), () => serializer.Deserialize<IBuilder<IProvider<Data<LocalTime>>>>(tokenReader) },
                        };
                        if (serializedTypes.TryGetValue(pathLookupType, out Func<IBuilder<IProvider<IData>>> factoryMethod))
                        {
                            return factoryMethod.Invoke();
                        }
                        else
                        {
                            return serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(tokenReader);
                        }

                    case JTokenType.Boolean:
                        return serializer.Deserialize<IBuilder<IProvider<Data<bool>>>>(tokenReader);
                }

                throw new NotSupportedException($"valueIfNotFound value type {defaultValueToken.Type} is not supported.");
            }

            return null;
        }
    }
}
