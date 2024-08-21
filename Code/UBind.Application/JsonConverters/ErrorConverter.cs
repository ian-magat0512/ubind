// <copyright file="ErrorConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.JsonConverters;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.Exceptions;

public class ErrorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Domain.Error);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        try
        {
            return serializer.Deserialize<Domain.Error>(reader);
        }
        catch (JsonException ex)
        {
            throw new ErrorException(Domain.Errors.JsonDocument.UnexpectedToken(reader, objectType, existingValue, ex.Message));
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var errorValue = value as Domain.Error;
        var headerObject = new JObject
        {
            { "code", errorValue.Code },
            { "title", errorValue.Title },
            { "message", errorValue.Message },
            { "httpStatusCode", (int)errorValue.HttpStatusCode },
        };

        if (errorValue.AdditionalDetails != null)
        {
            headerObject.Add("additionalDetails", JToken.FromObject(errorValue.AdditionalDetails));
        }

        if (errorValue.Data != null)
        {
            headerObject.Add("data", errorValue.Data);
        }

        headerObject.WriteTo(writer);
    }
}
