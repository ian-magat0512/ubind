// <copyright file="PersonEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for person entity provider expecting an object of type data object.
    /// </summary>
    public class PersonEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PersonEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new PersonEntityProviderConfigModel() { PersonId = id };
            }
            else
            {
                var obj = JObject.Load(reader);
                var personIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("personId"));
                var personId =
                    serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(personIdProperty != null ?
                    personIdProperty.Value.CreateReader() :
                    obj.CreateReader());

                return new PersonEntityProviderConfigModel() { PersonId = personId };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
