// <copyright file="StringObjectConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using UBind.Domain.Exceptions;

/// <summary>
/// Custom JSON converter that allows serialization and deserialization of objects of type T
/// to and from their string representation using a static Parse method or constructor that
/// accepts a single string parameter.
/// </summary>
/// <typeparam name="T">The type of object to be serialized or deserialized.</typeparam>
public class StringObjectConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// Reads the JSON representation of the object and converts it to an instance of type T.
    /// </summary>
    /// <param name="reader">The JSON reader to read the JSON data from.</param>
    /// <param name="objectType">The type of object to be deserialized.</param>
    /// <param name="existingValue">The existing value of type T.</param>
    /// <param name="hasExistingValue">A flag indicating whether an existing value exists.</param>
    /// <param name="serializer">The JSON serializer being used.</param>
    /// <returns>An instance of type T that represents the deserialized object.</returns>
    /// <exception cref="JsonSerializationException">Thrown when there's an error converting the value to type T.</exception>
    public override T ReadJson(
        JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            // Read the whole JSON object using JObject.Load
            var jsonObject = JObject.Load(reader);

            // Assuming you have a property named "Value" in the JSON object
            string stringValue = jsonObject["Value"].Value<string>();
            return this.Convert(stringValue);
        }
        else if (reader.TokenType == JsonToken.String)
        {
            string stringValue = (string)reader.Value;
            return this.Convert(stringValue);
        }

        throw new ErrorException(Domain.Errors.JsonDocument.UnexpectedToken(reader, objectType, existingValue));
    }

    /// <summary>
    /// Writes the JSON representation of the object to the JSON writer.
    /// </summary>
    /// <param name="writer">The JSON writer to write the JSON data to.</param>
    /// <param name="value">The object of type T to be serialized.</param>
    /// <param name="serializer">The JSON serializer being used.</param>
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteValue(value.ToString());
        }
        else
        {
            writer.WriteNull();
        }
    }

    private T Convert(string stringValue)
    {
        if (typeof(T).GetMethod("Parse", new[] { typeof(string) }) is MethodInfo parseMethod)
        {
            return (T)parseMethod.Invoke(null, new object[] { stringValue });
        }
        else if (typeof(T).GetConstructor(new[] { typeof(string) }) is ConstructorInfo ctor)
        {
            return (T)ctor.Invoke(new object[] { stringValue });
        }

        throw new ErrorException(Domain.Errors.JsonDocument.CannotDeserializeType(stringValue, typeof(T).Name));
    }
}
