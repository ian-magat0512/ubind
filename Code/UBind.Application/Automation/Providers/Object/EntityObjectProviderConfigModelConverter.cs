// <copyright file="EntityObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.Entity;

    /// <summary>
    /// This class is needed because ObjectEntityProvider has custom schema for entity and its related entities.
    /// </summary>
    public class EntityObjectProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EntityObjectProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var entity = default(IBuilder<BaseEntityProvider>);
            var optionalProperties = new List<string>();

            var obj = JObject.Load(reader);
            var entityProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("entity"));
            if (entityProperty != null)
            {
                entity = serializer.Deserialize<IBuilder<BaseEntityProvider>>(entityProperty.Value.CreateReader());
            }

            var optionalPropertiesJSON = obj.Properties()
                .FirstOrDefault(c => c.Name.Equals("includeOptionalProperties"));
            if (optionalPropertiesJSON != null)
            {
                optionalProperties = serializer.Deserialize<List<string>>(optionalPropertiesJSON.Value.CreateReader());
            }

            if (entity == default)
            {
                entity = serializer.Deserialize<IBuilder<BaseEntityProvider>>(obj.CreateReader());
            }

            return new EntityObjectProviderConfigModel() { Entity = entity, IncludeOptionalProperties = optionalProperties };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
