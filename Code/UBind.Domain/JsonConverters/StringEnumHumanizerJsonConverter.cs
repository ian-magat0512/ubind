// <copyright file="StringEnumHumanizerJsonConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Converts Enums to strings and back again using Humanizer.
    /// </summary>
    public class StringEnumHumanizerJsonConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Enum enumValue = value as Enum;
            writer.WriteValue(enumValue.Humanize());
        }

        /// <inheritdoc/>
        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string? enumString = (string?)reader.Value;
            return enumString?.ToEnumOrThrow(objectType);
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
