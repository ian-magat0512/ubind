// <copyright file="UserEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for user entity provider expecting an object of type data object.
    /// </summary>
    public class UserEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(UserEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new UserEntityProviderConfigModel() { UserId = id };
            }
            else
            {
                var userId = default(IBuilder<IProvider<Data<string>>>);
                var userAccountEmail = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var userIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("userId"));
                if (userIdProperty != null)
                {
                    userId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(userIdProperty.Value.CreateReader());
                }

                var userAccountEmailProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("userAccountEmail"));
                if (userAccountEmailProperty != null)
                {
                    userAccountEmail = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(userAccountEmailProperty.Value.CreateReader());
                }

                if (userId == default && userAccountEmail == default)
                {
                    userId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new UserEntityProviderConfigModel() { UserId = userId, UserAccountEmail = userAccountEmail };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
