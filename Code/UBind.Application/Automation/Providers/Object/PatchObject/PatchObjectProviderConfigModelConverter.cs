// <copyright file="PatchObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is used to convert a json object to an instance of <see cref="PatchObjectProviderConfigModel"/>.
    /// </summary>
    public class PatchObjectProviderConfigModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PatchObjectProviderConfigModel) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var objectValue = default(IBuilder<IObjectProvider>);
            var operations = new List<IBuilder<BaseOperation>>();
            var valueIfAbortedValue = default(IBuilder<IObjectProvider>);

            var objectProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("object"));
            if (objectProperty != null)
            {
                objectValue = serializer.Deserialize<IBuilder<IObjectProvider>>(objectProperty.Value.CreateReader());
            }

            var operationsProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("operations"));
            if (operationsProperty != null)
            {
                operations = serializer.Deserialize<List<IBuilder<BaseOperation>>>(operationsProperty.Value.CreateReader());
            }

            var valueIfAbortedProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("valueIfAborted"));
            if (valueIfAbortedProperty != null)
            {
                valueIfAbortedValue = serializer.Deserialize<IBuilder<IObjectProvider>>(valueIfAbortedProperty.Value.CreateReader());
            }

            return new PatchObjectProviderConfigModel(objectValue, operations, valueIfAbortedValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
