// <copyright file="SystemEventEntityProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Converter for event entity provider expecting an object of type data object.
    /// </summary>
    public class SystemEventEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(SystemEventEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new SystemEventEntityProviderConfigModel() { EventId = id };
            }
            else
            {
                var eventId = default(IBuilder<IProvider<Data<string>>>);
                var obj = JObject.Load(reader);
                var eventIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("eventId"));
                if (eventIdProperty != null)
                {
                    eventId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(eventIdProperty.Value.CreateReader());
                }

                if (eventId == default)
                {
                    var errorData = new JObject()
                    {
                        { ErrorDataKey.ErrorMessage,  $"event requires an eventId." },
                    };
                    throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
                }

                return new SystemEventEntityProviderConfigModel() { EventId = eventId };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
