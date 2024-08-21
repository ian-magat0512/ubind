// <copyright file="EmailMessageEntityProviderConfigModelConverter.cs" company="uBind">
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
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Converter for email entity provider expecting an object of type data object.
    /// </summary>
    public class EmailMessageEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EmailMessageEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new EmailMessageEntityProviderConfigModel() { EmailId = id };
            }
            else
            {
                var obj = JObject.Load(reader);
                var emailIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("emailId"));
                if (emailIdProperty != null)
                {
                    var emailId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(emailIdProperty.Value.CreateReader());
                    return new EmailMessageEntityProviderConfigModel() { EmailId = emailId };
                }
                else
                {
                    var emailId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                    return new EmailMessageEntityProviderConfigModel() { EmailId = emailId };
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
