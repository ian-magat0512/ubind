// <copyright file="JsonSchemaValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using File = System.IO.File;

    /// <summary>
    /// Validates a Json file both for JSON syntax errors and against a json schema.
    /// </summary>
    public class JsonSchemaValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchemaValidator"/> class.
        /// </summary>
        /// <param name="schemaFolderPath">The full path to the location where the schema json files are located.</param>
        public JsonSchemaValidator(string schemaFolderPath)
        {
            if (!Directory.Exists(schemaFolderPath))
            {
                throw new FileNotFoundException($"When instantiating JsonSchemaValidator, the schema folder path '{schemaFolderPath}' was not found.", schemaFolderPath);
            }

            this.SchemaFolderPath = schemaFolderPath;
        }

        /// <summary>
        /// Gets the full path of the folder which contains the schema files.
        /// </summary>
        public string SchemaFolderPath { get; }

        /// <summary>
        /// Validates a json string for valid syntax and against a schema.
        /// If there is a validation error, an exception is thrown.
        /// </summary>
        /// <param name="jsonDocumentName">The name of the json document to be validated.</param>
        /// <param name="jsonObject">The json object which we want to validate against a schema.</param>
        /// <param name="schemaName">The name of the schema. Validation will occur against a file in the /schemas/automations folder with this name,
        /// in the format {schemaName}.schema.{schemaVersion}.json.</param>
        public void Validate(string jsonDocumentName, JObject jsonObject, string schemaName, string? providedSchemaVersion = "")
        {
            string schemaVersion = string.IsNullOrEmpty(providedSchemaVersion)
                ? GetSchemaVersion(jsonObject, jsonDocumentName)
                : providedSchemaVersion;
            string schemaNameWithVersion = schemaName + ".schema." + schemaVersion;
            string schemaFilename = schemaNameWithVersion + ".json";
            string schemaFilePath = Path.Combine(this.SchemaFolderPath, schemaFilename);
            if (!File.Exists(schemaFilePath))
            {
                throw new ErrorException(Errors.JsonDocument.SchemaVersionNotFound(schemaName, schemaVersion));
            }

            using (StreamReader fileStreamReader = File.OpenText(schemaFilePath))
            using (JsonTextReader jsonTextReader = new JsonTextReader(fileStreamReader))
            {
                JSchema schema = JSchema.Load(jsonTextReader);
                IList<string> validationErrors;
                if (!jsonObject.IsValid(schema, out validationErrors))
                {
                    throw new ErrorException(Errors.JsonDocument.SchemaValidationFailure(jsonDocumentName, schemaNameWithVersion, validationErrors));
                }
            }
        }

        /// <summary>
        /// Validates a json against a schema.
        /// If there is a validation error, an exception is thrown.
        /// </summary>
        /// <param name="documentName">The name of the json document to be validated.</param>
        /// <param name="json">The json which we want to validate against a schema.</param>
        /// <param name="schemaName">The name of the schema. Validation will occur against a file in the /schemas/automations folder with this name,
        /// in the format {schemaName}.schema.{schemaVersion}.json.</param>
        public void ValidateJsonAgainstSchema(string documentName, string json, string schemaName, string? schemaVersion = "")
        {
            JObject jsonObject = this.ParseJSON(json, documentName);
            this.Validate(documentName, jsonObject, schemaName, schemaVersion);
        }

        /// <summary>
        /// Retrieves the json property <c>schemaName</c> from the <c>jsonObject</c> provided, throwing an exception if it's not found.
        /// </summary>
        /// <returns>A <c>string</c> representing the value of the <c>schemaName</c> property.</returns>
        /// <param name="jsonObject">The json to parse.</param>
        /// <param name="jsonDocumentName">The name of the json file, without the filename extension.</param>
        private static string GetSchemaVersion(JObject jsonObject, string jsonDocumentName)
        {
            string schemaVersion = (string)jsonObject["schemaVersion"];
            if (schemaVersion.IsNullOrEmpty())
            {
                throw new ErrorException(Errors.JsonDocument.SchemaVersionPropertyNotFound(jsonDocumentName));
            }

            return schemaVersion;
        }

        /// <summary>
        /// Parses and checks that <c>json</c> contains valid JSON, and if not, throws an exception with a message identifying it by <c>name</c>.
        /// </summary>
        /// <returns>A <c>JObject</c> representing the json in a C# object.</returns>
        /// <param name="json">The json to parse.</param>
        /// <param name="jsonDocumentName">The name of the json file, without the filename extension.</param>
        private JObject ParseJSON(string json, string jsonDocumentName)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                throw new ErrorException(Errors.JsonDocument.JsonInvalid(jsonDocumentName, ex.Message, ex.LineNumber, ex.LinePosition, ex.Path, json));
            }
        }
    }
}
