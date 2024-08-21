// <copyright file="PdfFileProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Converter for pdf file provider.
    /// </summary>
    public class PdfFileProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            var canConvert = typeof(PdfFileProviderConfigModel) == objectType;
            return canConvert;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobject = JObject.Load(reader);
            var sourceToken = jobject.SelectToken("sourceFile");
            if (sourceToken != null)
            {
                var outputFileNameToken = jobject.SelectToken("outputFilename");

                IBuilder<IProvider<Data<FileInfo>>> source = serializer.Deserialize<IBuilder<IProvider<Data<FileInfo>>>>(
                    sourceToken.CreateReader());

                IBuilder<IProvider<Data<string>>> outputFileName = null;
                if (outputFileNameToken != null)
                {
                    outputFileName = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(outputFileNameToken.CreateReader());
                }

                return new PdfFileProviderConfigModel
                {
                    SourceFile = source,
                    OutputFileName = outputFileName,
                };
            }
            else
            {
                var source = serializer.Deserialize<IBuilder<IProvider<Data<FileInfo>>>>(jobject.CreateReader());
                return new PdfFileProviderConfigModel
                {
                    SourceFile = source,
                };
            }
        }
    }
}
