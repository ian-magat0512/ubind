// <copyright file="PatchDocumentConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote;

using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
/// This class is used to read and write JSON Patch documents.
/// It also can read the old FormDataPatch format which is not longer used.
/// </summary>
public class PatchDocumentConverter : JsonConverter
{
    public PatchDocumentConverter()
    {
        System.Diagnostics.Debug.Write("Instantiating PatchDocumentConverter");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(JsonPatchDocument);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        JArray operationTokens;

        // Check if the token is a JObject and has '$values' field.
        if (token.Type == JTokenType.Object && token["$values"] != null)
        {
            operationTokens = (JArray)token["$values"];
        }
        else if (token.Type == JTokenType.Array)
        {
            operationTokens = (JArray)token;
        }
        else
        {
            throw new JsonSerializationException("Unexpected token type: " + token.Type.ToString());
        }

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None, // Ignore type information
            ContractResolver = serializer.ContractResolver,
            Formatting = serializer.Formatting,
            DateParseHandling = serializer.DateParseHandling,
            FloatParseHandling = serializer.FloatParseHandling,
            StringEscapeHandling = serializer.StringEscapeHandling,
            Culture = serializer.Culture,
        };

        var operations = operationTokens.ToObject<List<Operation>>(JsonSerializer.Create(settings));
        var patchDoc = new JsonPatchDocument();
        patchDoc.Operations.AddRange(operations);
        return patchDoc;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var patchDoc = value as JsonPatchDocument;

        // Create a new JsonSerializer that doesn't output type information.
        var newSerializer = new JsonSerializer
        {
            ContractResolver = serializer.ContractResolver,
            Formatting = serializer.Formatting,

            // Ensure TypeNameHandling is None
            TypeNameHandling = TypeNameHandling.None,
        };

        newSerializer.Serialize(writer, patchDoc.Operations);
    }
}
