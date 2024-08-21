// <copyright file="PathLookupDerivativeConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ServiceStack;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// This is a base class for all of the path lookup provider deriviations in the system.
    /// </summary>
    public abstract class PathLookupDerivativeConverter : DeserializationConverter
    {
        protected string ValueIfNotFoundProperty { get; } = "valueIfNotFound";

        protected string RaiseErrorIfNotFoundProperty { get; } = "raiseErrorIfNotFound";

        protected string RaiseErrorIfNullProperty { get; } = "raiseErrorIfNull";

        protected string ValueIfNullProperty { get; } = "valueIfNull";

        protected string RaiseErrorIfTypeMismatchProperty { get; } = "raiseErrorIfTypeMismatch";

        protected string ValueIfTypeMismatchProperty { get; } = "valueIfTypeMismatch";

        protected string DefaultValueProperty { get; } = "defaultValue";

        /// <summary>
        /// Creates the lookup model builder out of the given json configuration.
        /// </summary>
        /// <param name="readerValue">The json reader's current value, if available.</param>
        /// <param name="jObject">The json object derived from the json reader, should the value of the reader be null.</param>
        /// <param name="objectType">The type of object to create.</param>
        /// <param name="serializer">The json serializer to be used in reference to creating the model.</param>
        /// <param name="derivationSchemaReferenceKey">The schema reference key of the path lookup derivative being built.</param>
        /// <returns>An instance of the required object path lookup provider.</returns>
        protected IBuilder<IObjectPathLookupProvider> CreateLookupBuilder(object readerValue, JObject jObject, Type objectType, JsonSerializer serializer, string derivationSchemaReferenceKey)
        {
            if (readerValue != null)
            {
                var pathProviderConfigModel = new StaticBuilder<Data<string>> { Value = readerValue.ToString() };
                return new FixedObjectPathLookupConfigModel { Path = pathProviderConfigModel, SchemaReferenceKey = derivationSchemaReferenceKey };
            }
            else
            {
                var properties = jObject.Properties().Select(na => na.Name).ToList();
                if (properties.Count == 1 && !properties.First().EqualsIgnoreCase("path"))
                {
                    var pathProviderConfig = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(jObject.CreateReader());
                    return new FixedObjectPathLookupConfigModel { Path = pathProviderConfig, SchemaReferenceKey = derivationSchemaReferenceKey };
                }
                else
                {
                    var lookupConfig = new JObject
                    {
                        { "path", jObject.SelectToken("path") },
                    };
                    var dataObjectToken = jObject.SelectToken("dataObject");
                    if (dataObjectToken != null)
                    {
                        lookupConfig.Add("dataObject", dataObjectToken);
                    }

                    var pathLookupConfig = serializer.Deserialize<IBuilder<IObjectPathLookupProvider>>(lookupConfig.CreateReader());
                    var referenceKeyProperty = pathLookupConfig.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals("SchemaReferenceKey"));
                    referenceKeyProperty.SetValue(pathLookupConfig, derivationSchemaReferenceKey);
                    return pathLookupConfig;
                }
            }
        }

        protected IBuilder<IProvider<Data<bool>>> CreateRaiseErrorBuilder(JObject jObject, JsonSerializer serializer, string pathProperty)
        {
            IBuilder<IProvider<Data<bool>>> raiseErrorBuilder = null;
            if (jObject != null)
            {
                var raiseErrorToken = jObject.SelectToken(pathProperty);
                if (raiseErrorToken != null)
                {
                    raiseErrorBuilder = serializer.Deserialize<IBuilder<IProvider<Data<bool>>>>(raiseErrorToken.CreateReader());
                }
            }

            return raiseErrorBuilder;
        }

        protected IBuilder<IProvider<IData>> CreateLookupPropertiesBuilder(JObject jObject, JsonSerializer serializer, string path)
        {
            IBuilder<IProvider<IData>> builder = null;
            if (jObject != null)
            {
                var token = jObject.SelectToken(path);
                if (token != null)
                {
                    if (token.Type == JTokenType.Null)
                    {
                        var reader = token.CreateReader();
                        builder = new StaticBuilder<IData> { Value = (IData)reader.Value };
                    }
                    else
                    {
                        builder = serializer.Deserialize<IBuilder<IProvider<IData>>>(token.CreateReader());
                    }
                }
            }

            return builder;
        }
    }
}
