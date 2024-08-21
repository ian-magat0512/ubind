// <copyright file="PropertyDiscriminatorConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Export;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Converter for deserializing json into the correct child type based on
    /// the property key, and a map of the keys to the concrete types.
    /// </summary>
    /// <typeparam name="T">The parent type.</typeparam>
    public class PropertyDiscriminatorConverter<T> : DeserializationConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDiscriminatorConverter{T}"/> class.
        /// </summary>
        /// <param name="typeMap">A map of keys to concrete classes.</param>
        public PropertyDiscriminatorConverter(TypeMap typeMap)
        {
            this.TypeMap = typeMap;
        }

        /// <summary>
        /// Gets a map of property keys to object types that indicates which type to deserialize to based on the
        /// first property of the JSON object..
        /// </summary>
        protected TypeMap TypeMap { get; }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = this.TryDeserializeBasedOnTypeMap(reader, serializer);
            if (result.IsSuccess)
            {
                return result.Value;
            }

            var errorData = new JObject
            {
                { "failingAutomationConfiguration", result.Error },
            };
            throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
        }

        /// <summary>
        /// Tries to deserialize the object based on the value of its first property.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>A result with the deserialized object if successful, otherwise a failure result.</returns>
        protected Result<object> TryDeserializeBasedOnTypeMap(JsonReader reader, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var key = jObject.Properties().FirstOrDefault().Name.ToString();
            if (this.TypeMap.TryGetValue(key, out Type objectType))
            {
                var token = jObject.SelectToken(key);
                return Result.Success(serializer.Deserialize(token.CreateReader(), objectType));
            }

            return Result.Failure<object>(jObject.ToString());
        }
    }
}
