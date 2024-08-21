// <copyright file="GenericConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Triggers.ExtensionPointTrigger;

    /// <summary>
    /// Converter for deserializing json in to the correct child type based on
    /// according to a key in a "type" field, and a map of keys to concrete types.
    /// </summary>
    /// <typeparam name="T">The parent type.</typeparam>
    public class GenericConverter<T> : JsonConverter
    {
        private readonly TypeMap typeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericConverter{T}"/> class.
        /// </summary>
        /// <param name="typeMap">The map of keys to concrete types.</param>
        public GenericConverter(TypeMap typeMap)
        {
            this.typeMap = typeMap;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var key = jObject["type"].Value<string>();

            if (this.typeMap.TryGetValue(key, out objectType))
            {
                if (objectType == typeof(ExtensionPointTriggerData))
                {
                    var extensionPointConverter = new ExtensionPointTriggerDataConverter();
                    return extensionPointConverter.ReadJson(jObject.CreateReader(), objectType, existingValue, serializer);
                }

                var target = Activator.CreateInstance(objectType);
                serializer.Populate(jObject.CreateReader(), target);
                return target;
            }

            throw new NotSupportedException("Cannot parse type in json: " + key);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
