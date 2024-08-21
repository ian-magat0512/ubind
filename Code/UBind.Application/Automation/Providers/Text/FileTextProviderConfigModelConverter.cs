// <copyright file="FileTextProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This class is for the custom deserialization of a json object of the type <see cref="FileTextProviderConfigModel"/>.
    /// </summary>
    public class FileTextProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(FileTextProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fileProvider = serializer.Deserialize<IBuilder<IProvider<Data<FileInfo>>>>(reader);
            var fileTextModel = new FileTextProviderConfigModel() { File = fileProvider };
            return fileTextModel;
        }
    }
}
