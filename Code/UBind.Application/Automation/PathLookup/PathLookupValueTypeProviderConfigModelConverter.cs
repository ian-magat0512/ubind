// <copyright file="PathLookupValueTypeProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Application.Automation.Providers;

/// <summary>
/// Converter for deserializing object path lookup for value type provider objects from json.
/// So we can set the value type provider config model properties.
/// </summary>
/// <typeparam name="TModel">The config model of the provider to be set.</typeparam>
public class PathLookupValueTypeProviderConfigModelConverter<TModel> : PathLookupDerivativeConverter
{
    private readonly string schemaReference;

    public PathLookupValueTypeProviderConfigModelConverter(string schemaReference)
    {
        this.schemaReference = schemaReference;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(TModel) == objectType;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        JObject obj = value == null ? JObject.Load(reader) : null;
        TModel objectPathLookupModel = Activator.CreateInstance<TModel>();

        // Use reflection to set properties
        this.SetProperty(objectPathLookupModel, "PathLookup",
            this.CreateLookupBuilder(value, obj, objectType, serializer, this.schemaReference));
        this.SetProperty(objectPathLookupModel, "ValueIfNotFound",
            this.CreateLookupPropertiesBuilder(obj, serializer, this.ValueIfNotFoundProperty));
        this.SetProperty(objectPathLookupModel, "RaiseErrorIfNotFound",
            this.CreateRaiseErrorBuilder(obj, serializer, this.RaiseErrorIfNotFoundProperty));
        this.SetProperty(objectPathLookupModel, "RaiseErrorIfNull",
            this.CreateRaiseErrorBuilder(obj, serializer, this.RaiseErrorIfNullProperty));
        this.SetProperty(objectPathLookupModel, "ValueIfNull",
            this.CreateLookupPropertiesBuilder(obj, serializer, this.ValueIfNullProperty));
        this.SetProperty(objectPathLookupModel, "RaiseErrorIfTypeMismatch",
            this.CreateRaiseErrorBuilder(obj, serializer, this.RaiseErrorIfTypeMismatchProperty));
        this.SetProperty(objectPathLookupModel, "ValueIfTypeMismatch",
            this.CreateLookupPropertiesBuilder(obj, serializer, this.ValueIfTypeMismatchProperty));
        this.SetProperty(objectPathLookupModel, "DefaultValue",
            this.CreateLookupPropertiesBuilder(obj, serializer, this.DefaultValueProperty));

        return objectPathLookupModel;
    }

    private void SetProperty<TModel>(TModel model, string propertyName, object value)
    {
        var property = typeof(TModel).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(model, value, null);
        }
    }
}
