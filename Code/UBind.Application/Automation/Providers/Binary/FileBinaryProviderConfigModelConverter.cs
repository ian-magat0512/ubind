// <copyright file="FileBinaryProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This class is used to convert a json object to an instance of <see cref="FileBinaryProviderConfigModel"/>.
    /// </summary>
    public class FileBinaryProviderConfigModelConverter : DeserializationConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(FileBinaryProviderConfigModel) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fileProvider = serializer.Deserialize<IBuilder<IProvider<Data<FileInfo>>>>(reader);
            var fileBinaryModel = new FileBinaryProviderConfigModel() { FileProvider = fileProvider };
            return fileBinaryModel;
        }
    }
}
