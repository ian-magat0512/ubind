// <copyright file="ContextEntityProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class ContextEntityProviderConfigModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ContextEntityProviderConfigModel) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var entityPath = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new ContextEntityProviderConfigModel() { Path = entityPath };
            }

            var errorData = new JObject()
            {
                { ErrorDataKey.ErrorMessage,  $"contextEntity requires a #jsonPointer for a path." },
            };
            throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
