// <copyright file="AdditionalPropertyDefinitionJsonValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition;

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;

public class AdditionalPropertyDefinitionJsonValidator : IAdditionalPropertyDefinitionJsonValidator
{
    private readonly IHostEnvironment hostEnvironment;

    public AdditionalPropertyDefinitionJsonValidator(IHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
    }

    public void ThrowIfSchemaIsNotValid(
        string jsonSchema,
        string alias,
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        AdditionalPropertyDefinitionContextType contextType)
    {
        try
        {
            JSchema schema = JSchema.Parse(jsonSchema);
        }
        catch
        {
            throw new ErrorException(Errors.AdditionalProperties
                .NotAValidJsonSchema(alias, tenantId, entityType, contextType));
        }
    }

    public void ThrowIfValueFailsSchemaAssertion(
        AdditionalPropertyDefinitionSchemaType schemaType,
        string fieldName,
        string jsonString,
        string? customSchema)
    {
        if (schemaType == AdditionalPropertyDefinitionSchemaType.None
            || string.IsNullOrEmpty(jsonString)
            || (schemaType == AdditionalPropertyDefinitionSchemaType.Custom && string.IsNullOrEmpty(customSchema)))
        {
            throw new InvalidOperationException();
        }

        try
        {
            var stringObject = JsonConvert.DeserializeObject(jsonString);
            if (stringObject.GetType() == typeof(string))
            {
                jsonString = stringObject as string;
            }

            var jsonObject = JToken.Parse(jsonString);
            switch (schemaType)
            {
                case AdditionalPropertyDefinitionSchemaType.OptionList:
                    this.ValidateAgainstOptionListSchema(jsonObject, fieldName);
                    break;
                case AdditionalPropertyDefinitionSchemaType.Custom:
                    this.ValidateAgainstCustomSchema(customSchema, jsonObject, fieldName);
                    break;
                default:
                    return;
            }
        }
        catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
        {
            int lineNumber = 0, linePosition = 0;
            string path;
            if (ex is JsonSerializationException serializationException)
            {
                lineNumber = serializationException.LineNumber;
                linePosition = serializationException.LinePosition;
                path = serializationException.Path;
            }
            else
            {
                var readerException = ex as JsonReaderException;
                lineNumber = readerException?.LineNumber ?? 0;
                linePosition = readerException?.LinePosition ?? 0;
                path = readerException?.Path ?? string.Empty;
            }

            throw new ErrorException(Errors.AdditionalProperties.InvalidJsonObject(
                fieldName, jsonString, ex.Message, lineNumber, linePosition, path));
        }
    }

    private void ValidateAgainstOptionListSchema(JToken jsonObject, string fieldName)
    {
        var filePath = Path.Combine(this.hostEnvironment.ContentRootPath, "schemas", "additional-properties", "option-list.schema.json");
        using (StreamReader fileStreamReader = File.OpenText(filePath))
        {
            using (JsonTextReader jsonTextReader = new JsonTextReader(fileStreamReader))
            {
                JSchema schema = JSchema.Load(jsonTextReader);
                IList<string> validationErrors;
                if (!jsonObject.IsValid(schema, out validationErrors))
                {
                    throw new ErrorException(
                        Errors.AdditionalProperties.SchemaValidationFailure("Option List", validationErrors, fieldName));
                }
            }
        }
    }

    private void ValidateAgainstCustomSchema(string jsonSchema, JToken jsonObject, string fieldName)
    {
        JSchema schema = JSchema.Parse(jsonSchema);
        IList<string> validationErrors;
        if (!jsonObject.IsValid(schema, out validationErrors))
        {
            throw new ErrorException(
                Errors.AdditionalProperties.SchemaValidationFailure("Custom", validationErrors, fieldName));
        }
    }
}
