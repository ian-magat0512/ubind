// <copyright file="ContextEntitiesObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is used to convert a json object to an instance of <see cref="ContextEntitiesObjectProviderConfigModel"/>.
    /// </summary>
    public class ContextEntitiesObjectProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ContextEntitiesObjectProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonToken = JToken.Load(reader);
            var entityPaths = jsonToken.Type == JTokenType.Array ?
                this.ParseTypeValues(jsonToken) :
                this.ParseTypeValues(jsonToken.ToObject<JObject>().Properties().First().Value);
            return new ContextEntitiesObjectProviderConfigModel { ContextEntities = entityPaths };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IBuilder<IProvider<Data<string>>>> ParseTypeValues(JToken valueToken)
        {
            var arrayToken = valueToken as JArray;
            return arrayToken.Select(token => new StaticBuilder<Data<string>>() { Value = token.ToString() });
        }
    }
}
