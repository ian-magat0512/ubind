// <copyright file="JsonPathConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Json;

    /// <summary>
    /// Json converter for <see cref="JsonPath"/>.
    /// </summary>
    public class JsonPathConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonPath).IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new JsonPath((string)reader.Value);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonPath = (JsonPath)value;
            writer.WriteValue(jsonPath.Value);
        }
    }
}
