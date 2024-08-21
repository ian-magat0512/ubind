// <copyright file="JsonTextToObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Humanizer;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Generates a data object by parsing a JSON string passed as a text provider.
    /// </summary>
    public class JsonTextToObjectProvider : IObjectProvider
    {
        private readonly IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextToObjectProvider"/> class.
        /// </summary>
        /// <param name="textProvider">The text provider to be used.</param>
        public JsonTextToObjectProvider(IProvider<Data<string>> textProvider)
        {
            this.textProvider = textProvider;
        }

        public string SchemaReferenceKey => "jsonTextToObject";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            string jsonString = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (!jsonString.IsNullOrEmpty())
            {
                try
                {
                    // Clean string first as it could be double-serialized due to reading it from request.
                    var stringObject = JsonConvert.DeserializeObject(jsonString);
                    if (stringObject.GetType() == typeof(string))
                    {
                        jsonString = stringObject as string;
                    }

                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString, new AutomationDictionaryConverter());
                    return ProviderResult<Data<object>>.Success(new Data<object>(new ReadOnlyDictionary<string, object>(dictionary)));
                }
                catch (InvalidCastException ex)
                {
                    errorData.Add(ErrorDataKey.ValueToParse, jsonString.Truncate(80, "..."));
                    errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                    throw new ErrorException(Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData));
                }
                catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
                {
                    int lineNumber = 0, linePosition = 0;
                    string path;
                    if (ex is JsonSerializationException exception)
                    {
                        lineNumber = exception.LineNumber;
                        linePosition = exception.LinePosition;
                        path = exception.Path;
                    }
                    else
                    {
                        var readerException = ex as JsonReaderException;
                        lineNumber = readerException.LineNumber;
                        linePosition = readerException.LinePosition;
                        path = readerException.Path;
                    }

                    throw new ErrorException(Errors.JsonDocument.JsonInvalid(
                        "automation.json",
                        ex.Message,
                        lineNumber,
                        linePosition,
                        path,
                        jsonString));
                }
            }

            throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                "#text",
                this.SchemaReferenceKey));
        }
    }
}
