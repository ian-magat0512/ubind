// <copyright file="PortalEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for portal entity provider expecting an object of type data object.
    /// </summary>
    public class PortalEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PortalEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new PortalEntityProviderConfigModel() { PortalId = id };
            }
            else
            {
                var portalId = default(IBuilder<IProvider<Data<string>>>);
                var portalAlias = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var portalIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("portalId"));
                if (portalIdProperty != null)
                {
                    portalId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(portalIdProperty.Value.CreateReader());
                }

                var portalAliasProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("portalAlias"));
                if (portalAliasProperty != null)
                {
                    portalAlias = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(portalAliasProperty.Value.CreateReader());
                }

                if (portalId == default && portalAlias == default)
                {
                    portalId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new PortalEntityProviderConfigModel() { PortalId = portalId, PortalAlias = portalAlias };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
