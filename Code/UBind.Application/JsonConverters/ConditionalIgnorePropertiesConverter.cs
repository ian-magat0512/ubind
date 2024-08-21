// <copyright file="ConditionalIgnorePropertiesConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This converter is used to conditionally ignore properties during serialization, particularly for List, Dictionary, and IEnumerable types when they are empty.
/// It avoids serializing empty collections, null properties, or properties with the JsonIgnore attribute.
/// The properties are manually written to the JSON writer, and properties that should not be serialized are skipped.
/// This converter can be applied to other classes that require conditional serialization.
/// </summary>
public class ConditionalIgnorePropertiesConverter<T> : JsonConverter<T>
{
    private readonly PropertyInfo[] typeProperties;

    public ConditionalIgnorePropertiesConverter()
    {
        this.typeProperties = typeof(T).GetProperties();
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = JsonSerializer.Deserialize(ref reader, typeof(T), options);

        if (result is T genericResult)
        {
            return genericResult;
        }

        throw new JsonException($"Cannot deserialize {typeof(T).Name}.");
    }

    /// <summary>
    /// This method serializes the object to JSON and skips properties that should not be serialized.
    /// This is an implementation of conditional serialization. Since the JsonIgnore attribute does not completely address conditional serialization,
    /// we need to serialize the whole class and skip the properties that should not be serialized.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var property in this.typeProperties)
        {
            var propertyValue = property.GetValue(value);
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null
                || this.IsEmptyCollection(propertyValue)
                || !this.ShouldSerializeProperty(propertyValue))
            {
                continue; // Skip properties with JsonIgnore or properties that should not be serialized or value is null
            }

            var propertyName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
        }

        writer.WriteEndObject();
    }

    protected virtual bool ShouldSerializeProperty(object? propertyValue)
    {
        if (propertyValue == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the collections are empty so that they are not serialized.
    /// </summary>
    private bool IsEmptyCollection(object? propertyValue)
    {
        if (propertyValue == null)
        {
            return true;
        }

        // Check for IList<object> first as it covers most collection types like List<>
        if (propertyValue is IList<object> list && list.Count == 0)
        {
            return true;
        }

        // Check for IDictionary<object, object> as it covers most dictionary types like Dictionary<,>
        if (propertyValue is IDictionary<object, object> dictionary && dictionary.Count == 0)
        {
            return true;
        }

        if (propertyValue is IEnumerable<object> enumerable && !enumerable.Any())
        {
            return true;
        }

        var propertyType = propertyValue.GetType();
        if (propertyType.IsGenericType
            && (propertyType.GetGenericTypeDefinition() == typeof(List<>)
            || propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
        {
            var countProperty = propertyType.GetProperty("Count");
            if (countProperty != null)
            {
                var count = (int)(countProperty.GetValue(propertyValue) ?? 0);
                return count == 0;
            }
        }

        return false;
    }
}
