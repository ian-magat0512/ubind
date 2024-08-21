// <copyright file="PathLookupListObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Object;

    /// <summary>
    /// This class is needed to create an instance of <see cref="PathLookupListObjectProviderConfigModel"></see>
    /// out of a json object.
    /// </summary>
    public class PathLookupListObjectProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PathLookupListObjectProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonToken = JToken.Load(reader);
            IBuilder<IObjectProvider> dataObjectProviderBuilder = default;
            var dataObjectToken = jsonToken.Type == JTokenType.Object ? jsonToken.Value<JToken>("dataObject") : null;
            var pathPropertyTokenReader = jsonToken.Type == JTokenType.Array ?
                jsonToken.CreateReader() :
                jsonToken.Value<JToken>("properties").CreateReader();
            var propertyPathModels
                = serializer.Deserialize<IEnumerable<ObjectPathPropertyConfigModel>>(pathPropertyTokenReader);
            if (jsonToken.Type == JTokenType.Object && dataObjectToken != null)
            {
                dataObjectProviderBuilder
                    = serializer.Deserialize<IBuilder<IObjectProvider>>(dataObjectToken.CreateReader());
            }

            return new PathLookupListObjectProviderConfigModel(propertyPathModels, dataObjectProviderBuilder);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
